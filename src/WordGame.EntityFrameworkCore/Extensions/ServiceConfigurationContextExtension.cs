using Volo.Abp.AuditLogging;
using Volo.Abp.Data;
using Volo.Abp.FeatureManagement;
using Volo.Abp.Identity;
using Volo.Abp.OpenIddict;
using Volo.Abp.PermissionManagement;
using Volo.Abp.SettingManagement;

namespace WordGame.Extensions;

public static class ServiceConfigurationContextExtension
{
    public static void ResolveSchemaAndPrefix()
    {
        // common schema and prefix override:
        AbpCommonDbProperties.DbSchema = WordGameDatabaseConstants.SchemaName;
        AbpCommonDbProperties.DbTablePrefix = string.Empty;
      
        // setting management schema and prefix override:
        AbpSettingManagementDbProperties.DbSchema = WordGameDatabaseConstants.SchemaName;
        AbpSettingManagementDbProperties.DbTablePrefix = string.Empty;

        // permission management schema and prefix override:
        AbpPermissionManagementDbProperties.DbSchema = WordGameDatabaseConstants.SchemaName;
        AbpPermissionManagementDbProperties.DbTablePrefix = string.Empty;

        // audit management schema and prefix override:
        AbpAuditLoggingDbProperties.DbSchema = WordGameDatabaseConstants.SchemaName;
        AbpAuditLoggingDbProperties.DbTablePrefix = string.Empty;
      
        // identity schema and prefix override:
        AbpIdentityDbProperties.DbSchema = WordGameDatabaseConstants.SchemaName;
        AbpIdentityDbProperties.DbTablePrefix = string.Empty;

        // open iddict schema and prefix override:
        AbpOpenIddictDbProperties.DbSchema = WordGameDatabaseConstants.SchemaName;
        AbpOpenIddictDbProperties.DbTablePrefix = string.Empty;

        // feature management schema and prefix override:
        AbpFeatureManagementDbProperties.DbSchema = WordGameDatabaseConstants.SchemaName;
        AbpFeatureManagementDbProperties.DbTablePrefix = string.Empty;
    }
}
