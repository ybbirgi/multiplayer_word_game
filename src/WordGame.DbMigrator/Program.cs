using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Events;
using WordGame.Extensions;

namespace WordGame.DbMigrator;

class Program
{
    static async Task Main(string[] args)
    {
        ServiceConfigurationContextExtension.ResolveSchemaAndPrefix();

        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Information()
            .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
            .MinimumLevel.Override("Volo.Abp", LogEventLevel.Warning)
#if DEBUG
            .MinimumLevel.Override("WordGame", LogEventLevel.Debug)
#else
                .MinimumLevel.Override("WordGame", LogEventLevel.Information)
#endif
            .Enrich.FromLogContext()
            .WriteTo.Async(c => c.File("Logs/logs.txt"))
            .WriteTo.Async(c => c.Console())
            .CreateLogger();

        await CreateHostBuilder(args).RunConsoleAsync();
    }

    public static IHostBuilder CreateHostBuilder(
        string[] args
    ) =>
        Host.CreateDefaultBuilder(args)
            .AddAppSettingsSecretsJson()
            .ConfigureAppConfiguration((
                _,
                builder
            ) =>
            {
                builder.AddJsonFile(
                        $"{MultiEnvironmentConstants.AspNetCoreEnvironmentAppSettingFile}{MultiEnvironmentConstants.AspNetCoreEnvironmentExtension}",
                        false, true)
                    .AddJsonFile(
                        $"{MultiEnvironmentConstants.AspNetCoreEnvironmentAppSettingFile}." +
                        $"{Environment.GetEnvironmentVariable($"{MultiEnvironmentConstants.AspNetCoreEnvironment}")}" +
                        $"{MultiEnvironmentConstants.AspNetCoreEnvironmentExtension}",
                        true,
                        true
                    ).AddEnvironmentVariables();
            })
            .ConfigureLogging((
                context,
                logging
            ) => logging.ClearProviders())
            .ConfigureServices((
                hostContext,
                services
            ) =>
            {
                services.AddHostedService<DbMigratorHostedService>();
            });
}