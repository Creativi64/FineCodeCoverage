using FineCodeCoverage.Engine.ReportGenerator;
using System;
using System.Collections.Generic;

namespace FineCodeCoverage.Output
{
    internal class SourceFile : ISourceFile
    {
        public event EventHandler HasNewCodeChanged;
        public SourceFile(string path, List<SourceFileClass> classes)
        {
            this.Path = path;
            this.Classes = classes;
        }
        public string Path { get; }
        public IReadOnlyList<IClass> Classes { get; }
        public bool HasNewCode { get; private set; }

        internal void SetHasNewCode(bool hasNewCode)
        {
            this.HasNewCode = hasNewCode;
            this.HasNewCodeChanged?.Invoke(this, EventArgs.Empty);
        }
    }

}
