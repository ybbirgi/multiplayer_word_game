using Volo.Abp.Modularity;

namespace WordGame;

[DependsOn(
    typeof(WordGameApplicationModule),
    typeof(WordGameDomainTestModule)
    )]
public class WordGameApplicationTestModule : AbpModule
{

}
