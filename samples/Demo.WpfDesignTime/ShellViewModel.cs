using Caliburn.Light;

namespace Demo.WpfDesignTime
{
    public class ShellViewModel : BindableObject
    {
        private string _name;
        private NestedViewModel _nestedViewModel;

        public ShellViewModel()
        {
            _nestedViewModel = new NestedViewModel();
            _name = "Shell";
        }

        public string Name
        {
            get { return _name; }
            set { SetProperty(ref _name, value); }
        }

        public NestedViewModel NestedViewModel
        {
            get { return _nestedViewModel; }
            set { SetProperty(ref _nestedViewModel, value); }
        }
    }
}
