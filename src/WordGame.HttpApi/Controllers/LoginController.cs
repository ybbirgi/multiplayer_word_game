using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using WordGame.Dtos.Accounts;
using WordGame.Services;

namespace WordGame.Controllers;

[ApiController]
[Route("api/account")]
public class LoginController : WordGameController
{
    private readonly ILoginAppService _loginAppService;

    public LoginController(ILoginAppService loginAppService)
    {
        _loginAppService = loginAppService;
    }

    [HttpPost("basic-login")]
    public async Task<LoginResultDto> SendAsync(
        LoginDto loginDto,
        CancellationToken cancellationToken = default
    )
    {
        return await _loginAppService.LoginAsync(loginDto,cancellationToken);
    }
    
}