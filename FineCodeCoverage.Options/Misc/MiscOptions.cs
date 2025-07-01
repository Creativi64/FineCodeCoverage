using System.ComponentModel;

namespace FineCodeCoverage.Options
{
    /*
        Note that option properties must not be renamed
    */
    public sealed class MiscOptions
    {
        private const string ToolsCategory = "Tools";
        private const string SettingsCategory = "Settings";
        private const string ReadmeCategory = "Readme";

        [Category(ToolsCategory)]
        [Description("Folder to copy FCC tools into. Must alredy exist. Requires restart of VS.")]
        [DisplayName("Tools Directory")]
        public string ToolsDirectory { get; set; }

        [Category(SettingsCategory)]
        [Description("Option page to open from button on toolbar")]
        [DisplayName("Open Option Page")]
        public OpenOptionPage OpenOptionPage { get; set; }

        [Category(ReadmeCategory)]
        [Description("In the readme tool window show hyperlink urls on hover")]
        [DisplayName("Show hyperlink url on hover")]
        public bool ShowHyperlinkUrlHover { get; set; }
    }
}
