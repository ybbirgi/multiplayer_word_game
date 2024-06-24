using System;
using Volo.Abp.Domain.Entities.Auditing;

namespace WordGame.Entities;

public class ChallengeRequest : CreationAuditedEntity<Guid>
{
    public int ChannelId { get; set; }
    public Guid SenderUserId { get; set; }
    public Guid ReceiverUserId { get; set; }
    public int ChallengeStatusId { get; set; }

    public virtual PreGameInfo? PreGameInfo { get; set; }
}