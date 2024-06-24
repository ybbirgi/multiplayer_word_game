using System;

namespace WordGame.Models.Hubs;

public class LobbyHubConnectionModel
{
    public Guid UserId { get; set; }
    public int ChannelId { get; set; }
    public string ConnectionId { get; set; }
}