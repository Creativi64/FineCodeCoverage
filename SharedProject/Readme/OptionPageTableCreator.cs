using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Windows;
using System.Windows.Documents;

namespace FineCodeCoverage.Readme
{
    [Export(typeof(IOptionPageTableCreator))]
    internal class OptionPageTableCreator : IOptionPageTableCreator
    {
        private readonly List<ElementAndMarker> _elementAndMarkers = new List<ElementAndMarker>();
        private readonly IFCCOptionPageInfoProvider _fccOptionPageInfoProvider;
        private Table _table;
        private TableRowGroup _tableRowGroup;

        [ImportingConstructor]
        public OptionPageTableCreator(
            IFCCOptionPageInfoProvider fccOptionPageInfoProvider
        ) => this._fccOptionPageInfoProvider = fccOptionPageInfoProvider;

        public IReadOnlyList<ElementAndMarker> ElementAndMarkers => this._elementAndMarkers;

        private void AddElementAndMarker(TextElement element, MarkdownTypeMarker marker)
            => this._elementAndMarkers.Add(new ElementAndMarker(element, marker));

        public Table Create()
        {
            this.InitializeTable();
            foreach (OptionPageInfo optionPageInfo in this._fccOptionPageInfoProvider.Provide())
            {
                foreach (CategorizedOptionPropertyInfos propertyCategory in optionPageInfo.PropertyCategories)
                {
                    this.AddRow(
                        OptionPageTableDisplayInfo.PageNameCategoryDisplay(optionPageInfo.PageName, propertyCategory.Category),
                        "",
                        ""
                    );
                    foreach (OptionPropertyInfo optionPropertyInfo in propertyCategory.OptionPropertyInfos)
                    {
                        string isCoverageSettingDisplay = optionPropertyInfo.IsCoverageSetting
                            ? OptionPageTableDisplayInfo.IsCoverageSettingYes : OptionPageTableDisplayInfo.IsCoverageSettingNo;
                        this.AddRow(optionPropertyInfo.DisplayName, optionPropertyInfo.Description, isCoverageSettingDisplay);
                    }
                }
            }

            return this._table;
        }

        private void InitializeTable()
        {
            this._table = new Table();
            this.AddElementAndMarker(this._table, MarkdownTypeMarker.Table);
            this._table.Columns.Add(new TableColumn());
            this._table.Columns.Add(new TableColumn());
            this._table.Columns.Add(new TableColumn());
            this._tableRowGroup = new TableRowGroup();
            this._table.RowGroups.Add(this._tableRowGroup);
            this.AddRow(
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
                this.AddElementAndMarker(row, MarkdownTypeMarker.TableHeader);
            }

            this._tableRowGroup.Rows.Add(row);

            this.AddCell(row, cell1, isHeaderRow ? OptionPageTableCellAlignment.Left : OptionPageTableDisplayInfo.OptionCellAlignment);
            this.AddCell(row, cell2, isHeaderRow ? OptionPageTableCellAlignment.Left : OptionPageTableDisplayInfo.DescriptionCellAlignment);
            this.AddCell(row, cell3, isHeaderRow ? OptionPageTableCellAlignment.Left : OptionPageTableDisplayInfo.IsCoverageSettingCellAlignment);
        }

        private void AddCell(TableRow row, string cellStr, OptionPageTableCellAlignment alignment)
        {
            if (cellStr == null) return;
            var cell = new TableCell() { TextAlignment = GetTextAlignment(alignment) };
            var cellParagraph = new Paragraph(new Run(cellStr));
            cell.Blocks.Add(cellParagraph);
            this.AddElementAndMarker(cellParagraph, MarkdownTypeMarker.Paragraph);
            this.AddElementAndMarker(cell, MarkdownTypeMarker.TableCell);

            row.Cells.Add(cell);
        }

        private static TextAlignment GetTextAlignment(OptionPageTableCellAlignment alignment)
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
