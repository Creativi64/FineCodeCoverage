using System.ComponentModel.Composition;
using System.Linq;

namespace FineCodeCoverageTests.TestHelpers
{
    public static class MEFExportHelper
    {
        public static bool IsAndExports<T, TExport>()
        {
            var type = typeof(T);
            var exportAttributes = (ExportAttribute[])type.GetCustomAttributes(typeof(ExportAttribute), false);
            var hasExport = exportAttributes.Any(ea => ea.ContractType == typeof(TExport));

            return hasExport && typeof(TExport).IsAssignableFrom(type);
        }
    }
}
