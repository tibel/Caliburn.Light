using Caliburn.Light;

namespace Demo.HelloEventAggregator
{
    public class SubscriberViewModel : BindableObject
    {
        private readonly BindableCollection<string> _messages = new BindableCollection<string>(); 

        public SubscriberViewModel(IEventAggregator eventAggregator)
        {
            eventAggregator.Subscribe<string>(this, m => OnMessageReceived(m));
        }

        private void OnMessageReceived(string message)
        {
            _messages.Add(message);
        }

        public IBindableCollection<string> Messages
        {
            get { return _messages; }
        }
    }
}
