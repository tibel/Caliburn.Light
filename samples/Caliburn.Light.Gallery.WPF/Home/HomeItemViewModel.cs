using System;

namespace Caliburn.Light.Gallery.WPF.Home
{
    public sealed class HomeItemViewModel : BindableObject
    {
        private readonly Func<object> _createInstance;

        public HomeItemViewModel(string title, Func<object> createInstance)
        {
            Title = title;
            _createInstance = createInstance;
        }

        public string? Title { get; }

        public object CreateInstance() => _createInstance();
    }
}
