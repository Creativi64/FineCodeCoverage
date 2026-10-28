using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using FineCodeCoverage.Collection.Runners;
using FineCodeCoverage.Collection.TestingPlatform.TUnit;
using FineCodeCoverage.Initialization.ToolZip;
using FineCodeCoverage.Vs.Commands.FCCCommands;
using FineCodeCoverage.VSAbstractions.OutputWindow;
using FineCodeCoverage.VSAbstractions.Threading;
using Moq;
using NUnit.Framework;

namespace FineCodeCoverageTests
{
    public class MtpCancellation_Tests
    {
        [Test]
        public void Cancel_Command_Should_Cancel_Without_Starting_Another_Collection()
        {
            var coverage = new Mock<ITUnitCoverage>();
            var command = new CancelCollectTUnitCommand(coverage.Object);

            typeof(CancelCollectTUnitCommand).GetMethod("Execute", BindingFlags.Instance | BindingFlags.NonPublic)
                .Invoke(command, new object[] { null, EventArgs.Empty });

            coverage.Verify(instance => instance.Cancel(), Times.Once);
            coverage.Verify(instance => instance.CollectCoverage(), Times.Never);
        }

        [Test]
        public void Cancelling_The_Process_Wait_Should_Terminate_The_Owned_Mtp_Host()
        {
            var threading = new Mock<IThreadHelper>();
            threading.Setup(helper => helper.WaitForForProcessExitAsync(It.IsAny<Process>(), It.IsAny<CancellationToken>()))
                .Returns(async (Process process, CancellationToken token) =>
                {
                    await Task.Delay(Timeout.Infinite, token);
                    return 0;
                });
            var runner = new TUnitCoverageRunner(Mock.Of<ILogger>(), Mock.Of<IToolUnzipper>(), threading.Object);
            using (var cancellation = new CancellationTokenSource())
            using (Process host = Process.Start(new ProcessStartInfo
            {
                FileName = Path.Combine(Environment.SystemDirectory, "cmd.exe"),
                Arguments = "/d /c set /p FCC_MTP_CANCELLATION_TEST=",
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
            }))
            {
                try
                {
                    cancellation.CancelAfter(100);
                    OperationCanceledException exception = Assert.CatchAsync<OperationCanceledException>(() => runner.WaitForExitAsync(host, cancellation.Token));
                    Assert.That(exception.CancellationToken, Is.EqualTo(cancellation.Token));
                    Assert.That(host.HasExited, Is.True);
                }
                finally
                {
                    if (!host.HasExited)
                    {
                        host.Kill();
                        host.WaitForExit(5000);
                    }
                }
            }
        }

        [Test]
        public void Disposing_The_Run_Job_Should_Terminate_An_Instrumented_Child_Host()
        {
            string pidFile = Path.Combine(Path.GetTempPath(), "fcc-job-child-" + Guid.NewGuid().ToString("N") + ".pid");
            string script = "$info = New-Object Diagnostics.ProcessStartInfo; " +
                "$info.FileName = $env:ComSpec; $info.Arguments = '/d /c set /p FCC_JOB_CHILD='; " +
                "$info.UseShellExecute = $false; $info.CreateNoWindow = $true; $info.RedirectStandardInput = $true; " +
                "$child = [Diagnostics.Process]::Start($info); [IO.File]::WriteAllText('" + pidFile.Replace("'", "''") +
                "', $child.Id.ToString()); [Console]::ReadLine()";
            using (var job = new TUnitCoverageRunner.ProcessJob())
            using (Process root = Process.Start(new ProcessStartInfo
            {
                FileName = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), "System32", "WindowsPowerShell", "v1.0", "powershell.exe"),
                Arguments = "-NoProfile -NonInteractive -EncodedCommand " + Convert.ToBase64String(System.Text.Encoding.Unicode.GetBytes(script)),
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardInput = true,
            }))
            {
                Process child = null;
                try
                {
                    job.Attach(root);
                    var watch = Stopwatch.StartNew();
                    int childId = 0;
                    while (watch.ElapsedMilliseconds < 10000 && childId == 0)
                    {
                        if (File.Exists(pidFile))
                        {
                            int.TryParse(File.ReadAllText(pidFile), out childId);
                        }

                        Thread.Sleep(20);
                    }

                    Assert.That(childId, Is.GreaterThan(0), "The test helper did not start its child.");
                    child = Process.GetProcessById(childId);
                    Assert.That(child.HasExited, Is.False);

                    job.Dispose();

                    Assert.That(root.WaitForExit(5000), Is.True, "The run controller survived job disposal.");
                    Assert.That(child.WaitForExit(5000), Is.True, "The instrumented child survived job disposal.");
                }
                finally
                {
                    job.Dispose();
                    child?.Dispose();
                    if (!root.HasExited)
                    {
                        root.Kill();
                        root.WaitForExit(5000);
                    }

                    if (File.Exists(pidFile))
                    {
                        File.Delete(pidFile);
                    }
                }
            }
        }
    }
}
