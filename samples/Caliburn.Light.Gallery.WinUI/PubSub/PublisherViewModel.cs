using System.Windows.Input;

namespace Caliburn.Light.Gallery.WinUI.PubSub;

public sealed partial class PublisherViewModel : BindableObject
{
    private readonly IEventAggregator _eventAggregator;
    private string? _message;

    public PublisherViewModel(IEventAggregator eventAggregator)
    {
        _eventAggregator = eventAggregator;

        PublishCommand = DelegateCommandBuilder.NoParameter()
            .OnExecute(Publish)
            .OnCanExecute(() => !string.IsNullOrEmpty(Message))
            .Observe(this, nameof(Message))
            .Build();
    }

    public string? Message
    {
        get { return _message; }
        set { SetProperty(ref _message, value); }
    }

    public ICommand PublishCommand { get; }

    private void Publish()
    {
        if (Message is null)
            return;

        _eventAggregator.Publish(Message);
        Message = null;
    }
}
