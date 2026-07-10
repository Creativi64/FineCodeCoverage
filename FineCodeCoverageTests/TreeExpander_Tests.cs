using FineCodeCoverage.Output;
using NUnit.Framework;
using System.Collections.Generic;

namespace FineCodeCoverageTests
{
    internal class TreeExpander_Tests
    {
        class TreeItem { 
            public string Id { get; set; }
            public bool IsExpanded { get; set; }
            public List<TreeItem> Children { get; set; }
        }
        private readonly TreeExpander<TreeItem> treeExpander = new TreeExpander<TreeItem>(ti => ti.Id, ti => ti.IsExpanded, ti => ti.IsExpanded = true, ti => ti.Children);

        [TestCase(true)]
        [TestCase(false)]
        public void Should_Restore_Expansion_State_Of_Single(bool oldIsExpanded)
        {
            var newTreeItem = CreateRootTreeItem();

            treeExpander.RestoreExpansionState(new List<TreeItem> { CreateRootTreeItem(oldIsExpanded) }, new List<TreeItem> { newTreeItem });

            Assert.That(newTreeItem.IsExpanded, Is.EqualTo(oldIsExpanded));

            TreeItem CreateRootTreeItem(bool isExpanded = false)
            {
                return new TreeItem
                {
                    Id = "root",
                    IsExpanded = isExpanded,
                    Children = new List<TreeItem>
                    {
                        new TreeItem { Id = "child" }
                    }
                };
            }
        }

        [TestCase(true)]
        [TestCase(false)]
        public void Should_Restore_Expansion_State_Of_Single_Depth_2(bool oldIsExpanded)
        {
            var newTreeItem = CreateRootTreeItem();

            treeExpander.RestoreExpansionState(new List<TreeItem> { CreateRootTreeItem(oldIsExpanded) }, new List<TreeItem> { newTreeItem });

            Assert.That(newTreeItem.Children[0].IsExpanded, Is.EqualTo(oldIsExpanded));

            TreeItem CreateRootTreeItem(bool isExpanded = false)
            {
                return new TreeItem
                {
                    Id = "root",
                    IsExpanded = isExpanded,
                    Children = new List<TreeItem>
                    {
                        new TreeItem { Id = "child", IsExpanded = isExpanded, Children = new List<TreeItem>{ new TreeItem { Id="Grandchild"} } }
                    }
                };
            }
        }

        [TestCase(false,false)]
        [TestCase(false, true)]
        [TestCase(true, false)]
        [TestCase(true, true)]
        public void Should_Restore_Expansion_State_Of_Second(bool oldFirstIsExpanded, bool oldSecondIsExpanded)
        {
            var newTree = CreateTree();
            treeExpander.RestoreExpansionState(CreateTree(oldFirstIsExpanded, oldSecondIsExpanded),newTree);

            Assert.That(newTree[1].IsExpanded, Is.EqualTo(oldSecondIsExpanded));

            List<TreeItem> CreateTree(bool firstIsExpanded = false, bool secondIsExpanded = false)
            {
                return new List<TreeItem>
                {
                    new TreeItem { Id = "root1", IsExpanded = firstIsExpanded, Children = new List<TreeItem>{ new TreeItem { Id = "root1child"} } },
                    new TreeItem { Id = "root2", IsExpanded = secondIsExpanded, Children = new List<TreeItem>{ new TreeItem { Id = "root2child"} } },
                };
            }
        }
    }
}
