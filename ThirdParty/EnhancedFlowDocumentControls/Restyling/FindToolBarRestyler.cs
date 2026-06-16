using System.Windows;
using System.Windows.Controls;

namespace EnhancedFlowDocumentControls.Restyling
{
    /// <summary>
    /// Have decided not to use this as FindToolbar works perfectly.
    /// </summary>
    public class FindToolBarRestyler
    {
        public virtual void DoRestyleFindToolBar(ToolBar findToolBar)
        {
            RestyleFindToolBar(findToolBar);
            if (!(findToolBar.FindName("FindTextBoxBorder") is Border findTextBorder))
            {
                return;
            }

            RestyleFindTextBoxBorder(findTextBorder);

            Grid findTextBoxGrid = findTextBorder.FindName("FindTextBoxGrid") as Grid;
            Label findTextLabel = findTextBorder.FindName("FindTextLabel") as Label;
            TextBox findTextBox = findTextBorder.FindName("FindTextBox") as TextBox;
            RestyleFindTextControls(findTextBox, findTextLabel, findTextBoxGrid);

            var findPreviousButton = findTextBorder.FindName("FindPreviousButton") as Button;
            var findNextButton = findTextBorder.FindName("FindNextButton") as Button;
            if (findPreviousButton != null || findNextButton != null)
            {
                RestyleFindButtons(findNextButton, findPreviousButton);
            }

            if (!(findTextBorder.FindName("OptionsMenu") is Menu menu))
            {
                return;
            }

            if (!(menu.FindName("OptionsMenuItem") is MenuItem menuItem))
            {
                return;
            }

            RestyleOptionsMenu(menu, menuItem);
        }

        /// <summary>
        /// Applies style findToolBarToolBarStyle if finds it FindResource.
        /// </summary>
        /// <param name="findToolBar">The FindToolBar to restyle.</param>
        protected virtual void RestyleFindToolBar(ToolBar findToolBar) => ApplyRestyle(findToolBar);

        /// <summary>
        /// Invokes the individual methods if not null.
        /// </summary>
        /// <param name="findTextBox">The TextBox to restyle.</param>
        /// <param name="findLabel">The Label to restyle.</param>
        /// <param name="findGrid">The Grid to restyle.</param>
        protected virtual void RestyleFindTextControls(TextBox findTextBox, Label findLabel, Grid findGrid)
        {
            if (findTextBox != null)
            {
                RestyleFindTextBox(findTextBox);
            }

            if (findLabel != null)
            {
                RestyleFindTextLabel(findLabel);
            }

            if (findGrid == null)
            {
                return;
            }

            RestyleFindGrid(findGrid);
        }

        protected virtual void RestyleFindGrid(Grid grid) => ApplyRestyle(grid);

        // this has BorderBrush "{DynamicResource {x:Static JetSystemColors.ControlDarkBrushKey}}"
        // and Background that is LinearGradient with GotFocus background gradient stop animations

        /// <summary>
        /// Applies style findToolBarBorderStyle if finds it FindResource.
        /// </summary>
        /// <param name="findTextBorder">The Border to restyle.</param>
        protected virtual void RestyleFindTextBoxBorder(Border findTextBorder) => ApplyRestyle(findTextBorder);

        // Default Foreground is GrayTextBrushKey.

        /// <summary>
        /// Applies style findToolBarLabelStyle if finds it FindResource.
        /// </summary>
        /// <param name="findTextLabel">The placeholder Label to restyle.</param>
        protected virtual void RestyleFindTextLabel(Label findTextLabel) => ApplyRestyle(findTextLabel);

        // Default Background is Transparent.

        /// <summary>
        /// Applies style findToolBarTextBoxStyle if finds it FindResource.
        /// </summary>
        /// <param name="findTextBox">The TextBox to restyle.</param>
        protected virtual void RestyleFindTextBox(TextBox findTextBox) => ApplyRestyle(findTextBox);

        // Default style with control template that has triggers that change the Border and Background.

        /// <summary>
        /// Applies style findToolBarButtonStyle if finds it FindResource.
        /// </summary>
        /// <param name="findNextButton">The next button to restyle.</param>
        /// <param name="findPreviousButton">The previous button to restyle.</param>
        protected virtual void RestyleFindButtons(Button findNextButton, Button findPreviousButton)
        {
            RestyleButton(findNextButton);
            RestyleButton(findPreviousButton);
        }

        private void RestyleButton(Button button) => ApplyRestyle(button);

        /// <summary>
        /// Applies style findToolBarMenuStyle and findToolBarMenuItemStyle if finds it FindResource.
        /// </summary>
        /// <param name="menu">The Menu to restyle.</param>
        /// <param name="rootMenuItem">The root MenuItem to restyle.</param>
        protected virtual void RestyleOptionsMenu(Menu menu, MenuItem rootMenuItem)
        {
            ApplyRestyle(menu);
            ApplyRestyle(rootMenuItem);
        }

        protected void ApplyRestyle(FrameworkElement element, string styleSuffix = null)
        {
            if (element == null)
            {
                return;
            }

            styleSuffix = styleSuffix ?? element.GetType().Name;
            if (!(element.TryFindResource($"findToolBar{styleSuffix}Style") is Style style))
            {
                return;
            }

            element.Style = style;
        }
    }
}
