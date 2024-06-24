using System;
using System.IO;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace WordGame.EntityFrameworkCore;

/* This class is needed for EF Core console commands
 * (like Add-Migration and Update-Database commands) */
public class WordGameDbContextFactory : IDesignTimeDbContextFactory<WordGameDbContext>
{
    public WordGameDbContext CreateDbContext(string[] args)
    {
        // https://www.npgsql.org/efcore/release-notes/6.0.html#opting-out-of-the-new-timestamp-mapping-logic
        AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

        WordGameEfCoreEntityExtensionMappings.Configure();

        var configuration = BuildConfiguration();

        var builder = new DbContextOptionsBuilder<WordGameDbContext>()
            .UseNpgsql(configuration.GetConnectionString("Default"));

        return new WordGameDbContext(builder.Options);
    }

    private static IConfigurationRoot BuildConfiguration()
    {
        var builder = new ConfigurationBuilder()
            .SetBasePath(
                Path.Combine(Directory.GetCurrentDirectory(), "../../src/WordGame.HttpApi.Host/"))
            .AddJsonFile(
                $"{MultiEnvironmentConstants.AspNetCoreEnvironmentAppSettingFile}{MultiEnvironmentConstants.AspNetCoreEnvironmentExtension}",
                optional: false)
            .AddJsonFile(
                $"{MultiEnvironmentConstants.AspNetCoreEnvironmentAppSettingFile}." +
                $"{Environment.GetEnvironmentVariable($"{MultiEnvironmentConstants.AspNetCoreEnvironment}")}" +
                $"{MultiEnvironmentConstants.AspNetCoreEnvironmentExtension}",
                true,
                true
            ).AddEnvironmentVariables();

        return builder.Build();
    }
}
