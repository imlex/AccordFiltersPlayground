using System;
using System.CodeDom.Compiler;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using AccordFiltersPlayground.Filters;
using AccordFiltersPlayground.Utils;
using Microsoft.CSharp;

namespace AccordFiltersPlayground.ViewModels
{
    public class MainViewModel : BaseViewModel
    {
        public MainViewModel()
        {
            CodedFilters = new ObservableCollection<CodedFilter>();
            CodedFilters.CollectionChanged += CodedFilters_CollectionChanged;
            BindingOperations.EnableCollectionSynchronization(CodedFilters, CodedFilters);

            FilteredImages = new ObservableCollection<FilteredImages>();
            BindingOperations.EnableCollectionSynchronization(FilteredImages, FilteredImages);

            CodedFilters.Add(new CodedFilter
            {
                Name = "LoadFile",
                Code = "using (Bitmap bitmap = Accord.Imaging.Image.FromFile(SourceFilePath))" + Environment.NewLine +
                       "    return UnmanagedImage.FromManagedImage(bitmap);"
            });
            CodedFilters.Add(new CodedFilter
            {
                Name = "ApplyGrayscale",
                Code = "return Grayscale.CommonAlgorithms.Y.Apply(Results[0]);"
            });

            FilteredImages.Add(new FilteredImages(2) { InputFilePath = @"..\..\..\..\Data\BadDrivers.jpg" });
            FilteredImages.Add(new FilteredImages(2) { InputFilePath = @"..\..\..\..\Data\Cust016.png" });
            FilteredImages.Add(new FilteredImages(2) { InputFilePath = @"..\..\..\..\Data\OursA1BadLight.png" });

            EnqueueAsyncFunction(Run);
        }

        private void CodedFilters_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                case NotifyCollectionChangedAction.Replace:
                case NotifyCollectionChangedAction.Remove:
                    if (e.OldItems != null)
                    {
                        foreach (CodedFilter codedFilter in e.OldItems.Cast<CodedFilter>())
                            codedFilter.PropertyChanged -= CodedFilter_PropertyChanged;
                    }
                    if (e.NewItems != null)
                    {
                        foreach (CodedFilter codedFilter in e.NewItems.Cast<CodedFilter>())
                            codedFilter.PropertyChanged += CodedFilter_PropertyChanged;
                    }
                    break;
                case NotifyCollectionChangedAction.Move:
                    return;
                case NotifyCollectionChangedAction.Reset:
                default:
                    Debug.Assert(false);
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void CodedFilter_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(CodedFilter.Code))
                EnqueueAsyncFunction(Run);
        }

        public ObservableCollection<CodedFilter> CodedFilters { get; }

        public ObservableCollection<FilteredImages> FilteredImages { get; }

        private Task Run()
        {
            return Task.Run(() =>
            {
                StringBuilder builder = new StringBuilder();
                builder.Append("using System.Drawing;" + Environment.NewLine +
                               "using Accord.Imaging;" + Environment.NewLine +
                               "using Accord.Imaging.Filters;" + Environment.NewLine +
                               Environment.NewLine +
                               "public class TestCodedFilter : AccordFiltersPlayground.Filters.ICodedFilters" + Environment.NewLine +
                               "{" + Environment.NewLine +
                               "    public TestCodedFilter()" + Environment.NewLine +
                               "    {" + Environment.NewLine +
                               "        Results = new UnmanagedImage[" + CodedFilters.Count + "];" + Environment.NewLine +
                               "    }" + Environment.NewLine +
                               "    public string SourceFilePath { get; set; }" + Environment.NewLine +
                               "    public UnmanagedImage[] Results { get; private set; }" + Environment.NewLine +
                               "    " + Environment.NewLine);

                foreach (CodedFilter codedFilter in CodedFilters)
                {
                    builder.Append("    public UnmanagedImage " + codedFilter.Name + "()" + Environment.NewLine +
                                   "    {" + Environment.NewLine +
                                   string.Join(Environment.NewLine,
                                       codedFilter.Code.Split(
                                           new[] {Environment.NewLine}, StringSplitOptions.None).Select(x => "        " + x)) + Environment.NewLine +
                                   "    }" + Environment.NewLine +
                                   Environment.NewLine);
                }

                builder.Append("    public void Run()" + Environment.NewLine +
                               "    {" + Environment.NewLine);
                int i = 0;
                foreach (CodedFilter codedFilter in CodedFilters)
                {
                    builder.Append("        Results[" + i + "] = " + codedFilter.Name + "();" + Environment.NewLine);
                    i++;
                }

                builder.Append("    }" + Environment.NewLine);

                builder.Append("}");

                CSharpCodeProvider cSharpCodeProvider = new CSharpCodeProvider();
                CompilerParameters compilerParameters = new CompilerParameters {GenerateInMemory = true};
                compilerParameters.ReferencedAssemblies.Add("System.Drawing.dll");
                compilerParameters.ReferencedAssemblies.Add("Accord.Imaging.dll");
                compilerParameters.ReferencedAssemblies.Add("AccordFiltersPlayground.exe");
                CompilerResults compilerResults = cSharpCodeProvider.CompileAssemblyFromSource(compilerParameters, builder.ToString());
                ICodedFilters codedFilters = (ICodedFilters) Activator.CreateInstance(compilerResults.CompiledAssembly.GetType("TestCodedFilter"));

                foreach (FilteredImages filteredImages in FilteredImages)
                {
                    codedFilters.SourceFilePath = filteredImages.InputFilePath;
                    codedFilters.Run();

                    i = 0;
                    foreach (FilteredImage filteredImage in filteredImages.Images)
                    {
                        filteredImage.UnmanagedImage = codedFilters.Results[i];
                        codedFilters.Results[i] = null;
                        i++;
                    }
                }
            });
        }
    }
}