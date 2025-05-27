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
        public ShownToolWindowHistory(IFileUtil fileUtil)
        {
            this.fileUtil = fileUtil;
        }

        private string ShownToolWindowFilePath => Path.Combine(appDataFolderPath, "outputWindowInitialized");

        public bool HasShownToolWindow
        {
            get
            {
                if (!hasShownToolWindow && !checkedFileExists)
                {
                    hasShownToolWindow = fileUtil.Exists(ShownToolWindowFilePath);
                    checkedFileExists = true;
                }
                return hasShownToolWindow;
            }
        }

        public void ShowedToolWindow()
        {
            if (!hasShownToolWindow)
            {
                hasShownToolWindow = true;
                fileUtil.WriteAllText(ShownToolWindowFilePath, string.Empty);
            }
        }

        public Task InitializeAsync(string appDataFolderPath, CancellationToken cancellationToken)
        {
            this.appDataFolderPath = appDataFolderPath;
            return Task.CompletedTask;
        }
    }
}
