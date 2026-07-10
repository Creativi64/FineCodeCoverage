namespace FineCodeCoverage.Editor.DynamicCoverage.NewCode
{
    internal interface INewCodeTrackerFactory
    {
        INewCodeTracker Create(ILineExcluder lineExcluder);
    }
}
