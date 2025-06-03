using System.ComponentModel;

namespace FineCodeCoverage.Options
{
    /*
        Note that option properties must not be renamed
    */
    internal class MiscOptions
    {
        private const string toolsCategory = "Tools";
        private const string settingsCategory = "Settings";
        private const string readmeCategory = "Readme";

        [Category(toolsCategory)]
        [Description("Folder to copy FCC tools into. Must alredy exist. Requires restart of VS.")]
        [DisplayName("Tools Directory")]
        public string ToolsDirectory { get; set; }

        [Category(settingsCategory)]
        [Description("Option page to open from button on toolbar")]
        [DisplayName("Open Option Page")]
        public OpenOptionPage OpenOptionPage { get; set; }

        [Category(readmeCategory)]
        [Description("In the readme tool window show hyperlink urls on hover")]
        [DisplayName("Show hyperlink url on hover")]
        public bool ShowHyperlinkUrlHover { get; set; }
    }
}