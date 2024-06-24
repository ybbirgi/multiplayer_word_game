using System;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Localization;
using Volo.Abp;
using Volo.Abp.Domain.Services;
using WordGame.Entities;
using WordGame.ExceptionCodes;
using WordGame.Localization;
using WordGame.Repositories;

namespace WordGame.Managers;

public class GameManager : DomainService
{
    private readonly IGameRepository _gameRepository;
    private readonly IStringLocalizer<WordGameResource> _stringLocalizer;

    public GameManager(
        IGameRepository gameRepository,
        IStringLocalizer<WordGameResource> stringLocalizer
    )
    {
        _gameRepository = gameRepository;
        _stringLocalizer = stringLocalizer;
    }

    public Game Create(
        Guid preGameInfoId,
        Guid? parentGameId = null
    )
    {
        return new Game(GuidGenerator.Create())
        {
            PreGameInfoId = preGameInfoId,
            ParentGameId = parentGameId,
        };
    }

    public async Task<Game> GetByAsync
    (Expression<Func<Game, bool>> predicate,
        bool throwIfNotExists = false,
        bool includeDetails = false,
        CancellationToken cancellationToken = default
    )
    {
        var game =
            await _gameRepository.GetByAsync(
                predicate,
                cancellationToken: cancellationToken
            );
        if (game == null && throwIfNotExists)
        {
            throw new UserFriendlyException(_stringLocalizer[GameExceptionCodes.NotFound]);
        }

        return game;
    }
}