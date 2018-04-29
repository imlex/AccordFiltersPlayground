using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using AccordFiltersPlayground.Utils;

namespace AccordFiltersPlayground.Behaviors
{
    public class DataGridColumns : GridColumnsBase<DataGrid, DataGridColumn>
    {
        protected override void OnAttached()
        {
            AssociatedObject.AutoGenerateColumns = false;

            base.OnAttached();
        }

        protected override void ClearColumns(DataGrid grid)
        {
            grid.Columns.Clear();
        }

        protected override void AddColumn(DataGrid grid, DataGridColumn column)
        {
            grid.Columns.Add(column);
        }

        protected override DataGridColumn GetColumn(int columnIndex, object column)
        {
            if (column is DataGridColumn dataGridColumn)
                return dataGridColumn;

            if (column is string bindingPath)
                return new DataGridTextColumn {Header = bindingPath, Binding = new Binding(bindingPath)};

            DataGridTemplateColumn dataGridTemplateColumn = new DataGridTemplateColumn();
            if (HeaderTemplate != null)
            {
                FrameworkElement header = (FrameworkElement) HeaderTemplate.LoadContent();
                header.DataContext = column;
                dataGridTemplateColumn.Header = header;
            }

            if (CellTemplate != null)
                dataGridTemplateColumn.CellTemplate = RewriteCellTemplate(columnIndex);

            return dataGridTemplateColumn;
        }

        private DataTemplate RewriteCellTemplate(int columnIndex)
        {
            string xamlString = XamlUtils.ToXaml(CellTemplate);

            xamlString = xamlString.Replace("[0]", $"[{columnIndex}]");

            return XamlUtils.FromXaml<DataTemplate>(xamlString);
        }
    }
}