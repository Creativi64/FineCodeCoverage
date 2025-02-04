namespace FineCodeCoverage.Wpf
{
    public abstract class ResourceDialogWindowBase<T> : BaseDialogWindow<T> where T:IDialogViewModel
    {
        public ResourceDialogWindowBase(T dialogViewModel, string resourcePath) : base(dialogViewModel)
        {
            var resourceDictionary = ResourceDictionaryHelper.FromExecutingAssemembly(resourcePath);
            this.Resources.MergedDictionaries.Add(resourceDictionary);
        }
    }
}
