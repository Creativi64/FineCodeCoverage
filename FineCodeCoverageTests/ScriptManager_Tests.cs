using System.Threading.Tasks;
using FineCodeCoverage.Core.Utilities;
using FineCodeCoverage.Engine;
using FineCodeCoverage.Output;
using Moq;
using NUnit.Framework;

namespace Test
{
    public class ScriptManager_When_Called_Back_Window_External
    {
        private ScriptManager scriptManager;
        private Mock<IProcess> mockProcess;

        [SetUp]
        public void SetUp()
        {
            mockProcess = new Mock<IProcess>();
            scriptManager = new ScriptManager(mockProcess.Object);
        }

        [Test]
        public void Buy_Me_A_Coffee_Should_Open_PayPal()
        {
            scriptManager.BuyMeACoffee();
            mockProcess.Verify(p => p.Start(ScriptManager.payPal));
        }

        [Test]
        public void LogIssueOrSuggestion_Should_Open_Github_Issues()
        {
            scriptManager.LogIssueOrSuggestion();
            mockProcess.Verify(p => p.Start(FCCGithub.Issues));
        }

        [Test]
        public void RateAndReview_Should_Open_Market_Place_Rate_And_Review()
        {
            scriptManager.RateAndReview();
            mockProcess.Verify(p => p.Start(ScriptManager.marketPlaceRateAndReview));
        }
    }
}