namespace FineCodeCoverage.Output
{
    internal static class StatusMarkerProvider
    {
        internal static string Get(string status = "")
        {
            status = status.Length == 0 ? "" : $" {status} ";
            return $"=================================={status.ToUpper()}==================================";
        }
    }
}
