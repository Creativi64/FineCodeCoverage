using System;
using System.Linq;
using System.Diagnostics;
using System.Collections.Generic;
using Microsoft.VisualStudio.Shell;
using System.ComponentModel.Composition;
using Task = System.Threading.Tasks.Task;

namespace FineCodeCoverage.Output.Pane
{
    [Export(typeof(IShowFCCOutputPane))]
    [Export(typeof(ILogger))]
    internal class Logger : ILogger, IShowFCCOutputPane
    {
        private IFCCOutputWindowPane _pane;
        private readonly IFCCOutputWindowPaneCreator fccOutputWindowCreator;

        [ImportingConstructor]
        public Logger(
            IFCCOutputWindowPaneCreator fccOutputWindowCreator
        ) => this.fccOutputWindowCreator = fccOutputWindowCreator;

        private void LogImpl(object[] message, bool withTitle)
        {
            try
            {
                DateTime logTime = DateTime.Now;
                var messageList = new List<string>(message?.Select(x => x?.ToString()?.Trim(' ', '\r', '\n')).Where(x => !string.IsNullOrWhiteSpace(x)));

                if (!messageList.Any())
                {
                    return;
                }

                ThreadHelper.JoinableTaskFactory.Run(async () =>
                {
                    if (this._pane == null)
                    {
                        this._pane = await this.fccOutputWindowCreator.GetOrCreateAsync();
                    }

                    if (this._pane == null)
                    {
                        return;
                    }

                    string logs = string.Join(Environment.NewLine, messageList);

                    if (withTitle)
                    {
                        await this._pane.OutputStringThreadSafeAsync($"{Vsix.Name} {logTime}: {logs}{Environment.NewLine}");
                    }
                    else
                    {
                        await this._pane.OutputStringThreadSafeAsync($"{logs}{Environment.NewLine}");
                    }
                });
            }
            catch (Exception ex)
            {
                Debug.Write(ex);
            }
        }

        public void Log(params object[] message) => this.LogImpl(message, true);

        void ILogger.Log(params string[] message) => this.LogImpl(message, true);

        public void Log(IEnumerable<object> message) => this.LogImpl(message.ToArray(), true);

        public void Log(IEnumerable<string> message) => this.LogImpl(message.ToArray(), true);

        public void LogWithoutTitle(params object[] message) => this.LogImpl(message, false);

        public void LogWithoutTitle(params string[] message) => this.LogImpl(message, false);

        public void LogWithoutTitle(IEnumerable<object> message) => this.LogImpl(message.ToArray(), false);

        public void LogWithoutTitle(IEnumerable<string> message) => this.LogImpl(message.ToArray(), false);

        public async Task ShowAsync()
        {
            if (this._pane == null)
            {
                this._pane = await this.fccOutputWindowCreator.GetOrCreateAsync();
            }

            if (this._pane != null)
            {
                await this._pane.ShowAsync();
            }
        }
    }
}