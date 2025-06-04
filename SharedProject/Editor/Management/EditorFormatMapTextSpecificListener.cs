using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using Microsoft.VisualStudio.Text.Classification;

namespace FineCodeCoverage.Editor.Management
{
    [Export(typeof(IEditorFormatMapTextSpecificListener))]
    internal class EditorFormatMapTextSpecificListener : IEditorFormatMapTextSpecificListener
    {
        private List<string> _keys;
        private Action _callback;
        private bool _listening;

        [ImportingConstructor]
        public EditorFormatMapTextSpecificListener(
            IEditorFormatMapService editorFormatMapService
        ) => editorFormatMapService.GetEditorFormatMap("text").FormatMappingChanged += this.EditorFormatMap_FormatMappingChanged;

        private void EditorFormatMap_FormatMappingChanged(object sender, FormatItemsEventArgs e)
        {
            var watchedItems = e.ChangedItems.Where(changedItem => this._keys.Contains(changedItem)).ToList();
            if (!this._listening || watchedItems.Count == 0)
            {
                return;
            }

            this._callback();
        }

        public void ListenFor(List<string> keys, Action callback)
        {
            this._keys = keys;
            this._callback = callback;
            this._listening = true;
        }

        public void PauseListeningWhenExecuting(Action action)
        {
            this._listening = false;
            action();
            this._listening = true;
        }
    }
}