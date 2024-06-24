using WordGame.Dtos.Games;

namespace WordGame.Models;

public class CurrentGameStatusModel
{
    public string RelatedWord { get; set; }
    public GameCurrentStatusDto CurrentUserStatusDto { get; set; }
    public GameCurrentStatusDto OtherUserStatusDto { get; set; }
}