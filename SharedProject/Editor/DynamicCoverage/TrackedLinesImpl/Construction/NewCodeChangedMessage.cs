namespace FineCodeCoverage.Editor.DynamicCoverage
{
    internal class NewCodeChangedMessage
    {
        public NewCodeChangedMessage(string path, bool hasNewCode)
        {
            this.Path = path;
            this.HasNewCode = hasNewCode;
        }

        public string Path { get; }
        public bool HasNewCode { get; }
    }
}