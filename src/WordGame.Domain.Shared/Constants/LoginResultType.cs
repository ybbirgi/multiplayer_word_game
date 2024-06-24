using System.ComponentModel;

namespace WordGame.Constants;

public enum LoginResultType : byte
{
    [Description("LoginResultType:01")]
    Success = 1,
    [Description("LoginResultType:02")]
    InvalidUserNameOrPassword = 2,
    [Description("LoginResultType:03")]
    NotAllowed = 3,
    [Description("LoginResultType:04")]
    LockedOut = 4,
    [Description("LoginResultType:05")]
    RequiresTwoFactor = 5
}