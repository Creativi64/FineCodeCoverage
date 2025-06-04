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
        private readonly ISolutionOption[] _options;

        [ImportingConstructor]
        public SolutionOptions(
            [ImportMany]
            ISolutionOption[] options,
            ISolutionEvents solutionEvents
        )
        {
            this._options = options;
            solutionEvents.AfterClosing += this.SolutionEvents_AfterClosing;
        }

        private void SolutionEvents_AfterClosing(object sender, System.EventArgs e)
        {
            foreach (ISolutionOption option in this._options)
            {
                option.Unloaded();
            }
        }

        public Task<IEnumerable<string>> GetKeysAsync() => Task.FromResult(this._options.Select(o => o.Key));

        public void LoadOptions(string key, Stream stream) => this._options.First(o => o.Key == key).Load(stream);

        public void SaveOptions(string key, Stream stream) => this._options.First(o => o.Key == key).Save(stream);
    }
}
