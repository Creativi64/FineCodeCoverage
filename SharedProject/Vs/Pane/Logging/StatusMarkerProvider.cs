namespace FineCodeCoverage.Output
{
    internal static class StatusMarkerProvider
    {
        internal static string Get(string status = "")
        {
            status = status.Length == 0 ? string.Empty : $" {status} ";
            return $"=================================={status.ToUpper()}==================================";
        }
    }
}
