namespace FineCodeCoverage.Editor.DynamicCoverage
{
    internal interface INewCodeTrackerFactory
    {
        INewCodeTracker Create(ILineExcluder lineExcluder);
    }
}
