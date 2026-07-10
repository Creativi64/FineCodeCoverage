using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using Microsoft.VisualStudio.Text.Classification;

namespace FineCodeCoverage.Editor.Management
{
    [Export(typeof(IEditorFormatMapTextSpecificListener))]
    internal sealed class EditorFormatMapTextSpecificListener : IEditorFormatMapTextSpecificListener
    {
        private List<string> _keys;
        private Action _callback;
        private bool _listening;

        [ImportingConstructor]
        public EditorFormatMapTextSpecificListener(
            IEditorFormatMapService editorFormatMapService)
            => editorFormatMapService.GetEditorFormatMap("text").FormatMappingChanged += EditorFormatMap_FormatMappingChanged;

        private void EditorFormatMap_FormatMappingChanged(object sender, FormatItemsEventArgs e)
        {
            var watchedItems = e.ChangedItems.Where(changedItem => _keys.Contains(changedItem)).ToList();
            if (!_listening || watchedItems.Count == 0)
            {
                return;
            }

            _callback();
        }

        public void ListenFor(List<string> keys, Action callback)
        {
            _keys = keys;
            _callback = callback;
            _listening = true;
        }

        public void PauseListeningWhenExecuting(Action action)
        {
            _listening = false;
            action();
            _listening = true;
        }
    }
}
