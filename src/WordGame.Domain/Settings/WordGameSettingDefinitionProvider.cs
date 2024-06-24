using Volo.Abp.Settings;

namespace WordGame.Settings;

public class WordGameSettingDefinitionProvider : SettingDefinitionProvider
{
    public override void Define(ISettingDefinitionContext context)
    {
        //Define your own settings here. Example:
        //context.Add(new SettingDefinition(WordGameSettings.MySetting1));
    }
}
