using System;

namespace WordGame.Models.PreGameInfos;

public class PreGameInfoCreateModel
{
    public Guid ChallengeRequestId { get; set; }
    public int GameTypeId { get; set; }
    public int WordLength { get; set; }
}