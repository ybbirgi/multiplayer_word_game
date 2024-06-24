namespace WordGame.ExceptionCodes;

public static class ChallengeExceptionCodes
{
    private const string Prefix = "WordGame";
    private const string ChallengeExceptionCodesPrefix = $"{Prefix}.Challenge";
    
    public const string UserCannotChallengeToItself = $"{ChallengeExceptionCodesPrefix}:00001";
    public const string UserHasOtherPendingChallenge = $"{ChallengeExceptionCodesPrefix}:00002";
    public const string OtherUserHasOtherPendingChallenge = $"{ChallengeExceptionCodesPrefix}:00003";
    public const string OtherUserIsNotActive = $"{ChallengeExceptionCodesPrefix}:00004";
    public const string NotFound = $"{ChallengeExceptionCodesPrefix}:00005";
    public const string UserInGame = $"{ChallengeExceptionCodesPrefix}:00006";
}