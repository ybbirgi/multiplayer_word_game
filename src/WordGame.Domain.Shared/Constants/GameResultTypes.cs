using System.ComponentModel;

namespace WordGame.Constants;

public enum GameResultTypes
{
    [Description("GameResultType:01")]
    Win = 1,
    [Description("GameResultType:02")]
    Lose = 2,
    [Description("GameResultType:03")]
    Draw = 3
}