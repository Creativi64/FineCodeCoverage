using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using FineCodeCoverage.Core.Utilities.Telemetry;
using FineCodeCoverage.VSAbstractions.OutputWindow;
using Microsoft.VisualStudio.Shell;
using Task = System.Threading.Tasks.Task;

namespace FineCodeCoverage.Output.Pane
{
    [Export(typeof(ILogger))]
    internal sealed class Logger : ILogger
    {
        private readonly IFCCOutputWindowPaneWritableCreator _fccOutputWindowCreator;
        private readonly FaultEventName _logFaultEventName = FCCFaultEventName.Create<Logger>("LoggingSync");

        [ImportingConstructor]
        public Logger(
            IFCCOutputWindowPaneWritableCreator fccOutputWindowCreator) => _fccOutputWindowCreator = fccOutputWindowCreator;

        private static string GetFormattedNow()
        {
            DateTime now = DateTime.Now;
            return new StringBuilder()
                .Append('[')
                .Append(now.ToString("d", CultureInfo.CurrentCulture))
                .Append(' ')
                .Append(now.ToString("h:mm:ss.fff tt", CultureInfo.CurrentCulture))
                .Append(']')
                .Append(' ').ToString();
        }

        private static IEnumerable<string> GetMessageList(IEnumerable<string> message)
            => message?.Select(x => x?.Trim(' ', '\r', '\n')).Where(x => !string.IsNullOrWhiteSpace(x));

        private async Task LogMessagesAsync(IEnumerable<string> messageList)
        {
            try
            {
                if (!messageList.Any())
                {
                    return;
                }

                IFCCOutputWindowPaneWritable pane = await _fccOutputWindowCreator.GetOrCreateWritableAsync();
                if (pane == null)
                {
                    return;
                }

                string logs = string.Join(Environment.NewLine, messageList);
                await pane.OutputStringThreadSafeAsync($"{GetFormattedNow()}: {logs}{Environment.NewLine}");
            }
            catch (Exception ex)
            {
                Debug.Write(ex);
            }
        }

        public Task LogAsync(IEnumerable<string> message) => LogMessagesAsync(GetMessageList(message));

        public Task LogAsync(params string[] message) => LogAsync(message as IEnumerable<string>);

        public void LogFileAndForget(params string[] message)
            => LogAsync(message).FileAndForget(_logFaultEventName.ToString());
    }
}
