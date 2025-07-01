using System.ComponentModel.Composition;
using FineCodeCoverage.Options.Base;

namespace FineCodeCoverage.Options.Run
{
    [Export(typeof(IDefaultOptionsSetter<RunOptions>))]
    internal sealed class RunOptionsDefaults : IDefaultOptionsSetter<RunOptions>
    {
        public void Set(RunOptions options)
        {
            options.RunMsCodeCoverage = RunMsCodeCoverage.Yes;
            options.RunWhenTestsFail = true;
            options.Enabled = true;
            options.DisabledNoCoverage = true;
        }
    }
}
