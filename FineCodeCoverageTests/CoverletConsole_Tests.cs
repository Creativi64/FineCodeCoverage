using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMoq;
using FineCodeCoverage.Core.Utilities;
using FineCodeCoverage.Engine.Coverlet;
using FineCodeCoverage.Engine.Model;
using FineCodeCoverage.Options;
using FineCodeCoverage.Output;
using Moq;
using NUnit.Framework;

namespace Test
{
    public class CoverletExeArgumentsProvider_Tests
    {
        private const string testProjectName = "TestProject";

        [Test]
        public void Should_Have_ExcludeByAttribute_Setting_For_Each_ExcludeByAttribute()
        {
            var mockCoverageProject = SafeMockCoverageProject();
            mockCoverageProject.SetupGet(cp => cp.Settings.ExcludeByAttribute).Returns(new[] { "ExcludeByAttribute1", "ExcludeByAttribute2" });

            var coverletExeArgumentsProvider = new CoverletExeArgumentsProvider();
            var coverletSettings = coverletExeArgumentsProvider.GetArguments(mockCoverageProject.Object);

            AssertHasSetting(coverletSettings, "--exclude-by-attribute ExcludeByAttribute1");
            AssertHasSetting(coverletSettings, "--exclude-by-attribute ExcludeByAttribute2");
        }

        [Test] //https://github.com/coverlet-coverage/coverlet/issues/1589
        public void Should_Unqualified_Qualified_ExcludeByAttribute()
        {
            var mockCoverageProject = SafeMockCoverageProject();
            mockCoverageProject.SetupGet(cp => cp.Settings.ExcludeByAttribute).Returns(new[] { "Namespace.ExcludeByAttribute1"});

            var coverletExeArgumentsProvider = new CoverletExeArgumentsProvider();
            var coverletSettings = coverletExeArgumentsProvider.GetArguments(mockCoverageProject.Object);

            AssertHasSetting(coverletSettings, "--exclude-by-attribute ExcludeByAttribute1");
        }

        [Test]
        public void Should_Not_Add_Test_Test_Assembly_To_Includes_When_IncludeTestAssembly_And_No_Other_Includes()
        {
            var mockCoverageProject = SafeMockCoverageProject();
            mockCoverageProject.SetupGet(cp => cp.Settings.IncludeTestAssembly).Returns(true);

            var coverletExeArgumentsProvider = new CoverletExeArgumentsProvider();
            var coverletSettings = coverletExeArgumentsProvider.GetArguments(mockCoverageProject.Object);

            Assert.IsFalse(HasIncludedTestAssemblySetting(coverletSettings));
        }

        private bool HasIncludedTestAssemblySetting(List<string> coverletSettings)
        {
            return coverletSettings.Any(coverletSetting => coverletSetting == $@"--include ""[{testProjectName}]*""");
        }

        [Test]
        public void Should_Add_Test_Test_Assembly_To_Includes_When_IncludeTestAssembly_And_Other_Includes()
        {
            var mockCoverageProject = SafeMockCoverageProject();
            mockCoverageProject.SetupGet(cp => cp.Settings.IncludeTestAssembly).Returns(true);
            mockCoverageProject.SetupGet(cp => cp.Settings.Include).Returns(new string[] { "[anassembly]*" });

            var coverletExeArgumentsProvider = new CoverletExeArgumentsProvider();
            var coverletSettings = coverletExeArgumentsProvider.GetArguments(mockCoverageProject.Object);

            Assert.IsTrue(HasIncludedTestAssemblySetting(coverletSettings));
        }

        [Test]
        public void Should_Add_IncludedReferencedProjects_As_Include()
        {
            var mockCoverageProject = SafeMockCoverageProject();
            var mockReferencedProject = new Mock<IReferencedProject>();
            mockReferencedProject.SetupGet(rp => rp.AssemblyName).Returns("ReferencedProject");
            mockCoverageProject.SetupGet(cp => cp.IncludedReferencedProjects).Returns(new List<IReferencedProject> { mockReferencedProject.Object });

            var coverletExeArgumentsProvider = new CoverletExeArgumentsProvider();
            var coverletSettings = coverletExeArgumentsProvider.GetArguments(mockCoverageProject.Object);

            Assert.True(coverletSettings.Contains($@"--include ""[ReferencedProject]*"""));

        }

        [Test]
        public void Should_Include_From_Settings()
        {
            var mockCoverageProject = SafeMockCoverageProject();
            mockCoverageProject.SetupGet(cp => cp.Settings.Include).Returns(new string[] { "[Include]*" });

            var coverletExeArgumentsProvider = new CoverletExeArgumentsProvider();
            var coverletSettings = coverletExeArgumentsProvider.GetArguments(mockCoverageProject.Object);

            Assert.True(coverletSettings.Contains($@"--include ""[Include]*"""));

        }

        [Test]
        public void Should_Add_ExcludedReferencedProjects_As_Exclude()
        {
            var mockCoverageProject = SafeMockCoverageProject();
            var mockReferencedProject = new Mock<IReferencedProject>();
            mockReferencedProject.SetupGet(rp => rp.AssemblyName).Returns("ReferencedProject");
            mockCoverageProject.SetupGet(cp => cp.ExcludedReferencedProjects).Returns(new List<IReferencedProject> { mockReferencedProject.Object });

            var coverletExeArgumentsProvider = new CoverletExeArgumentsProvider();
            var coverletSettings = coverletExeArgumentsProvider.GetArguments(mockCoverageProject.Object);

            Assert.True(coverletSettings.Contains($@"--exclude ""[ReferencedProject]*"""));

        }

        [Test]
        public void Should_Exclude_From_Settings()
        {
            var mockCoverageProject = SafeMockCoverageProject();
            mockCoverageProject.SetupGet(cp => cp.Settings.Exclude).Returns(new string[] { "[Exclude]*" });

            var coverletExeArgumentsProvider = new CoverletExeArgumentsProvider();
            var coverletSettings = coverletExeArgumentsProvider.GetArguments(mockCoverageProject.Object);

            Assert.True(coverletSettings.Contains($@"--exclude ""[Exclude]*"""));

        }

        private Mock<ICoverageProject> SafeMockCoverageProject()
        {
            var mockCoverageProject = new Mock<ICoverageProject>();
            mockCoverageProject.SetupGet(coverageProject => coverageProject.IncludedReferencedProjects).Returns(new List<IReferencedProject>());
            mockCoverageProject.SetupGet(coverageProject => coverageProject.ExcludedReferencedProjects).Returns(new List<IReferencedProject>());
            mockCoverageProject.SetupGet(coverageProject => coverageProject.Settings).Returns(new Mock<IAppOptions>().Object);
            mockCoverageProject.Setup(coverageProject => coverageProject.ProjectName).Returns(testProjectName);
            return mockCoverageProject;
        }

        private void AssertHasSetting(List<string> coverletSettings, string setting)
        {
            Assert.IsTrue(coverletSettings.Any(coverletSetting => coverletSetting == setting));
        }

    }

    public class CoverletConsoleExecuteRequestProvider_Tests
    {
        [TestCase(0)]
        [TestCase(1)]
        [TestCase(2)]
        [TestCase(3)]
        public async Task Should_GetCoverletExePath_From_First_That_Returns_Non_Null_Async(int providingExeProvider)
        {
            var coverageProject = new Mock<ICoverageProject>().Object;
            var coverletSettings = "coverlet settings";

            List<ExecuteRequest> executeRequests = new List<ExecuteRequest>
            {
                new ExecuteRequest(),
                new ExecuteRequest(),
                new ExecuteRequest(),
                new ExecuteRequest()
            };

            ExecuteRequest GetExecuteRequest(int order)
            {
                if (order != providingExeProvider)
                {
                    return null;
                }
                return executeRequests[order];
            };

            var mockLocalExecutor = new Mock<ICoverletConsoleExecutor>();
            var mockCustomPathExecutor = new Mock<ICoverletConsoleExecutor>();
            var mockGlobalExecutor = new Mock<ICoverletConsoleExecutor>();
            var mockFCCCoverletConsoleExecutor = new Mock<IFCCCoverletConsoleExecutor>();
            var mockFCCExecutor = mockFCCCoverletConsoleExecutor.As<ICoverletConsoleExecutor>();
            var mockExecutors = new List<Mock<ICoverletConsoleExecutor>>
            {
                mockLocalExecutor,
                mockCustomPathExecutor,
                mockGlobalExecutor,
                mockFCCExecutor
            };
            var callOrder = new List<int>();
            for (var i = 0; i < mockExecutors.Count; i++)
            {
                var order = i;
                var mockExeProvider = mockExecutors[i];
                mockExeProvider.Setup(p => p.GetRequestAsync(coverageProject, coverletSettings)).ReturnsAsync(GetExecuteRequest(i)).Callback<ICoverageProject, string>((_, __) =>
                {
                    callOrder.Add(order);
                });
            }

            var coverletConsoleUtil = new CoverletConsoleExecuteRequestProvider(mockGlobalExecutor.Object, mockCustomPathExecutor.Object, mockLocalExecutor.Object, mockFCCCoverletConsoleExecutor.Object);

            var executeRequest = await coverletConsoleUtil.GetExecuteRequestAsync(coverageProject, coverletSettings);

            Assert.AreSame(GetExecuteRequest(providingExeProvider), executeRequest);
            Assert.AreEqual(providingExeProvider + 1, callOrder.Count);
            var previousCallOrder = -1;
            foreach (var call in callOrder)
            {
                Assert.AreEqual(call - previousCallOrder, 1);
                previousCallOrder = call;
            }

        }
    }

    public class CoverletConsoleUtil_Tests
    {
        private AutoMoqer mocker;
        private CoverletConsoleUtil coverletConsoleUtil;
        private bool executed;
        private readonly List<string> coverletSettings = new List<string> { "setting1", "setting2" };
        private readonly ExecuteResponse successfulExecuteResponse =  new ExecuteResponse { ExitCode = 3, Output = "Successful output" };

        [SetUp]
        public void SetUp()
        {
            executed = false;
            mocker = new AutoMoqer();
            coverletConsoleUtil = mocker.Create<CoverletConsoleUtil>();
        }
        [Test]
        public void Should_Initilize_IFCCCoverletConsoleExeProvider()
        {
            var ct = CancellationToken.None;
            coverletConsoleUtil.Initialize("appDataFolder", ct);
            mocker.Verify<IFCCCoverletConsoleExecutor>(fccExeProvider => fccExeProvider.Initialize("appDataFolder", ct));
        }

        [Test]
        public async Task Should_Execute_The_Request_From_The_Execute_Request_Provider_With_Space_Delimited_Settings_Async()
        {
            await RunSuccessfullyAsync();

            mocker.GetMock<IProcessUtil>().Verify();
        }

        [Test]
        public async Task Should_Log_Settings_Before_Executing_Async()
        {
            var mockLogger = mocker.GetMock<ILogger>();
            mockLogger.Setup(logger => logger.LogAsync(It.IsAny<IEnumerable<string>>())).Callback<IEnumerable<string>>(messages =>
            {
                Assert.Multiple(() =>
                {
                    Assert.That(messages.First(), Is.EqualTo("Coverlet Run (TheProjectName) - Arguments"));
                    Assert.That(messages.Skip(1), Is.EqualTo(coverletSettings));
                    Assert.That(executed, Is.False);
                });
            });

            await RunSuccessfullyAsync();

            mockLogger.VerifyAll();
        }

        [Test]
        public void Should_Throw_With_ExecuteResponse_Output_When_ExitCode_Is_Greater_Than_3()
        {
            var failureExecuteResponse = new ExecuteResponse { ExitCode = 4, Output = "failure message" };
            var exception = Assert.ThrowsAsync<Exception>(async() => await RunAsync(failureExecuteResponse));

            Assert.That(exception.Message, Is.EqualTo("Error. Exit code: 4"));
        }

        [Test]
        public void Should_Log_With_ExecuteResponse_ExitCode_And_Output_When_ExitCode_Is_Greater_Than_3()
        {
            var failureExecuteResponse = new ExecuteResponse { ExitCode = 4, Output = "failure message" };
            
            Assert.ThrowsAsync<Exception>(() => RunAsync(failureExecuteResponse));

            var mockLogger = mocker.GetMock<ILogger>();
#pragma warning disable VSTHRD110 // Observe result of async calls
            mockLogger.Verify(logger => logger.LogAsync("Coverlet Run (TheProjectName) Error. Exit code: 4", "failure message"));
#pragma warning restore VSTHRD110 // Observe result of async calls
        }

        [Test]
        public async Task Should_Log_The_ExecuteResponse_Output_On_Success_Async()
        {
            var mockLogger = mocker.GetMock<ILogger>();
            mockLogger.Setup(logger => logger.LogFileAndForget(It.IsAny<string[]>())).Callback<string[]>(messages =>
            {
                Assert.Multiple(() =>
                {
                    Assert.That(messages, Is.EqualTo(new string[] { "Coverlet Run (TheProjectName) - Output", successfulExecuteResponse.Output }));
                    Assert.That(executed, Is.True);
                });

            });

            await RunSuccessfullyAsync();

            mockLogger.Verify();
        }
        
        private async Task RunSuccessfullyAsync()
        {
            await RunAsync(successfulExecuteResponse);
        }

        private async Task RunAsync(ExecuteResponse executeResponse)
        {
            var mockCoverageProject = new Mock<ICoverageProject>();
            mockCoverageProject.SetupGet(cp => cp.ProjectName).Returns("TheProjectName");
            var coverageProject = mockCoverageProject.Object;
            var requestSettings = string.Join(" ", coverletSettings);
            var executeRequest = new ExecuteRequest { FilePath = "TheFilePath", Arguments = "TheArguments" };
            var cancellationToken = CancellationToken.None;

            mocker.Setup<ICoverletExeArgumentsProvider, List<string>>(coverletExeArgumentsProvider => coverletExeArgumentsProvider.GetArguments(coverageProject)).Returns(coverletSettings);
            mocker.Setup<ICoverletConsoleExecuteRequestProvider, Task<ExecuteRequest>>(
                coverletConsoleExecuteRequestProvider => coverletConsoleExecuteRequestProvider.GetExecuteRequestAsync(coverageProject, requestSettings)
            ).ReturnsAsync(executeRequest);
            var mockProcessUtil = mocker.GetMock<IProcessUtil>();
            mockProcessUtil.Setup(processUtil => processUtil.ExecuteAsync(executeRequest, cancellationToken))
                .Callback(() => executed = true)
               .ReturnsAsync(executeResponse);
            
            await coverletConsoleUtil.RunAsync(coverageProject, cancellationToken);
        }

    }

    public class CoverletConsoleGlobalExeProvider_Tests
    {
        private AutoMoqer mocker;
        private CoverletConsoleDotnetToolsGlobalExecutor globalExeProvider;

        [SetUp]
        public void SetUp()
        {
            mocker = new AutoMoqer();
            globalExeProvider = mocker.Create<CoverletConsoleDotnetToolsGlobalExecutor>();
        }

        [Test]
        public async Task Should_Return_Null_From_GetRequest_If_Not_Enabled_In_Options_Async()
        {
            var mockCoverageProject = new Mock<ICoverageProject>();
            mockCoverageProject.Setup(cp => cp.Settings.CoverletConsoleGlobal).Returns(false);
            Assert.IsNull(await globalExeProvider.GetRequestAsync(mockCoverageProject.Object, null));
        }

        [Test]
        public async Task Should_Return_Null_If_Enabled_But_Not_Installed_Async()
        {
            var mockCoverageProject = new Mock<ICoverageProject>();
            mockCoverageProject.Setup(cp => cp.Settings.CoverletConsoleGlobal).Returns(true);
            var dotNetToolListCoverlet = mocker.GetMock<IDotNetToolListCoverlet>();
            dotNetToolListCoverlet.Setup(dotnet => dotnet.GlobalAsync()).ReturnsAsync((CoverletToolDetails)null);

            Assert.IsNull(await globalExeProvider.GetRequestAsync(mockCoverageProject.Object, null));
            dotNetToolListCoverlet.VerifyAll();
        }

        [Test]
        public async Task Should_Log_When_Enabled_And_Unsuccessful_Async()
        {
            var mockCoverageProject = new Mock<ICoverageProject>();
            mockCoverageProject.Setup(cp => cp.Settings.CoverletConsoleGlobal).Returns(true);
            var dotNetToolListCoverlet = mocker.GetMock<IDotNetToolListCoverlet>();
            dotNetToolListCoverlet.Setup(dotnet => dotnet.GlobalAsync()).ReturnsAsync((CoverletToolDetails)null);

            await globalExeProvider.GetRequestAsync(mockCoverageProject.Object, null);
#pragma warning disable VSTHRD110 // Observe result of async calls
            mocker.Verify<ILogger>(l => l.LogAsync("Unable to use Coverlet console global tool"));
#pragma warning restore VSTHRD110 // Observe result of async calls

        }

        private Task<ExecuteRequest> GetRequest_For_Globally_Installed_Coverlet_Console_Async()
        {
            var mockCoverageProject = new Mock<ICoverageProject>();
            mockCoverageProject.Setup(cp => cp.Settings.CoverletConsoleGlobal).Returns(true);
            mockCoverageProject.Setup(cp => cp.ProjectOutputFolder).Returns("TheOutputFolder");
            var dotNetToolListCoverlet = mocker.GetMock<IDotNetToolListCoverlet>();
            dotNetToolListCoverlet.Setup(dotnet => dotnet.GlobalAsync()).ReturnsAsync(new CoverletToolDetails { Command = "TheCommand" });

            return globalExeProvider.GetRequestAsync(mockCoverageProject.Object, "coverlet settings");
        }

        [Test]
        public async Task Should_Request_Execute_With_The_Coverlet_Console_Command_Async()
        {
            var request = await GetRequest_For_Globally_Installed_Coverlet_Console_Async();
            Assert.AreEqual("TheCommand", request.FilePath);
        }

        [Test]
        public async Task Should_Request_Arguments_The_Coverlet_Settings_Async()
        {
            var request = await GetRequest_For_Globally_Installed_Coverlet_Console_Async();
            Assert.AreEqual("coverlet settings", request.Arguments);
        }

        [Test]
        public async Task Should_Request_WorkingDirectory_To_The_Project_Output_Folder_Async()
        {
            var request = await GetRequest_For_Globally_Installed_Coverlet_Console_Async();
            Assert.AreEqual("TheOutputFolder", request.WorkingDirectory);
        }

    }

    public class CoverletConsoleCustomPathExecutor_Tests
    {
        private string tempCoverletExe;
        private AutoMoqer mocker;
        private CoverletConsoleCustomPathExecutor customPathExecutor;

        [SetUp]
        public void SetUp()
        {
            mocker = new AutoMoqer();
            customPathExecutor = mocker.Create<CoverletConsoleCustomPathExecutor>();
        }

        [TearDown]
        public void Delete_ProjectFile()
        {
            if (tempCoverletExe != null)
            {
                File.Delete(tempCoverletExe);
            }
        }

        [TestCase(null)]
        [TestCase("")]
        public async Task Should_Return_Null_If_Not_Set_In_Options_Async(string coverletConsoleCustomPath)
        {
            var mockCoverageProject = new Mock<ICoverageProject>();
            mockCoverageProject.Setup(cp => cp.Settings.CoverletConsoleCustomPath).Returns(coverletConsoleCustomPath);
            Assert.IsNull(await customPathExecutor.GetRequestAsync(mockCoverageProject.Object,null));
        }

        [Test]
        public async Task Should_Return_Null_If_File_Does_Not_Exist_Async()
        {
            var mockCoverageProject = new Mock<ICoverageProject>();
            mockCoverageProject.Setup(cp => cp.Settings.CoverletConsoleCustomPath).Returns("alnlkalk.exe");
            Assert.IsNull(await customPathExecutor.GetRequestAsync(mockCoverageProject.Object, null));
        }

        [Test]
        public async Task Should_Return_Null_If_Not_An_Exe_Async()
        {
            tempCoverletExe = Path.Combine(Path.GetTempPath(), "thecoverletexecutable.notexe");
            File.WriteAllText(tempCoverletExe, "");

            var mockCoverageProject = new Mock<ICoverageProject>();
            mockCoverageProject.Setup(cp => cp.Settings.CoverletConsoleCustomPath).Returns(tempCoverletExe);

            Assert.IsNull(await customPathExecutor.GetRequestAsync(mockCoverageProject.Object, null));
        }

        private Task<ExecuteRequest> Get_Request_For_Custom_Path_Async()
        {
            tempCoverletExe = Path.Combine(Path.GetTempPath(), "thecoverletexecutable.exe");
            File.WriteAllText(tempCoverletExe, "");

            var mockCoverageProject = new Mock<ICoverageProject>();
            mockCoverageProject.Setup(cp => cp.Settings.CoverletConsoleCustomPath).Returns(tempCoverletExe);
            mockCoverageProject.Setup(cp => cp.ProjectOutputFolder).Returns("TheOutputFolder");
            return customPathExecutor.GetRequestAsync(mockCoverageProject.Object, "coverlet settings");
        }

        [Test]
        public async Task Should_Set_FilePath_To_The_Exe_Async()
        {
            var executeRequest = await Get_Request_For_Custom_Path_Async();
            Assert.AreEqual(tempCoverletExe, executeRequest.FilePath);
        }

        [Test]
        public async Task Should_Set_Arguments_To_Coverlet_Settings_Async()
        {
            var executeRequest = await Get_Request_For_Custom_Path_Async();
            Assert.AreEqual("coverlet settings", executeRequest.Arguments);
        }

        [Test]
        public async Task Should_Request_WorkingDirectory_To_The_Project_Output_Folder_Async()
        {
            var request = await Get_Request_For_Custom_Path_Async();
            Assert.AreEqual("TheOutputFolder", request.WorkingDirectory);
        }

    }

    public class CoverletConsoleLocalExeProvider_Tests
    {
        private AutoMoqer mocker;
        private CoverletConsoleDotnetToolsLocalExecutor localExecutor;

        [SetUp]
        public void SetUp()
        {
            mocker = new AutoMoqer();
            localExecutor = mocker.Create<CoverletConsoleDotnetToolsLocalExecutor>();
        }

        [Test]
        public async Task Should_Return_Null_If_Not_Enabled_In_Options_Async()
        {
            var mockCoverageProject = new Mock<ICoverageProject>();
            mockCoverageProject.Setup(cp => cp.Settings.CoverletConsoleLocal).Returns(false);
            Assert.IsNull(await localExecutor.GetRequestAsync(mockCoverageProject.Object,null));
        }

        [Test]
        public async Task Should_Return_Null_If_No_DotNetConfig_Ascendant_Directory_Async()
        {
            var projectOutputFolder = "projectoutputfolder";
            var mockCoverageProject = new Mock<ICoverageProject>();
            mockCoverageProject.Setup(cp => cp.Settings.CoverletConsoleLocal).Returns(true);
            mockCoverageProject.Setup(cp => cp.ProjectOutputFolder).Returns(projectOutputFolder);

            var mockDotNetConfigFinder = mocker.GetMock<IDotNetConfigFinder>();
            mockDotNetConfigFinder.Setup(f => f.GetConfigDirectories(projectOutputFolder)).Returns(new List<string>());
            Assert.IsNull(await localExecutor.GetRequestAsync(mockCoverageProject.Object, null));
            mockDotNetConfigFinder.VerifyAll();
        }
        
        [Test]
        public async Task Should_Return_Null_If_None_Of_The_DotNetConfig_Containing_Directories_Are_Local_Tool_Async()
        {
            var projectOutputFolder = "projectoutputfolder";
            var mockCoverageProject = new Mock<ICoverageProject>();
            mockCoverageProject.Setup(cp => cp.Settings.CoverletConsoleLocal).Returns(true);
            mockCoverageProject.Setup(cp => cp.ProjectOutputFolder).Returns(projectOutputFolder);

            var mockDotNetConfigFinder = mocker.GetMock<IDotNetConfigFinder>();
            mockDotNetConfigFinder.Setup(f => f.GetConfigDirectories(projectOutputFolder)).Returns(new List<string> { "ConfigDirectory1", "ConfigDirectory2" });

            var mockDotNetToolListCoverlet = mocker.GetMock<IDotNetToolListCoverlet>();
            mockDotNetToolListCoverlet.Setup(dotnet => dotnet.LocalAsync("ConfigDirectory1")).ReturnsAsync((CoverletToolDetails)null);
            mockDotNetToolListCoverlet.Setup(dotnet => dotnet.LocalAsync("ConfigDirectory2")).ReturnsAsync((CoverletToolDetails)null);
            Assert.IsNull(await localExecutor.GetRequestAsync(mockCoverageProject.Object, null));
            mockDotNetToolListCoverlet.VerifyAll();


        }

        [Test]
        public async Task Should_Log_If_None_Of_The_DotNetConfig_Containing_Directories_Are_Local_Tool_Async()
        {
            var projectOutputFolder = "projectoutputfolder";
            var mockCoverageProject = new Mock<ICoverageProject>();
            mockCoverageProject.Setup(cp => cp.Settings.CoverletConsoleLocal).Returns(true);
            mockCoverageProject.Setup(cp => cp.ProjectOutputFolder).Returns(projectOutputFolder);

            var mockDotNetConfigFinder = mocker.GetMock<IDotNetConfigFinder>();
            mockDotNetConfigFinder.Setup(f => f.GetConfigDirectories(projectOutputFolder)).Returns(new List<string> { "ConfigDirectory1", "ConfigDirectory2" });

            var mockDotNetToolListCoverlet = mocker.GetMock<IDotNetToolListCoverlet>();
            mockDotNetToolListCoverlet.Setup(dotnet => dotnet.LocalAsync("ConfigDirectory1")).ReturnsAsync((CoverletToolDetails)null);
            mockDotNetToolListCoverlet.Setup(dotnet => dotnet.LocalAsync("ConfigDirectory2")).ReturnsAsync((CoverletToolDetails)null);
            await localExecutor.GetRequestAsync(mockCoverageProject.Object, null);
#pragma warning disable VSTHRD110 // Observe result of async calls
            mocker.Verify<ILogger>(l => l.LogAsync("Unable to use Coverlet console local tool"));
#pragma warning restore VSTHRD110 // Observe result of async calls
        }

        private Task<ExecuteRequest> Get_Request_For_Local_Install_Async(bool firstConfigDirectoryLocalInstall, bool secondConfigDirectoryLocalInstall)
        {
            var projectOutputFolder = "projectoutputfolder";
            var mockCoverageProject = new Mock<ICoverageProject>();
            mockCoverageProject.Setup(cp => cp.Settings.CoverletConsoleLocal).Returns(true);
            mockCoverageProject.Setup(cp => cp.ProjectOutputFolder).Returns(projectOutputFolder);

            var mockDotNetConfigFinder = mocker.GetMock<IDotNetConfigFinder>();
            mockDotNetConfigFinder.Setup(f => f.GetConfigDirectories(projectOutputFolder)).Returns(new List<string> { "ConfigDirectory1", "ConfigDirectory2" });

            var mockDotNetToolListCoverlet = mocker.GetMock<IDotNetToolListCoverlet>();
            var coverletToolDetails = new CoverletToolDetails { Command = "TheCommand" };
            mockDotNetToolListCoverlet.Setup(dotnet => dotnet.LocalAsync("ConfigDirectory1")).ReturnsAsync(firstConfigDirectoryLocalInstall?coverletToolDetails:null);
            mockDotNetToolListCoverlet.Setup(dotnet => dotnet.LocalAsync("ConfigDirectory2")).ReturnsAsync(secondConfigDirectoryLocalInstall ? coverletToolDetails : null);
            return localExecutor.GetRequestAsync(mockCoverageProject.Object, "coverlet settings");
        }

        [Test]
        public async Task Should_Use_The_WorkingDirectory_Of_The_Nearest_Local_Tool_Install_Async()
        {
            var executeRequest = await Get_Request_For_Local_Install_Async(true, true);
            Assert.AreEqual("ConfigDirectory1",executeRequest.WorkingDirectory);
        }

        [Test]
        public async Task Should_Use_The_WorkingDirectory_Of_The_Nearest_Local_Tool_Install_Up_Async()
        {
            var executeRequest = await Get_Request_For_Local_Install_Async(false, true);
            Assert.AreEqual("ConfigDirectory2", executeRequest.WorkingDirectory);
        }

        [Test]
        public async Task Should_Use_The_DotNet_Command_Async()
        {
            var executeRequest = await Get_Request_For_Local_Install_Async(true, true);
            Assert.AreEqual("dotnet", executeRequest.FilePath);
        }

        [Test]
        public async Task Should_Use_The_Coverlet_Command_Async()
        {
            var executeRequest = await Get_Request_For_Local_Install_Async(true, true);
            Assert.AreEqual("TheCommand coverlet settings", executeRequest.Arguments);
        }
    }
}