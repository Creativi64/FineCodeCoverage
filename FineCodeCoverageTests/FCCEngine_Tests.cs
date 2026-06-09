using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMoq;
using FineCodeCoverage.Collection.CoverageProjectManagement;
using FineCodeCoverage.Collection.CoverageToolOutput;
using FineCodeCoverage.Collection.Engine;
using FineCodeCoverage.Collection.Messages;
using FineCodeCoverage.Collection.ReportGeneration;
using FineCodeCoverage.Utilities.Events;
using FineCodeCoverage.Utilities.Logging;
using FineCodeCoverage.VSAbstractions.OutputWindow;
using FineCodeCoverage.VSAbstractions.Threading;
using Moq;
using NUnit.Framework;

namespace Test
{
    public class FCCEngine_RunAndProcessReport_Tests
    {
        private AutoMoqer mocker;
        private FCCEngine fccEngine;
        private Mock<ICancellationTokenSource> mockCancellationTokenSource;
        private const string reportOutputFolder = "reportOutputFolder";
        private CancellationToken cancellationToken;

        [SetUp]
        public void SetUp()
        {
            Setup(false);
        }

        private void Setup(bool cancelled)
        {
            mocker = new AutoMoqer();
            mocker.Setup<ICoverageToolOutputManager, string>(
                coverageToolOutputManager => coverageToolOutputManager.GetReportOutputFolder()).Returns(reportOutputFolder);
            var mockDisposeAwareTaskRunner = mocker.GetMock<IDisposeAwareTaskRunner>();
#pragma warning disable VSTHRD002 // Avoid problematic synchronous waits
            mockDisposeAwareTaskRunner.Setup(
                runner => runner.RunAsyncFunc(
                    It.IsAny<Func<Task>>())
                ).Callback<Func<Task>>(taskProvider => taskProvider().Wait());
#pragma warning restore VSTHRD002 // Avoid problematic synchronous waits
            mockCancellationTokenSource = new Mock<ICancellationTokenSource>();
            var cancellationTokenSource = new CancellationTokenSource();
            cancellationToken = cancellationTokenSource.Token;
            if (cancelled)
            {
                cancellationTokenSource.Cancel();
            }
            mockCancellationTokenSource.SetupGet(cts => cts.Token).Returns(cancellationToken);
            mockDisposeAwareTaskRunner.Setup(disposeAwareTaskRunner => disposeAwareTaskRunner.CreateLinkedTokenSource()).Returns(mockCancellationTokenSource.Object);
            fccEngine = mocker.Create<FCCEngine>();
        }

        private void RunAndProcessReport(string[] coberturaFiles, List<ICoverageProject> coverageProjects = null, Action cleanUp = null)
            => fccEngine.RunAndProcessReport(coberturaFiles, coverageProjects ?? new List<ICoverageProject>(), cleanUp);

        [Test]
        public void Should_Run_Report_Generator_With_The_Supplied_Cobertura_Files()
        {
            var coberturaFiles = new string[] { "first.cobertura.xml", "second.cobertura.xml" };
            mocker.GetMock<IReportGeneratorUtil>().Setup(rg =>
                rg.GenerateAsync(
                    It.Is<string[]>(files => files.SequenceEqual(coberturaFiles)),
                    reportOutputFolder,
                    cancellationToken
                    )).ReturnsAsync(new ReportGeneratorResult { });

            RunAndProcessReport(coberturaFiles);

            mocker.GetMock<IReportGeneratorUtil>().VerifyAll();
        }

        [Test]
        public void Should_Not_Run_ReportGenerator_If_No_Cobertura_Files()
        {
            RunAndProcessReport(new string[] { });
#pragma warning disable VSTHRD110 // Observe result of async calls
            mocker.Verify<IReportGeneratorUtil>(rg => rg.GenerateAsync(
                It.IsAny<IEnumerable<string>>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()), Times.Never());
#pragma warning restore VSTHRD110 // Observe result of async calls
        }

        #region success path

        class SuccessState
        {
            public SuccessState(List<ICoverageProject> coverageProjects, IReportResult reportResult, string coberturaFile)
            {
                CoverageProjects = coverageProjects;
                ReportResult = reportResult;
                CoberturaFile = coberturaFile;
            }

            public List<ICoverageProject> CoverageProjects { get; }
            public IReportResult ReportResult { get; }
            public string CoberturaFile { get; }
        }

        private SuccessState Run_Success()
        {
            var coverageProject = new Mock<ICoverageProject>().Object;
            var coverageProjects = new List<ICoverageProject> { coverageProject };
            var reportResult = new Mock<IReportResult>().Object;

            var reportGeneratorResult = new ReportGeneratorResult
            {
                UnifiedXmlFile = "unified.cobertura",
                ReportResult = reportResult
            };
            var mockReportGenerator = mocker.GetMock<IReportGeneratorUtil>();
            mockReportGenerator.Setup(rg =>
                rg.GenerateAsync(
                    It.IsAny<IEnumerable<string>>(),
                    It.IsAny<string>(),
                    cancellationToken
                )).ReturnsAsync(reportGeneratorResult);

            RunAndProcessReport(new string[] { "input.cobertura.xml" }, coverageProjects);
            return new SuccessState(coverageProjects, reportResult, "unified.cobertura");
        }

        [Test]
        public void Should_Log_Done_When_Have_The_ReportResult()
        {
            Run_Success();

            VerifyLogsCoverageStatus("Done");
        }

        [Test]
        public void Should_Send_CoverageEndedMessage_When_Have_The_ReportResult()
        {
            Run_Success();

            mocker.Verify<IEventAggregator>(ea => ea.SendMessage(It.IsAny<CoverageEndedMessage>(), null));
        }

        [Test]
        public void Should_Send_NewReportMessage_When_Have_The_ReportResult()
        {
            var successState = Run_Success();

            var reportResult = successState.ReportResult;
            var coverageProject = successState.CoverageProjects[0];
            mocker.Verify<IEventAggregator>(ea => ea.SendMessage(
                It.Is<NewReportMessage>(
                    msg => msg.Report == reportResult &&
                        msg.CoverageProjects.Count == 1 &&
                        msg.CoverageProjects[0] == coverageProject), null));
        }

        [Test]
        public void Should_Send_ReportFilesMessage_When_Have_The_ReportResult()
        {
            var successState = Run_Success();

            var reportResult = successState.ReportResult;
            var coberturaFile = successState.CoberturaFile;
            mocker.Verify<IEventAggregator>(
                ea => ea.SendMessage(
                    It.Is<ReportFilesMessage>(msg => msg.ReportResult == reportResult && msg.CoberturaFile == coberturaFile), null));
        }

        #endregion

        #region error path
        public Exception ThrowError()
        {
            var exception = new Exception("Error Message");
            mocker.GetMock<IReportGeneratorUtil>().Setup(rg =>
                rg.GenerateAsync(It.IsAny<IEnumerable<string>>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(exception);
            RunAndProcessReport(new string[] { "input.cobertura.xml" });
            return exception;
        }

        [Test]
        public void Should_Log_Error_When_Exception()
        {
            var exception = ThrowError();

#pragma warning disable VSTHRD110 // Observe result of async calls
            mocker.Verify<ILogger>(l => l.LogAsync(StatusMarkerProvider.Get("Error"), exception.ToString()));
#pragma warning restore VSTHRD110 // Observe result of async calls
        }

        [Test]
        public void Should_Raise_Coverage_Ended_Message_When_Exception()
        {
            ThrowError();
            mocker.Verify<IEventAggregator>(ea => ea.SendMessage(It.IsAny<CoverageEndedMessage>(), null));
        }

        #endregion

        [Test]
        public void Should_Invoke_CleanUp()
        {
            var cleanedUp = false;
            RunAndProcessReport(new string[] { }, cleanUp: () => cleanedUp = true);
            Assert.That(cleanedUp, Is.True);
        }

        [Test]
        public void Should_Cancel_Running_Coverage_When_StopCoverage()
        {
            RunAndProcessReport(new string[] { });
            fccEngine.StopCoverage();
            mockCancellationTokenSource.Verify(cts => cts.Cancel());
        }

        [Test]
        public void Should_Not_Throw_When_StopCoverage_And_There_Is_No_Coverage_Running()
        {
            fccEngine.StopCoverage();
        }

        [Test]
        public void Should_Cancel_Existing_Run_When_RunAndProcessReport()
        {
            RunAndProcessReport(new string[] { });
            mockCancellationTokenSource.Verify(cts => cts.Cancel(), Times.Never());
            RunAndProcessReport(new string[] { });
            mockCancellationTokenSource.Verify(cts => cts.Cancel(), Times.Once());
        }

        private void VerifyLogsCoverageStatus(string coverageStatus)
        {
#pragma warning disable VSTHRD110 // Observe result of async calls
            mocker.Verify<ILogger>(l => l.LogAsync(StatusMarkerProvider.Get(coverageStatus)));
#pragma warning restore VSTHRD110 // Observe result of async calls
        }
    }
}
