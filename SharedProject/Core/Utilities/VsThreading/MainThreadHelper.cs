using System;
using FineCodeCoverage.Core.Utilities.Telemetry;
using Microsoft.VisualStudio.Shell;

namespace FineCodeCoverage.Core.Utilities
{
    /*
        https://learn.microsoft.com/en-us/dotnet/api/microsoft.visualstudio.shell.vstasklibraryhelper.fileandforget?view=visualstudiosdk-2022
        FileAndForget will 
        https://learn.microsoft.com/en-us/dotnet/api/microsoft.visualstudio.telemetry.telemetrysession.postevent?view=visualstudiosdk-2022#microsoft-visualstudio-telemetry-telemetrysession-postevent(microsoft-visualstudio-telemetry-telemetryevent)
        Microsoft.VisualStudio.Telemetry.TelemetryService.DefaultSession.PostEvent - a FaultEvent but not to Watson, to AI
        and
        https://learn.microsoft.com/en-us/dotnet/api/microsoft.visualstudio.shell.interop.ivsactivitylog.logentry?view=visualstudiosdk-2022
    */
    internal static class MainThreadHelper
    {
        public static void SwitchAndFileAndForget(
            FaultEventName faultEventName,
            Action action,
            string faultDescription = null)
#pragma warning disable VSSDK007
            => ThreadHelper.JoinableTaskFactory.RunAsync(async () =>
            {
                await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
                action();
            }).FileAndForget(faultEventName.ToString(), faultDescription);
#pragma warning restore VSSDK007

        private static void SwitchAndLogException(
            FaultEventName faultEventName,
            Action action,
            bool rethrow,
            string faultDescription = null
        ) => SwitchAndFileAndForget(
            faultEventName,
            () =>
            {
                try
                {
                    action();
                }
                catch (Exception ex)
                {
                    LoggerSingleton.Instance.LogFileAndForget(ex.ToString());
                    if (rethrow)
                    {
                        throw;
                    }
                }
            }, faultDescription);

        // given that catches and does not rethrow should be unnecessary
        private static readonly FaultEventName switchAndCatchFaultEventName = FCCFaultEventName.WithEntityName(nameof(MainThreadHelper))
              .BuildFromFeatureNameHierarchy("Utilities", "SwitchAndCatch");

        public static void SwitchAndLogException(Action action) => SwitchAndLogException(switchAndCatchFaultEventName, action, false);

        public static void SwitchLogExceptionRethrow(
            FaultEventName faultEventName,
            Action action,
            string faultDescription = null
        ) => SwitchAndLogException(faultEventName, action, true, faultDescription);
    }
}
