using FineCodeCoverage.Output;
using Microsoft.VisualStudio.Shell;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace OptionsExtractor
{
    public static class OptionPageInfoProvider
    {
        public static IEnumerable<OptionPageInfo> Provide()
        {
            return typeof(FCCPackage).GetCustomAttributes<ProvideOptionPageAttribute>().Select(a =>
            {
                return new OptionPageInfo(a.PageType.BaseType.GetGenericArguments()[0],a.PageName);
            });
        }
    }
}
