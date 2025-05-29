using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using FineCodeCoverage.Engine.Model;

namespace FineCodeCoverage.Engine.Coverlet
{
    [Export(typeof(ICoverletExeArgumentsProvider))]
    internal class CoverletExeArgumentsProvider : ICoverletExeArgumentsProvider
    {
        private IEnumerable<string> SanitizeExcludesByAttribute(string[] excludes)
        {
            return (excludes ?? new string[0])
                .Where(x => x != null)
                .Select(x => x.Trim(' ', '\'', '\"'))
                .Where(x => !string.IsNullOrWhiteSpace(x));
        }

        private static IEnumerable<string> SantitizeExcludeInclude(string[] excludesOrIncludes)
        {
            return (excludesOrIncludes ?? new string[0]).Where(x => !string.IsNullOrWhiteSpace(x)).Select(value =>
            {
                return value.Replace("\"", "\\\"").Trim(' ', '\'');
            });
        }

        private static void AddExcludesOrIncludes(List<string> coverletSettings, IEnumerable<string> excludesOrIncludes, bool isInclude)
        {
            foreach (string value in excludesOrIncludes)
            {
                coverletSettings.Add($@"--{(isInclude ? "include" : "exclude")} ""{value}""");
            }
        }

        private static IEnumerable<string> AddTestAssemblyIfNecessary(
            IEnumerable<string> projectIncludes,
            IEnumerable<string> includes,
            string projectName)
        {
            bool hasIncludes = projectIncludes.Any() || includes.Any();
            if (!hasIncludes)
            {
                return projectIncludes;
            }
            return projectIncludes.Concat(new string[] { projectName });
        }


        private static void AddProjectExcludesOrIncludes(List<string> coverletSettings, IEnumerable<string> excludesOrIncludes, bool isInclude)
        {
            AddExcludesOrIncludes(coverletSettings, excludesOrIncludes.Select(excludeOrInclude => $"[{excludeOrInclude}]*"), isInclude);
        }

        private static void AddExcludesIncludes(List<string> coverletSettings, ICoverageProject project)
        {
            AddExcludesOrIncludes(coverletSettings, SantitizeExcludeInclude(project.Settings.Exclude), false);
            AddProjectExcludesOrIncludes(coverletSettings, project.ExcludedReferencedProjects.Select(rp => rp.AssemblyName), false);
            IEnumerable<string> includes = SantitizeExcludeInclude(project.Settings.Include);
            AddExcludesOrIncludes(coverletSettings, includes, true);
            IEnumerable<string> projectIncludes = project.IncludedReferencedProjects.Select(rp => rp.AssemblyName);
            if (project.Settings.IncludeTestAssembly)
            {
                projectIncludes = AddTestAssemblyIfNecessary(projectIncludes, includes, project.ProjectName);
            }
            AddProjectExcludesOrIncludes(coverletSettings, projectIncludes, true);
        }

        public List<string> GetArguments(ICoverageProject project)
        {
            List<string> coverletSettings = new List<string>();

            coverletSettings.Add($@"""{project.TestDllFile}""");

            coverletSettings.Add(@"--format ""cobertura""");

            AddExcludesIncludes(coverletSettings, project);

            foreach (string value in (project.Settings.ExcludeByFile ?? new string[0]).Where(x => !string.IsNullOrWhiteSpace(x)))
            {
                coverletSettings.Add($@"--exclude-by-file ""{value.Replace("\"", "\\\"").Trim(' ', '\'')}""");
            }

            foreach (string value in SanitizeExcludesByAttribute(project.Settings.ExcludeByAttribute).Select(EnsureAttributeTypeUnqualified))
            {
                string withoutAttributeBrackets = value.Trim('[', ']');
                coverletSettings.Add($"--exclude-by-attribute {value}");
            }

            if (project.Settings.IncludeTestAssembly)
            {
                coverletSettings.Add("--include-test-assembly");
            }

            coverletSettings.Add(@"--target ""dotnet""");

            coverletSettings.Add("--threshold-type line");

            coverletSettings.Add("--threshold-stat total");

            coverletSettings.Add("--threshold 0");

            coverletSettings.Add($@"--output ""{project.CoverageOutputFile}""");

            string runSettings = !string.IsNullOrWhiteSpace(project.RunSettingsFile) ? $@"--settings """"{project.RunSettingsFile}""""" : default;
            coverletSettings.Add($@"--targetargs ""test  """"{project.TestDllFile}"""" --nologo --blame {runSettings} --results-directory """"{project.CoverageOutputFolder}"""" --diag """"{project.CoverageOutputFolder}/diagnostics.log""""  """);

            return coverletSettings;
        }

        private string EnsureAttributeTypeUnqualified(string attributeType) => attributeType.Split('.').Last();
    }

}
