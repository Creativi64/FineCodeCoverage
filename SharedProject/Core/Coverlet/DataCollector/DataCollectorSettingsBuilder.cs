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
            this.GenerateRunSettings();
            IEnumerable<string> args = new List<string>
            {
                this.ProjectDll,
                this.Blame,
                this.NoLogo,
                this.Diagnostics,
                this.RunSettings,
                this.ResultsDirectory
            }.Where(a => !string.IsNullOrEmpty(a));
            return string.Join(" ", args);
        }

        #region run settings xml generation
        private void GenerateRunSettings()
        {
            XDocument runSettingsDocument = this._existingRunSettings == null ? this.GenerateFullRunSettings() : this.GenerateRunSettingsFromExisting();
            runSettingsDocument.Save(this._generatedRunSettingsPath);
        }

        private XDocument GenerateFullRunSettings() => new XDocument(
            new XElement("RunSettings", this.DataCollectionRunSettings())
        );

        private XElement DataCollectionRunSettings() => new XElement("DataCollectionRunSettings", this.DataCollectors());

        private XElement DataCollectors() => new XElement("DataCollectors", this.GenerateDataCollectorElement());

        private XDocument GenerateRunSettingsFromExisting()
        {
            var existingRunSettingsDocument = XDocument.Load(this._existingRunSettings);
            XElement existingRunSettingsElement = existingRunSettingsDocument.Root;
            XElement dataCollectionRunSettings = existingRunSettingsElement.Element("DataCollectionRunSettings");
            if (dataCollectionRunSettings == null)
            {
                existingRunSettingsElement.Add(this.DataCollectionRunSettings());
            }
            else
            {
                XElement dataCollectors = dataCollectionRunSettings.Element("DataCollectors");
                if (dataCollectors == null)
                {
                    dataCollectionRunSettings.Add(this.DataCollectors());
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
                    XElement newCoverletCollector = this.GenerateDataCollectorElement();
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
                {GetElementIfNotNull("Format", this.Format)}
                {GetElementIfNotNull("Exclude", this.Exclude)}
                {GetElementIfNotNull("Include", this.Include)}
                {GetElementIfNotNull("ExcludeByAttribute", this.ExcludeByAttribute)}
                {GetElementIfNotNull("ExcludeByFile", this.ExcludeByFile)}
                {GetElementIfNotNull("IncludeDirectory", this.IncludeDirectory)}
                {GetElementIfNotNull("SingleHit", this.SingleHit)}
                {GetElementIfNotNull("UseSourceLink", this.UseSourceLink)}
                {GetElementIfNotNull("IncludeTestAssembly", this.IncludeTestAssembly)}
                {GetElementIfNotNull("SkipAutoProps", this.SkipAutoProps)}
</Configuration>
";

            return new XElement("DataCollector", new XAttribute("friendlyName", "XPlat Code Coverage"),
                XElement.Parse(configurationElement));

        }
        #endregion

        internal static string Quote(string settings) => $@"""{settings}""";

        #region With args
        public void WithBlame() => this.Blame = "--blame";

        public void WithDiagnostics(string logPath) => this.Diagnostics = $"--diag {Quote(logPath)}";

        public void WithNoLogo() => this.NoLogo = "--nologo";

        public void WithProjectDll(string projectDll) => this.ProjectDll = Quote(projectDll);

        public void WithResultsDirectory(string resultsDirectory)
            => this.ResultsDirectory = $"--results-directory {Quote(resultsDirectory)}";

        public void Initialize(bool runSettingsOnly, string runSettingsPath, string generatedRunSettingsPath)
        {
            this._runSettingsOnly = runSettingsOnly;
            this._generatedRunSettingsPath = generatedRunSettingsPath;
            this._existingRunSettings = runSettingsPath;
            this.RunSettings = $"--settings {Quote(generatedRunSettingsPath)}";
        }
        #endregion

        #region existing run settings or options
        private string RunSettingsOrProject(string[] project, string runSettings)
        {
            string DelimitProject() => project == null ? null : string.Join(",", project);

            return this._existingRunSettings == null ?
                DelimitProject() :
                runSettings ?? (!this._runSettingsOnly ? DelimitProject() : null);
        }

        public void WithExclude(string[] projectExclude, string runSettingsExclude)
            => this.Exclude = this.RunSettingsOrProject(projectExclude, runSettingsExclude);

        public void WithExcludeByAttribute(string[] projectExcludeByAttribute, string runSettingsExcludeByAttribute)
        {
            if (runSettingsExcludeByAttribute != null)
            {
                runSettingsExcludeByAttribute = string.Join(",", runSettingsExcludeByAttribute.Split(',').Select(Unqualify));
            }

            this.ExcludeByAttribute = this.RunSettingsOrProject(
                projectExcludeByAttribute?.Select(Unqualify).ToArray(),
                runSettingsExcludeByAttribute);

            string Unqualify(string excludeByAttribute) => excludeByAttribute.Split('.').Last();
        }

        public void WithExcludeByFile(string[] projectExcludeByFile, string runSettingsExcludeByFile)
            => this.ExcludeByFile = this.RunSettingsOrProject(projectExcludeByFile, runSettingsExcludeByFile);

        public void WithInclude(string[] projectInclude, string runSettingsInclude)
            => this.Include = this.RunSettingsOrProject(projectInclude, runSettingsInclude);

        public void WithIncludeTestAssembly(bool projectIncludeTestAssembly, string runSettingsIncludeTestAssembly)
        {
            string ProjectInclude() => projectIncludeTestAssembly.ToString().ToLower();

            string includeTestAssembly = null;
            if (this._existingRunSettings == null)
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
                    if (!this._runSettingsOnly) // default true
                    {
                        includeTestAssembly = ProjectInclude();
                    }
                }
            }

            this.IncludeTestAssembly = includeTestAssembly;
        }
        #endregion

        #region Coverlet Collector specific
        public void WithIncludeDirectory(string includeDirectory) => this.IncludeDirectory = includeDirectory;

        public void WithSingleHit(string singleHit) => this.SingleHit = singleHit;

        public void WithUseSourceLink(string useSourceLink) => this.UseSourceLink = useSourceLink;

        public void WithSkipAutoProps(string skipAutoProps) => this.SkipAutoProps = skipAutoProps;
        #endregion
    }
}