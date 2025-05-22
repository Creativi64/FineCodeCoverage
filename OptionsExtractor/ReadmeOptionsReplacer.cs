using System;
using System.ComponentModel.Composition;

namespace OptionsExtractor
{
    [Export(typeof(IReadmeOptionsReplacer))]
    public class ReadmeOptionsReplacer : IReadmeOptionsReplacer
    {
        private readonly IOptionTableProvider optionTableProvider;
        private readonly IStringReplacer stringReplacer;

        [ImportingConstructor]
        public ReadmeOptionsReplacer():this(new OptionTableProvider(), new StringReplacer())
        {
        }

        internal ReadmeOptionsReplacer(IOptionTableProvider optionTableProvider, IStringReplacer stringReplacer)
        {
            this.optionTableProvider = optionTableProvider;
            this.stringReplacer = stringReplacer;
        }

        public string ReplaceReadMeMarkerWithOptionsTable(
            string markedReadme, string marker, Type packageType, Type coverageSettingsType
        ) => this.stringReplacer.Replace(
                markedReadme,
                marker,
                optionTableProvider.GetTableString(packageType, coverageSettingsType)
            );
    }
}
