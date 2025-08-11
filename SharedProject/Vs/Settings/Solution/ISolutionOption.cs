using System.IO;

namespace FineCodeCoverage.Vs.Settings.Solution
{
    internal interface ISolutionOption
    {
        string Key { get; }

        void Load(Stream stream);

        void Save(Stream stream);

        void Unloaded();
    }
}
