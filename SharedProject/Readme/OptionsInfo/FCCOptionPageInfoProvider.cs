using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using FineCodeCoverage.Engine.Model;
using FineCodeCoverage.Output;

namespace FineCodeCoverage.Readme
{
    [Export(typeof(IFCCOptionPageInfoProvider))]
    public class FCCOptionPageInfoProvider : IFCCOptionPageInfoProvider
    {
        public IEnumerable<OptionPageInfo> Provide()
        {
            return new OptionPageInfoProvider().Provide(
                typeof(FCCPackage),
                typeof(CoverageSettings).GetProperties().Select(p => p.Name).ToList()
            );
        }
    }
}
