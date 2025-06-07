using AutoMoq;
using FineCodeCoverage.Core.Initialization;
using FineCodeCoverage.Core.Utilities;
using Moq;
using NUnit.Framework;
using System.Threading;
using System.Threading.Tasks;

namespace FineCodeCoverageTests
{
    internal class FirstTimeToolWindowOpener_Tests
    {
        private AutoMoqer mocker;
        private FirstTimeToolWindowOpener firstTimeToolWindowOpener;

        [SetUp]
        public void   SetUp()  {
            mocker = new AutoMoqer();
            firstTimeToolWindowOpener = mocker.Create<FirstTimeToolWindowOpener>();
        }

        [TestCase(true,false,true)]
        [TestCase(true, true, false)]
        [TestCase(false, false, false)]
        [TestCase(false, true, false)]
        public async Task It_Should_Open_If_Have_Never_Shown_The_ToolWindow_And_InitializedFromTestContainerDiscoverer_Async(
            bool initializedFromTestContainerDiscoverer,
            bool hasShownToolWindow,
            bool expectedShown
            )
        {
            mocker.GetMock<IInitializedFromTestContainerDiscoverer>().Setup(x => x.InitializedFromTestContainerDiscoverer).Returns(initializedFromTestContainerDiscoverer);
            mocker.GetMock<IShownReportToolWindowHistory>().Setup(x => x.HasShownToolWindow).Returns(hasShownToolWindow);

            await firstTimeToolWindowOpener.OpenIfFirstTimeAsync(CancellationToken.None);

            var expectedTimes = expectedShown ? Times.Once() : Times.Never();
#pragma warning disable VSTHRD110 // Observe result of async calls
            mocker.Verify<IReportToolWindowOpener>(toolWindowOpener => toolWindowOpener.TryOpenAsync(), expectedTimes);
#pragma warning restore VSTHRD110 // Observe result of async calls

        }
    }
}
