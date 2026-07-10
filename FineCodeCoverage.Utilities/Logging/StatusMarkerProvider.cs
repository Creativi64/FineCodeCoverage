namespace FineCodeCoverage.Utilities.Logging
{
    public static class StatusMarkerProvider
    {
        public static string Get(string status = "")
        {
            status = status.Length == 0 ? string.Empty : $" {status} ";
            return $"=================================={status.ToUpper()}==================================";
        }
    }
}
