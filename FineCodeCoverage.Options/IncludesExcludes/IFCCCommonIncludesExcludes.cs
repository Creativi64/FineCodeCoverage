namespace FineCodeCoverage.Options
{
    public interface IFCCCommonIncludesExcludes
    {
        bool IncludeTestAssembly { get; set; }

        bool IncludeReferencedProjects { get; set; }

        string[] ExcludeAssemblies { get; set; }

        string[] IncludeAssemblies { get; set; }
    }
}
