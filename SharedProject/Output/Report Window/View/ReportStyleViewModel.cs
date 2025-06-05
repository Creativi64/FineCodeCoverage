namespace FineCodeCoverage.Output
{
    internal class ReportStyleViewModel
    {
        public ReportStyleViewModel(ReportStyle reportStyle, string display)
        {
            ReportStyle = reportStyle;
            Display = display;
        }

        public ReportStyle ReportStyle { get; }

        public string Display { get; }

        public override string ToString() => Display;
    }
}
