using System.ComponentModel.Composition;

namespace FineCodeCoverage.Options
{
    [Export(typeof(IDefaultOptionsSetter<AppOptions>))]
    internal class AppOptionsDefaults : IDefaultOptionsSetter<AppOptions>
    {
        public void Set(AppOptions options)
        {
            options.ExcludeByAttribute = new[] { "GeneratedCode" };
            options.IncludeTestAssembly = true;
            options.ExcludeByFile = new[] { "**/Migrations/*" };
        }
    }
}
