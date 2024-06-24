using System;
using WordGame.Constants;
using WordGame.Extensions;

namespace WordGame.Dtos.Accounts;

public class PasswordLoginResult 
{
    public PasswordLoginResult(LoginResultType result)
    {
        Result = result;
    }
    
    public Guid UserId { get; set; } 
    public LoginResultType Result { get; }

    public string Description => Result.GetDescription();
}