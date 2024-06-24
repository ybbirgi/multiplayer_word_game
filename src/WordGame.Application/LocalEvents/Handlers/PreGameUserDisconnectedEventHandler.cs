using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Localization;
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

public class PreGameUserDisconnectedEventHandler : ILocalEventHandler<PreGameUserDisconnectedEventEto>,
    ITransientDependency
{
    private readonly IPreGameInfoRepository _preGameInfoRepository;
    private readonly IStringLocalizer<WordGameResource> _stringLocalizer;
    private readonly IHubContext<PregameHub> _preGameInfoHubContext;
    private readonly IHubContext<LobbyHub> _lobbyHubContext;
    private readonly IUnitOfWorkManager _unitOfWorkManager;

    public PreGameUserDisconnectedEventHandler(
        IPreGameInfoRepository preGameInfoRepository,
        IStringLocalizer<WordGameResource> stringLocalizer,
        IHubContext<PregameHub> preGameInfoHubContext,
        IHubContext<LobbyHub> lobbyHubContext,
        IUnitOfWorkManager unitOfWorkManager
    )
    {
        _preGameInfoRepository = preGameInfoRepository;
        _stringLocalizer = stringLocalizer;
        _preGameInfoHubContext = preGameInfoHubContext;
        _lobbyHubContext = lobbyHubContext;
        _unitOfWorkManager = unitOfWorkManager;
    }

    public virtual async Task HandleEventAsync(PreGameUserDisconnectedEventEto eventData)
    {
        using var uow = _unitOfWorkManager.Begin(true);
        CancellationToken cancellationToken = default;
        var timer = new Stopwatch();
        var remainingTime = TimeSpan.FromMinutes(1) -
                            (DateTime.Now.TimeOfDay -
                             eventData.PreGameHubConnectionModel.LastCommunicationTime.TimeOfDay);
        timer.Start();
        while (!PregameHub.Connections.Exists(p => p.UserId.Equals(eventData.PreGameHubConnectionModel.UserId)) &&
               timer.ElapsedTicks < remainingTime.Ticks)
        {
            await Task.Delay(TimeSpan.FromSeconds(1));
        }

        if (PregameHub.Connections.Exists(p => p.UserId.Equals(eventData.PreGameHubConnectionModel.UserId)))
        {
            return;
        }

        Guid otherUserId;

        var preGameInfo = await
            _preGameInfoRepository.GetByAsync(
                p =>
                    p.Id.Equals(eventData.PreGameHubConnectionModel.PreGameInfoId),
                includeDetails: true,
                cancellationToken: cancellationToken);

        if (preGameInfo == null)
        {
            throw new UserFriendlyException(_stringLocalizer[PreGameInfoExceptionCodes.PreGameInfoNotFound]);
        }

        if (preGameInfo.PreGameStatusId == (int)PreGameStatuses.Completed)
        {
            return;
        }

        if (preGameInfo.ChallengeRequest.SenderUserId == eventData.PreGameHubConnectionModel.UserId)
        {
            otherUserId = preGameInfo.ChallengeRequest.ReceiverUserId;
        }
        else
        {
            otherUserId = preGameInfo.ChallengeRequest.SenderUserId;
        }

        var relatedChannel = Lobby.Channels.First(p => p.Id.Equals(preGameInfo.ChallengeRequest.ChannelId));

        var users = Lobby.Channels
            .SelectMany(p => p.Users)
            .Where(p =>
                p.IdentityUser.Id.Equals(preGameInfo.ChallengeRequest.SenderUserId) ||
                p.IdentityUser.Id.Equals(preGameInfo.ChallengeRequest.ReceiverUserId))
            .ToList();

        users.ForEach(p => p.UserStatusTypes = UserStatusTypes.Online);

        await relatedChannel.UpdateActiveUserListAsync(_lobbyHubContext);

        var gameResultDto = new PreGameResultDto
        {
            GameResultTypeId = (int)GameResultTypes.Win,
            GameResultTypeName = GameResultTypes.Win.GetDescription()
        };

        await _preGameInfoHubContext.Clients.User(
                otherUserId.ToString())
            .SendAsync(SignalRConstants.GameHubConstants.Methods.GameResult,
                gameResultDto, cancellationToken: cancellationToken);

        preGameInfo.PreGameStatusId = (int)PreGameStatuses.Completed;
        await _preGameInfoRepository.UpdateAsync(preGameInfo, cancellationToken: cancellationToken);
        await uow.SaveChangesAsync(cancellationToken);
        await uow.CompleteAsync(cancellationToken);
    }
}