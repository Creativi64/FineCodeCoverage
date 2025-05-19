using System.ComponentModel;

namespace FineCodeCoverage.Options
{
    /*
        Note that option properties must not be renamed
    */
    internal class MiscOptions
    {
        private const string toolsCategory = "Tools";

        [Category(toolsCategory)]
        [Description("Folder to copy FCC tools into. Must alredy exist. Requires restart of VS.")]
        [DisplayName("Tools Directory")]
        public string ToolsDirectory { get; set; }

        [DisplayName("Option page to open from button on toolbar")]
        public OpenOptionPage OpenOptionPage { get; set; }
    }
}
