using System.Drawing;
using Accord.Imaging;
using Accord.Imaging.Filters;

namespace AccordFiltersPlayground.Filters
{
    public class TestFilter : ICodedFilters
    {
        public TestFilter()
        {
            Results = new UnmanagedImage[2];
        }

        public string SourceFilePath { get; set; }

        public UnmanagedImage[] Results { get; private set; }

        public UnmanagedImage LoadFile()
        {
            using (Bitmap bitmap = Accord.Imaging.Image.FromFile(SourceFilePath))
                return UnmanagedImage.FromManagedImage(bitmap);
        }

        public UnmanagedImage ApplyGrayscale()
        {
            return Grayscale.CommonAlgorithms.Y.Apply(Results[0]);
        }

        public void Run()
        {
            Results[0] = LoadFile();

            Results[1] = ApplyGrayscale();
        }
    }
}