using System;
using System.Collections.Generic;
using FineCodeCoverage.Engine.ReportGenerator;

namespace FineCodeCoverage.Output
{
    internal sealed class SourceFile : ISourceFile
    {
        public event EventHandler HasNewCodeChanged;

        public event EventHandler PathChanged;

        private string _path;

        public SourceFile(string path, List<SourceFileClass> classes, bool hasNewCode)
        {
            Path = path;
            Classes = classes;
            HasNewCode = hasNewCode;
        }

        public string Path
        {
            get => _path;
            set
            {
                _path = value;
                PathChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public IReadOnlyList<IClass> Classes { get; }

        public bool HasNewCode { get; private set; }

        internal void SetHasNewCode(bool hasNewCode)
        {
            HasNewCode = hasNewCode;
            HasNewCodeChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}
