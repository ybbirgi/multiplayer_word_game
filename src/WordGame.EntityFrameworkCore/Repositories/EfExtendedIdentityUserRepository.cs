using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;
using Volo.Abp.Identity;
using Volo.Abp.Identity.EntityFrameworkCore;

namespace WordGame.Repositories;

public class EfExtendedIdentityUserRepository : EfCoreIdentityUserRepository, IExtendedIdentityUserRepository
{
    public EfExtendedIdentityUserRepository(
        IDbContextProvider<IIdentityDbContext> dbContextProvider
    )
        : base(
            dbContextProvider
        )
    {
    }

    public async Task<IdentityUser?> GetByAsync(
        Expression<Func<IdentityUser, bool>> predicate,
        bool includeDetails = false,
        CancellationToken cancellationToken = default
    )
    {
        var dbSet = await GetDbSetAsync();
        return await dbSet
            .IncludeIf(includeDetails, c => c.Roles)
            .FirstOrDefaultAsync(predicate, cancellationToken: cancellationToken);
    }


    public async Task<List<IdentityUser>> GetListByAsync(
        Expression<Func<IdentityUser, bool>> predicate,
        bool includeDetails = false,
        CancellationToken cancellationToken = default
    )
    {
        var dbSet = await GetDbSetAsync();
        return await dbSet
            .IncludeIf(includeDetails, c => c.Roles)
            .Where(predicate).ToListAsync(cancellationToken: cancellationToken);
    }

    public async Task<bool> AnyAsync(
        Expression<Func<IdentityUser, bool>> predicate,
        CancellationToken cancellationToken = default
    )
    {
        var dbSet = await GetDbSetAsync();
        return await dbSet.AnyAsync(predicate, cancellationToken: cancellationToken);
    }
}