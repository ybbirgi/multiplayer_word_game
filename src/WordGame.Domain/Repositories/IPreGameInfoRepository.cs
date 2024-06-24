using System;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;
using WordGame.Entities;

namespace WordGame.Repositories;

public interface IPreGameInfoRepository : IRepository<PreGameInfo>
{
    Task<PreGameInfo?> GetByAsync(
        Expression<Func<PreGameInfo, bool>> predicate,
        bool includeDetails = false,
        CancellationToken cancellationToken = default
    );
}