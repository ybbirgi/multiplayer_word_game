namespace WordGame.Dtos.GameResults;

public class UserScoreDto
{
    public int TotalScore { get; set; }
    public int GreenCharacterScore { get; set; }
    public int YellowCharacterScore { get; set; }
    public int PregameRemainingTimeScore { get; set; }
    public int? ElapsedTimeSeconds { get; set; }
}