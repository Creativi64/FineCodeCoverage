using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.VisualStudio.Shell;

namespace FineCodeCoverage.Readme.Options.OptionPagesInfo
{
    internal static class OptionPageInfoProvider
    {
        public static IEnumerable<OptionPageInfo> Provide(Type packageType, List<string> coverageSettingsPropertyNames)
        {
            IEnumerable<ProvideOptionPageAttribute> provideOptionPageAttributes = packageType.GetCustomAttributes<ProvideOptionPageAttribute>();
            return provideOptionPageAttributes.Select(provideOptionPageAttribute
                => new OptionPageInfo(
                    provideOptionPageAttribute.PageType.BaseType.GetGenericArguments()[0],
                    provideOptionPageAttribute.PageName,
                    coverageSettingsPropertyNames));
        }
    }
}
