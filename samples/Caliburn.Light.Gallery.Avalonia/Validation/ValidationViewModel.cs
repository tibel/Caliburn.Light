using Caliburn.Light;
using Caliburn.Light.Avalonia;
using System;
using System.Collections;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Caliburn.Light.Gallery.Avalonia.Validation;

public sealed class ValidationViewModel : ViewAware, IHaveDisplayName, INotifyDataErrorInfo
{
    private readonly IWindowManager _windowManager;

    private string _name;
    private string _address;
    private string _contact;
    private string _website;

    public ValidationViewModel(IWindowManager windowManager)
    {
        ArgumentNullException.ThrowIfNull(windowManager);

        _windowManager = windowManager;

        _validation = new ValidationAdapter(OnErrorsChanged);
        _validation.Validator = SetupValidator();

        _name = "The Company";
        _address = "Some Road";
        _contact = string.Empty;
        _website = "http://thecompany.com";

        SaveCommand = DelegateCommandBuilder.NoParameter()
            .OnExecute(() => Save())
            .OnCanExecute(() => !HasErrors)
            .Observe(this, nameof(HasErrors))
            .Build();
    }

    public string? DisplayName => "Validation";

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
        return _windowManager.ShowDialog(new SaveConfirmationViewModel(), this);
    }

    private readonly ValidationAdapter _validation;

    public event EventHandler<DataErrorsChangedEventArgs>? ErrorsChanged;

    protected override bool SetProperty<T>(ref T field, T newValue, [CallerMemberName] string? propertyName = null)
    {
        var result = base.SetProperty(ref field, newValue, propertyName);
        if (result)
            _validation.ValidateProperty(this, propertyName!);
        return result;
    }

    public IEnumerable GetErrors(string? propertyName) => string.IsNullOrEmpty(propertyName) ? _validation.GetErrors() : _validation.GetPropertyErrors(propertyName);

    public bool HasErrors => _validation.HasErrors;

    private void OnErrorsChanged(string propertyName)
    {
        ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(propertyName));
        RaisePropertyChanged(nameof(HasErrors));
    }

    private static IValidator SetupValidator()
    {
        var ruleValidator = new RuleValidator();
        ruleValidator.AddStringLengthRule<ValidationViewModel>(nameof(Name), static m => m.Name, 1, 100, "Name is required.");
        ruleValidator.AddStringLengthRule<ValidationViewModel>(nameof(Address), static m => m.Address, 1, 100, "Address is required.");
        ruleValidator.AddStringLengthRule<ValidationViewModel>(nameof(Website), static m => m.Website, 1, 100, "Website is required.");
        ruleValidator.AddRegexRule<ValidationViewModel>(nameof(Website), static m => m.Website, "^http(s)?://[a-z0-9-]+(.[a-z0-9-]+)*(:[0-9]+)?(/.*)?$", "The format of the web address is not valid.");
        return ruleValidator;
    }
}
