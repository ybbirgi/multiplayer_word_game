using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Localization;
using Newtonsoft.Json;
using Volo.Abp;
using Volo.Abp.DependencyInjection;
using Volo.Abp.EventBus;
using Volo.Abp.Uow;
using WordGame.Constants;
using WordGame.Constants.Signal_R;
using WordGame.Dtos.GameResults;
using WordGame.Dtos.Users;
using WordGame.ExceptionCodes;
using WordGame.Extensions;
using WordGame.Hubs;
using WordGame.Hubs.Helpers;
using WordGame.LocalEvents.Etos;
using WordGame.Localization;
using WordGame.Models.Lobbies;
using WordGame.Repositories;

namespace WordGame.LocalEvents.Handlers;

public class GameUserDisconnectedEventHandler : ILocalEventHandler<GameUserDisconnectedEventEto>, ITransientDependency
{
    private readonly IHubContext<LobbyHub> _lobbyHubContext;
    private readonly IUnitOfWorkManager _unitOfWorkManager;
    private readonly IGameRepository _gameRepository;
    private readonly IStringLocalizer<WordGameResource> _stringLocalizer;
    private readonly IHubContext<GameHub> _gameHubContext;

    public GameUserDisconnectedEventHandler(
        IHubContext<LobbyHub> lobbyHubContext,
        IUnitOfWorkManager unitOfWorkManager,
        IGameRepository gameRepository,
        IStringLocalizer<WordGameResource> stringLocalizer,
        IHubContext<GameHub> gameHubContext
    )
    {
        _lobbyHubContext = lobbyHubContext;
        _unitOfWorkManager = unitOfWorkManager;
        _gameRepository = gameRepository;
        _stringLocalizer = stringLocalizer;
        _gameHubContext = gameHubContext;
    }

    public virtual async Task HandleEventAsync(GameUserDisconnectedEventEto eventData)
    {
        using var uow = _unitOfWorkManager.Begin(true);

        var timer = new Stopwatch();
        var remainingTime = TimeSpan.FromSeconds(10) -
                            (DateTime.Now.TimeOfDay -
                             eventData.GameHubConnectionModel.LastCommunicationTime.TimeOfDay);
        timer.Start();
        while (!PregameHub.Connections.Exists(p => p.UserId.Equals(eventData.GameHubConnectionModel.UserId)) &&
               timer.ElapsedTicks < remainingTime.Ticks)
        {
            await Task.Delay(TimeSpan.FromSeconds(1));
        }

        if (PregameHub.Connections.Exists(p => p.UserId.Equals(eventData.GameHubConnectionModel.UserId)))
        {
            return;
        }

        Guid otherUserId;

        var game = await _gameRepository.GetByAsync(
            p => p.Id.Equals(eventData.GameHubConnectionModel.GameId),
            includeDetails: true
        );

        if (game == null)
        {
            throw new UserFriendlyException(_stringLocalizer[GameExceptionCodes.NotFound]);
        }

        if (game.GameStatusId == (int)GameStatusTypes.Finished)
        {
            return;
        }

        if (game.PreGameInfo.ChallengeRequest.SenderUserId == eventData.GameHubConnectionModel.UserId)
        {
            otherUserId = game.PreGameInfo.ChallengeRequest.ReceiverUserId;
        }
        else
        {
            otherUserId = game.PreGameInfo.ChallengeRequest.SenderUserId;
        }


        var relatedChannel = Lobby.Channels.First(p => p.Id.Equals(game.PreGameInfo.ChallengeRequest.ChannelId));

        var users = Lobby.Channels
            .SelectMany(p => p.Users)
            .Where(p =>
                p.IdentityUser.Id.Equals(game.PreGameInfo.ChallengeRequest.SenderUserId) ||
                p.IdentityUser.Id.Equals(game.PreGameInfo.ChallengeRequest.ReceiverUserId))
            .ToList();

        users.ForEach(p => p.UserStatusTypes = UserStatusTypes.Online);
        
        await relatedChannel.UpdateActiveUserListAsync(_lobbyHubContext);

        var gameResultDto = new GameResultDto
        {
            GameId = eventData.GameHubConnectionModel.GameId,
            GameResultId = (int)GameResultTypes.Win,
            GameResultName = GameResultTypes.Win.GetDescription(),
            CurrentUserScore = null,
            OtherUserScore = null,
            PreviousGameResult = game.ParentGame != null
                ? eventData.GameHubConnectionModel.ChallengeUserType == ChallengeUserTypes.Receiver
                    ? JsonConvert.DeserializeObject<GameResultDto>(game.ParentGame.SenderUserGameResult)
                    : JsonConvert.DeserializeObject<GameResultDto>(game.ParentGame.ReceiverUserGameResult)
                : null
        };

        game.GameStatusId = (int)GameStatusTypes.Finished;

        await _gameHubContext.Clients.User(
                otherUserId.ToString())
            .SendAsync(SignalRConstants.GameHubConstants.Methods.GameResult,
                gameResultDto
            );

        await _gameRepository.UpdateAsync(game, autoSave: true);
        await uow.SaveChangesAsync();
        await uow.CompleteAsync();
    }
}