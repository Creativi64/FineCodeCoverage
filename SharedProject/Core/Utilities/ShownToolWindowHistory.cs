using System.ComponentModel.Composition;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using FineCodeCoverage.Core.Initialization;

namespace FineCodeCoverage.Core.Utilities
{
    [Export(typeof(IShownToolWindowHistory))]
    [Export(typeof(IAppDataFolderPathDependent))]
    internal class ShownToolWindowHistory : IShownToolWindowHistory, IAppDataFolderPathDependent
    {
        private readonly IFileUtil fileUtil;
        private bool hasShownToolWindow;
        private bool checkedFileExists;
        private string appDataFolderPath;

        [ImportingConstructor]
        public ShownToolWindowHistory(IFileUtil fileUtil) => this.fileUtil = fileUtil;

        private string ShownToolWindowFilePath => Path.Combine(this.appDataFolderPath, "outputWindowInitialized");

        public bool HasShownToolWindow
        {
            get
            {
                if (!this.hasShownToolWindow && !this.checkedFileExists)
                {
                    this.hasShownToolWindow = this.fileUtil.Exists(this.ShownToolWindowFilePath);
                    this.checkedFileExists = true;
                }

                return this.hasShownToolWindow;
            }
        }

        public void ShowedToolWindow()
        {
            if (!this.hasShownToolWindow)
            {
                this.hasShownToolWindow = true;
                this.fileUtil.WriteAllText(this.ShownToolWindowFilePath, string.Empty);
            }
        }

        public Task InitializeAsync(string appDataFolderPath, CancellationToken cancellationToken)
        {
            this.appDataFolderPath = appDataFolderPath;
            return Task.CompletedTask;
        }
    }
}
