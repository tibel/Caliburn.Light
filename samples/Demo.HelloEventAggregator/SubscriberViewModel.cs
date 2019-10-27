using Caliburn.Light;

namespace Demo.HelloEventAggregator
{
    public class SubscriberViewModel : BindableObject
    {
        private readonly BindableCollection<string> _messages = new BindableCollection<string>();

        public SubscriberViewModel(IEventAggregator eventAggregator)
        {
            eventAggregator.Subscribe<SubscriberViewModel, string>(this, (t, m) => t.OnMessageReceived(m));
        }

        private void OnMessageReceived(string message)
        {
            _messages.Add(message);
        }

        public IBindableCollection<string> Messages => _messages;
    }
}
