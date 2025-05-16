using System.ComponentModel;

namespace FineCodeCoverage.Options
{
    /*
        Note that option properties must not be renamed
    */
    internal class ToolsOptions
    {
        [Description("Folder to which copy tools subfolder. Must alredy exist. Requires restart of VS.")]
        [DisplayName("Tools Directory")]
        public string ToolsDirectory { get; set; }
    }
}
