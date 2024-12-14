using System;
using System.ComponentModel.Composition;

namespace FineCodeCoverage.Output
{
    internal class FCCGithub
    {
        internal const string Readme = "https://github.com/FortuneN/FineCodeCoverage/blob/master/README.md";
        internal const string Issues = "https://github.com/FortuneN/FineCodeCoverage/issues";
    }

    public class ScriptManager
    {
        internal const string payPal = "https://paypal.me/FortuneNgwenya";
        
        internal const string marketPlaceRateAndReview = "https://marketplace.visualstudio.com/items?itemName=FortuneNgwenya.FineCodeCoverage&ssr=false#review-details";
        private readonly IProcess process;
        internal System.Threading.Tasks.Task openFileTask;
        public event EventHandler ClearFCCWindowLogsEvent;
        public event EventHandler ShowFCCOutputPaneEvent;

        [ImportingConstructor]
        internal ScriptManager(IProcess process)
        {
            this.process = process;
        }
        
        public void ReadReadMe()
        {
            process.Start(FCCGithub.Readme);
        }

        public void BuyMeACoffee()
        {
            process.Start(payPal);
        }

        public void LogIssueOrSuggestion()
        {
            process.Start(FCCGithub.Issues);
        }

        public void RateAndReview()
        {
            process.Start(marketPlaceRateAndReview);
        }

        public void ShowFCCOutputPane()
        {
            ShowFCCOutputPaneEvent?.Invoke(this, EventArgs.Empty);
        }

    }
}