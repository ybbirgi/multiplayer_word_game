using System.Threading;
using System.Threading.Tasks;
using WordGame.Dtos.Accounts;

namespace WordGame.Services;

public interface ILoginAppService
{
    Task<LoginResultDto> LoginAsync(LoginDto loginDto,CancellationToken cancellationToken = default);
}