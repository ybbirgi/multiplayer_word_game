using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using WordGame.Data;
using Volo.Abp.DependencyInjection;

namespace WordGame.EntityFrameworkCore;

public class EntityFrameworkCoreWordGameDbSchemaMigrator
    : IWordGameDbSchemaMigrator, ITransientDependency
{
    private readonly IServiceProvider _serviceProvider;

    public EntityFrameworkCoreWordGameDbSchemaMigrator(
        IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task MigrateAsync()
    {
        await _serviceProvider
            .GetRequiredService<WordGameDbContext>()
            .Database
            .MigrateAsync();
    }
}
