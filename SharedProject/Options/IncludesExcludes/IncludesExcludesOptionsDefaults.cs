using System.ComponentModel.Composition;

namespace FineCodeCoverage.Options
{
    [Export(typeof(IDefaultOptionsSetter<IncludesExcludesOptions>))]
    internal class IncludesExcludesOptionsDefaults : IDefaultOptionsSetter<IncludesExcludesOptions>
    {
        public void Set(IncludesExcludesOptions options)
        {
            options.ExcludeByAttribute = new[] { "GeneratedCode" };
            options.IncludeTestAssembly = true;
            options.ExcludeByFile = new[] { "**/Migrations/*" };
        }
    }
}
