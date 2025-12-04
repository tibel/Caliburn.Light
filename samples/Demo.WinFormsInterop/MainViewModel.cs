using Caliburn.Light;
using System.Windows.Forms;
using System.Windows.Input;

namespace Demo.WinFormsInterop;

public sealed class MainViewModel : ViewAware
{
    private string? _name;

    public MainViewModel()
    {
        SayHelloCommand = DelegateCommandBuilder.NoParameter()
            .OnExecute(SayHello)
            .OnCanExecute(() => !string.IsNullOrWhiteSpace(Name))
            .Observe(this, nameof(Name))
            .Build();
    }

    public string? Name
    {
        get { return _name; }
        set { SetProperty(ref _name, value); }
    }

    public ICommand SayHelloCommand { get; }

    private void SayHello()
    {
        MessageBox.Show(Form.ActiveForm, string.Format("Hello {0}!", Name), "Message", MessageBoxButtons.OK, MessageBoxIcon.Information);
    }
}
