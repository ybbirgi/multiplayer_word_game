using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper.Internal;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Localization;
using Volo.Abp;
using Volo.Abp.AspNetCore.SignalR;
using Volo.Abp.EventBus.Local;
using Volo.Abp.Uow;
using WordGame.Constants;
using WordGame.Constants.Signal_R;
using WordGame.Dtos.GameResults;
using WordGame.Dtos.Games;
using WordGame.Dtos.Users;
using WordGame.Entities;
using WordGame.ExceptionCodes;
using WordGame.Extensions;
using WordGame.Hubs.Helpers;
using WordGame.LocalEvents.Etos;
using WordGame.Localization;
using WordGame.Managers;
using WordGame.Models.Hubs;
using WordGame.Models.Lobbies;
using WordGame.Repositories;

namespace WordGame.Hubs;

[Authorize]
public class PregameHub : AbpHub
{
    public static readonly List<PreGameHubConnectionModel> Connections = new();
    private readonly IPreGameInfoRepository _preGameInfoRepository;
    private readonly IStringLocalizer<WordGameResource> _stringLocalizer;
    private readonly ILocalEventBus _localEventBus;
    private readonly IUnitOfWorkManager _unitOfWorkManager;
    private readonly IHubContext<LobbyHub> _lobbyHubContext;
    private readonly GameManager _gameManager;
    private readonly IGameRepository _gameRepository;

    public PregameHub(
        IPreGameInfoRepository preGameInfoRepository,
        IStringLocalizer<WordGameResource> stringLocalizer,
        ILocalEventBus localEventBus,
        IUnitOfWorkManager unitOfWorkManager,
        IHubContext<LobbyHub> lobbyHubContext,
        GameManager gameManager,
        IGameRepository gameRepository
    )
    {
        _preGameInfoRepository = preGameInfoRepository;
        _stringLocalizer = stringLocalizer;
        _localEventBus = localEventBus;
        _unitOfWorkManager = unitOfWorkManager;
        _lobbyHubContext = lobbyHubContext;
        _gameManager = gameManager;
        _gameRepository = gameRepository;
    }

    public override async Task OnConnectedAsync()
    {
        var preGameInfoId = Context.GetHttpContext().Request.Query["pre_game_info_id"].ToString();
        var isRematch = Context.GetHttpContext().Request.Query["is_rematch"].ToString();

        if (preGameInfoId == null)
        {
            throw new UserFriendlyException(_stringLocalizer[PreGameInfoExceptionCodes.InvalidParameters]);
        }

        Connections.TryAdd(new PreGameHubConnectionModel
        {
            UserId = CurrentUser.Id.GetValueOrDefault(),
            ConnectionId = Context.ConnectionId,
            PreGameInfoId = Guid.Parse(preGameInfoId),
            LastCommunicationTime = DateTime.Now,
            IsRematch = bool.Parse(isRematch)
        });

        RemoveObsoleteConnections();

        await base.OnConnectedAsync();
    }

    [HubMethodName(SignalRConstants.GameHubConstants.Methods.SetWord)]
    public async Task SetWordAsync(string selectedWord)
    {
        using var uow = _unitOfWorkManager.Begin();
        var connection = Connections.Find(c => c.ConnectionId == Context.ConnectionId);
        var preGameInfo = await _preGameInfoRepository.GetByAsync(
            c =>
                c.Id.Equals(connection.PreGameInfoId),
            includeDetails: true
        );

        if (preGameInfo == null)
        {
            throw new UserFriendlyException(_stringLocalizer[PreGameInfoExceptionCodes.PreGameInfoNotFound]);
        }

        var relatedChannel = Lobby.Channels.First(p => p.Id.Equals(preGameInfo.ChallengeRequest.ChannelId));

        if (relatedChannel.WordLength != selectedWord.Length && selectedWord != SignalRConstants.EscapeString)
        {
            throw new UserFriendlyException(_stringLocalizer[PreGameInfoExceptionCodes.WordLengthNotValid]);
        }

        string otherUserWord;

        if (preGameInfo.ChallengeRequest.SenderUserId == connection.UserId)
        {
            var remainingTime = TimeSpan.FromMinutes(1) -
                                (DateTime.Now.TimeOfDay -
                                 connection.LastCommunicationTime.TimeOfDay
                                );
            preGameInfo.SenderPlayerWord = selectedWord;
            preGameInfo.SenderUserRemainingSeconds = remainingTime.Seconds;
            otherUserWord = preGameInfo.ReceiverPlayerWord;
        }
        else
        {
            var remainingTime = TimeSpan.FromMinutes(1) -
                                (DateTime.Now.TimeOfDay -
                                 connection.LastCommunicationTime.TimeOfDay
                                );
            preGameInfo.ReceiverPlayerWord = selectedWord;
            preGameInfo.ReceiverUserRemainingSeconds = remainingTime.Seconds;
            otherUserWord = preGameInfo.SenderPlayerWord;
        }

        connection.LastCommunicationTime = DateTime.Now;

        var otherUserConnection = Connections.Find(
            c =>
                c.PreGameInfoId == connection.PreGameInfoId &&
                c.UserId != connection.UserId
        );

        if (
            (otherUserWord == SignalRConstants.EscapeString &&
             !string.IsNullOrEmpty(selectedWord) && selectedWord != SignalRConstants.EscapeString) ||
            (!string.IsNullOrEmpty(otherUserWord) && otherUserWord != SignalRConstants.EscapeString &&
             selectedWord == SignalRConstants.EscapeString)
        )
        {
            Guid winnerUserId;
            Guid loserUserId;

            if (otherUserWord == SignalRConstants.EscapeString)
            {
                winnerUserId = connection.UserId;
                loserUserId = otherUserConnection.UserId;
            }
            else
            {
                winnerUserId = otherUserConnection.UserId;
                loserUserId = connection.UserId;
            }

            var users = Lobby.Channels
                .SelectMany(p => p.Users)
                .Where(p =>
                    p.IdentityUser.Id.Equals(preGameInfo.ChallengeRequest.SenderUserId) ||
                    p.IdentityUser.Id.Equals(preGameInfo.ChallengeRequest.ReceiverUserId))
                .ToList();

            users.ForEach(p => p.UserStatusTypes = UserStatusTypes.Online);

            await relatedChannel.UpdateActiveUserListAsync(_lobbyHubContext);

            var winnerGameResultDto = new PreGameResultDto
            {
                GameResultTypeId = (int)GameResultTypes.Win,
                GameResultTypeName = GameResultTypes.Win.GetDescription()
            };
            var loserGameResultDto = new PreGameResultDto
            {
                GameResultTypeId = (int)GameResultTypes.Lose,
                GameResultTypeName = GameResultTypes.Win.GetDescription()
            };

            preGameInfo.PreGameStatusId = (int)PreGameStatuses.Completed;
            await _preGameInfoRepository.UpdateAsync(preGameInfo, autoSave: true);

            await Clients.User(winnerUserId.ToString())
                .SendAsync(SignalRConstants.GameHubConstants.Methods.GameResult,
                    winnerGameResultDto
                );
            await Clients.User(loserUserId.ToString())
                .SendAsync(SignalRConstants.GameHubConstants.Methods.GameResult,
                    loserGameResultDto
                );
        }

        if (!string.IsNullOrEmpty(selectedWord) &&
            selectedWord != SignalRConstants.EscapeString &&
            !string.IsNullOrEmpty(otherUserWord) &&
            otherUserWord != SignalRConstants.EscapeString)
        {
            Game game = null;
            if (!connection.IsRematch)
            {
                game = _gameManager.Create(preGameInfo.Id);
                await _gameRepository.InsertAsync(game, autoSave: true);
            }
            else
            {
                game = await _gameManager.GetByAsync(p => p.PreGameInfoId.Equals(connection.PreGameInfoId),
                    includeDetails: true
                );
            }

            var gameCreationDto = new GameCreationDto
            {
                GameId = game.Id,
                PreGameId = preGameInfo.Id
            };


            preGameInfo.PreGameStatusId = (int)PreGameStatuses.Completed;
            await _preGameInfoRepository.UpdateAsync(preGameInfo, autoSave: true);

            await Clients.User(connection.UserId.ToString())
                .SendAsync(SignalRConstants.GameHubConstants.Methods.NewGame,
                    gameCreationDto
                );

            await Clients.User(otherUserConnection.UserId.ToString())
                .SendAsync(SignalRConstants.GameHubConstants.Methods.NewGame,
                    gameCreationDto
                );
        }

        if (preGameInfo.ReceiverPlayerWord == SignalRConstants.EscapeString &&
            preGameInfo.SenderPlayerWord == SignalRConstants.EscapeString)
        {
            preGameInfo.ReceiverPlayerWord = string.Empty;
            preGameInfo.SenderPlayerWord = string.Empty;
        }

        await _preGameInfoRepository.UpdateAsync(preGameInfo, autoSave: true);

        await uow.SaveChangesAsync();
        await uow.CompleteAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var connection = Connections.Find(c => c.ConnectionId == Context.ConnectionId);
        if (connection is not null)
        {
            Connections.Remove(connection);
            await _localEventBus.PublishAsync(new PreGameUserDisconnectedEventEto
            {
                PreGameHubConnectionModel = connection
            });
        }
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