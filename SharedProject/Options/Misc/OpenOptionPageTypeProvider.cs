using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;

namespace FineCodeCoverage.Options
{
    [Export(typeof(IOpenOptionPageTypeProvider))]
    internal class OpenOptionPageTypeProvider : IOpenOptionPageTypeProvider
    {
        private readonly Dictionary<OpenOptionPage, Type> _lookup = new Dictionary<OpenOptionPage, Type>
        {
            { OpenOptionPage.Run, typeof(RunOptionsPage) },
            { OpenOptionPage.IncludesExcludes, typeof(IncludesExcludesOptionsPage) },
            { OpenOptionPage.Report, typeof(ReportOptionsPage) },
            { OpenOptionPage.Output, typeof(OutputOptionsPage) },
            { OpenOptionPage.Coverlet, typeof(CoverletOptionsPage) },
            { OpenOptionPage.OpenCover, typeof(OpenCoverOptionsPage) },
            { OpenOptionPage.EditorCoverageColouring, typeof(EditorCoverageColouringOptionsPage) },
            { OpenOptionPage.HotspotThresholds, typeof(HotspotThresholdsOptionsPage) },
            { OpenOptionPage.Misc, typeof(MiscOptionsPage) },
        };

        private readonly IDialogPageOptionsProvider<MiscOptions> _miscOptionsProvider;

        [ImportingConstructor]
        public OpenOptionPageTypeProvider(IDialogPageOptionsProvider<MiscOptions> miscOptionsProvider)
            => _miscOptionsProvider = miscOptionsProvider;

        public Type Get() => _lookup[_miscOptionsProvider.Options.OpenOptionPage];
    }
}
