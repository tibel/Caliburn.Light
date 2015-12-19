using Caliburn.Light;
using System;
using System.Collections;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;

namespace Demo.Validation
{
    public class MainViewModel : BindableObject, INotifyDataErrorInfo
    {
        private readonly Company _company;

        public MainViewModel(Company company)
        {
            _company = company;

            _validation = new ValidationAdapter(OnErrorsChanged);
            _validation.Validator = SetupValidator();
            
            SaveCommand = DelegateCommand
                .NoParameter()
                .OnExecute(() => Save())
                .OnCanExecute(() => CanSave)
                .Observe(this, nameof(CanSave))
                .Build();
        }

        public string CName
        {
            get { return _company.Name; }
            set
            {
                _company.Name = value;
                OnPropertyChanged();
            }
        }

        public string Address
        {
            get { return _company.Address; }
            set
            {
                _company.Address = value;
                OnPropertyChanged();
            }
        }

        public string Website
        {
            get { return _company.Website; }
            set
            {
                _company.Website = value;
                OnPropertyChanged();
            }
        }

        public string Contact
        {
            get { return _company.Contact; }
            set
            {
                _company.Contact = value;
                OnPropertyChanged();
            }
        }

        public ICommand SaveCommand { get; private set; }

        private void Save()
        {
            MessageBox.Show("Your changes where saved.", "Save", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        public bool CanSave
        {
            get { return !_validation.HasErrors; }
        }

        protected void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            _validation.ValidateProperty(this, propertyName);
            RaisePropertyChanged(propertyName);
        }

        #region Validation

        private readonly ValidationAdapter _validation;

        public event EventHandler<DataErrorsChangedEventArgs> ErrorsChanged;

        public IEnumerable GetErrors(string propertyName)
        {
            return _validation.GetPropertyError(propertyName);
        }

        public bool HasErrors
        {
            get { return _validation.HasErrors; }
        }
       
        private void OnErrorsChanged(string propertyName)
        {
            var handler = ErrorsChanged;
            if (handler != null)
                handler(this, new DataErrorsChangedEventArgs(propertyName));

            RaisePropertyChanged(nameof(CanSave));
        }

        private static IValidator SetupValidator()
        {
            var ruleValidator = new RuleValidator<MainViewModel>();
            ruleValidator.AddRule(new StringLengthValidationRule<MainViewModel>(nameof(CName), m => m.CName, 1, 100, "Name is required."));
            ruleValidator.AddRule(new StringLengthValidationRule<MainViewModel>(nameof(Address), m => m.Address, 1, 100, "Address is required."));
            ruleValidator.AddRule(new StringLengthValidationRule<MainViewModel>(nameof(Website), m => m.Website, 1, 100, "Website is required."));
            ruleValidator.AddRule(new RegexValidationRule<MainViewModel>(nameof(Website), m => m.Website, "^http(s)?://[a-z0-9-]+(.[a-z0-9-]+)*(:[0-9]+)?(/.*)?$", "The format of the web address is not valid."));
            return ruleValidator;
        }

        #endregion
    }
}
