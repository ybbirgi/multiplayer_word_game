using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WordGame.Dtos.Challenges;
using WordGame.Services;

namespace WordGame.Controllers.ChallengeRequests;

[Authorize]
[ApiController]
[Route("api/challenge-requests")]
public class ChallengeRequestController
{
    private readonly IChallengeRequestAppService _challengeRequestAppService;

    public ChallengeRequestController(IChallengeRequestAppService challengeRequestAppService)
    {
        _challengeRequestAppService = challengeRequestAppService;
    }

    [HttpPost("send/{userId}")]
    public async Task<ChallengeRequestDto> SendAsync(
        Guid userId,
        CancellationToken cancellationToken = default
    )
    {
        return await _challengeRequestAppService.SendAsync(userId,cancellationToken);
    }

    [HttpPost("{challengeRequestId}/reject")]
    public async Task<ChallengeRequestDto> RejectAsync(
        Guid challengeRequestId,
        CancellationToken cancellationToken = default
    )
    {
        return await _challengeRequestAppService.RejectAsync(challengeRequestId, cancellationToken);
    }

    [HttpPost("{challengeRequestId}/accept")]
    public async Task<ChallengeRequestDto> AcceptAsync(
        Guid challengeRequestId,
        CancellationToken cancellationToken = default
    )
    {
        return await _challengeRequestAppService.AcceptAsync(challengeRequestId,cancellationToken);
    }
}