using System;
using System.CodeDom.Compiler;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Input;
using AccordFiltersPlayground.DTO;
using AccordFiltersPlayground.Filters;
using AccordFiltersPlayground.Utils;
using Microsoft.CSharp;
using Microsoft.Win32;
using Newtonsoft.Json;

namespace AccordFiltersPlayground.ViewModels
{
    public class MainViewModel : BaseAsyncViewModel
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

            FilteredImages.Add(new FilteredImages(CodedFilters.Count) { InputFilePath = @"..\..\..\..\Data\BadDrivers.jpg" });
            FilteredImages.Add(new FilteredImages(CodedFilters.Count) { InputFilePath = @"..\..\..\..\Data\Cust016.png" });
            FilteredImages.Add(new FilteredImages(CodedFilters.Count) { InputFilePath = @"..\..\..\..\Data\OursA1BadLight.png" });

            LoadCommand = new ActionCommand(DoLoadCommand);
            SaveCommand = new ActionCommand(DoSaveCommand);

            RunCommand = new ActionCommand(() => EnqueueAsyncFunction(DoRunCommand));
            CancelCommand = new ActionCommand(DoCancelCommand);

            EnqueueAsyncFunction(DoRunCommand);
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
            if (AutoRunOnChanges && e.PropertyName == nameof(CodedFilter.Code))
                EnqueueAsyncFunction(DoRunCommand);
        }

        public ObservableCollection<CodedFilter> CodedFilters { get; }

        public ObservableCollection<FilteredImages> FilteredImages { get; }

        public ICommand LoadCommand { get; }

        private void DoLoadCommand()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            if (openFileDialog.ShowDialog() == true)
            {
                MainModelDto mainModelDto = JsonConvert.DeserializeObject<MainModelDto>(File.ReadAllText(openFileDialog.FileName));

                for (int i = CodedFilters.Count - 1; i >= 0; i--)
                    CodedFilters.RemoveAt(i);

                for (int i = FilteredImages.Count - 1; i >= 0; i--)
                {
                    foreach (FilteredImage filteredImage in FilteredImages[i].Images)
                        filteredImage.UnmanagedImage = null;

                    FilteredImages.RemoveAt(i);
                }

                foreach (CodedFilterDto codedFilterDto in mainModelDto.CodedFilters)
                    CodedFilters.Add(new CodedFilter {Name = codedFilterDto.Name, Code = codedFilterDto.Code});

                foreach (string inputFilePath in mainModelDto.InputFilePaths)
                    FilteredImages.Add(new FilteredImages(CodedFilters.Count) {InputFilePath = inputFilePath});
            }
        }

        public ICommand SaveCommand { get; }

        private void DoSaveCommand()
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            if (saveFileDialog.ShowDialog() == true)
            {
                MainModelDto mainModelDto = new MainModelDto
                {
                    CodedFilters = CodedFilters.Select(x => new CodedFilterDto {Name = x.Name, Code = x.Code}).ToList(),
                    InputFilePaths = FilteredImages.Select(x => x.InputFilePath).ToList()
                };

                File.WriteAllText(saveFileDialog.FileName, JsonConvert.SerializeObject(mainModelDto));
            }
        }

        private bool _autoRunOnChanges;

        public bool AutoRunOnChanges
        {
            get => _autoRunOnChanges;
            set => Set(ref _autoRunOnChanges, value);
        }

        public ICommand RunCommand { get; }

        private Task DoRunCommand(CancellationToken cancellationToken)
        {
            return Task.Run(() =>
            {
                if (cancellationToken.IsCancellationRequested)
                    return;

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

                if (cancellationToken.IsCancellationRequested)
                    return;

                CompilerResults compilerResults = cSharpCodeProvider.CompileAssemblyFromSource(compilerParameters, builder.ToString());

                if (cancellationToken.IsCancellationRequested)
                    return;

                ICodedFilters codedFilters = (ICodedFilters) Activator.CreateInstance(compilerResults.CompiledAssembly.GetType("TestCodedFilter"));

                foreach (FilteredImages filteredImages in FilteredImages)
                {
                    if (cancellationToken.IsCancellationRequested)
                        return;

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

        public ICommand CancelCommand { get; }

        private void DoCancelCommand()
        {
            CancelAsyncFunctions();
        }
    }
}