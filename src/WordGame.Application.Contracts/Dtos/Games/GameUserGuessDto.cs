using System;
using System.Collections.Generic;

namespace WordGame.Dtos.Games;

public class GameUserGuessDto
{
    public Guid UserId { get; set; }
    public List<List<WordGuessDto>> WordGuess { get; set; }
}