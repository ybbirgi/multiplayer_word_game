using System.Threading.Tasks;
using Volo.Abp.DependencyInjection;

namespace WordGame.Data;

/* This is used if database provider does't define
 * IWordGameDbSchemaMigrator implementation.
 */
public class NullWordGameDbSchemaMigrator : IWordGameDbSchemaMigrator, ITransientDependency
{
    public Task MigrateAsync()
    {
        return Task.CompletedTask;
    }
}
