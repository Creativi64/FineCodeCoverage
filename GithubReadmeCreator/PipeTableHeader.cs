namespace GithubReadmeCreator
{
    public class PipeTableHeader
    {
        public PipeTableHeader(string contents, PipeTableColumnAlignment alignment = default)
        {
            Contents = contents;
            Alignment = alignment;
        }

        public string Contents { get; }
        public PipeTableColumnAlignment Alignment { get; }
    }
}
