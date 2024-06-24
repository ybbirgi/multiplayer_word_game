using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper.Internal;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Localization;
using Newtonsoft.Json;
using Volo.Abp;
using Volo.Abp.AspNetCore.SignalR;
using Volo.Abp.EventBus.Local;
using Volo.Abp.Uow;
using WordGame.Constants;
using WordGame.Constants.Signal_R;
using WordGame.Dtos.Challenges;
using WordGame.Dtos.GameResults;
using WordGame.Dtos.Games;
using WordGame.Dtos.PreGameInfos;
using WordGame.Dtos.Users;
using WordGame.Entities;
using WordGame.ExceptionCodes;
using WordGame.Extensions;
using WordGame.Hubs.Helpers;
using WordGame.LocalEvents.Etos;
using WordGame.Localization;
using WordGame.Managers;
using WordGame.Models;
using WordGame.Models.Hubs;
using WordGame.Models.Lobbies;
using WordGame.Models.PreGameInfos;
using WordGame.Repositories;
using IObjectMapper = Volo.Abp.ObjectMapping.IObjectMapper;

namespace WordGame.Hubs;

public class GameHub : AbpHub
{
    public static readonly List<GameHubConnectionModel> Connections = new();
    private readonly IGameRepository _gameRepository;
    private readonly GameManager _gameManager;
    private readonly IHubContext<LobbyHub> _lobbyHubContext;
    private readonly IUnitOfWorkManager _unitOfWorkManager;
    private readonly IStringLocalizer<WordGameResource> _stringLocalizer;
    private readonly ILocalEventBus _localEventBus;
    private readonly ChallengeRequestManager _challengeRequestManager;
    private readonly IChallengeRequestRepository _challengeRequestRepository;
    private readonly IObjectMapper _objectMapper;
    private readonly PreGameInfoManager _preGameInfoManager;
    private readonly IPreGameInfoRepository _preGameInfoRepository;


    public GameHub(
        IGameRepository gameRepository,
        GameManager gameManager,
        IHubContext<LobbyHub> lobbyHubContext,
        IUnitOfWorkManager unitOfWorkManager,
        IStringLocalizer<WordGameResource> stringLocalizer,
        ILocalEventBus localEventBus,
        ChallengeRequestManager challengeRequestManager,
        IChallengeRequestRepository challengeRequestRepository,
        IObjectMapper objectMapper,
        PreGameInfoManager preGameInfoManager,
        IPreGameInfoRepository preGameInfoRepository
    )
    {
        _gameRepository = gameRepository;
        _gameManager = gameManager;
        _lobbyHubContext = lobbyHubContext;
        _unitOfWorkManager = unitOfWorkManager;
        _stringLocalizer = stringLocalizer;
        _localEventBus = localEventBus;
        _challengeRequestManager = challengeRequestManager;
        _challengeRequestRepository = challengeRequestRepository;
        _objectMapper = objectMapper;
        _preGameInfoManager = preGameInfoManager;
        _preGameInfoRepository = preGameInfoRepository;
    }

    public override async Task OnConnectedAsync()
    {
        RemoveObsoleteConnections();

        var gameId = Context.GetHttpContext()!.Request.Query[HttpContextConstants.Headers.GameId].ToString();

        var game = await _gameManager.GetByAsync(p => p.Id.Equals(Guid.Parse(gameId)),
            throwIfNotExists: true,
            includeDetails: true
        );

        var relatedChannel = Lobby.Channels.First(p => p.Id.Equals(game!.PreGameInfo.ChallengeRequest.ChannelId));

        var challengeUserType = game.PreGameInfo.ChallengeRequest.SenderUserId == CurrentUser.Id
            ? ChallengeUserTypes.Sender
            : ChallengeUserTypes.Receiver;

        var gameHubConnectionModel = new GameHubConnectionModel()
        {
            UserId = CurrentUser.Id.GetValueOrDefault(),
            ConnectionId = Context.ConnectionId,
            GameId = Guid.Parse(gameId),
            ChannelId = relatedChannel.Id,
            LastCommunicationTime = DateTime.Now,
            ChallengeUserType = challengeUserType,
            InitialConnectionTime = DateTime.Now
        };

        Connections.TryAdd(gameHubConnectionModel);

        var user = Lobby.Channels
            .SelectMany(p => p.Users)
            .FirstOrDefault(p =>
                p.IdentityUser.Id.Equals(CurrentUser.Id));

        user.UpdateStatus(UserStatusTypes.InGame);

        await relatedChannel.UpdateActiveUserListAsync(_lobbyHubContext);

        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var connection = Connections.Find(c => c.ConnectionId == Context.ConnectionId);
        if (connection is not null)
        {
            var game = await _gameManager.GetByAsync(p => p.Id.Equals(connection.GameId),
                throwIfNotExists: true,
                includeDetails: true
            );
            if (game.GameStatusId == (int)GameStatusTypes.Finished)
            {
                var user = Lobby.Channels
                    .SelectMany(p => p.Users)
                    .FirstOrDefault(p =>
                        p.IdentityUser.Id.Equals(connection.UserId));

                user.UpdateStatus(UserStatusTypes.Online);

                var relatedChannel = Lobby.Channels.First(p => p.Id.Equals(connection.ChannelId));

                await relatedChannel.UpdateActiveUserListAsync(_lobbyHubContext);
            }

            Connections.Remove(connection);
            await _localEventBus.PublishAsync(new GameUserDisconnectedEventEto
            {
                GameHubConnectionModel = connection
            });
        }
    }

    [HubMethodName(SignalRConstants.GameHubConstants.Methods.SendReMatch)]
    public async Task SendRematchAsync()
    {
        await _unitOfWorkManager.ExecuteInUnitOfWork(async () =>
        {
            var connection = Connections.Find(c => c.ConnectionId == Context.ConnectionId);

            var otherConnection = Connections
                .Find(
                    p =>
                        p.GameId == connection.GameId &&
                        p.UserId != CurrentUser.Id
                );

            var challengeRequest = _challengeRequestManager.Create(otherConnection.UserId, connection.ChannelId);

            await _challengeRequestRepository.InsertAsync(challengeRequest, autoSave: true);

            var challengeRequestDto = _objectMapper.Map<ChallengeRequest, ChallengeRequestDto>(challengeRequest);

            await Clients.User(otherConnection.UserId.ToString()).SendAsync(
                SignalRConstants.GameHubConstants.Methods.ChallengeReceived,
                challengeRequestDto
            );
        });
    }

    [HubMethodName(SignalRConstants.GameHubConstants.Methods.RejectReMatch)]
    public async Task RejectRematchAsync(string challengeRequestId)
    {
        ChallengeRequest challengeRequest = null;
        await _unitOfWorkManager.ExecuteInUnitOfWork(async () =>
        {
            challengeRequest = await _challengeRequestManager.GetByAsync(
                p => p.Id.Equals(Guid.Parse(challengeRequestId)),
                throwIfNotExists: true
            );

            challengeRequest.ChallengeStatusId = (int)ChallengeStatus.Rejected;

            await _challengeRequestRepository.UpdateAsync(challengeRequest);
        });

        var challengeRequestResultDto = new ChallengeRequestResultDto
        {
            IsAccepted = false
        };

        await Clients.User(challengeRequest.SenderUserId.ToString()).SendAsync(
            SignalRConstants.GameHubConstants.Methods.ReMatchResult,
            challengeRequestResultDto
        );

        await Clients.User(challengeRequest.ReceiverUserId.ToString()).SendAsync(
            SignalRConstants.GameHubConstants.Methods.ReMatchResult,
            challengeRequestResultDto
        );

        var users = Lobby.Channels
            .SelectMany(p => p.Users)
            .Where(p =>
                p.IdentityUser.Id.Equals(challengeRequest.SenderUserId) ||
                p.IdentityUser.Id.Equals(challengeRequest.ReceiverUserId))
            .ToList();

        users.ForEach(p => p.UserStatusTypes = UserStatusTypes.Online);

        var connection = Connections.Find(c => c.ConnectionId == Context.ConnectionId);

        var relatedChannel = Lobby.Channels.First(p => p.Id.Equals(connection.ChannelId));

        await relatedChannel.UpdateActiveUserListAsync(_lobbyHubContext);
    }

    [HubMethodName(SignalRConstants.GameHubConstants.Methods.AcceptReMatch)]
    public async Task AcceptRematchAsync(string challengeRequestId)
    {
        using var uow = _unitOfWorkManager.Begin(true);

        var connection = Connections.Find(c => c.ConnectionId == Context.ConnectionId);

        var challengeRequest = await _challengeRequestManager.GetByAsync(
            p => p.Id.Equals(Guid.Parse(challengeRequestId)),
            throwIfNotExists: true
        );

        var relatedChannel = Lobby.Channels.First(p => p.Id.Equals(connection.ChannelId));

        challengeRequest.ChallengeStatusId = (int)ChallengeStatus.Accepted;
        await _challengeRequestRepository.UpdateAsync(challengeRequest);

        var preGameInfo = _preGameInfoManager.Create(new PreGameInfoCreateModel
        {
            ChallengeRequestId = challengeRequest.Id,
            GameTypeId = (int)relatedChannel.GameTypeId,
            WordLength = relatedChannel.WordLength
        });

        await _preGameInfoRepository.InsertAsync(preGameInfo, true);

        var preGameInfoDto = _objectMapper.Map<PreGameInfo, PreGameInfoDto>(preGameInfo);

        var challengeRequestResultDto = new ChallengeRequestResultDto
        {
            IsAccepted = true,
            PreGameInfoDto = preGameInfoDto
        };

        var game = _gameManager.Create(preGameInfo.Id, connection.GameId);

        await _gameRepository.InsertAsync(game, true);

        await uow.CompleteAsync();
        await uow.SaveChangesAsync();

        await Clients.User(challengeRequest.SenderUserId.ToString()).SendAsync(
            SignalRConstants.GameHubConstants.Methods.ReMatchResult,
            challengeRequestResultDto
        );

        await Clients.User(challengeRequest.ReceiverUserId.ToString()).SendAsync(
            SignalRConstants.GameHubConstants.Methods.ReMatchResult,
            challengeRequestResultDto
        );

        var users = Lobby.Channels
            .SelectMany(p => p.Users)
            .Where(p =>
                p.IdentityUser.Id.Equals(game.PreGameInfo.ChallengeRequest.SenderUserId) ||
                p.IdentityUser.Id.Equals(game.PreGameInfo.ChallengeRequest.ReceiverUserId))
            .ToList();

        users.ForEach(p => p.UserStatusTypes = UserStatusTypes.InGame);

        await relatedChannel.UpdateActiveUserListAsync(_lobbyHubContext);
    }

    [HubMethodName(SignalRConstants.GameHubConstants.Methods.GuessWord)]
    public async Task GuessWordAsync(string word)
    {
        using var uow = _unitOfWorkManager.Begin(true);

        var connection = Connections.Find(c => c.UserId == CurrentUser.Id);

        var otherConnection = Connections
            .Find(
                p =>
                    p.GameId == connection.GameId &&
                    p.UserId != CurrentUser.Id
            );

        var relatedChannel = Lobby.Channels.First(p => p.Id.Equals(connection.ChannelId));

        connection.LastCommunicationTime = DateTime.Now;

        if (word.Length != relatedChannel.WordLength && word != SignalRConstants.EscapeString)
        {
            throw new UserFriendlyException(_stringLocalizer[PreGameInfoExceptionCodes.WordLengthNotValid]);
        }

        var game = await _gameManager.GetByAsync(p => p.Id.Equals(connection.GameId),
            throwIfNotExists: true,
            includeDetails: true
        );

        var currentGameStatusModel = await SendCurrentGameStatusAsync(word, connection, game, otherConnection);

        if (word == SignalRConstants.EscapeString)
        {
            await CurrentUserHasLeft(game, connection, otherConnection, relatedChannel);
        }

        if (currentGameStatusModel.RelatedWord == word)
        {
            await CurrentUserCorrectGuessAsync(connection, game, otherConnection);
        }

        if (currentGameStatusModel.CurrentUserStatusDto.CurrentUserGuesses.WordGuess.Count ==
            SignalRConstants.MaxGuessCount)
        {
            connection.ElapsedSeconds =
                (DateTime.Now.TimeOfDay - connection.InitialConnectionTime.TimeOfDay).Seconds;
        }

        if (
            game.ReceiverUserGuessCount == SignalRConstants.MaxGuessCount &&
            game.SenderUserGuessCount == SignalRConstants.MaxGuessCount
        )
        {
            await MaxGuessCountReached(connection, game, otherConnection);
        }

        await _gameRepository.UpdateAsync(game, autoSave: true);

        await uow.SaveChangesAsync();
        await uow.CompleteAsync();
    }

    private async Task CurrentUserHasLeft(Game game, GameHubConnectionModel connection,
        GameHubConnectionModel? otherConnection, Channel relatedChannel)
    {
        var currentUserGameResult = new GameResultDto
        {
            GameId = game.Id,
            GameResultId = (int)GameResultTypes.Lose,
            GameResultName = GameResultTypes.Lose.GetDescription(),
            CurrentUserScore = null,
            OtherUserScore = null,
            PreviousGameResult = game.ParentGame != null
                ? connection.ChallengeUserType == ChallengeUserTypes.Receiver
                    ? JsonConvert.DeserializeObject<GameResultDto>(game.ParentGame.ReceiverUserGameResult)
                    : JsonConvert.DeserializeObject<GameResultDto>(game.ParentGame.SenderUserGameResult)
                : null
        };
        var otherUserGameResult = new GameResultDto
        {
            GameId = game.Id,
            GameResultId = (int)GameResultTypes.Win,
            GameResultName = GameResultTypes.Win.GetDescription(),
            CurrentUserScore = null,
            OtherUserScore = null,
            PreviousGameResult = game.ParentGame != null
                ? connection.ChallengeUserType == ChallengeUserTypes.Receiver
                    ? JsonConvert.DeserializeObject<GameResultDto>(game.ParentGame.SenderUserGameResult)
                    : JsonConvert.DeserializeObject<GameResultDto>(game.ParentGame.ReceiverUserGameResult)
                : null
        };

        await Clients.User(connection.UserId.ToString())
            .SendAsync(
                SignalRConstants.GameHubConstants.Methods.GameResult,
                currentUserGameResult
            );

        await Clients.User(otherConnection.UserId.ToString())
            .SendAsync(
                SignalRConstants.GameHubConstants.Methods.GameResult,
                otherUserGameResult
            );

        game.GameStatusId = (int)GameStatusTypes.Finished;
        game.ReceiverUserGameResult = connection.ChallengeUserType == ChallengeUserTypes.Receiver
            ? JsonConvert.SerializeObject(currentUserGameResult)
            : JsonConvert.SerializeObject(otherUserGameResult);
        game.SenderUserGameResult = connection.ChallengeUserType == ChallengeUserTypes.Receiver
            ? JsonConvert.SerializeObject(otherUserGameResult)
            : JsonConvert.SerializeObject(currentUserGameResult);

        var users = Lobby.Channels
            .SelectMany(p => p.Users)
            .Where(p =>
                p.IdentityUser.Id.Equals(game.PreGameInfo.ChallengeRequest.SenderUserId) ||
                p.IdentityUser.Id.Equals(game.PreGameInfo.ChallengeRequest.ReceiverUserId))
            .ToList();

        users.ForEach(p => p.UserStatusTypes = UserStatusTypes.Online);

        await relatedChannel.UpdateActiveUserListAsync(_lobbyHubContext);
    }

    private async Task MaxGuessCountReached(
        GameHubConnectionModel connection,
        Game game,
        GameHubConnectionModel? otherConnection
    )
    {
        var currentUserAnswers = connection.ChallengeUserType == ChallengeUserTypes.Receiver
            ? JsonConvert.DeserializeObject<GameUserGuessDto>(game.ReceiverUserAnswer)
            : JsonConvert.DeserializeObject<GameUserGuessDto>(game.SenderUserAnswer);

        var otherUserAnswers = connection.ChallengeUserType == ChallengeUserTypes.Receiver
            ? JsonConvert.DeserializeObject<GameUserGuessDto>(game.SenderUserAnswer)
            : JsonConvert.DeserializeObject<GameUserGuessDto>(game.ReceiverUserAnswer);

        var currentUserGreenCharacterScore = currentUserAnswers.WordGuess.Last()
            .Count(p => p.PositionColor == (int)PositionColors.Green) * CharacterPoints.CorrectCharacterPoint;
        var currentUserYellowCharacterScore = currentUserAnswers.WordGuess.Last()
            .Count(p => p.PositionColor == (int)PositionColors.Yellow) * CharacterPoints.ContainingCharacterPoint;
        var currentUserRemainingSecondScore = connection.ChallengeUserType == ChallengeUserTypes.Receiver
            ? (int)game.PreGameInfo.ReceiverUserRemainingSeconds!
            : (int)game.PreGameInfo.SenderUserRemainingSeconds!;
        var currentUserTotalScore =
            currentUserGreenCharacterScore +
            currentUserYellowCharacterScore +
            currentUserRemainingSecondScore;

        var otherUserGreenCharacterScore = otherUserAnswers.WordGuess.Last()
            .Count(p => p.PositionColor == (int)PositionColors.Green) * CharacterPoints.CorrectCharacterPoint;
        var otherUserYellowCharacterScore = otherUserAnswers.WordGuess.Last()
            .Count(p => p.PositionColor == (int)PositionColors.Yellow) * CharacterPoints.ContainingCharacterPoint;
        var otherUserRemainingSecondScore = connection.ChallengeUserType == ChallengeUserTypes.Receiver
            ? (int)game.PreGameInfo.SenderUserRemainingSeconds!
            : (int)game.PreGameInfo.ReceiverUserRemainingSeconds!;

        var otherUserTotalScore =
            otherUserGreenCharacterScore +
            otherUserYellowCharacterScore +
            otherUserRemainingSecondScore;

        var currentUserGameResult = new GameResultDto
        {
            GameId = game.Id,
            CurrentUserScore = new UserScoreDto
            {
                TotalScore = currentUserTotalScore,
                GreenCharacterScore = currentUserGreenCharacterScore,
                YellowCharacterScore = currentUserYellowCharacterScore,
                PregameRemainingTimeScore = currentUserRemainingSecondScore,
                ElapsedTimeSeconds = connection.ElapsedSeconds ??
                                     (DateTime.Now.TimeOfDay - connection.InitialConnectionTime.TimeOfDay).Seconds
            },
            OtherUserScore = new UserScoreDto
            {
                TotalScore = otherUserTotalScore,
                GreenCharacterScore = otherUserGreenCharacterScore,
                YellowCharacterScore = otherUserYellowCharacterScore,
                PregameRemainingTimeScore = otherUserRemainingSecondScore,
                ElapsedTimeSeconds = otherConnection.ElapsedSeconds ??
                                     (DateTime.Now.TimeOfDay - otherConnection.InitialConnectionTime.TimeOfDay).Seconds
            },
            PreviousGameResult = game.ParentGame != null
                ? connection.ChallengeUserType == ChallengeUserTypes.Receiver
                    ? JsonConvert.DeserializeObject<GameResultDto>(game.ParentGame.ReceiverUserGameResult)
                    : JsonConvert.DeserializeObject<GameResultDto>(game.ParentGame.SenderUserGameResult)
                : null
        };

        var otherUserGameResult = new GameResultDto
        {
            GameId = game.Id,
            CurrentUserScore = currentUserGameResult.OtherUserScore,
            OtherUserScore = currentUserGameResult.CurrentUserScore,
            PreviousGameResult = game.ParentGame != null
                ? connection.ChallengeUserType == ChallengeUserTypes.Receiver
                    ? JsonConvert.DeserializeObject<GameResultDto>(game.ParentGame.SenderUserGameResult)
                    : JsonConvert.DeserializeObject<GameResultDto>(game.ParentGame.ReceiverUserGameResult)
                : null
        };

        currentUserGameResult.GameResultId = currentUserTotalScore == otherUserTotalScore
            ? (int)GameResultTypes.Draw
            : currentUserTotalScore > otherUserTotalScore
                ? (int)GameResultTypes.Win
                : (int)GameResultTypes.Lose;
        currentUserGameResult.GameResultName = ((GameResultTypes)currentUserGameResult.GameResultId).GetDescription();

        otherUserGameResult.GameResultId = currentUserTotalScore == otherUserTotalScore
            ? (int)GameResultTypes.Draw
            : currentUserTotalScore > otherUserTotalScore
                ? (int)GameResultTypes.Lose
                : (int)GameResultTypes.Win;
        otherUserGameResult.GameResultName = ((GameResultTypes)otherUserGameResult.GameResultId).GetDescription();

        await Clients.User(connection.UserId.ToString())
            .SendAsync(
                SignalRConstants.GameHubConstants.Methods.GameResult,
                currentUserGameResult
            );

        await Clients.User(otherConnection.UserId.ToString())
            .SendAsync(
                SignalRConstants.GameHubConstants.Methods.GameResult,
                otherUserGameResult
            );

        game.GameStatusId = (int)GameStatusTypes.Finished;
        game.ReceiverUserGameResult = connection.ChallengeUserType == ChallengeUserTypes.Receiver
            ? JsonConvert.SerializeObject(currentUserGameResult)
            : JsonConvert.SerializeObject(otherUserGameResult);
        game.SenderUserGameResult = connection.ChallengeUserType == ChallengeUserTypes.Receiver
            ? JsonConvert.SerializeObject(otherUserGameResult)
            : JsonConvert.SerializeObject(currentUserGameResult);
    }

    private async Task CurrentUserCorrectGuessAsync(
        GameHubConnectionModel connection,
        Game game,
        GameHubConnectionModel? otherConnection
    )
    {
        var currentUserGameResult = new GameResultDto
        {
            GameId = game.Id,
            GameResultId = (int)GameResultTypes.Win,
            GameResultName = GameResultTypes.Win.GetDescription(),
            CurrentUserScore = null,
            OtherUserScore = null,
            PreviousGameResult = game.ParentGame != null
                ? connection.ChallengeUserType == ChallengeUserTypes.Receiver
                    ? JsonConvert.DeserializeObject<GameResultDto>(game.ParentGame.ReceiverUserGameResult)
                    : JsonConvert.DeserializeObject<GameResultDto>(game.ParentGame.SenderUserGameResult)
                : null
        };

        var otherUserGameResult = new GameResultDto
        {
            GameId = game.Id,
            GameResultId = (int)GameResultTypes.Lose,
            GameResultName = GameResultTypes.Lose.GetDescription(),
            CurrentUserScore = currentUserGameResult.OtherUserScore,
            OtherUserScore = currentUserGameResult.CurrentUserScore,
            PreviousGameResult = game.ParentGame != null
                ? connection.ChallengeUserType == ChallengeUserTypes.Receiver
                    ? JsonConvert.DeserializeObject<GameResultDto>(game.ParentGame.SenderUserGameResult)
                    : JsonConvert.DeserializeObject<GameResultDto>(game.ParentGame.ReceiverUserGameResult)
                : null
        };

        await Clients.User(connection.UserId.ToString())
            .SendAsync(
                SignalRConstants.GameHubConstants.Methods.GameResult,
                currentUserGameResult
            );

        await Clients.User(otherConnection.UserId.ToString())
            .SendAsync(
                SignalRConstants.GameHubConstants.Methods.GameResult,
                otherUserGameResult
            );

        game.GameStatusId = (int)GameStatusTypes.Finished;
        game.ReceiverUserGameResult = connection.ChallengeUserType == ChallengeUserTypes.Receiver
            ? JsonConvert.SerializeObject(currentUserGameResult)
            : JsonConvert.SerializeObject(otherUserGameResult);
        game.SenderUserGameResult = connection.ChallengeUserType == ChallengeUserTypes.Receiver
            ? JsonConvert.SerializeObject(otherUserGameResult)
            : JsonConvert.SerializeObject(currentUserGameResult);
    }

    private async Task<CurrentGameStatusModel> SendCurrentGameStatusAsync(
        string word,
        GameHubConnectionModel connection,
        Game game,
        GameHubConnectionModel? otherConnection
    )
    {
        var relatedWord = connection.ChallengeUserType == ChallengeUserTypes.Receiver
            ? game.PreGameInfo.SenderPlayerWord
            : game.PreGameInfo.ReceiverPlayerWord;

        var currentUserGameCurrentStatusDto = new GameCurrentStatusDto
        {
            CurrentUserGuesses = (connection.ChallengeUserType == ChallengeUserTypes.Receiver
                                     ? JsonConvert.DeserializeObject<GameUserGuessDto>(
                                         game.ReceiverUserAnswer ?? string.Empty)
                                     : JsonConvert.DeserializeObject<GameUserGuessDto>(
                                         game.SenderUserAnswer ?? string.Empty)) ??
                                 new GameUserGuessDto
                                 {
                                     UserId = (Guid)CurrentUser.Id,
                                     WordGuess = new List<List<WordGuessDto>>()
                                 }
        };

        var currentGuess = new List<WordGuessDto>();

        for (int i = 0; i < word.Length; i++)
        {
            currentGuess.Add(new WordGuessDto
            {
                Character = word[i],
                PositionColor = word[i] == relatedWord[i]
                    ? (int)PositionColors.Green
                    : relatedWord.Contains(word[i])
                        ? (int)PositionColors.Yellow
                        : (int)PositionColors.Grey
            });
        }

        currentUserGameCurrentStatusDto.CurrentUserGuesses.WordGuess.Add(currentGuess);

        var otherUserGuesses = otherConnection.ChallengeUserType == ChallengeUserTypes.Receiver
            ? game.ReceiverUserAnswer
            : game.SenderUserAnswer;

        currentUserGameCurrentStatusDto.OtherUserGuesses =
            (!string.IsNullOrWhiteSpace(otherUserGuesses)
                ? JsonConvert.DeserializeObject<GameUserGuessDto>(otherUserGuesses)
                : new GameUserGuessDto
                {
                    UserId = otherConnection.UserId,
                    WordGuess = new List<List<WordGuessDto>>()
                })!;

        var otherUserGameCurrentStatusDto = new GameCurrentStatusDto
        {
            CurrentUserGuesses = currentUserGameCurrentStatusDto.OtherUserGuesses,
            OtherUserGuesses = currentUserGameCurrentStatusDto.CurrentUserGuesses
        };

        await Clients.User(connection.UserId.ToString())
            .SendAsync(
                SignalRConstants.GameHubConstants.Methods.CurrentGameStatus,
                currentUserGameCurrentStatusDto
            );

        await Clients.User(otherConnection.UserId.ToString())
            .SendAsync(
                SignalRConstants.GameHubConstants.Methods.CurrentGameStatus,
                otherUserGameCurrentStatusDto
            );

        if (connection.ChallengeUserType == ChallengeUserTypes.Receiver)
        {
            game.ReceiverUserAnswer = JsonConvert.SerializeObject(currentUserGameCurrentStatusDto.CurrentUserGuesses);
            game.ReceiverUserGuessCount++;
        }
        else
        {
            game.SenderUserAnswer = JsonConvert.SerializeObject(currentUserGameCurrentStatusDto.CurrentUserGuesses);
            game.SenderUserGuessCount++;
        }

        return new CurrentGameStatusModel
        {
            RelatedWord = relatedWord,
            CurrentUserStatusDto = currentUserGameCurrentStatusDto,
            OtherUserStatusDto = otherUserGameCurrentStatusDto
        };
    }

    private void RemoveObsoleteConnections()
    {
        var obsoleteConnections = Connections
            .Where(c =>
                c.UserId == CurrentUser.Id.GetValueOrDefault() &&
                c.ConnectionId != Context.ConnectionId
            )
            .ToList();
        if (!obsoleteConnections.IsNullOrEmpty())
        {
            obsoleteConnections.ForEach(obsoleteConnection => { Connections.Remove(obsoleteConnection); });
        }
    }
}