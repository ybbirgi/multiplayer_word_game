using System;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WordGame.Entities;

namespace WordGame.Extensions;

public static class MessageManagementTableNameProvider
{
    public static string GetTableName<T>(this EntityTypeBuilder<T> entityTypeBuilder) where T : class => typeof(T).Name
        switch
        {
            nameof(ChallengeRequest) => "ChallengeRequests",
            nameof(PreGameInfo) => "PreGameInfos",
            nameof(Game) => "Games",
            _ => throw new NotImplementedException()
        };
}