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
        ) => _fccOptionPageInfoProvider = fccOptionPageInfoProvider;

        public IReadOnlyList<ElementAndMarker> ElementAndMarkers => _elementAndMarkers;

        private void AddElementAndMarker(TextElement element, MarkdownTypeMarker marker)
            => _elementAndMarkers.Add(new ElementAndMarker(element, marker));

        public Table Create()
        {
            InitializeTable();
            foreach (OptionPageInfo optionPageInfo in _fccOptionPageInfoProvider.Provide())
            {
                foreach (CategorizedOptionPropertyInfos propertyCategory in optionPageInfo.PropertyCategories)
                {
                    AddRow(
                        OptionPageTableDisplayInfo.PageNameCategoryDisplay(optionPageInfo.PageName, propertyCategory.Category),
                        "",
                        ""
                    );
                    foreach (OptionPropertyInfo optionPropertyInfo in propertyCategory.OptionPropertyInfos)
                    {
                        string isCoverageSettingDisplay = optionPropertyInfo.IsCoverageSetting
                            ? OptionPageTableDisplayInfo.IsCoverageSettingYes : OptionPageTableDisplayInfo.IsCoverageSettingNo;
                        AddRow(optionPropertyInfo.DisplayName, optionPropertyInfo.Description, isCoverageSettingDisplay);
                    }
                }
            }

            return _table;
        }

        private void InitializeTable()
        {
            _table = new Table();
            AddElementAndMarker(_table, MarkdownTypeMarker.Table);
            _table.Columns.Add(new TableColumn());
            _table.Columns.Add(new TableColumn());
            _table.Columns.Add(new TableColumn());
            _tableRowGroup = new TableRowGroup();
            _table.RowGroups.Add(_tableRowGroup);
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

            _tableRowGroup.Rows.Add(row);

            AddCell(row, cell1, isHeaderRow ? OptionPageTableCellAlignment.Left : OptionPageTableDisplayInfo.OptionCellAlignment);
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
