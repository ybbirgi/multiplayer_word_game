namespace WordGame.ExceptionCodes.Login;

public static class LoginExceptionCodes
{
    private const string Prefix = "WordGame";
    private const string ChallengeExceptionCodesPrefix = $"{Prefix}.Login";
    
    public const string ErrorDuringTokenRequest = $"{ChallengeExceptionCodesPrefix}:00001";

}