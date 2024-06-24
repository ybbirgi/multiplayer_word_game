namespace WordGame.ExceptionCodes;

public static class PreGameInfoExceptionCodes
{
    private const string Prefix = "WordGame";
    private const string PregameExceptionCodesPrefix = $"{Prefix}.PreGameInfo";

    public const string InvalidParameters = $"{PregameExceptionCodesPrefix}:00001";
    public const string PreGameInfoNotFound = $"{PregameExceptionCodesPrefix}:00002";
    public const string WordLengthNotValid = $"{PregameExceptionCodesPrefix}:00003";
}