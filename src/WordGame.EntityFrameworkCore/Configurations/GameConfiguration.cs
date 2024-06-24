using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Volo.Abp.EntityFrameworkCore.Modeling;
using WordGame.Entities;
using WordGame.Extensions;

namespace WordGame.Configurations;

public class GameConfiguration : IEntityTypeConfiguration<Game>
{
    public void Configure(EntityTypeBuilder<Game> builder)
    {
        builder.ToTable(builder.GetTableName(), WordGameDatabaseConstants.SchemaName);
        builder.ConfigureByConvention();

        builder.HasOne(p => p.ParentGame)
            .WithOne()
            .HasForeignKey<Game>(p => p.ParentGameId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(c => c.PreGameInfo)
            .WithOne(p => p.Game)
            .HasForeignKey<Game>(p => p.PreGameInfoId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}