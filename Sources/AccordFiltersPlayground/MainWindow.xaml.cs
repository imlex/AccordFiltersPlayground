using System.Windows;
using AccordFiltersPlayground.ViewModels;

namespace AccordFiltersPlayground
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        public MainViewModel MainViewModel
        {
            get => (MainViewModel) DataContext;
        }

        private void MainWindow_OnGotFocus(object sender, RoutedEventArgs e)
        {
            if (((FrameworkElement) e.OriginalSource).DataContext is CodedFilter codedFilter)
                codedFilter.IsSelected = true;
        }

        private void MainWindow_OnLostFocus(object sender, RoutedEventArgs e)
        {
            if (((FrameworkElement)e.OriginalSource).DataContext is CodedFilter codedFilter)
                codedFilter.IsSelected = false;
        }
    }
}
