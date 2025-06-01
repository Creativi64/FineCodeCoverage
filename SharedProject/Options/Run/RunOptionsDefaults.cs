using System.ComponentModel.Composition;

namespace FineCodeCoverage.Options
{
    [Export(typeof(IDefaultOptionsSetter<RunOptions>))]
    internal class RunOptionsDefaults : IDefaultOptionsSetter<RunOptions>
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
