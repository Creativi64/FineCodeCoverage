using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Windows.Controls;

namespace EnhancedFlowDocumentControls.ViewModel
{
    internal sealed class FindToolBarFinder : IFinder
    {
        private readonly IFindToolBarReflector _findToolBarReflector;
        private TextBox _findTextBox;

        private sealed class CachedMenuItem
        {
            private readonly Func<MenuItem> _provider;
            private MenuItem _menuItem;

            public CachedMenuItem(Func<MenuItem> provider) => _provider = provider;

            public MenuItem GetMenuItem() => _menuItem ?? (_menuItem = _provider());
        }

        private readonly Dictionary<string, CachedMenuItem> _menuItems;

        public FindToolBarFinder(ToolBar findToolbar)
            : this(findToolbar, new FindToolBarReflectorFactory())
        {
        }

        internal FindToolBarFinder(ToolBar findToolbar, IFindToolBarReflectorFactory findToolBarReflectorFactory)
        {
            _findToolBarReflector = findToolBarReflectorFactory.CreateReflector(findToolbar);
            _menuItems = new Dictionary<string, CachedMenuItem>
            {
                { nameof(SelectMatchAlefHamza), new CachedMenuItem(_findToolBarReflector.GetMatchAlefHamzaMenuItem) },
                { nameof(SelectMatchCase), new CachedMenuItem(_findToolBarReflector.GetMatchCaseMenuItem) },
                { nameof(SelectMatchDiacritic), new CachedMenuItem(_findToolBarReflector.GetMatchDiacriticMenuItem) },
                { nameof(SelectMatchKashida), new CachedMenuItem(_findToolBarReflector.GetMatchKashidaMenuItem) },
                { nameof(SelectMatchWholeWord), new CachedMenuItem(_findToolBarReflector.GetMatchWholeWordMenuItem) },
            };
        }

        public void Find(IFindParameters findParameters)
        {
            SetIfChanged(findParameters.FindText, SetFindText);
            SetIfChanged(findParameters.IsSearchUp, _findToolBarReflector.SetSearchUp);
            SetIfChanged(findParameters.MatchWholeWord, SelectMatchWholeWord);
            SetIfChanged(findParameters.MatchCase, SelectMatchCase);
            SetIfChanged(findParameters.MatchDiacritic, SelectMatchDiacritic);
            SetIfChanged(findParameters.MatchKashida, SelectMatchKashida);
            SetIfChanged(findParameters.MatchAlefHamza, SelectMatchAlefHamza);
            _findToolBarReflector.InvokeFind();
        }

        private void SelectMatchAlefHamza(bool selected) => SelectOption(selected);

        private void SelectMatchKashida(bool selected) => SelectOption(selected);

        private void SelectMatchDiacritic(bool selected) => SelectOption(selected);

        private void SelectMatchCase(bool selected) => SelectOption(selected);

        private void SelectMatchWholeWord(bool selected) => SelectOption(selected);

        private void SelectOption(bool selected, [CallerMemberName] string option = "")
            => _menuItems[option].GetMenuItem().IsChecked = selected;

        private static void SetIfChanged<T>(IFindParameter<T> findParameter, Action<T> setter)
        {
            if (!findParameter.Changed)
            {
                return;
            }

            setter(findParameter.Value);
            findParameter.Reset();
        }

        private void SetFindText(string findText)
        {
            if (_findTextBox == null)
            {
                _findTextBox = _findToolBarReflector.GetFindTextBox();
            }

            _findTextBox.Text = findText;
        }
    }
}
