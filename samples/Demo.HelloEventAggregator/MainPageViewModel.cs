using Caliburn.Light;

namespace Demo.HelloEventAggregator
{
    public class MainPageViewModel : BindableObject
    {
        public MainPageViewModel(PublisherViewModel publisher, SubscriberViewModel subscriber)
        {
            Publisher = publisher;
            Subscriber = subscriber;
        }

        public PublisherViewModel Publisher { get; }

        public SubscriberViewModel Subscriber { get; }
    }
}
