using System;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Localization;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;
using Volo.Abp.Users;
using WordGame.Constants;
using WordGame.Entities;
using WordGame.ExceptionCodes;
using WordGame.Localization;
using WordGame.Repositories;

namespace WordGame.Managers;

public class ChallengeRequestManager : DomainService
{
    private readonly IChallengeRequestRepository _challengeRequestRepository;
    private readonly IStringLocalizer<WordGameResource> _stringLocalizer;
    private readonly ICurrentUser _currentUser;

    public ChallengeRequestManager(
        IChallengeRequestRepository challengeRequestRepository,
        IStringLocalizer<WordGameResource> stringLocalizer,
        ICurrentUser currentUser
    )
    {
        _challengeRequestRepository = challengeRequestRepository;
        _stringLocalizer = stringLocalizer;
        _currentUser = currentUser;
    }

    public ChallengeRequest Create(Guid userId,int channelId)
    {
        return new ChallengeRequest
        {
            SenderUserId = _currentUser.GetId(),
            ReceiverUserId = userId,
            ChallengeStatusId = (int)ChallengeStatus.Pending,
            ChannelId = channelId
        };
    }

    public async Task<bool> ExistsByAsync(
        Expression<Func<ChallengeRequest, bool>> predicate,
        string exceptionMessage,
        bool throwIfNotExists = false,
        CancellationToken cancellationToken = default)
    {
        var isExist = await _challengeRequestRepository.AnyAsync(predicate, cancellationToken);
        if (throwIfNotExists && !isExist)
        {
            throw new UserFriendlyException(_stringLocalizer[exceptionMessage]);
        }

        return isExist;
    }
    public async Task<bool> AlreadyExistsAsync(
        Expression<Func<ChallengeRequest, bool>> predicate,
        string exceptionMessage,
        bool throwIfExists = false,
        CancellationToken cancellationToken = default)
    {
        var isExist = await _challengeRequestRepository.AnyAsync(predicate, cancellationToken);
        if (throwIfExists && isExist)
        {
            throw new UserFriendlyException(_stringLocalizer[exceptionMessage]);
        }

        return isExist;
    }

    public async Task<ChallengeRequest> GetByAsync(
        Expression<Func<ChallengeRequest, bool>> predicate,
        bool throwIfNotExists = false,
        CancellationToken cancellationToken = default
    )
    {
        var challengeRequest =
            await _challengeRequestRepository.FirstOrDefaultAsync(predicate, cancellationToken: cancellationToken);
        if (challengeRequest == null)
        {
            throw new UserFriendlyException(_stringLocalizer[ChallengeExceptionCodes.NotFound]);
        }

        return challengeRequest;
    }
}