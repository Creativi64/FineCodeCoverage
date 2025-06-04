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
        private readonly IFileUtil _fileUtil;
        private bool _hasShownToolWindow;
        private bool _checkedFileExists;
        private string _appDataFolderPath;

        [ImportingConstructor]
        public ShownToolWindowHistory(IFileUtil fileUtil) => this._fileUtil = fileUtil;

        private string ShownToolWindowFilePath => Path.Combine(this._appDataFolderPath, "outputWindowInitialized");

        public bool HasShownToolWindow
        {
            get
            {
                if (!this._hasShownToolWindow && !this._checkedFileExists)
                {
                    this._hasShownToolWindow = this._fileUtil.Exists(this.ShownToolWindowFilePath);
                    this._checkedFileExists = true;
                }

                return this._hasShownToolWindow;
            }
        }

        public void ShowedToolWindow()
        {
            if (!this._hasShownToolWindow)
            {
                this._hasShownToolWindow = true;
                this._fileUtil.WriteAllText(this.ShownToolWindowFilePath, string.Empty);
            }
        }

        public Task InitializeAsync(string appDataFolderPath, CancellationToken cancellationToken)
        {
            this._appDataFolderPath = appDataFolderPath;
            return Task.CompletedTask;
        }
    }
}