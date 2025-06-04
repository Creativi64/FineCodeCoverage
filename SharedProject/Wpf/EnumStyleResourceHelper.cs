using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace FineCodeCoverage.Wpf
{
    internal static class EnumStyleResourceHelper
    {
        public static Dictionary<TEnum, Style> GetEnumKeyedStyles<TEnum>(this FrameworkElement frameworkElement) where TEnum : Enum
        {
            var styleDictionary = new Dictionary<TEnum, Style>();
            Enum.GetValues(typeof(TEnum)).OfType<TEnum>().ToList().ForEach(enumValue =>
            {
                if (!(frameworkElement.TryFindResource(enumValue) is Style style))
                {
                    return;
                }

                styleDictionary.Add(enumValue, style);
            });
            return styleDictionary;
        }
    }
}
