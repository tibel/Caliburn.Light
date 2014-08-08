using Caliburn.Light;
using System;
using System.Collections;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
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
            _validation = new ValidationAdapter(OnErrorsChanged);
            _validation.Validators.Add(new DataAnnotationsValidator(GetType()));

            _company = company;
            SaveCommand = DelegateCommand.FromAction(Save);
        }

        [Required(ErrorMessage = @"Name is required.")]
        public string CName
        {
            get { return _company.Name; }
            set
            {
                _company.Name = value;
                OnPropertyChanged(value);
            }
        }

        [Required(ErrorMessage = @"Address is required.")]
        public string Address
        {
            get { return _company.Address; }
            set
            {
                _company.Address = value;
                OnPropertyChanged(value);
            }
        }

        [Required(ErrorMessage = @"Website is required.")]
        [RegularExpression(@"^http(s)?://[a-z0-9-]+(.[a-z0-9-]+)*(:[0-9]+)?(/.*)?$",
            ErrorMessage = @"The format of the web address is not valid")]
        public string Website
        {
            get { return _company.Website; }
            set
            {
                _company.Website = value;
                OnPropertyChanged(value);
            }
        }

        public string Contact
        {
            get { return _company.Contact; }
            set
            {
                _company.Contact = value;
                OnPropertyChanged(value);
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

        protected void OnPropertyChanged(object value, [CallerMemberName] string propertyName = "")
        {
            _validation.ValidateProperty(propertyName, value);
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

            RaisePropertyChanged("CanSave");
        }

        #endregion
    }
}
