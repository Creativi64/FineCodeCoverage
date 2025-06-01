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
        private readonly IOptionsProvider<OutputOptions> outputOptionsProvider;
        private readonly IFileSynchronizationUtil fileSynchronizationUtil;
        private readonly ICoverageProjectSettingsManager settingsManager;
        private readonly IReferencedProjectsHelper referencedProjectsHelper;
        private XElement projectFileXElement;
        private ICoverageSettings settings;
        private string targetFramework;
        private readonly string fccFolderName = "fine-code-coverage";
        private readonly string buildOutputFolderName = "build-output";
        private string buildOutputPath;
        private string BuildOutputPath
        {
            get
            {
                if (this.buildOutputPath == null)
                {
                    bool adjacentBuildOutput = this.outputOptionsProvider.Get().AdjacentBuildOutput;
                    if (adjacentBuildOutput)
                    {
                        // Net framework - Debug | Debug-NET45
                        // SDK style - Debug/netcoreapp3.1 etc
                        var projectOutputDirectory = new DirectoryInfo(this.ProjectOutputFolder);
                        string projectOutputDirectoryName = projectOutputDirectory.Name;
                        string containingDirectoryPath = projectOutputDirectory.Parent.FullName;
                        this.buildOutputPath = Path.Combine(containingDirectoryPath, $"{this.fccFolderName}-{projectOutputDirectoryName}");
                    }
                    else
                    {
                        this.buildOutputPath = Path.Combine(this.FCCOutputFolder, this.buildOutputFolderName);
                    }
                }

                return this.buildOutputPath;
            }
        }

        private readonly string coverageToolOutputFolderName = "coverage-tool-output";
        private bool? isDotNetSdkStyle;

        public CoverageProject(
            IOptionsProvider<OutputOptions> outputOptionsProvider,
            IFileSynchronizationUtil fileSynchronizationUtil,
            ICoverageProjectSettingsManager settingsManager,
            IReferencedProjectsHelper referencedProjectsHelper)
        {
            this.outputOptionsProvider = outputOptionsProvider;
            this.fileSynchronizationUtil = fileSynchronizationUtil;
            this.settingsManager = settingsManager;
            this.referencedProjectsHelper = referencedProjectsHelper;
        }

        public string FCCOutputFolder => Path.Combine(this.ProjectOutputFolder, this.fccFolderName);

        public bool IsDotNetSdkStyle()
        {
            if (this.isDotNetSdkStyle.HasValue)
            {
                return this.isDotNetSdkStyle.Value;
            }

            this.isDotNetSdkStyle = this.ProjectFileXElement
            .DescendantsAndSelf()
            .Any(x =>
                //https://docs.microsoft.com/en-us/visualstudio/msbuild/how-to-use-project-sdk?view=vs-2019
                IsRootProjectElementWithSdkAttribute(x) ||
                IsRootProjectElementSdkElementChild(x) ||
                IsRootImportElementWithSdkAttribute(x));

            return this.isDotNetSdkStyle.Value;

            bool HasSdkAttribute(XElement x) => x?.Attributes()?
                .FirstOrDefault(attr => attr?.Name?.LocalName?.Equals("Sdk", StringComparison.OrdinalIgnoreCase) == true) != null;

            bool IsRootProjectElementWithSdkAttribute(XElement x)
                => x?.Name?.LocalName?.Equals("Project", StringComparison.OrdinalIgnoreCase) == true &&
                    x?.Parent == null && HasSdkAttribute(x);

            bool IsRootProjectElementSdkElementChild(XElement x)
                => x?.Name?.LocalName?.Equals("Sdk", StringComparison.OrdinalIgnoreCase) == true &&
                    x?.Parent?.Name?.LocalName?.Equals("Project", StringComparison.OrdinalIgnoreCase) == true &&
                    x?.Parent?.Parent == null;

            bool IsRootImportElementWithSdkAttribute(XElement x)
                => x?.Name?.LocalName?.Equals("Import", StringComparison.OrdinalIgnoreCase) == true &&
                    x?.Parent?.Name?.LocalName?.Equals("Project", StringComparison.OrdinalIgnoreCase) == true &&
                    x?.Parent?.Parent == null && HasSdkAttribute(x);
        }

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
                if (this.settings == null)
                {
#pragma warning disable VSTHRD102 // Implement internal logic asynchronously
                    this.settings = ThreadHelper.JoinableTaskFactory.Run(
                        async () => await this.settingsManager.GetSettingsAsync(this)
                    );
#pragma warning restore VSTHRD102 // Implement internal logic asynchronously
                }

                return this.settings;
            }
        }
        public string CoverageOutputFolder { get; set; }
        public string DefaultCoverageOutputFolder => Path.Combine(this.FCCOutputFolder, this.coverageToolOutputFolderName);

        public XElement ProjectFileXElement => this.projectFileXElement ??
            (this.projectFileXElement = LinqToXmlUtil.Load(this.ProjectFilePath, true));

        public List<IReferencedProject> ExcludedReferencedProjects { get; } = new List<IReferencedProject>();
        public List<IReferencedProject> IncludedReferencedProjects { get; set; } = new List<IReferencedProject>();
        public bool Is64Bit { get; set; }
        public string RunSettingsFile { get; set; }
        public bool IsDotNetFramework { get; private set; }
        public string TargetFramework
        {
            get => this.targetFramework;
            set
            {
                this.targetFramework = value;
                switch (this.targetFramework)
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
            List<IExcludableReferencedProject> referencedProjects = await this.referencedProjectsHelper.GetReferencedProjectsAsync(this.ProjectFilePath, () => this.ProjectFileXElement);
            this.SetExcludedReferencedProjects(referencedProjects);
            this.SetIncludedReferencedProjects(referencedProjects);
        }

        private void SetIncludedReferencedProjects(List<IExcludableReferencedProject> referencedProjects)
        {
            if (this.Settings.IncludeReferencedProjects)
            {
                this.IncludedReferencedProjects = new List<IReferencedProject>(referencedProjects);
            }
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
            if (!Directory.Exists(path))
            {
                _ = Directory.CreateDirectory(path);
            }
        }

        /// <summary>
        /// Delete all files and sub-directories from the output folder if it exists, or creates the directory if it does not exist.
        /// </summary>
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
            var exclusions = new List<string> { this.buildOutputFolderName, this.coverageToolOutputFolderName };
            var fccDirectory = new DirectoryInfo(this.FCCOutputFolder);

            fccDirectory.EnumerateFileSystemInfos().AsParallel().ForAll(fileOrDirectory =>
               {
                   if (!exclusions.Contains(fileOrDirectory.Name))
                   {
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
                   }
               });

        }

        private CoverageProjectFileSynchronizationDetails SynchronizeBuildOutput()
        {
            DateTime start = DateTime.Now;
            List<string> logs = this.fileSynchronizationUtil.Synchronize(this.ProjectOutputFolder, this.BuildOutputPath, this.fccFolderName);
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
