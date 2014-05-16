using Caliburn.Light;

namespace Demo.HelloEventAggregator
{
    public class PublisherViewModel : BindableObject
    {
        private readonly IEventAggregator _eventAggregator;
        private string _message;

        public PublisherViewModel(IEventAggregator eventAggregator)
        {
            _eventAggregator = eventAggregator;
        }

        public string Message
        {
            get { return _message; }
            set
            {
                if (SetProperty(ref _message, value))
                    RaisePropertyChanged(() => CanPublish);
            }
        }

        public bool CanPublish
        {
            get { return !string.IsNullOrEmpty(_message); }
        }

        public void Publish()
        {
            _eventAggregator.Publish(Message);
            Message = null;
        }
    }
}
