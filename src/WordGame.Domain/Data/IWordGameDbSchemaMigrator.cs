using System.Threading.Tasks;

namespace WordGame.Data;

public interface IWordGameDbSchemaMigrator
{
    Task MigrateAsync();
}
