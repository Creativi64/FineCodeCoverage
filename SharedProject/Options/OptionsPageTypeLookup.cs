using FineCodeCoverage.Output;
using Microsoft.VisualStudio.Shell;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace FineCodeCoverage.Options
{
    internal static class OptionsPageTypeLookup
    {
        private static readonly Dictionary<Type, Type> optionPageLookup;

        static OptionsPageTypeLookup() => optionPageLookup = typeof(FCCPackage).GetCustomAttributes<ProvideOptionPageAttribute>().
                ToDictionary(a => a.PageType.BaseType.GetGenericArguments()[0], a => a.PageType);

        public static Type GetOptionPageType<TOption>()
        {
            return optionPageLookup[typeof(TOption)];
        }
    }
}
