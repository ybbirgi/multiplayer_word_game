using WordGame.EntityFrameworkCore;
using Volo.Abp.Modularity;

namespace WordGame;

[DependsOn(
    typeof(WordGameEntityFrameworkCoreTestModule)
    )]
public class WordGameDomainTestModule : AbpModule
{

}
