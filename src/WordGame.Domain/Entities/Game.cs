using System;
using Volo.Abp.Domain.Entities.Auditing;
using WordGame.Constants;

namespace WordGame.Entities;

public class Game : CreationAuditedEntity<Guid>
{
    public Guid PreGameInfoId { get; set; }
    public Guid? ParentGameId { get; set; }
    public string? SenderUserAnswer { get; set; }
    public string? ReceiverUserAnswer { get; set; }
    public int SenderUserGuessCount { get; set; } = 0;
    public int ReceiverUserGuessCount { get; set; } = 0;
    public string? SenderUserGameResult { get; set; }
    public string? ReceiverUserGameResult { get; set; }
    public int? GameStatusId { get; set; } = (int)GameStatusTypes.Finished;
    public virtual Game? ParentGame { get; set; }
    public virtual PreGameInfo PreGameInfo { get; set; }

    public Game(Guid id) : base(id)
    {
    }
}