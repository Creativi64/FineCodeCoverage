using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Xml.Linq;

namespace FineCodeCoverage.Engine.Coverlet
{
    [Export(typeof(IDataCollectorSettingsBuilder))]
    internal class DataCollectorSettingsBuilder : IDataCollectorSettingsBuilder
    {
        private string _generatedRunSettingsPath;
        private string _existingRunSettings;
        private bool _runSettingsOnly;

        #region Arguments
        internal string ProjectDll { get; set; }

        internal string Blame { get; set; }

        internal string NoLogo { get; set; }

        internal string Diagnostics { get; set; }

        internal string RunSettings { get; set; }

        internal string ResultsDirectory { get; set; }
        #endregion

        #region DataCollector Configuration

        internal string Format { get; set; } = "cobertura";

        internal string Exclude { get; set; }

        internal string ExcludeByAttribute { get; set; }

        internal string ExcludeByFile { get; set; }

        internal string Include { get; set; }

        internal string IncludeTestAssembly { get; set; }

        internal string IncludeDirectory { get; set; }

        internal string SingleHit { get; set; }

        internal string SkipAutoProps { get; set; }

        internal string UseSourceLink { get; set; }
        #endregion

        public string Build()
        {
            GenerateRunSettings();
            IEnumerable<string> args = new List<string>
            {
                ProjectDll,
                Blame,
                NoLogo,
                Diagnostics,
                RunSettings,
                ResultsDirectory
            }.Where(a => !string.IsNullOrEmpty(a));
            return string.Join(" ", args);
        }

        #region run settings xml generation
        private void GenerateRunSettings()
        {
            XDocument runSettingsDocument = _existingRunSettings == null ? GenerateFullRunSettings() : GenerateRunSettingsFromExisting();
            runSettingsDocument.Save(_generatedRunSettingsPath);
        }

        private XDocument GenerateFullRunSettings() => new XDocument(
            new XElement("RunSettings", DataCollectionRunSettings())
        );

        private XElement DataCollectionRunSettings() => new XElement("DataCollectionRunSettings", DataCollectors());

        private XElement DataCollectors() => new XElement("DataCollectors", GenerateDataCollectorElement());

        private XDocument GenerateRunSettingsFromExisting()
        {
            var existingRunSettingsDocument = XDocument.Load(_existingRunSettings);
            XElement existingRunSettingsElement = existingRunSettingsDocument.Root;
            XElement dataCollectionRunSettings = existingRunSettingsElement.Element("DataCollectionRunSettings");
            if (dataCollectionRunSettings == null)
            {
                existingRunSettingsElement.Add(DataCollectionRunSettings());
            }
            else
            {
                XElement dataCollectors = dataCollectionRunSettings.Element("DataCollectors");
                if (dataCollectors == null)
                {
                    dataCollectionRunSettings.Add(DataCollectors());
                }
                else
                {
                    XElement coverletCollectorElement = dataCollectors.Elements("DataCollector").FirstOrDefault(e =>
                    {
                        XAttribute friendlyNameAttribute = e.Attribute("friendlyName");
                        return friendlyNameAttribute != null && string.Equals(
                            friendlyNameAttribute.Value,
                            "xplat code coverage",
                            System.StringComparison.OrdinalIgnoreCase);
                    });
                    XElement newCoverletCollector = GenerateDataCollectorElement();
                    if (coverletCollectorElement != null)
                    {
                        coverletCollectorElement.ReplaceWith(newCoverletCollector);
                    }
                    else
                    {
                        dataCollectors.Add(newCoverletCollector);
                    }
                }
            }

            return existingRunSettingsDocument;
        }

        private static string GetElementIfNotNull(string elementName, string value)
            => value == null ? "" : $"<{elementName}>{value}</{elementName}>";

        private XElement GenerateDataCollectorElement()
        {
            string configurationElement = $@"<Configuration>
                {GetElementIfNotNull("Format", Format)}
                {GetElementIfNotNull("Exclude", Exclude)}
                {GetElementIfNotNull("Include", Include)}
                {GetElementIfNotNull("ExcludeByAttribute", ExcludeByAttribute)}
                {GetElementIfNotNull("ExcludeByFile", ExcludeByFile)}
                {GetElementIfNotNull("IncludeDirectory", IncludeDirectory)}
                {GetElementIfNotNull("SingleHit", SingleHit)}
                {GetElementIfNotNull("UseSourceLink", UseSourceLink)}
                {GetElementIfNotNull("IncludeTestAssembly", IncludeTestAssembly)}
                {GetElementIfNotNull("SkipAutoProps", SkipAutoProps)}
</Configuration>
";

            return new XElement("DataCollector", new XAttribute("friendlyName", "XPlat Code Coverage"),
                XElement.Parse(configurationElement));

        }
        #endregion

        internal static string Quote(string settings) => $@"""{settings}""";

        #region With args
        public void WithBlame() => Blame = "--blame";

        public void WithDiagnostics(string logPath) => Diagnostics = $"--diag {Quote(logPath)}";

        public void WithNoLogo() => NoLogo = "--nologo";

        public void WithProjectDll(string projectDll) => ProjectDll = Quote(projectDll);

        public void WithResultsDirectory(string resultsDirectory)
            => ResultsDirectory = $"--results-directory {Quote(resultsDirectory)}";

        public void Initialize(bool runSettingsOnly, string runSettingsPath, string generatedRunSettingsPath)
        {
            _runSettingsOnly = runSettingsOnly;
            _generatedRunSettingsPath = generatedRunSettingsPath;
            _existingRunSettings = runSettingsPath;
            RunSettings = $"--settings {Quote(generatedRunSettingsPath)}";
        }
        #endregion

        #region existing run settings or options
        private string RunSettingsOrProject(string[] project, string runSettings)
        {
            string DelimitProject() => project == null ? null : string.Join(",", project);

            return _existingRunSettings == null ?
                DelimitProject() :
                runSettings ?? (!_runSettingsOnly ? DelimitProject() : null);
        }

        public void WithExclude(string[] projectExclude, string runSettingsExclude)
            => Exclude = RunSettingsOrProject(projectExclude, runSettingsExclude);

        public void WithExcludeByAttribute(string[] projectExcludeByAttribute, string runSettingsExcludeByAttribute)
        {
            if (runSettingsExcludeByAttribute != null)
            {
                runSettingsExcludeByAttribute = string.Join(",", runSettingsExcludeByAttribute.Split(',').Select(Unqualify));
            }

            ExcludeByAttribute = RunSettingsOrProject(
                projectExcludeByAttribute?.Select(Unqualify).ToArray(),
                runSettingsExcludeByAttribute);

            string Unqualify(string excludeByAttribute) => excludeByAttribute.Split('.').Last();
        }

        public void WithExcludeByFile(string[] projectExcludeByFile, string runSettingsExcludeByFile)
            => ExcludeByFile = RunSettingsOrProject(projectExcludeByFile, runSettingsExcludeByFile);

        public void WithInclude(string[] projectInclude, string runSettingsInclude)
            => Include = RunSettingsOrProject(projectInclude, runSettingsInclude);

        public void WithIncludeTestAssembly(bool projectIncludeTestAssembly, string runSettingsIncludeTestAssembly)
        {
            string ProjectInclude() => projectIncludeTestAssembly.ToString().ToLower();

            string includeTestAssembly = null;
            if (_existingRunSettings == null)
            {
                includeTestAssembly = ProjectInclude();
            }
            else
            {
                if (runSettingsIncludeTestAssembly != null)
                {
                    includeTestAssembly = runSettingsIncludeTestAssembly;
                }
                else
                {
                    if (!_runSettingsOnly) // default true
                    {
                        includeTestAssembly = ProjectInclude();
                    }
                }
            }

            IncludeTestAssembly = includeTestAssembly;
        }
        #endregion

        #region Coverlet Collector specific
        public void WithIncludeDirectory(string includeDirectory) => IncludeDirectory = includeDirectory;

        public void WithSingleHit(string singleHit) => SingleHit = singleHit;

        public void WithUseSourceLink(string useSourceLink) => UseSourceLink = useSourceLink;

        public void WithSkipAutoProps(string skipAutoProps) => SkipAutoProps = skipAutoProps;
        #endregion
    }
}
