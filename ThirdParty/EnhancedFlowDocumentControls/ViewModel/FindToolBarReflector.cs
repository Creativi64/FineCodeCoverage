using System;
using System.Reflection;
using System.Windows.Controls;

namespace EnhancedFlowDocumentControls.ViewModel
{
    // Vendored change: the original used the Fasterflect package for these reflection lookups.
    // Replaced with plain System.Reflection to drop that third-party dependency (these run rarely
    // - once per find-toolbar - so the optimized delegates Fasterflect built are not needed).
    internal sealed class FindToolBarReflector : IFindToolBarReflector
    {
        private const BindingFlags InstancePrivate = BindingFlags.Instance | BindingFlags.NonPublic;
        private const BindingFlags InstancePublic = BindingFlags.Instance | BindingFlags.Public;

        private readonly ToolBar _findToolBar;
        private static bool s_isInitialized;
        private static FieldInfo s_optionsWholeWordMenuItemField;
        private static FieldInfo s_optionsCaseMenuItemField;
        private static FieldInfo s_optionsDiacriticMenuItemField;
        private static FieldInfo s_optionsKashidaMenuItemField;
        private static FieldInfo s_optionsAlefHamzaMenuItemField;
        private static PropertyInfo s_searchUpProperty;
        private static FieldInfo s_findTextBoxField;
        private static MethodInfo s_onFindClickMethod;

        public FindToolBarReflector(ToolBar findToolBar)
        {
            _findToolBar = findToolBar;
            if (s_isInitialized)
            {
                return;
            }

            Type findToolBarType = findToolBar.GetType();
            s_optionsWholeWordMenuItemField = findToolBarType.GetField("OptionsWholeWordMenuItem", InstancePrivate);
            s_optionsCaseMenuItemField = findToolBarType.GetField("OptionsCaseMenuItem", InstancePrivate);
            s_optionsDiacriticMenuItemField = findToolBarType.GetField("OptionsDiacriticMenuItem", InstancePrivate);
            s_optionsKashidaMenuItemField = findToolBarType.GetField("OptionsKashidaMenuItem", InstancePrivate);
            s_optionsAlefHamzaMenuItemField = findToolBarType.GetField("OptionsAlefHamzaMenuItem", InstancePrivate);
            s_searchUpProperty = findToolBarType.GetProperty("SearchUp", InstancePublic);
            s_findTextBoxField = findToolBarType.GetField("FindTextBox", InstancePrivate);
            s_onFindClickMethod = findToolBarType.GetMethod("OnFindClick", InstancePrivate);
            s_isInitialized = true;
        }

        public void InvokeFind() => s_onFindClickMethod.Invoke(_findToolBar, null);

        public void SetSearchUp(bool isSearchUp) => s_searchUpProperty.SetValue(_findToolBar, isSearchUp);

        public TextBox GetFindTextBox() => s_findTextBoxField.GetValue(_findToolBar) as TextBox;

        public MenuItem GetMatchWholeWordMenuItem() => s_optionsWholeWordMenuItemField.GetValue(_findToolBar) as MenuItem;

        public MenuItem GetMatchCaseMenuItem() => s_optionsCaseMenuItemField.GetValue(_findToolBar) as MenuItem;

        public MenuItem GetMatchDiacriticMenuItem() => s_optionsDiacriticMenuItemField.GetValue(_findToolBar) as MenuItem;

        public MenuItem GetMatchKashidaMenuItem() => s_optionsKashidaMenuItemField.GetValue(_findToolBar) as MenuItem;

        public MenuItem GetMatchAlefHamzaMenuItem() => s_optionsAlefHamzaMenuItemField.GetValue(_findToolBar) as MenuItem;
    }
}
