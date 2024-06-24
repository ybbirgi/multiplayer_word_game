using System;

namespace WordGame.Models.Hubs;

public class PreGameHubConnectionModel
{
    public Guid UserId { get; set; }
    public Guid PreGameInfoId { get; set; }
    public string ConnectionId { get; set; }
    public DateTime LastCommunicationTime { get; set; }
    public bool IsRematch { get; set; }
}