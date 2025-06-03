using System;
using System.Collections.Generic;
using System.Windows;
using Markdig.Extensions.Tables;
using Markdig.Renderers;
using WpfTable = System.Windows.Documents.Table;
using WpfTableCell = System.Windows.Documents.TableCell;
using WpfTableColumn = System.Windows.Documents.TableColumn;
using WpfTableRow = System.Windows.Documents.TableRow;
using WpfTableRowGroup = System.Windows.Documents.TableRowGroup;

namespace FineCodeCoverage.Readme
{
    public class TableRenderer : NotifyingObjectRenderer<Table>
    {
        protected override List<ElementAndMarker> WriteAndReturns(WpfRenderer renderer, Table table)
        {
            var elementsAndMarkers = new List<ElementAndMarker>();
            if (renderer == null) throw new ArgumentNullException(nameof(renderer));
            if (table == null) throw new ArgumentNullException(nameof(table));

            var wpfTable = new WpfTable();
            elementsAndMarkers.Add(new ElementAndMarker(wpfTable, MarkdownTypeMarker.Table));
            foreach (TableColumnDefinition tableColumnDefinition in table.ColumnDefinitions)
            {
                float width = tableColumnDefinition?.Width ?? 0;
                wpfTable.Columns.Add(new WpfTableColumn
                {
                    Width = width != 0 ?
                        new GridLength(width, GridUnitType.Star) :
                        GridLength.Auto,
                });
            }

            var wpfRowGroup = new WpfTableRowGroup();

            renderer.Push(wpfTable);
            renderer.Push(wpfRowGroup);

            foreach (Markdig.Syntax.Block rowObj in table)
            {
                var row = (TableRow)rowObj;
                var wpfRow = new WpfTableRow();

                renderer.Push(wpfRow);

                if (row.IsHeader)
                {
                    elementsAndMarkers.Add(new ElementAndMarker(wpfRow, MarkdownTypeMarker.TableHeader));
                }

                for (int i = 0; i < row.Count; i++)
                {
                    Markdig.Syntax.Block cellObj = row[i];
                    var cell = (TableCell)cellObj;
                    var wpfCell = new WpfTableCell
                    {
                        ColumnSpan = cell.ColumnSpan,
                        RowSpan = cell.RowSpan,
                    };
                    elementsAndMarkers.Add(new ElementAndMarker(wpfCell, MarkdownTypeMarker.TableCell));

                    renderer.Push(wpfCell);
                    renderer.Write(cell);
                    renderer.Pop();

                    if (table.ColumnDefinitions.Count > 0)
                    {
                        int columnIndex = cell.ColumnIndex < 0 || cell.ColumnIndex >= table.ColumnDefinitions.Count
                            ? i
                            : cell.ColumnIndex;
                        columnIndex = columnIndex >= table.ColumnDefinitions.Count ? table.ColumnDefinitions.Count - 1 : columnIndex;
                        TableColumnAlign? alignment = table.ColumnDefinitions[columnIndex].Alignment;
                        if (alignment.HasValue)
                        {
                            switch (alignment)
                            {
                                case TableColumnAlign.Center:
                                    wpfCell.TextAlignment = TextAlignment.Center;
                                    break;
                                case TableColumnAlign.Right:
                                    wpfCell.TextAlignment = TextAlignment.Right;
                                    break;
                                case TableColumnAlign.Left:
                                    wpfCell.TextAlignment = TextAlignment.Left;
                                    break;
                            }
                        }
                    }
                }

                renderer.Pop();
            }

            renderer.Pop();
            renderer.Pop();
            return elementsAndMarkers;
        }
    }
}