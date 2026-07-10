using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;

namespace StylableFindFlowDocumentReader
{
    internal sealed class FindToolBarShimDecorator : Decorator
    {
        private readonly ToolBar _findToolBar;
        private readonly Decorator _originalDecorator;
        private readonly FindToolbarWrapper _findToolBarWrapper;

        public FindToolBarShimDecorator(Decorator originalDecorator, FrameworkElement findToolbarContent)
        {
            _findToolBar = originalDecorator.Child as ToolBar;
            _originalDecorator = originalDecorator;
            _findToolBarWrapper = new FindToolbarWrapper(_findToolBar);
            findToolbarContent.DataContext = new FindToolBarViewModel(_findToolBarWrapper);
            _originalDecorator.Child = findToolbarContent;
            originalDecorator.Replace(this);
            base.Child = _originalDecorator;
        }

        public void Find(bool searchUp)
        {
            _findToolBarWrapper.SetSearchUp(searchUp);
            _findToolBarWrapper.Find();
        }

        public void Find() => _findToolBarWrapper.Find();

        public override UIElement Child
        {
            get => ReturnOriginalFindToolBar(new StackTrace()) ? _findToolBar : (UIElement)_originalDecorator;

            set => base.Child = value;
        }

        private static bool ReturnOriginalFindToolBar(StackTrace stackTrace)
            => stackTrace.TypeIsACaller<FlowDocumentReader>();

        internal void ResetOriginalDecorator() => this.Replace(_originalDecorator);
    }
}
