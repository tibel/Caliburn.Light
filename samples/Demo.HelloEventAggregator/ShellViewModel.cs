using Caliburn.Light;

namespace Demo.HelloEventAggregator
{
    public class ShellViewModel : BindableObject
    {
        public ShellViewModel(PublisherViewModel publisher, SubscriberViewModel subscriber)
        {
            Publisher = publisher;
            Subscriber = subscriber;
        }

        public PublisherViewModel Publisher { get; }

        public SubscriberViewModel Subscriber { get; }
    }
}
