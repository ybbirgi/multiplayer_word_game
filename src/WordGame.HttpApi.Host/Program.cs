using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;

namespace WordGame;

public class Program
{
    public async static Task<int> Main(string[] args)
    {
        Log.Logger = new LoggerConfiguration()
#if DEBUG
            .MinimumLevel.Debug()
#else
            .MinimumLevel.Information()
#endif
            .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
            .MinimumLevel.Override("Microsoft.EntityFrameworkCore", LogEventLevel.Warning)
            .Enrich.FromLogContext()
            .WriteTo.Async(c => c.File("Logs/logs.txt"))
            .WriteTo.Async(c => c.Console())
            .CreateLogger();

        try
        {
            Log.Information("Starting WordGame.HttpApi.Host.");
            var builder = WebApplication.CreateBuilder(args);
            builder.Host.AddAppSettingsSecretsJson()
                .UseAutofac()
                .UseSerilog();
            builder.Host.AddAppSettingsSecretsJson().ConfigureAppConfiguration(
                builder =>
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
                });
            await builder.AddApplicationAsync<WordGameHttpApiHostModule>();
            var app = builder.Build();
            await app.InitializeApplicationAsync();
            await app.RunAsync();
            return 0;
        }
        catch (Exception ex)
        {
            if (ex is HostAbortedException)
            {
                throw;
            }

            Log.Fatal(ex, "Host terminated unexpectedly!");
            return 1;
        }
        finally
        {
            Log.CloseAndFlush();
        }
    }
}
