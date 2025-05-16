using System.ComponentModel.Composition;

namespace FineCodeCoverage.Options
{
    [Export(typeof(IDefaultOptionsSetter<RunOptions>))]
    internal class AppOptionsDefaults : IDefaultOptionsSetter<AppOptions>
    {
        public void Set(AppOptions options)
        {
            options.RunSettingsOnly = true;
            options.ExcludeByAttribute = new[] { "GeneratedCode" };
            options.IncludeTestAssembly = true;
            options.ExcludeByFile = new[] { "**/Migrations/*" };
        }
    }
}
