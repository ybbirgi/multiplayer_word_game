namespace WordGame.Dtos.Games;

public class GameCurrentStatusDto
{
    public GameUserGuessDto CurrentUserGuesses { get; set; } 
    public GameUserGuessDto OtherUserGuesses { get; set; } 
}