using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Volo.Abp.EntityFrameworkCore.Modeling;
using WordGame.Entities;
using WordGame.Extensions;

namespace WordGame.Configurations;

public class ChallengeRequestConfiguration : IEntityTypeConfiguration<ChallengeRequest>
{
    public void Configure(EntityTypeBuilder<ChallengeRequest> builder)
    {
        builder.ToTable(builder.GetTableName(), WordGameDatabaseConstants.SchemaName);
        builder.ConfigureByConvention();
    }
}