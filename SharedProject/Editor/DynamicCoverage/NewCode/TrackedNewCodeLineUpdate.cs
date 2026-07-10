namespace FineCodeCoverage.Editor.DynamicCoverage.NewCode
{
    internal readonly struct TrackedNewCodeLineUpdate
    {
        public TrackedNewCodeLineUpdate(string text, int newLineNumber, int oldLineNumber)
        {
            Text = text;
            NewLineNumber = newLineNumber;
            OldLineNumber = oldLineNumber;
        }

        public string Text { get; }

        public int NewLineNumber { get; }

        public int OldLineNumber { get; }
    }
}
