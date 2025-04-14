using System;
using System.Collections.Generic;

namespace FineCodeCoverage.Output
{
    public class TreeExpander<T>
    {
        private TreeExpansionStateWrapper wrapper;
        private readonly Func<T, string> getId;
        private readonly Func<T, bool> getIsExpanded;
        private readonly Action<T> setIsExpanded;
        private readonly Func<T, IList<T>> getChildren;

        private class TreeExpansionStateWrapper
        {
            public List<TreeExpansionState> Roots { get; set; } = new List<TreeExpansionState>();
        }

        private class TreeExpansionState
        {
            public string Id { get; set; }
            public List<TreeExpansionState> Children { get; set; } = new List<TreeExpansionState>();
        }

        private void RestoreExpansionState(IList<T> newRoots, TreeExpansionStateWrapper savedState)
        {
            if (newRoots == null || savedState == null)
                return;

            int newIndex = 0, savedIndex = 0;

            // Traverse roots using pointers
            while (newIndex < newRoots.Count && savedIndex < savedState.Roots.Count)
            {
                var newRoot = newRoots[newIndex];
                var savedRoot = savedState.Roots[savedIndex];
                var newRootId = getId(newRoot);
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
                return;
            var newTreeId = getId(newTree);
            if (newTreeId == savedState.Id)
            {
                setIsExpanded(newTree);

                int newIndex = 0, savedIndex = 0;
                var children = getChildren(newTree);
                while (newIndex < children.Count && savedIndex < savedState.Children.Count)
                {
                    var newChild = children[newIndex];
                    var savedChild = savedState.Children[savedIndex];
                    var newChildId = getId(newChild);
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
        }

        private void SaveExpansionState(IList<T> roots)
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

        private TreeExpansionState SaveExpansionStateForNode(T item)
        {
            if (!getIsExpanded(item))
                return null;

            var state = new TreeExpansionState { Id = getId(item) };
            var children = getChildren(item);
            foreach (var child in children)
            {
                var childState = SaveExpansionStateForNode(child);
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
            Func<T, IList<T>> getChildren
        )
        {
            this.getId = getId;
            this.getIsExpanded = getIsExpanded;
            this.setIsExpanded = setIsExpanded;
            this.getChildren = getChildren;
        }

        public void RestoreExpansionState(
            IList<T> oldItems, IList<T> newItems)
        {
            SaveExpansionState(oldItems);
            RestoreExpansionState(newItems, wrapper);
            this.wrapper = null;
        }
    }

}
