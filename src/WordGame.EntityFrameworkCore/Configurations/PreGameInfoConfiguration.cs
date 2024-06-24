using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Volo.Abp.EntityFrameworkCore.Modeling;
using WordGame.Entities;
using WordGame.Extensions;

namespace WordGame.Configurations;

public class PreGameInfoConfiguration : IEntityTypeConfiguration<PreGameInfo>
{
    public void Configure(EntityTypeBuilder<PreGameInfo> builder)
    {
        builder.ToTable(builder.GetTableName(), WordGameDatabaseConstants.SchemaName);
        builder.ConfigureByConvention();

        builder.HasOne(c => c.ChallengeRequest)
            .WithOne(c => c.PreGameInfo)
            .HasForeignKey<PreGameInfo>(c => c.ChallengeRequestId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}