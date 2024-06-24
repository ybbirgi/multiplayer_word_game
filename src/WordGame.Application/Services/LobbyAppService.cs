using System.Linq;
using System.Threading;
using WordGame.Dtos.Lobbies;
using WordGame.Extensions;
using WordGame.Models.Lobbies;

namespace WordGame.Services;

public class LobbyAppService : WordGameAppService, ILobbyAppService
{
    public LobbyDto GetLobby(
        CancellationToken cancellationToken = default
    )
    {
        return new LobbyDto
        {
            Channels = Lobby.Channels.Select(p => new ChannelDto
            {
                Id = p.Id,
                GameTypeId = (int)p.GameTypeId,
                GameTypeName = p.GameTypeId.GetDescription(),
                WordLength = p.WordLength,
                TotalUserCount = p.Users.Count
            }).ToList()
        };
    }
}