using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using AutoMoq;
using FineCodeCoverage.Collection.ReportGeneration;
using FineCodeCoverage.Collection.ReportGeneration.PalmmediaImpl;
using Moq;
using NUnit.Framework;

namespace FineCodeCoverageTests
{
    public class ReportCancellation_Tests
    {
        [Test]
        public void Should_Generate_A_Report_When_Not_Cancelled()
        {
            string directory = Path.Combine(Path.GetTempPath(), "fcc-report-test-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(directory);
            try
            {
                string coverageFile = Path.Combine(directory, "input.xml");
                File.WriteAllText(coverageFile, @"<coverage line-rate=""1"" branch-rate=""0"" version=""1""><sources><source>.</source></sources><packages><package name=""Sample"" line-rate=""1"" branch-rate=""0""><classes><class name=""Sample.Class"" filename=""Sample.cs"" line-rate=""1"" branch-rate=""0""><methods/><lines><line number=""1"" hits=""1""/></lines></class></classes></package></packages></coverage>");
                File.WriteAllText(Path.Combine(directory, "Sample.cs"), "class Sample { }");
                var generator = new PalmmediaReportGenerator(Mock.Of<IHtmlFilesToFolder>());

                IReportResult result = generator.Generate(new[] { coverageFile }, directory, new[] { "Cobertura" }, CancellationToken.None);

                Assert.That(result, Is.Not.Null);
                Assert.That(File.Exists(Path.Combine(directory, "Cobertura.xml")), Is.True);
            }
            finally
            {
                Directory.Delete(directory, true);
            }
        }

        [Test]
        public void Should_Forward_Cancellation_To_The_Generator()
        {
            var mocker = new AutoMoqer();
            using (var cancellation = new CancellationTokenSource())
            {
                var files = new[] { "coverage.xml" };
                mocker.GetMock<IFCCReportGenerator>()
                    .Setup(generator => generator.Generate(files, "report", It.IsAny<System.Collections.Generic.IEnumerable<string>>(), cancellation.Token))
                    .Throws(new OperationCanceledException(cancellation.Token));

                Assert.ThrowsAsync<OperationCanceledException>(() =>
                    mocker.Create<ReportGeneratorUtil>().GenerateAsync(files, "report", cancellation.Token));
            }
        }

        [Test]
        public void Should_Not_Parse_Or_Start_A_Report_When_Already_Cancelled()
        {
            var generator = new PalmmediaReportGenerator(Mock.Of<IHtmlFilesToFolder>());
            Assert.Throws<OperationCanceledException>(() =>
                generator.Generate(null, null, null, new CancellationToken(true)));
        }

        [Test]
        public void Should_Terminate_A_Running_Report_Process_When_Cancelled()
        {
            // cmd's built-in set /p waits for stdin without spawning a child process.
            using (Process process = StartProcess("/d /c set /p FCC_REPORT_TEST="))
            using (var cancellation = new CancellationTokenSource())
            {
                try
                {
                    cancellation.CancelAfter(200);
                    OperationCanceledException exception = Assert.Throws<OperationCanceledException>(() =>
                        PalmmediaReportGenerator.WaitForExit(process, cancellation.Token));
                    Assert.That(exception.CancellationToken, Is.EqualTo(cancellation.Token));
                    Assert.That(process.HasExited, Is.True);
                }
                finally
                {
                    if (!process.HasExited)
                    {
                        process.Kill();
                        process.WaitForExit(5000);
                    }
                }
            }
        }

        [Test]
        public void Should_Preserve_The_Report_Process_Exit_Code()
        {
            using (Process process = StartProcess("/d /c exit 7"))
            {
                PalmmediaReportGenerator.WaitForExit(process, CancellationToken.None);
                Assert.That(process.ExitCode, Is.EqualTo(7));
            }
        }

        private static Process StartProcess(string arguments)
            => Process.Start(new ProcessStartInfo
            {
                FileName = Path.Combine(Environment.SystemDirectory, "cmd.exe"),
                Arguments = arguments,
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
            });
    }
}
