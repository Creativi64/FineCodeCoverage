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
                T newRoot = newRoots[newIndex];
                TreeExpansionState savedRoot = savedState.Roots[savedIndex];
                string newRootId = this.getId(newRoot);
                if (newRootId == savedRoot.Id)
                {
                    // Recursively restore expansion state for the root and its children
                    this.RestoreExpansionStateForNode(newRoot, savedRoot);
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
            string newTreeId = this.getId(newTree);
            if (newTreeId == savedState.Id)
            {
                this.setIsExpanded(newTree);

                int newIndex = 0, savedIndex = 0;
                IList<T> children = this.getChildren(newTree);
                while (newIndex < children.Count && savedIndex < savedState.Children.Count)
                {
                    T newChild = children[newIndex];
                    TreeExpansionState savedChild = savedState.Children[savedIndex];
                    string newChildId = this.getId(newChild);
                    if (newChildId == savedChild.Id)
                    {
                        this.RestoreExpansionStateForNode(newChild, savedChild);
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
            this.wrapper = new TreeExpansionStateWrapper();

            foreach (T root in roots)
            {
                TreeExpansionState state = this.SaveExpansionStateForNode(root);
                if (state != null)
                {
                    this.wrapper.Roots.Add(state);
                }
            }
        }

        private TreeExpansionState SaveExpansionStateForNode(T item)
        {
            if (!this.getIsExpanded(item))
                return null;

            var state = new TreeExpansionState { Id = this.getId(item) };
            IList<T> children = this.getChildren(item);
            foreach (T child in children)
            {
                TreeExpansionState childState = this.SaveExpansionStateForNode(child);
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
            this.SaveExpansionState(oldItems);
            this.RestoreExpansionState(newItems, this.wrapper);
            this.wrapper = null;
        }
    }
}