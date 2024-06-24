using WordGame.Dtos.PreGameInfos;

namespace WordGame.Dtos.Challenges;

public class ChallengeRequestResultDto
{
    public bool IsAccepted { get; set; }
    public PreGameInfoDto? PreGameInfoDto { get; set; } = null;
}