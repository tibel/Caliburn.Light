namespace Caliburn.Light.Gallery.WinUI.PubSub;

public sealed partial class SubscriberViewModel : BindableObject
{
    private readonly BindableCollection<string> _messages = new BindableCollection<string>();

    public SubscriberViewModel(IEventAggregator eventAggregator)
    {
        eventAggregator.Subscribe<SubscriberViewModel, string>(this, static (t, m) => t.OnMessageReceived(m));
    }

    private void OnMessageReceived(string message)
    {
        _messages.Add(message);
    }

    public IReadOnlyBindableCollection<string> Messages => _messages;
}
