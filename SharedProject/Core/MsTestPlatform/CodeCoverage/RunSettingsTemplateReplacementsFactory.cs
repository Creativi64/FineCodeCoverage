using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using FineCodeCoverage.Engine.Model;
using FineCodeCoverage.Options;
using Microsoft.VisualStudio.TestWindow.Extensibility;

namespace FineCodeCoverage.Engine.MsTestPlatform.CodeCoverage
{
    [Export(typeof(IRunSettingsTemplateReplacementsFactory))]
    internal class RunSettingsTemplateReplacementsFactory : IRunSettingsTemplateReplacementsFactory
    {
        private class RunSettingsTemplateReplacements : IRunSettingsTemplateReplacements
        {
            public string Enabled { get; set; }
            public string ResultsDirectory { get; set; }
            public string TestAdapter { get; set; }
            public string ModulePathsExclude { get; set; }
            public string ModulePathsInclude { get; set; }
            public string FunctionsExclude { get; set; }
            public string FunctionsInclude { get; set; }
            public string AttributesExclude { get; set; }
            public string AttributesInclude { get; set; }
            public string SourcesExclude { get; set; }
            public string SourcesInclude { get; set; }
            public string CompanyNamesExclude { get; set; }
            public string CompanyNamesInclude { get; set; }
            public string PublicKeyTokensExclude { get; set; }
            public string PublicKeyTokensInclude { get; set; }

            public RunSettingsTemplateReplacements(
                IMsCodeCoverageIncludesExcludesOptions settings,
                string resultsDirectory,
                string enabled,
                string testAdapter
            )
            {
                this.ResultsDirectory = resultsDirectory;
                this.TestAdapter = testAdapter;
                this.Enabled = enabled;
                this.ModulePathsExclude = GetExcludeIncludeElementsString(settings.ModulePathsExclude, "ModulePath");
                this.ModulePathsInclude = GetExcludeIncludeElementsString(settings.ModulePathsInclude, "ModulePath");
                this.FunctionsExclude = GetExcludeIncludeElementsString(settings.FunctionsExclude, "Function");
                this.FunctionsInclude = GetExcludeIncludeElementsString(settings.FunctionsInclude, "Function");
                this.AttributesExclude = GetExcludeIncludeElementsString(settings.AttributesExclude, "Attribute");
                this.AttributesInclude = GetExcludeIncludeElementsString(settings.AttributesInclude, "Attribute");
                this.SourcesExclude = GetExcludeIncludeElementsString(settings.SourcesExclude, "Source");
                this.SourcesInclude = GetExcludeIncludeElementsString(settings.SourcesInclude, "Source");
                this.CompanyNamesExclude = GetExcludeIncludeElementsString(settings.CompanyNamesExclude, "CompanyName");
                this.CompanyNamesInclude = GetExcludeIncludeElementsString(settings.CompanyNamesInclude, "CompanyName");
                this.PublicKeyTokensExclude = GetExcludeIncludeElementsString(settings.PublicKeyTokensExclude, "PublicKeyToken");
                this.PublicKeyTokensInclude = GetExcludeIncludeElementsString(settings.PublicKeyTokensInclude, "PublicKeyToken");
            }

            private static string GetExcludeIncludeElementsString(IEnumerable<string> excludeIncludes, string elementName)
            {
                if (excludeIncludes == null)
                {
                    return string.Empty;
                }

                IEnumerable<string> elements = excludeIncludes.Select(excludeInclude => $"<{elementName}>{excludeInclude}</{elementName}>").Distinct();
                return string.Concat(elements);
            }
        }

        private class MergedIncludesExcludesOptions : IMsCodeCoverageIncludesExcludesOptions
        {
            private readonly List<ICoverageSettings> _allOptions;
            public MergedIncludesExcludesOptions(IEnumerable<ICoverageSettings> allOptions)
            {
                this._allOptions = allOptions.ToList();

                this.ModulePathsExclude = this.Merge(options => options.ModulePathsExclude);
                this.ModulePathsInclude = this.Merge(options => options.ModulePathsInclude);
                this.CompanyNamesExclude = this.Merge(options => options.CompanyNamesExclude);
                this.CompanyNamesInclude = this.Merge(options => options.CompanyNamesInclude);
                this.PublicKeyTokensExclude = this.Merge(options => options.PublicKeyTokensExclude);
                this.PublicKeyTokensInclude = this.Merge(options => options.PublicKeyTokensInclude);
                this.SourcesExclude = this.Merge(options => options.SourcesExclude);
                this.SourcesInclude = this.Merge(options => options.SourcesInclude);
                this.AttributesExclude = this.Merge(options => options.AttributesExclude);
                this.AttributesInclude = this.Merge(options => options.AttributesInclude);
                this.FunctionsExclude = this.Merge(options => options.FunctionsExclude);
                this.FunctionsInclude = this.Merge(options => options.FunctionsInclude);
            }

            private string[] Merge(Func<ICoverageSettings, string[]> selector)
                => this._allOptions.SelectMany(options => selector(options) ?? Array.Empty<string>()).ToArray();

            public string[] ModulePathsExclude { get; set; }
            public string[] ModulePathsInclude { get; set; }
            public string[] CompanyNamesExclude { get; set; }
            public string[] CompanyNamesInclude { get; set; }
            public string[] PublicKeyTokensExclude { get; set; }
            public string[] PublicKeyTokensInclude { get; set; }
            public string[] SourcesExclude { get; set; }
            public string[] SourcesInclude { get; set; }
            public string[] AttributesExclude { get; set; }
            public string[] AttributesInclude { get; set; }
            public string[] FunctionsInclude { get; set; }
            public string[] FunctionsExclude { get; set; }
        }

        private class CombinedIncludesExcludesOptions : IMsCodeCoverageIncludesExcludesOptions
        {
            public CombinedIncludesExcludesOptions(
                MergedIncludesExcludesOptions includesExcludesOptions,
                IEnumerable<string> additionalModulePathsIncludes,
                IEnumerable<string> additionalModulePathsExcludes
            ) : this(includesExcludesOptions.ModulePathsInclude,
                includesExcludesOptions.ModulePathsExclude,
                additionalModulePathsIncludes,
                additionalModulePathsExcludes)
            {
                this.CompanyNamesInclude = includesExcludesOptions.CompanyNamesInclude;
                this.CompanyNamesExclude = includesExcludesOptions.CompanyNamesExclude;
                this.PublicKeyTokensInclude = includesExcludesOptions.PublicKeyTokensInclude;
                this.PublicKeyTokensExclude = includesExcludesOptions.PublicKeyTokensExclude;
                this.SourcesExclude = includesExcludesOptions.SourcesExclude;
                this.SourcesInclude = includesExcludesOptions.SourcesInclude;
                this.AttributesExclude = includesExcludesOptions.AttributesExclude;
                this.AttributesInclude = includesExcludesOptions.AttributesInclude;
                this.FunctionsInclude = includesExcludesOptions.FunctionsInclude;
                this.FunctionsExclude = includesExcludesOptions.FunctionsExclude;
            }
            public CombinedIncludesExcludesOptions(
                ICoverageSettings includesExcludesOptions,
                IEnumerable<string> additionalModulePathsIncludes,
                IEnumerable<string> additionalModulePathsExcludes
            ) : this(
                includesExcludesOptions.ModulePathsInclude,
                includesExcludesOptions.ModulePathsExclude,
                additionalModulePathsIncludes,
                additionalModulePathsExcludes)
            {
                this.CompanyNamesInclude = includesExcludesOptions.CompanyNamesInclude;
                this.CompanyNamesExclude = includesExcludesOptions.CompanyNamesExclude;
                this.PublicKeyTokensInclude = includesExcludesOptions.PublicKeyTokensInclude;
                this.PublicKeyTokensExclude = includesExcludesOptions.PublicKeyTokensExclude;
                this.SourcesExclude = includesExcludesOptions.SourcesExclude;
                this.SourcesInclude = includesExcludesOptions.SourcesInclude;
                this.AttributesExclude = includesExcludesOptions.AttributesExclude;
                this.AttributesInclude = includesExcludesOptions.AttributesInclude;
                this.FunctionsInclude = includesExcludesOptions.FunctionsInclude;
                this.FunctionsExclude = includesExcludesOptions.FunctionsExclude;
            }
            private CombinedIncludesExcludesOptions(string[] modulePathsInclude, string[] modulePathsExclude, IEnumerable<string> additionalModulePathsIncludes, IEnumerable<string> additionalModulePathsExcludes)
            {
                IEnumerable<string> modulePathsIncludesFromOptions = modulePathsInclude ?? Enumerable.Empty<string>();
                IEnumerable<string> modulePathsExcludesFromOptions = modulePathsExclude ?? Enumerable.Empty<string>();
                this.ModulePathsInclude = additionalModulePathsIncludes.Concat(modulePathsIncludesFromOptions).ToArray();
                this.ModulePathsExclude = additionalModulePathsExcludes.Concat(modulePathsExcludesFromOptions).ToArray();
            }
            public string[] ModulePathsExclude { get; set; }
            public string[] ModulePathsInclude { get; set; }
            public string[] CompanyNamesExclude { get; set; }
            public string[] CompanyNamesInclude { get; set; }
            public string[] PublicKeyTokensExclude { get; set; }
            public string[] PublicKeyTokensInclude { get; set; }
            public string[] SourcesExclude { get; set; }
            public string[] SourcesInclude { get; set; }
            public string[] AttributesExclude { get; set; }
            public string[] AttributesInclude { get; set; }
            public string[] FunctionsInclude { get; set; }
            public string[] FunctionsExclude { get; set; }
        }

        public IRunSettingsTemplateReplacements Create(
            IEnumerable<ITestContainer> testContainers,
            Dictionary<string, IUserRunSettingsProjectDetails> userRunSettingsProjectDetailsLookup,
            string testAdapter)
        {
            var allProjectDetails = testContainers.Select(tc => userRunSettingsProjectDetailsLookup[tc.Source]).ToList();
            string resultsDirectory = allProjectDetails[0].CoverageOutputFolder;
            IEnumerable<ICoverageSettings> allSettings = allProjectDetails.Select(pd => pd.Settings);
            bool allProjectsDisabled = allSettings.All(s => !s.Enabled);
            var mergedSettings = new MergedIncludesExcludesOptions(allSettings);

            IEnumerable<string> additionalModulePathsExclude = allProjectDetails.SelectMany(pd =>
                GetAdditionalModulePathsExclude(pd.ExcludedReferencedProjects, pd.TestDllFile, pd.Settings.IncludeTestAssembly));

            bool hasIncludes = allProjectDetails.Any(pd => HasIncludes(pd.Settings.ModulePathsInclude, pd.IncludedReferencedProjects));
            IEnumerable<string> additionalModulePathsInclude = allProjectDetails.SelectMany(pd =>
                GetAdditionalModulePathsInclude(hasIncludes, pd.IncludedReferencedProjects, pd.TestDllFile, pd.Settings.IncludeTestAssembly));

            var settings = new CombinedIncludesExcludesOptions(mergedSettings, additionalModulePathsInclude, additionalModulePathsExclude);
            return new RunSettingsTemplateReplacements(settings, resultsDirectory, (!allProjectsDisabled).ToString().ToLower(), testAdapter);
        }

        private static IEnumerable<string> GetAdditionalModulePaths(
            IEnumerable<IReferencedProject> referencedProjects,
            string testDllFile,
            bool includeTestAssembly,
            bool isInclude
            )
        {
            IEnumerable<string> additionalReferenced = referencedProjects.Select(
                rp => MsCodeCoverageRegex.RegexModuleName(rp.AssemblyName, rp.IsDll));
            if (includeTestAssembly == isInclude)
            {
                additionalReferenced = additionalReferenced.Append(MsCodeCoverageRegex.RegexEscapePath(testDllFile));
            }

            return additionalReferenced;

        }

        private static IEnumerable<string> GetAdditionalModulePathsExclude(
     IEnumerable<IReferencedProject> referencedProjects, string testDllFile, bool includeTestAssembly)
            => GetAdditionalModulePaths(referencedProjects, testDllFile, includeTestAssembly, false);

        private static bool HasIncludes(
            string[] modulePathsInclude,
            List<IReferencedProject> includedReferencedProjects)
            => modulePathsInclude?.Any() == true || includedReferencedProjects.Any();

        private static IEnumerable<string> GetAdditionalModulePathsInclude(
            bool hasIncludes,
            List<IReferencedProject> includedReferencedProjects,
            string testDllFile,
            bool includeTestAssembly)

        {
            includeTestAssembly = includeTestAssembly && hasIncludes;
            return GetAdditionalModulePaths(
                includedReferencedProjects,
                testDllFile,
                includeTestAssembly,
                true);

        }

        public IRunSettingsTemplateReplacements Create(ICoverageProject coverageProject, string testAdapter)
        {
            ICoverageSettings projectSettings = coverageProject.Settings;
            IEnumerable<string> additionalModulePathsExclude = GetAdditionalModulePathsExclude(
                coverageProject.ExcludedReferencedProjects,
                coverageProject.TestDllFile,
                projectSettings.IncludeTestAssembly);

            IEnumerable<string> additionalModulePathsInclude = GetAdditionalModulePathsInclude(
                HasIncludes(coverageProject.Settings.ModulePathsInclude, coverageProject.IncludedReferencedProjects),
                coverageProject.IncludedReferencedProjects,
                coverageProject.TestDllFile,
                projectSettings.IncludeTestAssembly);

            var settings = new CombinedIncludesExcludesOptions(projectSettings, additionalModulePathsInclude, additionalModulePathsExclude);
            return new RunSettingsTemplateReplacements(settings, coverageProject.CoverageOutputFolder, projectSettings.Enabled.ToString(), testAdapter);
        }
    }
}