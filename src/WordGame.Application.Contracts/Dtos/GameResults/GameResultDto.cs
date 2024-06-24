using System;

namespace WordGame.Dtos.GameResults;

public class GameResultDto
{
    public Guid GameId { get; set; }
    public int GameResultId { get; set; }
    public string GameResultName { get; set; }
    public UserScoreDto? CurrentUserScore { get; set; }
    public UserScoreDto? OtherUserScore { get; set; }
    public GameResultDto? PreviousGameResult { get; set; }
}