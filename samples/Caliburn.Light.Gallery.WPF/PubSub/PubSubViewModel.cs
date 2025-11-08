using Caliburn.Light;

namespace Caliburn.Light.Gallery.WPF.PubSub
{
    public sealed class PubSubViewModel : BindableObject, IHaveDisplayName
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
}
