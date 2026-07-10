using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestWindow.Extensibility;

namespace FineCodeCoverage.Collection.Ms
{
    public static class IOperationExtensions
    {
        public static IEnumerable<Uri> GetRunSettingsMsDataCollectorResultUri(this IOperation operation)
            => operation.GetRunSettingsDataCollectorResultUri(new Uri(RunSettingsHelper.MsDataCollectorUri));
    }
}
