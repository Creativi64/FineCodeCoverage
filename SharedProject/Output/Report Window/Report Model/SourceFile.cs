using System;
using System.Collections.Generic;
using FineCodeCoverage.Engine.ReportGenerator;

namespace FineCodeCoverage.Output
{
    internal class SourceFile : ISourceFile
    {
        public event EventHandler HasNewCodeChanged;
        public event EventHandler PathChanged;

        private string path;
        public SourceFile(string path, List<SourceFileClass> classes, bool hasNewCode)
        {
            this.Path = path;
            this.Classes = classes;
            HasNewCode = hasNewCode;
        }

        public string Path
        {
            get => path;
            set
            {
                path = value;
                PathChanged?.Invoke(this, EventArgs.Empty);
            }
        }
        public IReadOnlyList<IClass> Classes { get; }
        public bool HasNewCode { get; private set; }

        internal void SetHasNewCode(bool hasNewCode)
        {
            this.HasNewCode = hasNewCode;
            this.HasNewCodeChanged?.Invoke(this, EventArgs.Empty);
        }
    }

}
