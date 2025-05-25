using System.ComponentModel.Composition;
using System.Windows.Documents;

namespace FineCodeCoverage.Readme
{
    [Export(typeof(IOptionPageTableCreator))]
    internal class OptionPageTableCreator : IOptionPageTableCreator
    {
        private readonly IFCCOptionPageInfoProvider fccOptionPageInfoProvider;

        [ImportingConstructor]
        public OptionPageTableCreator(
            IFCCOptionPageInfoProvider fccOptionPageInfoProvider
        )
        {
            this.fccOptionPageInfoProvider = fccOptionPageInfoProvider;
        }

        public Table Create()
        {
            // temp
            var table = new Table();
            table.Columns.Add(new TableColumn());
            var tableRowGroup = (new TableRowGroup());
            table.RowGroups.Add(tableRowGroup);
            var row = new TableRow();
            var cell = new TableCell();
            cell.Blocks.Add(new Paragraph(new Run("This is a table cell.")));
            row.Cells.Add(cell);
            tableRowGroup.Rows.Add(row);
            return table;
        }
    }

}
