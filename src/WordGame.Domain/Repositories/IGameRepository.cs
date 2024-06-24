using System;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;
using WordGame.Entities;

namespace WordGame.Repositories;

public interface IGameRepository : IRepository<Game>
{
    Task<Game?> GetByAsync(
        Expression<Func<Game, bool>> predicate,
        bool includeDetails = false,
        CancellationToken cancellationToken = default
    );
}