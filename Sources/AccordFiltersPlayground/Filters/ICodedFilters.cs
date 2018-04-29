using Accord.Imaging;

namespace AccordFiltersPlayground.Filters
{
    public interface ICodedFilters
    {
        string SourceFilePath { get; set; }
        UnmanagedImage[] Results { get; }
        void Run();
    }
}