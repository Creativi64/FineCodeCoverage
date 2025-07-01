using AutoMoq;
using FineCodeCoverage.Engine;
using FineCodeCoverage.Initialization;
using FineCodeCoverage.Initialization.History;
using FineCodeCoverage.Utilities.Wrappers;
using FineCodeCoverageTests.TestHelpers;
using Moq;
using NUnit.Framework;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace FineCodeCoverageTests
{
    internal class ShownToolWindowHistory_Tests
    {
        private AutoMoqer mocker;
        private ShownReportToolWindowHistory shownToolWindowHistory;
        private string markerFilePath;

        [SetUp]
        public async Task SetUpAsync()
        {
            mocker = new AutoMoqer();
            shownToolWindowHistory = mocker.Create<ShownReportToolWindowHistory>();
            await shownToolWindowHistory.InitializeAsync("AppDataFolderPath", CancellationToken.None);
            markerFilePath = Path.Combine("AppDataFolderPath", "outputWindowInitialized");
        }

        [Test]
        public void Should_Be_IAppDataFolderPathDependent()
        {
            Assert.That(MEFExportHelper.IsAndExports<ShownReportToolWindowHistory, IAppDataFolderPathDependent>(), Is.True);
        }

        [Test]
        public void It_Should_Write_Marker_File_When_ShowedToolWindow_First_Time()
        {
            shownToolWindowHistory.ShowedToolWindow();
            mocker.Verify<IFileUtil>(f => f.WriteAllText(markerFilePath, string.Empty));
            shownToolWindowHistory.ShowedToolWindow();
            mocker.Verify<IFileUtil>(f => f.WriteAllText(markerFilePath, string.Empty),Times.Once());
        }

        [Test]
        public void It_Should_HasShownToolWindow_Without_Searching_For_Marker_File_When_ShowedToolWindow_Is_Invoked()
        {
            shownToolWindowHistory.ShowedToolWindow();
            mocker.Verify<IFileUtil>(f => f.Exists(It.IsAny<string>()), Times.Never());
            Assert.That(shownToolWindowHistory.HasShownToolWindow, Is.True);
        }

        [TestCase(true)]
        [TestCase(false)]
        public void When_ShowedToolWindow_Has_Not_Been_Invoked_Should_Search_For_Marker_File_Once_When_HasShownToolWindow(bool fileExists)
        {
            mocker.GetMock<IFileUtil>().Setup(f => f.Exists(markerFilePath)).Returns(fileExists);

            void HasShownToolWindow()
            {
                var hasShownToolWindow = shownToolWindowHistory.HasShownToolWindow;
                Assert.That(hasShownToolWindow, Is.EqualTo(fileExists));
            }
            HasShownToolWindow();
            HasShownToolWindow();

            mocker.Verify<IFileUtil>(f => f.Exists(markerFilePath), Times.Once());
        }
    }
}
