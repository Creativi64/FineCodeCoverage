using System;
using System.Windows.Controls;
using System.Windows.Documents;

namespace FineCodeCoverage.Readme
{
    public class TaskListRenderer : NotifyingObjectRenderer<TaskList>
    {
        protected override ElementAndMarker WriteAndReturn(WpfRenderer renderer, TaskList taskList)
        {
            if (renderer == null) throw new ArgumentNullException(nameof(renderer));
            if (taskList == null) throw new ArgumentNullException(nameof(taskList));

            var checkBox = new CheckBox
            {
                IsEnabled = false,
                IsChecked = taskList.Checked,

            };
            var inlineUIContainer = new InlineUIContainer(checkBox);
            renderer.WriteInline(inlineUIContainer);
            return new ElementAndMarker(checkBox, MarkdownTypeMarker.TaskList);
        }
    }
}
