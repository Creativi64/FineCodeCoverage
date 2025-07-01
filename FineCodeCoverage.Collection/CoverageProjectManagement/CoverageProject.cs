using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using FineCodeCoverage.Collection.CoverageProjectManagement.FileSynchronization;
using FineCodeCoverage.Collection.CoverageProjectManagement.ReferencedProjects;
using FineCodeCoverage.Core.Utilities;
using FineCodeCoverage.Engine.Model;
using FineCodeCoverage.Options;
using Task = System.Threading.Tasks.Task;

namespace FineCodeCoverage.Collection.CoverageProjectManagement
{
    public sealed class CoverageProject : ICoverageProject
    {
        private const string FCCFolderName = "fine-code-coverage";
        private const string BuildOutputFolderName = "build-output";
        private readonly IOptionsProvider<OutputOptions> _outputOptionsProvider;
        private readonly IFileSynchronizationUtil _fileSynchronizationUtil;
        private readonly ICoverageProjectSettingsManager _settingsManager;
        private readonly IReferencedProjectsHelper _referencedProjectsHelper;
        private XElement _projectFileXElement;
        private ICoverageSettings _settings;
        private string _targetFramework;
        private string _buildOutputPath;

        private string BuildOutputPath
        {
            get
            {
                if (_buildOutputPath == null)
                {
                    bool adjacentBuildOutput = _outputOptionsProvider.Get().AdjacentBuildOutput;
                    if (adjacentBuildOutput)
                    {
                        var projectOutputDirectory = new DirectoryInfo(ProjectOutputFolder);
                        string projectOutputDirectoryName = projectOutputDirectory.Name;
                        string containingDirectoryPath = projectOutputDirectory.Parent.FullName;
                        _buildOutputPath = Path.Combine(containingDirectoryPath, $"{FCCFolderName}-{projectOutputDirectoryName}");
                    }
                    else
                    {
                        _buildOutputPath = Path.Combine(FCCOutputFolder, BuildOutputFolderName);
                    }
                }

                return _buildOutputPath;
            }
        }

        private readonly string _coverageToolOutputFolderName = "coverage-tool-output";
        private bool? _isDotNetSdkStyle;

        public CoverageProject(
            IOptionsProvider<OutputOptions> outputOptionsProvider,
            IFileSynchronizationUtil fileSynchronizationUtil,
            ICoverageProjectSettingsManager settingsManager,
            IReferencedProjectsHelper referencedProjectsHelper
        )
        {
            _outputOptionsProvider = outputOptionsProvider;
            _fileSynchronizationUtil = fileSynchronizationUtil;
            _settingsManager = settingsManager;
            _referencedProjectsHelper = referencedProjectsHelper;
        }

        public string FCCOutputFolder => Path.Combine(ProjectOutputFolder, FCCFolderName);

        public bool IsDotNetSdkStyle()
        {
            if (_isDotNetSdkStyle.HasValue)
            {
                return _isDotNetSdkStyle.Value;
            }

            _isDotNetSdkStyle = ProjectFileXElement
            .DescendantsAndSelf()
            .Any(x =>
                IsRootProjectElementWithSdkAttribute(x) ||
                IsRootProjectElementSdkElementChild(x) ||
                IsRootImportElementWithSdkAttribute(x));

            return _isDotNetSdkStyle.Value;
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

        public string ProjectOutputFolder => Path.GetDirectoryName(TestDllFile);

        public string FailureDescription { get; set; }

        public string FailureStage { get; set; }

        public bool HasFailed => !string.IsNullOrWhiteSpace(FailureStage) || !string.IsNullOrWhiteSpace(FailureDescription);

        public string ProjectFilePath { get; set; }

        public Guid Id { get; set; }

        public string ProjectName { get; set; }

        public string CoverageOutputFile => Path.Combine(CoverageOutputFolder, $"{ProjectName}.coverage.xml");

        public ICoverageSettings Settings => _settings ?? (_settings = _settingsManager.GetSettings(this));

        public string CoverageOutputFolder { get; set; }

        public string DefaultCoverageOutputFolder => Path.Combine(FCCOutputFolder, _coverageToolOutputFolderName);

        public XElement ProjectFileXElement => _projectFileXElement ??
            (_projectFileXElement = LinqToXmlUtil.Load(ProjectFilePath, true));

        public List<IReferencedProject> ExcludedReferencedProjects { get; } = new List<IReferencedProject>();

        public List<IReferencedProject> IncludedReferencedProjects { get; set; } = new List<IReferencedProject>();

        public bool Is64Bit { get; set; }

        public string RunSettingsFile { get; set; }

        public bool IsDotNetFramework { get; private set; }

        public string TargetFramework
        {
            get => _targetFramework;
            set
            {
                _targetFramework = value;
                switch (_targetFramework)
                {
                    case "Framework35":
                    case "Framework40":
                    case "Framework45":
                        IsDotNetFramework = true;
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
            if (HasFailed)
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
                FailureStage = stepName;
                FailureDescription = exception.ToString();
            }
        }

        public async Task<CoverageProjectFileSynchronizationDetails> PrepareForCoverageAsync(CancellationToken cancellationToken, bool synchronizeBuildOuput = true)
        {
            cancellationToken.ThrowIfCancellationRequested();
            EnsureDirectories();

            cancellationToken.ThrowIfCancellationRequested();
            CleanFCCDirectory();

            CoverageProjectFileSynchronizationDetails synchronizationDetails = null;
            if (synchronizeBuildOuput)
            {
                cancellationToken.ThrowIfCancellationRequested();
                synchronizationDetails = SynchronizeBuildOutput();
            }

            cancellationToken.ThrowIfCancellationRequested();
            await SetIncludedExcludedReferencedProjectsAsync();

            return synchronizationDetails;
        }

        private async Task SetIncludedExcludedReferencedProjectsAsync()
        {
            List<IExcludableReferencedProject> referencedProjects = await _referencedProjectsHelper.GetReferencedProjectsAsync(ProjectFilePath, () => ProjectFileXElement);
            SetExcludedReferencedProjects(referencedProjects);
            SetIncludedReferencedProjects(referencedProjects);
        }

        private void SetIncludedReferencedProjects(List<IExcludableReferencedProject> referencedProjects)
        {
            if (!Settings.IncludeReferencedProjects)
            {
                return;
            }

            IncludedReferencedProjects = new List<IReferencedProject>(referencedProjects);
        }

        private void SetExcludedReferencedProjects(List<IExcludableReferencedProject> referencedProjects)
        {
            foreach (IExcludableReferencedProject referencedProject in referencedProjects)
            {
                if (referencedProject.ExcludeFromCodeCoverage)
                {
                    ExcludedReferencedProjects.Add(referencedProject);
                }
            }
        }

        private void EnsureDirectories()
        {
            EnsureFccDirectory();
            EnsureBuildOutputDirectory();
            EnsureEmptyOutputFolder();
        }

        private void EnsureFccDirectory() => CreateIfDoesNotExist(FCCOutputFolder);

        private void EnsureBuildOutputDirectory() => CreateIfDoesNotExist(BuildOutputPath);

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
            var directoryInfo = new DirectoryInfo(CoverageOutputFolder);
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
                _ = Directory.CreateDirectory(CoverageOutputFolder);
            }
        }

        private void CleanFCCDirectory()
        {
            var exclusions = new List<string> { BuildOutputFolderName, _coverageToolOutputFolderName };
            var fccDirectory = new DirectoryInfo(FCCOutputFolder);

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
                   catch
                   {
                   }
               });
        }

        private CoverageProjectFileSynchronizationDetails SynchronizeBuildOutput()
        {
            DateTime start = DateTime.Now;
            List<string> logs = _fileSynchronizationUtil.Synchronize(ProjectOutputFolder, BuildOutputPath, FCCFolderName);
            TimeSpan duration = DateTime.Now - start;
            TestDllFile = Path.Combine(BuildOutputPath, Path.GetFileName(TestDllFile));
            return new CoverageProjectFileSynchronizationDetails
            {
                Logs = logs,
                Duration = duration,
            };
        }
    }
}
