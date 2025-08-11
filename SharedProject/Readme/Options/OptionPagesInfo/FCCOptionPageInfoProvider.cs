using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using FineCodeCoverage.Collection.CoverageProjectManagement.Settings;
using FineCodeCoverage.Vs.Package;

namespace FineCodeCoverage.Readme.OptionPagesInfo
{
    [Export(typeof(IFCCOptionPageInfoProvider))]
    internal sealed class FCCOptionPageInfoProvider : IFCCOptionPageInfoProvider
    {
        public IEnumerable<OptionPageInfo> Provide()
            => new OptionPageInfoProvider().Provide(
                typeof(FCCPackage),
                typeof(CoverageSettings).GetProperties().Select(p => p.Name).ToList());
    }
}
