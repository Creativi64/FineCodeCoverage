using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace FineCodeCoverage.Core.Utilities.Solution
{
    [Export(typeof(ISolutionOptions))]
    internal class SolutionOptions : ISolutionOptions
    {
        private readonly ISolutionOption[] options;

        [ImportingConstructor]
        public SolutionOptions(
            [ImportMany]
            ISolutionOption[] options,
            ISolutionEvents solutionEvents
        )
        {
            this.options = options;
            solutionEvents.AfterClosing += SolutionEvents_AfterClosing;
        }

        private void SolutionEvents_AfterClosing(object sender, System.EventArgs e)
        {
            foreach (ISolutionOption option in options)
            {
                option.Unloaded();
            }
        }

        public Task<IEnumerable<string>> GetKeysAsync()
        {
            return Task.FromResult(options.Select(o => o.Key));
        }

        public void LoadOptions(string key, Stream stream)
        {
            options.First(o => o.Key == key).Load(stream);
        }

        public void SaveOptions(string key, Stream stream)
        {
            options.First(o => o.Key == key).Save(stream);
        }
    }
}
