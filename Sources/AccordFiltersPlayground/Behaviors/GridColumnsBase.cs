using System.Collections;
using System.Collections.Specialized;
using System.Windows;
using System.Windows.Interactivity;

namespace AccordFiltersPlayground.Behaviors
{
    public abstract class GridColumnsBase<TGrid, TColumn> : Behavior<TGrid> where TGrid : DependencyObject
    {
        public static readonly DependencyProperty HeaderTemplateProperty = DependencyProperty.Register(
            nameof(HeaderTemplate), typeof(DataTemplate), typeof(GridColumnsBase<TGrid, TColumn>), new PropertyMetadata(OnHeaderTemplateChanged));

        private static void OnHeaderTemplateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            GridColumnsBase<TGrid, TColumn> gridColumnsBase = (GridColumnsBase<TGrid, TColumn>)d;

            gridColumnsBase.OnColumnsChanged(d, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }

        public DataTemplate HeaderTemplate
        {
            get { return (DataTemplate)GetValue(HeaderTemplateProperty); }
            set { SetValue(HeaderTemplateProperty, value); }
        }


        public static readonly DependencyProperty CellTemplateProperty = DependencyProperty.Register(
            nameof(CellTemplate), typeof(DataTemplate), typeof(GridColumnsBase<TGrid, TColumn>), new PropertyMetadata(OnCellTemplateChanged));

        private static void OnCellTemplateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            GridColumnsBase<TGrid, TColumn> gridColumnsBase = (GridColumnsBase<TGrid, TColumn>)d;

            gridColumnsBase.OnColumnsChanged(d, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }

        public DataTemplate CellTemplate
        {
            get { return (DataTemplate)GetValue(CellTemplateProperty); }
            set { SetValue(CellTemplateProperty, value); }
        }


        public static readonly DependencyProperty ColumnsProperty = 
            DependencyProperty.Register(nameof(Columns), typeof(IEnumerable), typeof(GridColumnsBase<TGrid, TColumn>), new PropertyMetadata(OnColumnsChanged));

        private static void OnColumnsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            GridColumnsBase<TGrid, TColumn> gridColumnsBase = (GridColumnsBase<TGrid, TColumn>)d;

            if (e.OldValue is INotifyCollectionChanged oldCollectionChanged)
                oldCollectionChanged.CollectionChanged -= gridColumnsBase.OnColumnsChanged;

            if (e.NewValue is INotifyCollectionChanged newCollectionChanged)
                newCollectionChanged.CollectionChanged += gridColumnsBase.OnColumnsChanged;

            gridColumnsBase.OnColumnsChanged(d, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }

        public IEnumerable Columns
        {
            get { return (IEnumerable)GetValue(ColumnsProperty); }
            set { SetValue(ColumnsProperty, value); }
        }


        protected override void OnAttached()
        {
            OnColumnsChanged(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }

        private void OnColumnsChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (AssociatedObject != null)
            {
                ClearColumns(AssociatedObject);

                if (Columns != null)
                {
                    int i = 0;
                    foreach (object column in Columns)
                    {
                        AddColumn(AssociatedObject, GetColumn(i, column));
                        i++;
                    }
                }
            }
        }

        protected abstract void ClearColumns(TGrid grid);

        protected abstract void AddColumn(TGrid grid, TColumn column);

        protected abstract TColumn GetColumn(int columnIndex, object column);
    }
}