using System;
using WordGame.Constants;

namespace WordGame.Models.Hubs;

public class GameHubConnectionModel
{
    public Guid UserId { get; set; }
    public Guid GameId { get; set; }
    public string ConnectionId { get; set; }
    public int ChannelId { get; set; }
    public DateTime LastCommunicationTime { get; set; }
    public DateTime InitialConnectionTime { get; set; }
    public int? ElapsedSeconds { get; set; }
    public ChallengeUserTypes ChallengeUserType { get; set; }
}