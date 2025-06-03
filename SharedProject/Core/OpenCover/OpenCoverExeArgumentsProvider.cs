using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using FineCodeCoverage.Engine.Model;
using FineCodeCoverage.Options;

namespace FineCodeCoverage.Engine.OpenCover
{
    [Export(typeof(IOpenCoverExeArgumentsProvider))]
    internal class OpenCoverExeArgumentsProvider : IOpenCoverExeArgumentsProvider
    {
        private enum Delimiter { Semicolon, Space }

        private static void AddFilter(ICoverageProject project, List<string> opencoverSettings)
        {
            IEnumerable<string> includes = SanitizeExcludesOrIncludes(project.Settings.Include);
            var excludes = SanitizeExcludesOrIncludes(project.Settings.Exclude).ToList();

            List<string> includedModules = project.IncludedReferencedProjects.ConvertAll(rp => rp.AssemblyName);
            if (project.Settings.IncludeTestAssembly && (includes.Any() || project.IncludedReferencedProjects.Any()))
            {
                includedModules.Add(project.ProjectName);
            }

            List<string> includeFilters = GetExcludesOrIncludes(includes, includedModules, true);
            List<string> excludeFilters = GetExcludesOrIncludes(excludes, project.ExcludedReferencedProjects.Select(rp => rp.AssemblyName), false);
            AddIncludeAllIfExcludingWithoutIncludes();
            var filters = includeFilters.Concat(excludeFilters).ToList();
            SafeAddToSettingsDelimitedIfAny(opencoverSettings, "filter", filters, Delimiter.Space);

            void AddIncludeAllIfExcludingWithoutIncludes()
            {
                if (excludeFilters.Any() && !includeFilters.Any())
                {
                    includeFilters.Add("+[*]*");
                }
            }

            List<string> GetExcludesOrIncludes(
                IEnumerable<string> excludesOrIncludes, IEnumerable<string> moduleExcludesOrIncludes, bool isInclude)
            {
                var excludeOrIncludeFilters = new List<string>();
                string includeExcludeSymbol = isInclude ? "+" : "-";

                foreach (string value in excludesOrIncludes)
                {
                    excludeOrIncludeFilters.Add($"{includeExcludeSymbol}{value}");
                }

                foreach (string moduleExcludeOrInclude in moduleExcludesOrIncludes)
                {
                    excludeOrIncludeFilters.Add($"{includeExcludeSymbol}[{moduleExcludeOrInclude}]*");
                }

                return excludeOrIncludeFilters.Distinct().ToList();
            }
        }

        private static IEnumerable<string> SanitizeExcludesOrIncludes(IEnumerable<string> excludesOrIncludes)
            => (excludesOrIncludes ?? Array.Empty<string>())
                .Where(x => x != null)
                .Select(x => x.Trim(' ', '\'', '\"'))
                .Where(x => !string.IsNullOrWhiteSpace(x));

        private static void SafeAddToSettingsDelimitedIfAny(
            List<string> opencoverSettings,
            string settingName,
            IEnumerable<string> settings,
            Delimiter delimiter = Delimiter.Semicolon
        )
        {
            if (settings.Any())
            {
                string delimit = delimiter == Delimiter.Semicolon ? ";" : " ";
                opencoverSettings.Add($@"""-{settingName}:{string.Join(delimit, settings)}""");
            }
        }

        private static void AddExcludeByFile(ICoverageProject project, List<string> opencoverSettings)
        {
            var excludes = SanitizeExcludesOrIncludes(project.Settings.ExcludeByFile).ToList();
            SafeAddToSettingsDelimitedIfAny(opencoverSettings, "excludebyfile", excludes);
        }

        private void AddExcludeByAttribute(ICoverageProject project, List<string> opencoverSettings)
        {
            var excludeFromCodeCoverageAttributes = new List<string>()
                {
					// coverlet knows these implicitly
					"ExcludeFromCoverage",
                    "ExcludeFromCodeCoverage"
                };

            IEnumerable<string> excludes = SanitizeExcludesOrIncludes(project.Settings.ExcludeByAttribute)
                .Concat(excludeFromCodeCoverageAttributes)
                .SelectMany(exclude => new[] { exclude, GetAlternateName(exclude) })
                .OrderBy(exclude => exclude)
                .Select(WildCardIfShortName);

            SafeAddToSettingsDelimitedIfAny(opencoverSettings, "excludebyattribute", excludes);

            string WildCardIfShortName(string exclude) => exclude.IndexOf(".") == -1 ? $"*.{exclude}" : exclude;

            string GetAlternateName(string exclude)
            {
                if (exclude.EndsWith("Attribute"))
                {
                    // remove 'Attribute' suffix
                    return exclude.Substring(0, exclude.Length - 9);
                }
                else
                {
                    // add 'Attribute' suffix
                    return $"{exclude}Attribute";
                }
            }
        }

        private static string GetTargetArgs(ICoverageProject project)
        {
            string runSettings = !string.IsNullOrWhiteSpace(project.RunSettingsFile) ? $" /Settings:{CommandLineArgumentsHelper.AddEscapeQuotes(project.RunSettingsFile)}" : default;
            string openCoverTargetArgs = project.Settings.OpenCoverTargetArgs;
            string additionalTargetArgs = !string.IsNullOrWhiteSpace(openCoverTargetArgs) ? $" {openCoverTargetArgs}" : default;
            return $@"""-targetargs:{CommandLineArgumentsHelper.AddEscapeQuotes(project.TestDllFile)}{runSettings}{additionalTargetArgs}""";
        }

        private static void AddTargetAndTargetArgs(ICoverageProject project, List<string> opencoverSettings, string msTestPlatformExePath)
        {
            string target = !string.IsNullOrWhiteSpace(project.Settings.OpenCoverTarget) ? project.Settings.OpenCoverTarget : msTestPlatformExePath;
            opencoverSettings.Add(CommandLineArgumentsHelper.AddQuotes($"-target:{target}"));
            opencoverSettings.Add(GetTargetArgs(project));
        }

        private static string GetRegister(ICoverageProject project)
        {
            OpenCoverRegister openCoverRegister = project.Settings.OpenCoverRegister;
            return openCoverRegister == OpenCoverRegister.Default
                ? $":path{(project.Is64Bit ? "64" : "32")}"
                : openCoverRegister == OpenCoverRegister.NoArg ? "" :
                $":{project.Settings.OpenCoverRegister.ToString().ToLower()}";
        }

        public List<string> Provide(ICoverageProject project, string msTestPlatformExePath)
        {
            var opencoverSettings = new List<string>();
            AddTargetAndTargetArgs(project, opencoverSettings, msTestPlatformExePath);

            opencoverSettings.Add(CommandLineArgumentsHelper.AddQuotes($"-output:{project.CoverageOutputFile}"));

            AddFilter(project, opencoverSettings);
            AddExcludeByFile(project, opencoverSettings);
            this.AddExcludeByAttribute(project, opencoverSettings);
            opencoverSettings.Add($"-register{GetRegister(project)}");
            opencoverSettings.Add("-mergebyhash");
            opencoverSettings.Add("-hideskipped:all");

            return opencoverSettings;
        }
    }
}