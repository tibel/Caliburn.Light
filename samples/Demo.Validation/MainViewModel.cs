using Caliburn.Light;
using Caliburn.Light.WPF;
using System;
using System.Collections;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace Demo.Validation
{
    public sealed class MainViewModel : ViewAware, INotifyDataErrorInfo
    {
        private readonly IWindowManager _windowManager;

        private string _name;
        private string _address;
        private string _contact;
        private string _website;

        public MainViewModel(IWindowManager windowManager)
        {
            if (windowManager is null)
                throw new ArgumentNullException(nameof(windowManager));

            _windowManager = windowManager;

            _validation = new ValidationAdapter(OnErrorsChanged);
            _validation.Validator = SetupValidator();

            Name = "The Company";
            Address = "Some Road";
            Website = "http://thecompany.com";

            SaveCommand = DelegateCommandBuilder.NoParameter()
                .OnExecute(() => Save())
                .OnCanExecute(() => !HasErrors)
                .Observe(this, nameof(HasErrors))
                .Build();
        }

        public string Name
        {
            get => _name;
            set => SetProperty(ref _name, value);
        }

        public string Address
        {
            get => _address;
            set => SetProperty(ref _address, value);
        }

        public string Website
        {
            get => _website;
            set => SetProperty(ref _website, value);
        }

        public string Contact
        {
            get => _contact;
            set => SetProperty(ref _contact, value);
        }

        public ICommand SaveCommand { get; }

        private Task Save()
        {
            var settings = new MessageBoxSettings
            {
                Caption = "Save",
                Text = "Your changes where saved.",
                Image = MessageBoxImage.Information,
            };

            return _windowManager.ShowMessageBox(settings, this);
        }

        #region Validation

        private readonly ValidationAdapter _validation;

        public event EventHandler<DataErrorsChangedEventArgs> ErrorsChanged;

        protected override bool SetProperty<T>(ref T field, T newValue, [CallerMemberName] string propertyName = null)
        {
            var result = base.SetProperty(ref field, newValue, propertyName);
            if (result)
                _validation.ValidateProperty(this, propertyName);
            return result;
        }

        public IEnumerable GetErrors(string propertyName) => _validation.GetPropertyErrors(propertyName);

        public bool HasErrors => _validation.HasErrors;

        private void OnErrorsChanged(string propertyName)
        {
            ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(propertyName));
            RaisePropertyChanged(nameof(HasErrors));
        }

        private static IValidator SetupValidator()
        {
            var ruleValidator = new RuleValidator();
            ruleValidator.AddStringLengthRule<MainViewModel>(nameof(Name), m => m.Name, 1, 100, "Name is required.");
            ruleValidator.AddStringLengthRule<MainViewModel>(nameof(Address), m => m.Address, 1, 100, "Address is required.");
            ruleValidator.AddStringLengthRule<MainViewModel>(nameof(Website), m => m.Website, 1, 100, "Website is required.");
            ruleValidator.AddRegexRule<MainViewModel>(nameof(Website), m => m.Website, "^http(s)?://[a-z0-9-]+(.[a-z0-9-]+)*(:[0-9]+)?(/.*)?$", "The format of the web address is not valid.");
            return ruleValidator;
        }

        #endregion
    }
}
