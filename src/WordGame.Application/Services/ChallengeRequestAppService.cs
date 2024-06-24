using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Localization;
using Volo.Abp;
using WordGame.Constants;
using WordGame.Constants.Signal_R;
using WordGame.Dtos.Challenges;
using WordGame.Dtos.PreGameInfos;
using WordGame.Dtos.Users;
using WordGame.Entities;
using WordGame.ExceptionCodes;
using WordGame.Extensions;
using WordGame.Hubs;
using WordGame.Hubs.Helpers;
using WordGame.Localization;
using WordGame.Managers;
using WordGame.Models.Lobbies;
using WordGame.Models.PreGameInfos;
using WordGame.Repositories;

namespace WordGame.Services;

public class ChallengeRequestAppService : WordGameAppService, IChallengeRequestAppService
{
    private readonly IChallengeRequestRepository _challengeRequestRepository;
    private readonly IHubContext<LobbyHub> _hubContext;
    private readonly IStringLocalizer<WordGameResource> _stringLocalizer;
    private readonly ChallengeRequestManager _challengeRequestManager;
    private readonly PreGameInfoManager _preGameInfoManager;
    private readonly IPreGameInfoRepository _preGameInfoRepository;

    public ChallengeRequestAppService(
        IChallengeRequestRepository challengeRequestRepository,
        IHubContext<LobbyHub> hubContext,
        IStringLocalizer<WordGameResource> stringLocalizer,
        ChallengeRequestManager challengeRequestManager,
        PreGameInfoManager preGameInfoManager,
        IPreGameInfoRepository preGameInfoRepository
    )
    {
        _challengeRequestRepository = challengeRequestRepository;
        _hubContext = hubContext;
        _stringLocalizer = stringLocalizer;
        _challengeRequestManager = challengeRequestManager;
        _preGameInfoManager = preGameInfoManager;
        _preGameInfoRepository = preGameInfoRepository;
    }

    public async Task<ChallengeRequestDto> SendAsync(
        Guid userId,
        CancellationToken cancellationToken = default
    )
    {
        if (CurrentUser.Id.Equals(userId))
        {
            throw new UserFriendlyException(_stringLocalizer[ChallengeExceptionCodes.UserCannotChallengeToItself]);
        }

        await _challengeRequestManager.AlreadyExistsAsync(
            p =>
                (p.SenderUserId.Equals(CurrentUser.Id) ||
                 p.ReceiverUserId.Equals(CurrentUser.Id)) &&
                p.ChallengeStatusId.Equals((int)ChallengeStatus.Pending),
            ChallengeExceptionCodes.UserHasOtherPendingChallenge,
            throwIfExists: true,
            cancellationToken: cancellationToken
        );

        await _challengeRequestManager.AlreadyExistsAsync(
            p =>
                (p.ReceiverUserId.Equals(userId) ||
                 p.SenderUserId.Equals(userId)) &&
                p.ChallengeStatusId.Equals((int)ChallengeStatus.Pending),
            ChallengeExceptionCodes.OtherUserHasOtherPendingChallenge,
            throwIfExists: true,
            cancellationToken: cancellationToken
        );

        var connection = LobbyHub.Connections.FirstOrDefault(p => p.UserId.Equals(userId));
        if (connection == null)
        {
            throw new UserFriendlyException(_stringLocalizer[ChallengeExceptionCodes.OtherUserIsNotActive]);
        }

        var challengeRequest = _challengeRequestManager.Create(userId, connection.ChannelId);

        await _challengeRequestRepository.InsertAsync(challengeRequest, cancellationToken: cancellationToken);

        var challengeRequestDto = ObjectMapper.Map<ChallengeRequest, ChallengeRequestDto>(challengeRequest);

        await _hubContext.Clients.User(userId.ToString()).SendAsync(
            SignalRConstants.GameHubConstants.Methods.ChallengeReceived,
            challengeRequestDto,
            cancellationToken: cancellationToken
        );

        return challengeRequestDto;
    }

    public async Task<ChallengeRequestDto> AcceptAsync(
        Guid challengeRequestId,
        CancellationToken cancellationToken = default
    )
    {
        var challengeRequest = await _challengeRequestManager.GetByAsync(
            p =>
                p.Id.Equals(challengeRequestId),
            throwIfNotExists: true,
            cancellationToken
        );

        challengeRequest.ChallengeStatusId = (int)ChallengeStatus.Accepted;

        var users = Lobby.Channels
            .SelectMany(p => p.Users)
            .Where(p =>
                p.IdentityUser.Id.Equals(challengeRequest.SenderUserId) ||
                p.IdentityUser.Id.Equals(challengeRequest.ReceiverUserId))
            .ToList();

        users.ForEach(p => p.UserStatusTypes = UserStatusTypes.InGame);

        var relatedChannel = Lobby.Channels.First(p => p.Id.Equals(challengeRequest.ChannelId));

        await relatedChannel.UpdateActiveUserListAsync(_hubContext);

        var preGameInfo = _preGameInfoManager.Create(new PreGameInfoCreateModel
        {
            ChallengeRequestId = challengeRequestId,
            GameTypeId = (int)relatedChannel.GameTypeId,
            WordLength = relatedChannel.WordLength
        });

        await _preGameInfoRepository.InsertAsync(preGameInfo, cancellationToken: cancellationToken);

        var preGameInfoDto = ObjectMapper.Map<PreGameInfo, PreGameInfoDto>(preGameInfo);

        var challengeRequestResultDto = new ChallengeRequestResultDto
        {
            IsAccepted = true,
            PreGameInfoDto = preGameInfoDto
        };

        await _hubContext.Clients.User(challengeRequest.ReceiverUserId.ToString())
            .SendAsync(
                SignalRConstants.GameHubConstants.Methods.ChallengeResult,
                challengeRequestResultDto,
                cancellationToken: cancellationToken
            );

        await _hubContext.Clients.User(challengeRequest.SenderUserId.ToString())
            .SendAsync(
                SignalRConstants.GameHubConstants.Methods.ChallengeResult,
                challengeRequestResultDto,
                cancellationToken: cancellationToken
            );

        var challengeRequestDto = ObjectMapper.Map<ChallengeRequest, ChallengeRequestDto>(challengeRequest);

        return challengeRequestDto;
    }

    public async Task<ChallengeRequestDto> RejectAsync(
        Guid challengeRequestId,
        CancellationToken cancellationToken = default
    )
    {
        var challengeRequest = await _challengeRequestManager.GetByAsync(
            p =>
                p.Id.Equals(challengeRequestId),
            throwIfNotExists: true,
            cancellationToken
        );

        challengeRequest.ChallengeStatusId = (int)ChallengeStatus.Rejected;

        var challengeRequestDto = ObjectMapper.Map<ChallengeRequest, ChallengeRequestDto>(challengeRequest);

        var challengeRequestResultDto = new ChallengeRequestResultDto
        {
            IsAccepted = false
        };

        await _hubContext.Clients.User(challengeRequest.SenderUserId.ToString()).SendAsync(
            SignalRConstants.GameHubConstants.Methods.ChallengeResult,
            challengeRequestResultDto,
            cancellationToken: cancellationToken
        );

        await _hubContext.Clients.User(challengeRequest.ReceiverUserId.ToString()).SendAsync(
            SignalRConstants.GameHubConstants.Methods.ChallengeResult,
            challengeRequestResultDto,
            cancellationToken: cancellationToken
        );

        await _challengeRequestRepository.UpdateAsync(challengeRequest, cancellationToken: cancellationToken);

        return challengeRequestDto;
    }
}