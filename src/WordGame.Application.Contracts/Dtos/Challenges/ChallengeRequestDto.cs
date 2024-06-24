using System;

namespace WordGame.Dtos.Challenges;

public class ChallengeRequestDto
{
    public Guid Id { get; set; }
    public Guid SenderUserId { get; set; }
    public string SenderUserName { get; set; }
    public Guid ReceiverUserId { get; set; }
    public string ReceiverUserName { get; set; }
    public int ChallengeStatusId { get; set; }
    public string ChallengeRequestStatusName { get; set; }
}