using Caliburn.Light;
using System.Windows.Input;

namespace Demo.HelloEventAggregator
{
    public class PublisherViewModel : BindableObject
    {
        private readonly IEventAggregator _eventAggregator;
        private string _message;

        public PublisherViewModel(IEventAggregator eventAggregator)
        {
            _eventAggregator = eventAggregator;

            PublishCommand = new DelegateCommand(() => Publish(), () => !string.IsNullOrEmpty(Message), this, nameof(Message));
        }

        public string Message
        {
            get { return _message; }
            set { SetProperty(ref _message, value); }
        }

        public ICommand PublishCommand { get; private set; }

        private void Publish()
        {
            _eventAggregator.Publish(Message);
            Message = null;
        }
    }
}
