using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestWindow.Extensibility;

namespace FineCodeCoverage.Engine.MsTestPlatform.CodeCoverage
{
    public static class IOperationExtensions
    {
        public static IEnumerable<Uri> GetRunSettingsMsDataCollectorResultUri(this IOperation operation)
            => operation.GetRunSettingsDataCollectorResultUri(new Uri(RunSettingsHelper.MsDataCollectorUri));
    }
}
