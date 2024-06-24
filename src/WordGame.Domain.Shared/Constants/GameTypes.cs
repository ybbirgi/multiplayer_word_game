using System.ComponentModel;

namespace WordGame.Constants;

public enum GameTypes
{
    [Description("GameTypes:01")]
    Classic = 1, 
    [Description("GameTypes:02")]
    RandomWordGenerated = 2
}