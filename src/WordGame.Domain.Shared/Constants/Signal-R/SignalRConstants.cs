namespace WordGame.Constants.Signal_R;

public static class SignalRConstants
{
    public const string EscapeString = "-1";
    public const int MaxGuessCount = 5;
    public static class GameHubConstants
    {
        public static class Methods
        {
            public const string ActiveUserList = "activeUsers";
            public const string ChallengeReceived = "challengeReceived";
            public const string ChallengeResult = "challengeResult";
            public const string SetWord = "setWord";
            public const string GameResult = "gameResult";
            public const string NewGame = "newGame";
            public const string GuessWord = "guessWord";
            public const string CurrentGameStatus = "currentGameStatus";
            public const string SendReMatch = "sendReMatch";
            public const string AcceptReMatch = "acceptRematch";
            public const string RejectReMatch = "rejectRematch";
            public const string ReMatchResult = "rematchResult";
        }
    }
}