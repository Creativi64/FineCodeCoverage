using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using FineCodeCoverage.Core.Utilities;
using FineCodeCoverage.Engine.FileSynchronization;
using FineCodeCoverage.Options;
using Microsoft.VisualStudio.Shell;
using Task = System.Threading.Tasks.Task;

namespace FineCodeCoverage.Engine.Model
{
    internal class CoverageProject : ICoverageProject
    {
        private readonly IOptionsProvider<OutputOptions> _outputOptionsProvider;
        private readonly IFileSynchronizationUtil _fileSynchronizationUtil;
        private readonly ICoverageProjectSettingsManager _settingsManager;
        private readonly IReferencedProjectsHelper _referencedProjectsHelper;
        private XElement _projectFileXElement;
        private ICoverageSettings _settings;
        private string _targetFramework;
        private readonly string _fccFolderName = "fine-code-coverage";
        private readonly string _buildOutputFolderName = "build-output";
        private string _buildOutputPath;
        private string BuildOutputPath
        {
            get
            {
                if (this._buildOutputPath == null)
                {
                    bool adjacentBuildOutput = this._outputOptionsProvider.Get().AdjacentBuildOutput;
                    if (adjacentBuildOutput)
                    {
                        var projectOutputDirectory = new DirectoryInfo(this.ProjectOutputFolder);
                        string projectOutputDirectoryName = projectOutputDirectory.Name;
                        string containingDirectoryPath = projectOutputDirectory.Parent.FullName;
                        this._buildOutputPath = Path.Combine(containingDirectoryPath, $"{this._fccFolderName}-{projectOutputDirectoryName}");
                    }
                    else
                    {
                        this._buildOutputPath = Path.Combine(this.FCCOutputFolder, this._buildOutputFolderName);
                    }
                }

                return this._buildOutputPath;
            }
        }

        private readonly string _coverageToolOutputFolderName = "coverage-tool-output";
        private bool? _isDotNetSdkStyle;

        public CoverageProject(
            IOptionsProvider<OutputOptions> outputOptionsProvider,
            IFileSynchronizationUtil fileSynchronizationUtil,
            ICoverageProjectSettingsManager settingsManager,
            IReferencedProjectsHelper referencedProjectsHelper)
        {
            this._outputOptionsProvider = outputOptionsProvider;
            this._fileSynchronizationUtil = fileSynchronizationUtil;
            this._settingsManager = settingsManager;
            this._referencedProjectsHelper = referencedProjectsHelper;
        }

        public string FCCOutputFolder => Path.Combine(this.ProjectOutputFolder, this._fccFolderName);

        public bool IsDotNetSdkStyle()
        {
            if (this._isDotNetSdkStyle.HasValue)
            {
                return this._isDotNetSdkStyle.Value;
            }

            this._isDotNetSdkStyle = this.ProjectFileXElement
            .DescendantsAndSelf()
            .Any(x =>
                IsRootProjectElementWithSdkAttribute(x) ||
                IsRootProjectElementSdkElementChild(x) ||
                IsRootImportElementWithSdkAttribute(x));

            return this._isDotNetSdkStyle.Value;
        }

        private static bool HasSdkAttribute(XElement x)
            => x.Attributes().Any(IsSdkAttribute);

        private static bool IsSdkAttribute(XAttribute attr)
            => attr.Name.LocalName.Equals("Sdk", StringComparison.OrdinalIgnoreCase);
        private static bool IsRootImportElementWithSdkAttribute(XElement x)
            => x?.Name?.LocalName?.Equals("Import", StringComparison.OrdinalIgnoreCase) == true &&
                x?.Parent?.Name?.LocalName?.Equals("Project", StringComparison.OrdinalIgnoreCase) == true &&
                x?.Parent?.Parent == null && HasSdkAttribute(x);

        private static bool IsRootProjectElementSdkElementChild(XElement x)
            => x?.Name?.LocalName?.Equals("Sdk", StringComparison.OrdinalIgnoreCase) == true &&
            x?.Parent?.Name?.LocalName?.Equals("Project", StringComparison.OrdinalIgnoreCase) == true &&
            x?.Parent?.Parent == null;

        private static bool IsRootProjectElementWithSdkAttribute(XElement x)
            => x?.Name?.LocalName?.Equals("Project", StringComparison.OrdinalIgnoreCase) == true &&
                x?.Parent == null && HasSdkAttribute(x);

        public string TestDllFile { get; set; }
        public string ProjectOutputFolder => Path.GetDirectoryName(this.TestDllFile);
        public string FailureDescription { get; set; }
        public string FailureStage { get; set; }
        public bool HasFailed => !string.IsNullOrWhiteSpace(this.FailureStage) || !string.IsNullOrWhiteSpace(this.FailureDescription);
        public string ProjectFilePath { get; set; }
        public Guid Id { get; set; }
        public string ProjectName { get; set; }
        public string CoverageOutputFile => Path.Combine(this.CoverageOutputFolder, $"{this.ProjectName}.coverage.xml");

        public ICoverageSettings Settings
        {
            get
            {
                if (this._settings == null)
                {
#pragma warning disable VSTHRD102 // Implement internal logic asynchronously
                    this._settings = ThreadHelper.JoinableTaskFactory.Run(
                        async () => await this._settingsManager.GetSettingsAsync(this)
                    );
#pragma warning restore VSTHRD102 // Implement internal logic asynchronously
                }

                return this._settings;
            }
        }
        public string CoverageOutputFolder { get; set; }
        public string DefaultCoverageOutputFolder => Path.Combine(this.FCCOutputFolder, this._coverageToolOutputFolderName);

        public XElement ProjectFileXElement => this._projectFileXElement ??
            (this._projectFileXElement = LinqToXmlUtil.Load(this.ProjectFilePath, true));

        public List<IReferencedProject> ExcludedReferencedProjects { get; } = new List<IReferencedProject>();
        public List<IReferencedProject> IncludedReferencedProjects { get; set; } = new List<IReferencedProject>();
        public bool Is64Bit { get; set; }
        public string RunSettingsFile { get; set; }
        public bool IsDotNetFramework { get; private set; }
        public string TargetFramework
        {
            get => this._targetFramework;
            set
            {
                this._targetFramework = value;
                switch (this._targetFramework)
                {
                    case "Framework35":
                    case "Framework40":
                    case "Framework45":
                        this.IsDotNetFramework = true;
                        break;
                    case "FrameworkCore10":
                    case "FrameworkUap10":
                    case "None":
                        break;
                }
            }
        }

        public async Task StepAsync(string stepName, Func<ICoverageProject, Task> action)
        {
            if (this.HasFailed)
            {
                return;
            }

            try
            {
                await action(this);
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception exception)
            {
                this.FailureStage = stepName;
                this.FailureDescription = exception.ToString();
            }
        }

        public async Task<CoverageProjectFileSynchronizationDetails> PrepareForCoverageAsync(CancellationToken cancellationToken, bool synchronizeBuildOuput = true)
        {
            cancellationToken.ThrowIfCancellationRequested();
            this.EnsureDirectories();

            cancellationToken.ThrowIfCancellationRequested();
            this.CleanFCCDirectory();

            CoverageProjectFileSynchronizationDetails synchronizationDetails = null;
            if (synchronizeBuildOuput)
            {
                cancellationToken.ThrowIfCancellationRequested();
                synchronizationDetails = this.SynchronizeBuildOutput();
            }

            cancellationToken.ThrowIfCancellationRequested();
            await this.SetIncludedExcludedReferencedProjectsAsync();

            return synchronizationDetails;
        }

        private async Task SetIncludedExcludedReferencedProjectsAsync()
        {
            List<IExcludableReferencedProject> referencedProjects = await this._referencedProjectsHelper.GetReferencedProjectsAsync(this.ProjectFilePath, () => this.ProjectFileXElement);
            this.SetExcludedReferencedProjects(referencedProjects);
            this.SetIncludedReferencedProjects(referencedProjects);
        }

        private void SetIncludedReferencedProjects(List<IExcludableReferencedProject> referencedProjects)
        {
            if (!this.Settings.IncludeReferencedProjects)
            {
                return;
            }

            this.IncludedReferencedProjects = new List<IReferencedProject>(referencedProjects);
        }

        private void SetExcludedReferencedProjects(List<IExcludableReferencedProject> referencedProjects)
        {
            foreach (IExcludableReferencedProject referencedProject in referencedProjects)
            {
                if (referencedProject.ExcludeFromCodeCoverage)
                {
                    this.ExcludedReferencedProjects.Add(referencedProject);
                }
            }
        }

        private void EnsureDirectories()
        {
            this.EnsureFccDirectory();
            this.EnsureBuildOutputDirectory();
            this.EnsureEmptyOutputFolder();
        }

        private void EnsureFccDirectory() => CreateIfDoesNotExist(this.FCCOutputFolder);

        private void EnsureBuildOutputDirectory() => CreateIfDoesNotExist(this.BuildOutputPath);

        private static void CreateIfDoesNotExist(string path)
        {
            if (Directory.Exists(path))
            {
                return;
            }

            _ = Directory.CreateDirectory(path);
        }

        private void EnsureEmptyOutputFolder()
        {
            var directoryInfo = new DirectoryInfo(this.CoverageOutputFolder);
            if (directoryInfo.Exists)
            {
                foreach (FileInfo file in directoryInfo.GetFiles())
                {
                    file.TryDelete();
                }

                foreach (DirectoryInfo subDir in directoryInfo.GetDirectories())
                {
                    subDir.TryDelete(true);
                }
            }
            else
            {
                _ = Directory.CreateDirectory(this.CoverageOutputFolder);
            }
        }
        private void CleanFCCDirectory()
        {
            var exclusions = new List<string> { this._buildOutputFolderName, this._coverageToolOutputFolderName };
            var fccDirectory = new DirectoryInfo(this.FCCOutputFolder);

            fccDirectory.EnumerateFileSystemInfos().AsParallel().ForAll(fileOrDirectory =>
               {
                   if (exclusions.Contains(fileOrDirectory.Name))
                   {
                       return;
                   }

                   try
                   {
                       if (fileOrDirectory is FileInfo)
                       {
                           fileOrDirectory.Delete();
                       }
                       else
                       {
                           (fileOrDirectory as DirectoryInfo)?.Delete(true);
                       }
                   }
                   catch { }
               });

        }

        private CoverageProjectFileSynchronizationDetails SynchronizeBuildOutput()
        {
            DateTime start = DateTime.Now;
            List<string> logs = this._fileSynchronizationUtil.Synchronize(this.ProjectOutputFolder, this.BuildOutputPath, this._fccFolderName);
            TimeSpan duration = DateTime.Now - start;
            this.TestDllFile = Path.Combine(this.BuildOutputPath, Path.GetFileName(this.TestDllFile));
            return new CoverageProjectFileSynchronizationDetails
            {
                Logs = logs,
                Duration = duration
            };
        }
    }
}