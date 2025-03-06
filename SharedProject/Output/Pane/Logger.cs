using System;
using System.Linq;
using System.Diagnostics;
using System.Collections.Generic;
using Microsoft.VisualStudio.Shell;
using System.ComponentModel.Composition;
using Task = System.Threading.Tasks.Task;
using System.Globalization;
using System.Text;

namespace FineCodeCoverage.Output.Pane
{
    [Export(typeof(ILogger))]
    internal class Logger : ILogger
    {
        private readonly IFCCOutputWindowPaneCreator fccOutputWindowCreator;

        [ImportingConstructor]
        public Logger(
            IFCCOutputWindowPaneCreator fccOutputWindowCreator
        ) => this.fccOutputWindowCreator = fccOutputWindowCreator;

        private static string GetFormattedNow()
        {
            var stringBuilder = new StringBuilder();
            DateTime now = DateTime.Now;
            stringBuilder.Append('[');
            stringBuilder.Append(now.ToString("d", CultureInfo.CurrentCulture));
            stringBuilder.Append(' ');
            stringBuilder.Append(now.ToString("h:mm:ss.fff tt", CultureInfo.CurrentCulture));
            stringBuilder.Append(']');
            stringBuilder.Append(' ');
            return stringBuilder.ToString();
        }

        private IEnumerable<string> GetMessageList(IEnumerable<string> message)
        {
            return message?.Select(x => x?.Trim(' ', '\r', '\n')).Where(x => !string.IsNullOrWhiteSpace(x));
        }

        private async Task LogMessagesAsync(IEnumerable<string> messageList)
        {
            try
            {
                if (!messageList.Any())
                {
                    return;
                }

                var pane = await fccOutputWindowCreator.GetOrCreateAsync();
                if (pane == null) return;

                string logs = string.Join(Environment.NewLine, messageList);
                await pane.OutputStringThreadSafeAsync($"{GetFormattedNow()}: {logs}{Environment.NewLine}");

            }
            catch (Exception ex)
            {
                Debug.Write(ex);
            }
        }

        public Task LogAsync(IEnumerable<string> message)
        {
            return LogMessagesAsync(GetMessageList(message));
        }

        public Task LogAsync(params string[] message)
        {
            return LogAsync(message as IEnumerable<string>);
        }

        public void Log(params string[] message){
            ThreadHelper.JoinableTaskFactory.Run(async () => await LogAsync(message));
        }
    }
}