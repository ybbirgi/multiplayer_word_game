using WordGame.EntityFrameworkCore;
using Volo.Abp.Autofac;
using Volo.Abp.Modularity;

namespace WordGame.DbMigrator;

[DependsOn(
    typeof(AbpAutofacModule),
    typeof(WordGameEntityFrameworkCoreModule),
    typeof(WordGameApplicationContractsModule)
    )]
public class WordGameDbMigratorModule : AbpModule
{
}
