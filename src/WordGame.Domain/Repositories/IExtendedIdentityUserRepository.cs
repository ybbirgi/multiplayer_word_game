using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Volo.Abp.Identity;

namespace WordGame.Repositories;

public interface IExtendedIdentityUserRepository : IIdentityUserRepository
{
    Task<IdentityUser?> GetByAsync(
        Expression<Func<IdentityUser, bool>> predicate,
        bool includeDetails = false,
        CancellationToken cancellationToken = default
    );

    Task<List<IdentityUser>> GetListByAsync(
        Expression<Func<IdentityUser, bool>> predicate,
        bool includeDetails = false,
        CancellationToken cancellationToken = default
    );

    Task<bool> AnyAsync(
        Expression<Func<IdentityUser, bool>> predicate,
        CancellationToken cancellationToken = default
    );
}