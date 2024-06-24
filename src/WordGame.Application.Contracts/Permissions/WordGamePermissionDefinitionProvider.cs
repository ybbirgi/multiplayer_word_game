using WordGame.Localization;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Localization;

namespace WordGame.Permissions;

public class WordGamePermissionDefinitionProvider : PermissionDefinitionProvider
{
    public override void Define(IPermissionDefinitionContext context)
    {
        var myGroup = context.AddGroup(WordGamePermissions.GroupName);
        //Define your own permissions here. Example:
        //myGroup.AddPermission(WordGamePermissions.MyPermission1, L("Permission:MyPermission1"));
    }

    private static LocalizableString L(string name)
    {
        return LocalizableString.Create<WordGameResource>(name);
    }
}
