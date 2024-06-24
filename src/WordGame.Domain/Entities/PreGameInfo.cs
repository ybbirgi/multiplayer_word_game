using System;
using Volo.Abp.Domain.Entities.Auditing;

namespace WordGame.Entities;

public class PreGameInfo : CreationAuditedEntity<Guid>
{
    public Guid ChallengeRequestId { get; set; }
    public int GameTypeId { get; set; }
    public int WordLength { get; set; }
    public string? SenderPlayerWord { get; set; }
    public string? ReceiverPlayerWord { get; set; }
    public int? SenderUserRemainingSeconds { get; set; }
    public int? ReceiverUserRemainingSeconds { get; set; }
    public int PreGameStatusId { get; set; } = (int)PreGameStatuses.Pending;
    public virtual ChallengeRequest ChallengeRequest { get; set; }
    public virtual Game? Game { get; set; }
    public PreGameInfo(Guid id) : base(id)
    {
    }
}