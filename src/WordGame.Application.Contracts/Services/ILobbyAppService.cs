using System.Threading;
using WordGame.Dtos.Lobbies;

namespace WordGame.Services;

public interface ILobbyAppService
{
    LobbyDto GetLobby(
        CancellationToken cancellationToken = default
    );
}