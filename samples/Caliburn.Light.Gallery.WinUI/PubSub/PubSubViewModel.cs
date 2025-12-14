namespace Caliburn.Light.Gallery.WinUI.PubSub;

public sealed partial class PubSubViewModel : BindableObject, IHaveDisplayName
{
    public string? DisplayName => "Pub/Sub";

    public PubSubViewModel(IEventAggregator eventAggregator)
    {
        Publisher = new PublisherViewModel(eventAggregator);
        Subscriber = new SubscriberViewModel(eventAggregator);
    }

    public PublisherViewModel Publisher { get; }

    public SubscriberViewModel Subscriber { get; }
}
