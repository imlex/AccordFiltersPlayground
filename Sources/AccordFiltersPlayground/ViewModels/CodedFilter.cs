using AccordFiltersPlayground.Utils;

namespace AccordFiltersPlayground.ViewModels
{
    public class CodedFilter : BaseViewModel
    {
        private bool _isSelected;

        public bool IsSelected
        {
            get => _isSelected;
            set => Set(ref _isSelected, value);
        }

        private bool _isHighlighted;

        public bool IsHighlighted
        {
            get => _isHighlighted;
            set => Set(ref _isHighlighted, value);
        }

        private string _name;

        public string Name
        {
            get => _name;
            set => Set(ref _name, value);
        }

        private string _code;

        public string Code
        {
            get => _code;
            set => Set(ref _code, value);
        }
    }
}