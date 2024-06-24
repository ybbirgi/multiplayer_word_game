using System.ComponentModel;

namespace WordGame.Constants;

public enum UserStatusTypes
{
    [Description("UserStatusType:01")]
    Online = 1,
    [Description("UserStatusType:02")]
    InGame = 2,
    [Description("UserStatusType:03")]
    Away = 3
}