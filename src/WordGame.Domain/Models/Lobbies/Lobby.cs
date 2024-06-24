using System.Collections.Generic;
using WordGame.Constants;

namespace WordGame.Models.Lobbies;

public static class Lobby
{
    public static List<Channel> Channels { get; set; }

    public static void Initialize()
    {
        Channels = new List<Channel>
        {
            new Channel
            {
                Id = 1,
                ChannelName = "Classic 1",
                GameTypeId = GameTypes.Classic,
                WordLength = 4,
                Users = new List<UserWithStatus>()
            },
            new Channel()
            {
                Id = 2,
                ChannelName = "Classic 2",
                GameTypeId = GameTypes.Classic,
                WordLength = 5,
                Users = new List<UserWithStatus>()
            },
            new Channel()
            {
                Id = 3,
                ChannelName = "Classic 3",
                GameTypeId = GameTypes.Classic,
                WordLength = 6,
                Users = new List<UserWithStatus>()
            },
            new Channel()
            {
                Id = 4,
                ChannelName = "Classic 4",
                GameTypeId = GameTypes.Classic,
                WordLength = 7,
                Users = new List<UserWithStatus>()
            },
            new Channel()
            {
                Id = 5,
                ChannelName = "Random Word Generated 1",
                GameTypeId = GameTypes.RandomWordGenerated,
                WordLength = 4,
                Users = new List<UserWithStatus>()
            },
            new Channel()
            {
                Id = 6,
                ChannelName = "Random Word Generated 2",
                GameTypeId = GameTypes.RandomWordGenerated,
                WordLength = 5,
                Users = new List<UserWithStatus>()
            },
            new Channel()
            {
                Id = 7,
                ChannelName = "Random Word Generated 3",
                GameTypeId = GameTypes.RandomWordGenerated,
                WordLength = 6,
                Users = new List<UserWithStatus>()
            },
            new Channel()
            {
                Id = 8,
                ChannelName = "Random Word Generated 4",
                GameTypeId = GameTypes.RandomWordGenerated,
                WordLength = 7,
                Users = new List<UserWithStatus>()
            }
        };
    }
}