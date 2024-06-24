using System;

namespace WordGame.Dtos.Games;

public class GameCreationDto
{
    public Guid GameId { get; set; }
    public Guid PreGameId { get; set; }
}