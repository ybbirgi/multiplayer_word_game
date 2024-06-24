using WordGame.Models.Hubs;

namespace WordGame.LocalEvents.Etos;

public class PreGameUserDisconnectedEventEto
{
    public PreGameHubConnectionModel PreGameHubConnectionModel { get; set; } 
}