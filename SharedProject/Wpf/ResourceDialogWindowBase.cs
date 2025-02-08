namespace FineCodeCoverage.Wpf
{
    public abstract class ResourceDialogWindowBase<T> : BaseDialogWindow
    {
        public ResourceDialogWindowBase(IDialogViewModel dialogViewModel, string resourcePath) : base(dialogViewModel)
        {
            var resourceDictionary = ResourceDictionaryHelper.FromExecutingAssemembly(resourcePath);
            this.Resources.MergedDictionaries.Add(resourceDictionary);
        }
    }
}
