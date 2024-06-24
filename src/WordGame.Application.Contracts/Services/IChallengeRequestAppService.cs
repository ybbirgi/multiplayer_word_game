using System;
using System.Threading;
using System.Threading.Tasks;
using WordGame.Dtos.Challenges;

namespace WordGame.Services;

public interface IChallengeRequestAppService
{
    Task<ChallengeRequestDto> SendAsync(
        Guid userId,
        CancellationToken cancellationToken = default
    );

    Task<ChallengeRequestDto> AcceptAsync(
        Guid challengeRequestId,
        CancellationToken cancellationToken = default
    );

    Task<ChallengeRequestDto> RejectAsync(
        Guid challengeRequestId,
        CancellationToken cancellationToken = default
    );
}