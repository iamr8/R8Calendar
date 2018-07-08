using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;

namespace R8Calendar.Converter
{
    public static class Enums
    {
        public static string[] ToArray<T>()
        {
            if (!typeof(T).IsEnum)
                throw new InvalidOperationException();

            return (string[])Enum.GetValues(typeof(T));
        }

        public static T To<T>(this int value)
        {
            return (T)(object)value;
        }

        public static T To<T>(this string value)
        {
            return (T)Enum.Parse(typeof(T), value, true);
        }

        public static T NameFromDisplay<T>(this string displayName)
        {
            var type = typeof(T);
            if (!type.IsEnum) throw new InvalidOperationException();

            foreach (var field in type.GetFields())
            {
                if (Attribute.GetCustomAttribute(field,
                    typeof(DisplayAttribute)) is DisplayAttribute attribute)
                {
                    if (attribute.Name == displayName)
                        return (T)field.GetValue(null);
                }
                else
                {
                    if (field.Name == displayName)
                        return (T)field.GetValue(null);
                }
            }

            throw new ArgumentOutOfRangeException(nameof(displayName));
        }

        private static TAttribute GetEnumMemberAttribute<TAttribute>(this Type enumType, string enumMemberName) where TAttribute : Attribute =>
            enumType.GetMember(enumMemberName).Single().GetCustomAttribute<TAttribute>();

        public static string GetDisplay(this Enum value)
        {
            var enumType = value.GetType();
            var enumMemberName = Enum.GetName(enumType, value);
            return enumType
                .GetEnumMemberAttribute<DisplayAttribute>(enumMemberName)
                ?.GetName()
                ?? enumMemberName;
        }

        public static List<int> ToList<T>()
        {
            return Enum.GetValues(typeof(T)).Cast<int>().ToList();
        }

        public static string GetDescription(this Enum value)
        {
            var enumType = value.GetType();
            var enumMemberName = Enum.GetName(enumType, value);
            return enumType
                .GetEnumMemberAttribute<DescriptionAttribute>(enumMemberName)
                ?.Description
                ?? enumMemberName;
        }
    }
}