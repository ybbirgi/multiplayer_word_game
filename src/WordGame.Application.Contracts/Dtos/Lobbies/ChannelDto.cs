namespace WordGame.Dtos.Lobbies;

public class ChannelDto
{
    public int Id { get; set; }
    public int GameTypeId { get; set; }
    public string GameTypeName { get; set; }
    public int WordLength { get; set; }
    public int TotalUserCount { get; set; }
}