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

public class EfPreGameInfoRepository : EfCoreRepository<WordGameDbContext, PreGameInfo, Guid>, IPreGameInfoRepository
{
    public EfPreGameInfoRepository(IDbContextProvider<WordGameDbContext> dbContextProvider) : base(dbContextProvider)
    {
    }

    public async Task<PreGameInfo?> GetByAsync(
        Expression<Func<PreGameInfo, bool>> predicate,
        bool includeDetails = false,
        CancellationToken cancellationToken = default
    )
    {
        var dbSet = await GetDbSetAsync();
        return await dbSet
            .IncludeIf(includeDetails,c=>c.ChallengeRequest)
            .FirstOrDefaultAsync(
                predicate,
                cancellationToken: cancellationToken
            );
    }
}