using System;
using System.Collections.Generic;

namespace FineCodeCoverage.Output
{
    public class TreeExpander<T>
    {
        private readonly Func<T, string> _getId;
        private readonly Func<T, bool> _getIsExpanded;
        private readonly Action<T> _setIsExpanded;
        private readonly Func<T, IReadOnlyList<T>> _getChildren;
        private TreeExpansionStateWrapper _wrapper;

        private sealed class TreeExpansionStateWrapper
        {
            public List<TreeExpansionState> Roots { get; set; } = new List<TreeExpansionState>();
        }

        private sealed class TreeExpansionState
        {
            public string Id { get; set; }

            public List<TreeExpansionState> Children { get; set; } = new List<TreeExpansionState>();
        }

        private void RestoreExpansionState(IList<T> newRoots, TreeExpansionStateWrapper savedState)
        {
            if (newRoots == null || savedState == null)
            {
                return;
            }

            int newIndex = 0, savedIndex = 0;

            // Traverse roots using pointers
            while (newIndex < newRoots.Count && savedIndex < savedState.Roots.Count)
            {
                T newRoot = newRoots[newIndex];
                TreeExpansionState savedRoot = savedState.Roots[savedIndex];
                string newRootId = _getId(newRoot);
                if (newRootId == savedRoot.Id)
                {
                    // Recursively restore expansion state for the root and its children
                    RestoreExpansionStateForNode(newRoot, savedRoot);
                    newIndex++;
                    savedIndex++;
                }
                else if (string.CompareOrdinal(newRootId, savedRoot.Id) < 0)
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

        private void RestoreExpansionStateForNode(T newTree, TreeExpansionState savedState)
        {
            if (newTree == null || savedState == null)
            {
                return;
            }

            string newTreeId = _getId(newTree);
            if (newTreeId != savedState.Id)
            {
                return;
            }

            _setIsExpanded(newTree);

            int newIndex = 0, savedIndex = 0;
            IReadOnlyList<T> children = _getChildren(newTree);
            while (newIndex < children.Count && savedIndex < savedState.Children.Count)
            {
                T newChild = children[newIndex];
                TreeExpansionState savedChild = savedState.Children[savedIndex];
                string newChildId = _getId(newChild);
                if (newChildId == savedChild.Id)
                {
                    RestoreExpansionStateForNode(newChild, savedChild);
                    newIndex++;
                    savedIndex++;
                }
                else if (string.CompareOrdinal(newChildId, savedChild.Id) < 0)
                {
                    newIndex++;
                }
                else
                {
                    savedIndex++;
                }
            }
        }

        private void SaveExpansionState(IList<T> roots)
        {
            _wrapper = new TreeExpansionStateWrapper();

            foreach (T root in roots)
            {
                TreeExpansionState state = SaveExpansionStateForNode(root);
                if (state != null)
                {
                    _wrapper.Roots.Add(state);
                }
            }
        }

        private TreeExpansionState SaveExpansionStateForNode(T item)
        {
            if (!_getIsExpanded(item))
            {
                return null;
            }

            var state = new TreeExpansionState { Id = _getId(item) };
            IReadOnlyList<T> children = _getChildren(item);
            foreach (T child in children)
            {
                TreeExpansionState childState = SaveExpansionStateForNode(child);
                if (childState != null)
                {
                    state.Children.Add(childState);
                }
            }

            return state;
        }

        public TreeExpander(
            Func<T, string> getId,
            Func<T, bool> getIsExpanded,
            Action<T> setIsExpanded,
            Func<T, IReadOnlyList<T>> getChildren)
        {
            _getId = getId;
            _getIsExpanded = getIsExpanded;
            _setIsExpanded = setIsExpanded;
            _getChildren = getChildren;
        }

        public void RestoreExpansionState(
            IList<T> oldItems, IList<T> newItems)
        {
            SaveExpansionState(oldItems);
            RestoreExpansionState(newItems, _wrapper);
            _wrapper = null;
        }
    }
}
