namespace FineCodeCoverage.Output
{
    public class CoverageTooltipViewModel
    {
        public CoverageTooltipViewModel(double percentage, double covered, double coverable, int? partial)
        {
            this.Percentage = percentage;
            this.Covered = covered;
            this.Coverable = coverable;
            this.Uncovered = this.Coverable - this.Covered - (this.Partial ?? 0);
            this.Partial = partial;
        }

        public double Percentage { get; }
        public double Uncovered { get; }
        public double Covered { get; }
        public double Coverable { get; }
        public int? Partial { get; }
    }
}
