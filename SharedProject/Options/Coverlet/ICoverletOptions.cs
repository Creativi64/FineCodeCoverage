namespace FineCodeCoverage.Options
{
    internal interface ICoverletOptions
    {
        bool CoverletConsoleGlobal { get; set; }
        string CoverletConsoleCustomPath { get; set; }
        bool CoverletConsoleLocal { get; set; }
        string CoverletCollectorDirectoryPath { get; set; }
        bool RunSettingsOnly { get; set; }
    }
}