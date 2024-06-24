using WordGame.Models.Hubs;

namespace WordGame.LocalEvents.Etos;

public class GameUserDisconnectedEventEto
{
    public GameHubConnectionModel GameHubConnectionModel { get; set; }
}