using System.Collections.Generic;
using WordGame.Constants;

namespace WordGame.Models.Lobbies;

public class Channel
{
    public int Id { get; set; }
    public string ChannelName { get; set; }
    public GameTypes GameTypeId { get; set; }
    public int WordLength { get; set; }
    public List<UserWithStatus> Users { get; set; }
}