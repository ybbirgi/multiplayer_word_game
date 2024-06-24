using System;
using System.ComponentModel;
using System.Reflection;
using Microsoft.Extensions.Localization;
using WordGame.Localization;

namespace WordGame.Extensions;

public static class GlobalLocalizationProvider
{
    public static IStringLocalizer<WordGameResource> StringLocalizer;

    public static string GetDescription(this Enum value)
    {
        Type type = value.GetType();
        string? name = Enum.GetName(type, value);
        if (name != null)
        {
            FieldInfo? field = type.GetField(name);
            if (field != null && Attribute.GetCustomAttribute(field,
                    typeof(DescriptionAttribute)) is DescriptionAttribute attr)
            {
                return StringLocalizer[attr.Description];
            }
        }

        return null;
    }

    public static void SetLocalizer(IStringLocalizer<WordGameResource> stringLocalizer)
    {
        StringLocalizer = stringLocalizer;
    }
}