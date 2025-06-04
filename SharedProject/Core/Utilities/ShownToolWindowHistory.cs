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
        public ShownToolWindowHistory(IFileUtil fileUtil) => _fileUtil = fileUtil;

        private string ShownToolWindowFilePath => Path.Combine(_appDataFolderPath, "outputWindowInitialized");

        public bool HasShownToolWindow
        {
            get
            {
                if (!_hasShownToolWindow && !_checkedFileExists)
                {
                    _hasShownToolWindow = _fileUtil.Exists(ShownToolWindowFilePath);
                    _checkedFileExists = true;
                }

                return _hasShownToolWindow;
            }
        }

        public void ShowedToolWindow()
        {
            if (_hasShownToolWindow)
            {
                return;
            }

            _hasShownToolWindow = true;
            _fileUtil.WriteAllText(ShownToolWindowFilePath, string.Empty);
        }

        public Task InitializeAsync(string appDataFolderPath, CancellationToken cancellationToken)
        {
            _appDataFolderPath = appDataFolderPath;
            return Task.CompletedTask;
        }
    }
}
