using System;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Volo.Abp.Domain.Repositories.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;
using WordGame.Entities;
using WordGame.EntityFrameworkCore;

namespace WordGame.Repositories;

public class EfGameRepository : EfCoreRepository<WordGameDbContext, Game, Guid>, IGameRepository
{
    public EfGameRepository(IDbContextProvider<WordGameDbContext> dbContextProvider) : base(dbContextProvider)
    {
    }

    public async Task<Game?> GetByAsync(
        Expression<Func<Game, bool>> predicate,
        bool includeDetails = false,
        CancellationToken cancellationToken = default
    )
    {
        var dbSet = await GetDbSetAsync();
        return await dbSet
            .Include(c=>c.ParentGame)
            .ThenInclude(c=>c.PreGameInfo)
            .ThenInclude(c=>c.ChallengeRequest)
            .Include(c => c.PreGameInfo)
            .ThenInclude(c=>c.ChallengeRequest)
            .FirstOrDefaultAsync(predicate, cancellationToken: cancellationToken);
    }
}