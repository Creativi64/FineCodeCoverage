using System.ComponentModel;

namespace FineCodeCoverage.Options
{
    /*
        Note that option properties must not be renamed
        Interfaces to be retained for reflection - AppOptions => CoverageSettings
    */
    internal class AppOptions: IAppOptions
    {
        private const string oldExcludeIncludeCategory = "Exclude / Include ( Coverlet / OpenCover )";
        private const string commonExcludeIncludeCategory = "Exclude / Include ( Common )";
        private const string msExcludeIncludeCategory = "Exclude / Include ( Microsoft )";
        private const string openCoverToolCategory = "Tool ( OpenCover )";

        #region exclude / include
        #region common exclude include
        [Category(commonExcludeIncludeCategory)]
        [Description("Set to true to add all referenced projects to Include.")]
        //[DisplayName("Include Referenced Projects")]
        public bool IncludeReferencedProjects { get; set; }

        [Category(commonExcludeIncludeCategory)]
        [Description(
        @"Specifies whether to report code coverage of the test assembly
		")]
        //[DisplayName("Include Test Assembly")]
        public bool IncludeTestAssembly { get; set; }

        [Category(commonExcludeIncludeCategory)]
        [Description(
        @"Provide a list of assemblies to exclude from coverage.  The dll name without extension is used for matching.
		")]
        //[DisplayName("Exclude Assemblies")]
        public string[] ExcludeAssemblies { get; set; }

        [Category(commonExcludeIncludeCategory)]
        [Description(
        @"Provide a list of assemblies to include in coverage. The dll name without extension is used for matching.
		")]
        //[DisplayName("Include Assemblies")]
        public string[] IncludeAssemblies { get; set; }
        #endregion

        #region old exclude include
        [Category(oldExcludeIncludeCategory)]
        [Description(
        @"Filter expressions to exclude specific modules and types (multiple)
		
		Wildcards
		* => matches zero or more characters
		
		Examples
		[*]* => Excludes all types in all assemblies (nothing is instrumented)
		[coverlet.*]Coverlet.Core.Coverage => Excludes the Coverage class in the Coverlet.Core namespace belonging to any assembly that matches coverlet.* (e.g coverlet.core)
		[*]Coverlet.Core.Instrumentation.* => Excludes all types belonging to Coverlet.Core.Instrumentation namespace in any assembly
		[coverlet.*.tests]* => Excludes all types in any assembly starting with coverlet. and ending with .tests
		
		Both 'Exclude' and 'Include' options can be used together but 'Exclude' takes precedence.
		")]
        //[DisplayName("Exclude")]
        public string[] Exclude { get; set; }

        [Category(oldExcludeIncludeCategory)]
        [Description(
        @"Filter expressions to include specific modules and types (multiple)
		
		Wildcards
		* => matches zero or more characters
		
		Examples
		[*]*"" => Includes all types in all assemblies (nothing is instrumented)
		[coverlet.*]Coverlet.Core.Coverage => Includes the Coverage class in the Coverlet.Core namespace belonging to any assembly that matches coverlet.* (e.g coverlet.core)
		[*]Coverlet.Core.Instrumentation.* => Includes all types belonging to Coverlet.Core.Instrumentation namespace in any assembly
		[coverlet.*.tests]* => Includes all types in any assembly starting with coverlet. and ending with .tests
		
		Both 'Exclude' and 'Include' options can be used together but 'Exclude' takes precedence.
		")]
        //[DisplayName("Include")]
        public string[] Include { get; set; }

        [Category(oldExcludeIncludeCategory)]
        [Description(
        @"Glob patterns specifying source files to exclude (multiple)
		Use file path or directory path with globbing (e.g. **/Migrations/*)
		")]
        //[DisplayName("Exclude By File")]
        public string[] ExcludeByFile { get; set; }

        [Category(oldExcludeIncludeCategory)]
        [Description(
        @"Attributes to exclude from code coverage (multiple)

		You can ignore a method or an entire class from code coverage by creating and applying the [ExcludeFromCodeCoverage] attribute present in the System.Diagnostics.CodeAnalysis namespace.
		You can also ignore additional attributes by adding to this list (short name or full name supported) e.g. :
		[GeneratedCode] => Present in the System.CodeDom.Compiler namespace
		[MyCustomExcludeFromCodeCoverage] => Any custom attribute that you may define
		")]
        //[DisplayName("Exclude By Attribute")]
        public string[] ExcludeByAttribute { get; set; }
        #endregion

        #region ms exclude include
        [Category(msExcludeIncludeCategory)]
        [Description("Multiple regexes that match assemblies specified by assembly name or file path - for exclusion")]
        //[DisplayName("Module Paths Exclude")]
        public string[] ModulePathsExclude { get; set; }

        [Category(msExcludeIncludeCategory)]
        [Description("Multiple regexes that match assemblies specified by assembly name or file path - for inclusion")]
        //[DisplayName("Module Paths Include")]
        public string[] ModulePathsInclude { get; set; }

        [Category(msExcludeIncludeCategory)]
        [Description("Multiple regexes that match assemblies by the Company attribute - for exclusion")]
        //[DisplayName("Company Names Exclude")]
        public string[] CompanyNamesExclude { get; set; }

        [Category(msExcludeIncludeCategory)]
        [Description("Multiple regexes that match assemblies by the Company attribute - for inclusion")]
        //[DisplayName("Company Names Include")]
        public string[] CompanyNamesInclude { get; set; }

        [Category(msExcludeIncludeCategory)]
        [Description("Multiple regexes that match assemblies by the public key token - for exclusion")]
        //[DisplayName("Public Key Tokens Exclude")]
        public string[] PublicKeyTokensExclude { get; set; }

        [Category(msExcludeIncludeCategory)]
        [Description("Multiple regexes that match assemblies by the public key token - for inclusion")]
        //[DisplayName("Public Key Tokens Include")]
        public string[] PublicKeyTokensInclude { get; set; }

        [Category(msExcludeIncludeCategory)]
        [Description("Multiple regexes that match elements by the path name of the source file in which they're defined - for exclusion")]
        //[DisplayName("Sources Exclude")]
        public string[] SourcesExclude { get; set; }

        [Category(msExcludeIncludeCategory)]
        [Description("Multiple regexes that match elements by the path name of the source file in which they're defined - for inclusion")]
        //[DisplayName("Sources Include")]
        public string[] SourcesInclude { get; set; }

        [Category(msExcludeIncludeCategory)]
        [Description("Multiple regexes that match elements that have the specified attribute by full name - for exclusion")]
        //[DisplayName("Attributes Exclude")]
        public string[] AttributesExclude { get; set; }

        [Category(msExcludeIncludeCategory)]
        [Description("Multiple regexes that match elements that have the specified attribute by full name - for inclusion")]
        //[DisplayName("Attributes Include")]
        public string[] AttributesInclude { get; set; }

        [Category(msExcludeIncludeCategory)]
        [Description("Multiple regexes that match procedures, functions, or methods by fully qualified name, including the parameter list. - for exclusion")]
        //[DisplayName("Functions Exclude")]
        public string[] FunctionsExclude { get; set; }

        [Category(msExcludeIncludeCategory)]
        [Description("Multiple regexes that match procedures, functions, or methods by fully qualified name, including the parameter list. - for inclusion")]
        //[DisplayName("Functions Include")]
        public string[] FunctionsInclude { get; set; }
        #endregion
        #endregion

        #region open cover tool only
        [Description("Specify path to open cover exe if you need functionality that the FCC version does not provide.")]
        [Category(openCoverToolCategory)]
        //[DisplayName("OpenCover Custom Path")]
        public string OpenCoverCustomPath { get; set; }

        [Description("Change from Default if FCC determination of path32 or path64 is incorrect.")]
        [Category(openCoverToolCategory)]
        //[DisplayName("OpenCover Register")]
        public OpenCoverRegister OpenCoverRegister { get; set; }

        [Category(openCoverToolCategory)]
        [Description("Supply your own target if required.")]
        //[DisplayName("OpenCover Target")]
        public string OpenCoverTarget { get; set; }

        [Category(openCoverToolCategory)]
        [Description("If supplying your own target you can also supply additional arguments.  FCC supplies the test dll path.")]
        //[DisplayName("OpenCover Target Args")]
        public string OpenCoverTargetArgs { get; set; }
        #endregion
    }
}
