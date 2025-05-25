namespace GithubReadmeCreator
{
    public class ReadmeOptionsReplacer
    {
        private readonly IOptionTableProvider optionTableProvider;
        private readonly IStringReplacer stringReplacer;

        public ReadmeOptionsReplacer() : this(new OptionTableProvider(), new StringReplacer())
        {
        }

        internal ReadmeOptionsReplacer(IOptionTableProvider optionTableProvider, IStringReplacer stringReplacer)
        {
            this.optionTableProvider = optionTableProvider;
            this.stringReplacer = stringReplacer;
        }

        public string ReplaceReadMeMarkerWithOptionsTable(
            string markedReadme, string marker
        ) => this.stringReplacer.Replace(
                markedReadme,
                marker,
                optionTableProvider.GetTableString()
            );
    }
}
