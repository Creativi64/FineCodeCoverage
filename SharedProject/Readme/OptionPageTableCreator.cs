using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Windows;
using System.Windows.Documents;

namespace FineCodeCoverage.Readme
{
    [Export(typeof(IOptionPageTableCreator))]
    internal class OptionPageTableCreator : IOptionPageTableCreator
    {
        private readonly List<ElementAndMarker> elementAndMarkers = new List<ElementAndMarker>();
        private readonly IFCCOptionPageInfoProvider fccOptionPageInfoProvider;
        private Table table;
        private TableRowGroup tableRowGroup;

        [ImportingConstructor]
        public OptionPageTableCreator(
            IFCCOptionPageInfoProvider fccOptionPageInfoProvider
        )
        {
            this.fccOptionPageInfoProvider = fccOptionPageInfoProvider;
        }

        public IReadOnlyList<ElementAndMarker> ElementAndMarkers => elementAndMarkers;

        private void AddElementAndMarker(TextElement element, MarkdownTypeMarker marker)
        {
            elementAndMarkers.Add(new ElementAndMarker(element, marker));
        }

        public Table Create()
        {
            InitializeTable();
            foreach (var optionPageInfo in fccOptionPageInfoProvider.Provide())
            {
                foreach (var propertyCategory in optionPageInfo.PropertyCategories)
                {
                    AddRow(
                        OptionPageTableDisplayInfo.PageNameCategoryDisplay(optionPageInfo.PageName,propertyCategory.Category),
                        "",
                        ""
                    );
                    foreach (var optionPropertyInfo in propertyCategory.OptionPropertyInfos)
                    {
                        var isCoverageSettingDisplay = optionPropertyInfo.IsCoverageSetting
                            ? OptionPageTableDisplayInfo.IsCoverageSettingYes : OptionPageTableDisplayInfo.IsCoverageSettingNo;
                        AddRow(optionPropertyInfo.DisplayName, optionPropertyInfo.Description, isCoverageSettingDisplay);
                    }
                }
            }

            return table;
        }

        private void InitializeTable()
        {
            this.table = new Table();
            AddElementAndMarker(table, MarkdownTypeMarker.Table);
            table.Columns.Add(new TableColumn());
            table.Columns.Add(new TableColumn());
            table.Columns.Add(new TableColumn());
            this.tableRowGroup = (new TableRowGroup());
            table.RowGroups.Add(tableRowGroup);
            AddRow(
                OptionPageTableDisplayInfo.OptionHeader,
                OptionPageTableDisplayInfo.DescriptionHeader,
                OptionPageTableDisplayInfo.IsCoverageSettingHeader,
            true);
        }

        private void AddRow(string cell1, string cell2, string cell3, bool isHeaderRow = false)
        {
            var row = new TableRow();
            if (isHeaderRow)
            {
                AddElementAndMarker(row, MarkdownTypeMarker.TableHeader);
            }

            tableRowGroup.Rows.Add(row);

            AddCell(row, cell1,isHeaderRow ? OptionPageTableCellAlignment.Left : OptionPageTableDisplayInfo.OptionCellAlignment);
            AddCell(row, cell2, isHeaderRow ? OptionPageTableCellAlignment.Left : OptionPageTableDisplayInfo.DescriptionCellAlignment);
            AddCell(row, cell3, isHeaderRow ? OptionPageTableCellAlignment.Left : OptionPageTableDisplayInfo.IsCoverageSettingCellAlignment);
        }

        private void AddCell(TableRow row, string cellStr, OptionPageTableCellAlignment alignment)
        {
            if (cellStr == null) return;
            var cell = new TableCell() { TextAlignment = GetTextAlignment(alignment) };
            var cellParagraph = new Paragraph(new Run(cellStr));
            cell.Blocks.Add(cellParagraph);
            AddElementAndMarker(cellParagraph, MarkdownTypeMarker.Paragraph);
            AddElementAndMarker(cell, MarkdownTypeMarker.TableCell);

            row.Cells.Add(cell);
        }

        private TextAlignment GetTextAlignment(OptionPageTableCellAlignment alignment)
        {
            switch (alignment)
            {
                case OptionPageTableCellAlignment.Right:
                    return TextAlignment.Right;
                case OptionPageTableCellAlignment.Center:
                    return TextAlignment.Center;
            }
            return TextAlignment.Left;
        }
    }

}
