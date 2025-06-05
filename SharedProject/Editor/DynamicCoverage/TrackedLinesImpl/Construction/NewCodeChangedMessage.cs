namespace FineCodeCoverage.Editor.DynamicCoverage
{
    internal class NewCodeChangedMessage
    {
        public NewCodeChangedMessage(string path, bool hasNewCode)
        {
            Path = path;
            HasNewCode = hasNewCode;
        }

        public string Path { get; }

        public bool HasNewCode { get; }
    }
}
