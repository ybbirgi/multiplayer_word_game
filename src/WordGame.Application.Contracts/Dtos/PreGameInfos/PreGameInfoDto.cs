using System;

namespace WordGame.Dtos.PreGameInfos;

public class PreGameInfoDto
{
    public Guid Id { get; set; }
    public Guid ChallengeRequestId { get; set; }
    public int GameTypeId { get; set; }
    public string GameTypeName { get; set; }
    public int WordLength { get; set; }
}