using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;

namespace FineCodeCoverage.Editor.DynamicCoverage
{
    [ExcludeFromCodeCoverage]
    internal class TextInfo : ITextInfo
    {
        private bool _triedGetTextDocumentProperty;
        private ITextDocument _document;

        private ITextDocument TextDocument
        {
            get
            {
                if (!_triedGetTextDocumentProperty)
                {
                    _triedGetTextDocumentProperty = true;
                    if (TextBuffer.Properties.TryGetProperty(typeof(ITextDocument), out ITextDocument document))
                    {
                        _document = document;
                    }
                }

                return _document;
            }
        }
        public TextInfo(ITextView textView, ITextBuffer textBuffer)
        {
            TextView = textView;
            TextBuffer = textBuffer as ITextBuffer2;
        }

        public ITextView TextView { get; }
        public ITextBuffer2 TextBuffer { get; }
        public string FilePath => TextDocument?.FilePath;
        public string GetFileText() => File.Exists(FilePath) ? File.ReadAllText(FilePath) : null;
        public DateTime GetLastWriteTime() => new FileInfo(FilePath).LastWriteTime;
    }
}
