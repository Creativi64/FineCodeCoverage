using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;

namespace FineCodeCoverage.Output
{
    [Export(typeof(ITreeExpander))]
    public class TreeExpander : ITreeExpander
    {
        private TreeExpansionStateWrapper wrapper;

        private class TreeExpansionStateWrapper
        {
            public List<TreeExpansionState> Roots { get; set; } = new List<TreeExpansionState>();
        }

        private class TreeExpansionState
        {
            public string Id { get; set; }
            public List<TreeExpansionState> Children { get; set; } = new List<TreeExpansionState>();
        }

        private void RestoreExpansionState(IList<ReportTreeItemBase> newRoots, TreeExpansionStateWrapper savedState)
        {
            if (newRoots == null || savedState == null)
                return;

            int newIndex = 0, savedIndex = 0;

            // Traverse roots using pointers
            while (newIndex < newRoots.Count && savedIndex < savedState.Roots.Count)
            {
                var newRoot = newRoots[newIndex];
                var savedRoot = savedState.Roots[savedIndex];

                if (newRoot.Name == savedRoot.Id)
                {
                    // Recursively restore expansion state for the root and its children
                    RestoreExpansionStateForNode(newRoot, savedRoot);
                    newIndex++;
                    savedIndex++;
                }
                else if (string.Compare(newRoot.Name, savedRoot.Id, StringComparison.Ordinal) < 0)
                {
                    // New tree has additional roots; skip them
                    newIndex++;
                }
                else
                {
                    // Old state has roots no longer present; skip them
                    savedIndex++;
                }
            }
        }

        private void RestoreExpansionStateForNode(ReportTreeItemBase newTree, TreeExpansionState savedState)
        {
            if (newTree == null || savedState == null)
                return;

            if (newTree.Name == savedState.Id)
            {
                newTree.IsExpanded = true;

                int newIndex = 0, savedIndex = 0;

                while (newIndex < newTree.observableChildren.Count && savedIndex < savedState.Children.Count)
                {
                    var newChild = newTree.observableChildren[newIndex];
                    var savedChild = savedState.Children[savedIndex];

                    if (newChild.Name == savedChild.Id)
                    {
                        RestoreExpansionStateForNode(newChild, savedChild);
                        newIndex++;
                        savedIndex++;
                    }
                    else if (string.Compare(newChild.Name, savedChild.Id, StringComparison.Ordinal) < 0)
                    {
                        newIndex++;
                    }
                    else
                    {
                        savedIndex++;
                    }
                }
            }
        }

        private void SaveExpansionState(IList<ReportTreeItemBase> roots)
        {
            wrapper = new TreeExpansionStateWrapper();

            foreach (var root in roots)
            {
                var state = SaveExpansionStateForNode(root);
                if (state != null)
                {
                    wrapper.Roots.Add(state);
                }
            }
        }

        private TreeExpansionState SaveExpansionStateForNode(ReportTreeItemBase item)
        {
            if (item == null || !item.IsExpanded)
                return null;

            var state = new TreeExpansionState { Id = item.Name };

            foreach (var child in item.observableChildren)
            {
                var childState = SaveExpansionStateForNode(child);
                if (childState != null)
                {
                    state.Children.Add(childState);
                }
            }

            return state;
        }

        public void RestoreExpansionState(IList<ReportTreeItemBase> oldItems, IList<ReportTreeItemBase> newItems)
        {
            SaveExpansionState(oldItems);
            RestoreExpansionState(newItems, wrapper);
            this.wrapper = null;
        }
        
    }

}
