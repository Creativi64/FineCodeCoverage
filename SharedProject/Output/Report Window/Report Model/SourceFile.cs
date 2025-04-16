using FineCodeCoverage.Engine.ReportGenerator;
using System.Collections.Generic;

namespace FineCodeCoverage.Output
{
    internal class SourceFile : ISourceFile
    {
        public SourceFile(string path, List<SourceFileClass> classes)
        {
            Path = path;
            Classes = classes;
        }
        public string Path { get; }
        public IReadOnlyList<IClass> Classes { get; }
    }

}
