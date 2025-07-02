using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Reflection;
using FineCodeCoverage.Utilities.Vs;
using Microsoft.VisualStudio.PlatformUI;

namespace FineCodeCoverage.Vs
{
    [Export(typeof(IDialogWindowService))]
    internal sealed class DialogWindowService : IDialogWindowService
    {
        private static readonly Type[] s_dialogWindowTypes;

        static DialogWindowService() => s_dialogWindowTypes = Assembly.GetExecutingAssembly().GetTypes()
                .Where(t => t.IsSubclassOf(typeof(DialogWindow)) && !t.IsAbstract).ToArray();

        public void Show(object viewModel) => ConstructDialogWindow(viewModel).Show();

        public void ShowModal(object viewModel) => ConstructDialogWindow(viewModel).ShowModal();

        private static DialogWindow ConstructDialogWindow(object viewModel)
        {
            Type viewModelType = viewModel.GetType();
            foreach (Type dialogWindowType in s_dialogWindowTypes)
            {
                ConstructorInfo ctor = GetConstructor(dialogWindowType, viewModelType);
                if (ctor != null)
                {
                    return ctor.Invoke(new[] { viewModel }) as DialogWindow;
                }
            }

            throw new ArgumentException(nameof(viewModel));
        }

        private static ConstructorInfo GetConstructor(Type dialogWindowType, Type viewModelType)
            => dialogWindowType.GetConstructor(new[] { viewModelType });
    }
}
