using System.Threading;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WordGame.Dtos.Lobbies;
using WordGame.Services;

namespace WordGame.Controllers.Lobbies;

[Authorize]
[ApiController]
[Route("api/lobbies")]
public class LobbyController : WordGameController
{
    private readonly ILobbyAppService _lobbyAppService;

    public LobbyController(ILobbyAppService lobbyAppService)
    {
        _lobbyAppService = lobbyAppService;
    }

    [HttpGet]
    public LobbyDto GetLobby(
        CancellationToken cancellationToken = default
    )
    {
        return _lobbyAppService.GetLobby();
    }
}