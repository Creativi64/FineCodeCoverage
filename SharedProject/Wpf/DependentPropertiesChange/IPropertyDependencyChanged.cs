namespace FineCodeCoverage.Wpf
{
    public interface IPropertyDependencyChanged
    {
        void NotifyDependentPropertyChanged(string propertyName);
    }
}
