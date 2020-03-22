using System.ComponentModel;
using Windows.UI.Xaml;

namespace Caliburn.Light.WinUI
{
    /// <summary>
    /// Wraps the <see cref="FrameworkElement.DataContext"/> for usage with 'x:Bind'.
    /// </summary>
    /// <typeparam name="T">The view-model type.</typeparam>
    public sealed class DataContextWrapper<T> : INotifyPropertyChanged
        where T : class
    {
        /// <summary>
        /// The typed data context.
        /// </summary>
        public T Entity { get; private set; }

        /// <summary>
        /// Occurs when a property value changes.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Initializes a new instance of <see cref="DataContextWrapper&lt;T&gt;"/>.
        /// </summary>
        /// <param name="element">The element </param>
        public DataContextWrapper(FrameworkElement element)
        {
            Entity = CoerceValue(element.DataContext);
            element.DataContextChanged += OnDataContextChanged;
        }

        private static T CoerceValue(object value)
        {
            return value is T typed ? typed : default;
        }

        private void OnDataContextChanged(FrameworkElement sender, DataContextChangedEventArgs args)
        {
            Entity = CoerceValue(args.NewValue);
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Entity)));
        }
    }
}
