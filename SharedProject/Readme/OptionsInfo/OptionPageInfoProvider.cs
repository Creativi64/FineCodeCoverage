using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.VisualStudio.Shell;

namespace FineCodeCoverage.Readme
{
    internal class OptionPageInfoProvider : IOptionPageInfoProvider
    {
        public IEnumerable<OptionPageInfo> Provide(Type packageType, List<string> coverageSettingsPropertyNames)
        {
            var provideOptionPageAttributes = packageType.GetCustomAttributes<ProvideOptionPageAttribute>();
            return provideOptionPageAttributes.Select(provideOptionPageAttribute =>
            {
                return new OptionPageInfo(
                    provideOptionPageAttribute.PageType.BaseType.GetGenericArguments()[0],
                    provideOptionPageAttribute.PageName,
                    coverageSettingsPropertyNames
                    );
            });
        }
    }
}
