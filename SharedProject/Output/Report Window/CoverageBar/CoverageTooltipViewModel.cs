namespace FineCodeCoverage.Output
{
    public class CoverageTooltipViewModel
    {
        public CoverageTooltipViewModel(double percentage, double covered, double coverable, int? partial)
        {
            Percentage = percentage;
            Covered = covered;
            Coverable = coverable;
            Uncovered = Coverable - Covered - (Partial ?? 0);
            Partial = partial;
        }

        public double Percentage { get; }
        public double Uncovered { get; }
        public double Covered { get; }
        public double Coverable { get; }
        public int? Partial { get; }
    }

}
