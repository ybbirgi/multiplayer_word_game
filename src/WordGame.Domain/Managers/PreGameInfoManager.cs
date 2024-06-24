using System;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Localization;
using Volo.Abp;
using Volo.Abp.Domain.Services;
using WordGame.Entities;
using WordGame.Localization;
using WordGame.Models.PreGameInfos;
using WordGame.Repositories;

namespace WordGame.Managers;

public class PreGameInfoManager : DomainService
{
    private readonly IPreGameInfoRepository _preGameInfoRepository;
    private readonly IStringLocalizer<WordGameResource> _stringLocalizer;

    public PreGameInfoManager(
        IPreGameInfoRepository preGameInfoRepository,
        IStringLocalizer<WordGameResource> stringLocalizer
    )
    {
        _preGameInfoRepository = preGameInfoRepository;
        _stringLocalizer = stringLocalizer;
    }

    public PreGameInfo Create(
        PreGameInfoCreateModel preGameInfoCreateModel
    )
    {
        return new PreGameInfo(GuidGenerator.Create())
        {
            ChallengeRequestId = preGameInfoCreateModel.ChallengeRequestId,
            GameTypeId = preGameInfoCreateModel.GameTypeId,
            WordLength = preGameInfoCreateModel.WordLength
        };
    }

    public async Task<PreGameInfo?> GetByAsync(
        Expression<Func<PreGameInfo, bool>> predicate,
        string exceptionMessage,
        bool throwIfNotExists = true,
        bool includeDetails = true,
        CancellationToken cancellationToken = default
    )
    {
        var preGameInfo =
            await _preGameInfoRepository.GetByAsync(
                predicate,
                includeDetails,
                cancellationToken: cancellationToken
            );
        if (preGameInfo == null)
        {
            throw new UserFriendlyException(_stringLocalizer[exceptionMessage]);
        }

        return preGameInfo;
    }
}