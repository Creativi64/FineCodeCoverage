using System.IO;

namespace FineCodeCoverage.Core.Utilities.Solution
{
    interface ISolutionOption
    {
        string Key { get; }

        void Load(Stream stream);
        void Save(Stream stream);
        void Unloaded();
    }
}