using Volo.Abp.DependencyInjection;
using Volo.Abp.Ui.Branding;

namespace WordGame;

[Dependency(ReplaceServices = true)]
public class WordGameBrandingProvider : DefaultBrandingProvider
{
    public override string AppName => "WordGame";
}
