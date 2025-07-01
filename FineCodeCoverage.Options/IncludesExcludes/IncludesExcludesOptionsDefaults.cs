using System.ComponentModel.Composition;
using FineCodeCoverage.Options.Base;

namespace FineCodeCoverage.Options.IncludesExcludes
{
    [Export(typeof(IDefaultOptionsSetter<IncludesExcludesOptions>))]
    internal sealed class IncludesExcludesOptionsDefaults : IDefaultOptionsSetter<IncludesExcludesOptions>
    {
        public void Set(IncludesExcludesOptions options)
        {
            options.ExcludeByAttribute = new[] { "GeneratedCode" };
            options.IncludeTestAssembly = true;
            options.ExcludeByFile = new[] { "**/Migrations/*" };
        }
    }
}
