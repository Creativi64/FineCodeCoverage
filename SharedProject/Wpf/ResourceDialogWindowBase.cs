namespace FineCodeCoverage.Wpf
{
    public abstract class ResourceDialogWindowBase<T> : BaseDialogWindow
    {
        protected ResourceDialogWindowBase(IDialogViewModel dialogViewModel, string resourcePath) : base(dialogViewModel)
        {
            var resourceDictionary = ResourceDictionaryHelper.FromExecutingAssemembly(resourcePath);
            this.Resources.MergedDictionaries.Add(resourceDictionary);
        }
    }
}
