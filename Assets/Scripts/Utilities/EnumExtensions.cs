using System;
using System.Reflection;
using UnityEngine;

public static class EnumExtensions
{
    /// <summary>
    /// Will get the string value for a given enums value, this will
    /// only work if you assign the StringValue attribute to
    /// the items in your enum.
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public static string GetStringValue(this Enum value)
    {
        // Get the type
        Type type = value.GetType();

        // Get fieldinfo for this type
        FieldInfo fieldInfo = type.GetField(value.ToString());

        // Get the stringvalue attributes
        StringValueAttribute[] attribs = fieldInfo.GetCustomAttributes(
            typeof(StringValueAttribute), false) as StringValueAttribute[];

        // Return the first if there was a match.
        return attribs.Length > 0 ? attribs[0].StringValue : null;
    }

    public static string GetShorthand(this Enum value)
    {
        Type type = value.GetType();

        FieldInfo fieldInfo = type.GetField(value.ToString());

        ShorthandValueAttribute[] attribs = fieldInfo.GetCustomAttributes(
            typeof(ShorthandValueAttribute), false) as ShorthandValueAttribute[];

        return attribs.Length > 0 ? attribs[0].ShorthandValue : null;
    }
}
