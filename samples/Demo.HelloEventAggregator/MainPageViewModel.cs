using Caliburn.Light;

namespace Demo.HelloEventAggregator
{
    public class MainPageViewModel : BindableObject
    {
        private readonly PublisherViewModel _publisher;
        private readonly SubscriberViewModel _subscriber;

        public MainPageViewModel(PublisherViewModel publisher, SubscriberViewModel subscriber)
        {
            _publisher = publisher;
            _subscriber = subscriber;
        }

        public PublisherViewModel Publisher
        {
            get { return _publisher; }
        }

        public SubscriberViewModel Subscriber
        {
            get { return _subscriber; }
        }
    }
}
