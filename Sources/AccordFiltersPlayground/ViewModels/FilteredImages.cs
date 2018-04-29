using System.Collections.ObjectModel;
using System.Windows.Data;
using Accord.Imaging;
using AccordFiltersPlayground.Utils;

namespace AccordFiltersPlayground.ViewModels
{
    public class FilteredImages : BaseViewModel
    {
        public FilteredImages(int count = 0)
        {
            Images = new ObservableCollection<FilteredImage>();
            for (int i = 0; i < count; i++)
                Images.Add(new FilteredImage());
            BindingOperations.EnableCollectionSynchronization(Images, Images);
        }

        public string InputFilePath { get; set; }

        public ObservableCollection<FilteredImage> Images { get; }
    }

    public class FilteredImage : BaseViewModel
    {
        private UnmanagedImage _unmanagedImage;

        public UnmanagedImage UnmanagedImage
        {
            get => _unmanagedImage;
            set
            {
                UnmanagedImage oldValue = _unmanagedImage;
                if (Set(ref _unmanagedImage, value))
                    oldValue?.Dispose();
            }
        }
    }
}