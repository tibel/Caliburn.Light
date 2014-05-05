using Microsoft.Xaml.Interactivity;
using System;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel;
using Windows.UI.Xaml;

namespace Caliburn.Light
{
    /// <summary>
    /// A trigger that listens for a specified event on its source and fires when that event is fired.
    /// </summary>
    public sealed class EventTrigger : TriggerBase
    {
        /// <summary>
        /// Identifies the <seealso cref="EventTrigger.EventName"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty EventNameProperty = DependencyProperty.Register("EventName",
            typeof (string), typeof (EventTrigger), new PropertyMetadata("Loaded", OnEventNameChanged));

        /// <summary>
        /// Identifies the <seealso cref="EventTrigger.SourceObject"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty SourceObjectProperty = DependencyProperty.Register("SourceObject",
            typeof (object), typeof (EventTrigger), new PropertyMetadata(null, OnSourceObjectChanged));
        
        private object _resolvedSource;
        private Delegate _eventHandler;
        private bool _isLoadedEventRegistered;
        private bool _isWindowsRuntimeEvent;

        /// <summary>
        /// Gets or sets the name of the event to listen for. This is a dependency property.
        /// </summary>
        public string EventName
        {
            get { return (string) GetValue(EventNameProperty); }
            set { SetValue(EventNameProperty, value); }
        }

        /// <summary>
        /// Gets or sets the source object. If SourceObject is not set, the target will default to the AssociatedObject. This is a dependency property.
        /// </summary>
        /// <value>The target object.</value>
        public object SourceObject
        {
            get { return GetValue(SourceObjectProperty); }
            set { SetValue(SourceObjectProperty, value); }
        }

        /// <summary>
        /// Called after the trigger is attached to an AssociatedObject.
        /// </summary>
        protected override void OnAttached()
        {
            SetResolvedSource(ComputeResolvedSource());
        }

        /// <summary>
        /// Called when the trigger is being detached from its AssociatedObject, but before it has actually occurred.
        /// </summary>
        protected override void OnDetaching()
        {
            SetResolvedSource(null);
        }

        private void SetResolvedSource(object newSource)
        {
            if (AssociatedObject == null || _resolvedSource == newSource)
                return;

            if (_resolvedSource != null)
                UnregisterEvent(EventName);

            _resolvedSource = newSource;

            if (_resolvedSource != null)
                RegisterEvent(EventName);
        }

        private object ComputeResolvedSource()
        {
            if (ReadLocalValue(SourceObjectProperty) != DependencyProperty.UnsetValue)
                return SourceObject;

            return AssociatedObject;
        }

        private void RegisterEvent(string eventName)
        {
            if (string.IsNullOrEmpty(eventName))
                return;

            if (eventName == "Loaded")
            {
                if (!_isLoadedEventRegistered)
                {
                    var frameworkElement = _resolvedSource as FrameworkElement;
                    if (frameworkElement != null && !ViewHelper.IsElementLoaded(frameworkElement))
                    {
                        _isLoadedEventRegistered = true;
                        frameworkElement.Loaded += OnEvent;
                    }
                }
                return;
            }

            var type = _resolvedSource.GetType();
            var info = type.GetRuntimeEvent(EventName);
            if (info == null)
            {
                throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, "Cannot find event {0} on type {1}.", EventName, type.Name));
            }

            var declaredMethod = typeof(EventTrigger).GetTypeInfo().GetDeclaredMethod("OnEvent");
            _eventHandler = declaredMethod.CreateDelegate(info.EventHandlerType, this);
            _isWindowsRuntimeEvent = IsWindowsRuntimeType(info.EventHandlerType);
            if (_isWindowsRuntimeEvent)
            {
                WindowsRuntimeMarshal.AddEventHandler(
                    add => (EventRegistrationToken) info.AddMethod.Invoke(_resolvedSource, new object[] {add}),
                    token => info.RemoveMethod.Invoke(_resolvedSource, new object[] {token}), _eventHandler);
                return;
            }

            info.AddEventHandler(_resolvedSource, _eventHandler);
        }

        private void UnregisterEvent(string eventName)
        {
            if (string.IsNullOrEmpty(eventName))
                return;

            if (eventName == "Loaded")
            {
                if (_isLoadedEventRegistered)
                {
                    _isLoadedEventRegistered = false;
                    var frameworkElement = (FrameworkElement)_resolvedSource;
                    frameworkElement.Loaded -= OnEvent;
                }
                return;
            }

            if (_eventHandler == null)
                return;

            var info = _resolvedSource.GetType().GetRuntimeEvent(eventName);
            if (_isWindowsRuntimeEvent)
            {
                WindowsRuntimeMarshal.RemoveEventHandler(
                    token => info.RemoveMethod.Invoke(_resolvedSource, new object[] {token}), _eventHandler);
            }
            else
            {
                info.RemoveEventHandler(_resolvedSource, _eventHandler);
            }

            _eventHandler = null;
        }

        private void OnEvent(object sender, object eventArgs)
        {
            if (DesignMode.DesignModeEnabled)
                return;

            foreach (var action in Actions.OfType<IAction>())
            {
                action.Execute(_resolvedSource, eventArgs);
            }
        }

        private static void OnSourceObjectChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs args)
        {
            var eventTrigger = (EventTrigger) dependencyObject;
            eventTrigger.SetResolvedSource(eventTrigger.ComputeResolvedSource());
        }

        private static void OnEventNameChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs args)
        {
            var eventTrigger = (EventTrigger) dependencyObject;
            if (eventTrigger.AssociatedObject == null || eventTrigger._resolvedSource == null)
                return;

            eventTrigger.UnregisterEvent((string)args.OldValue);
            eventTrigger.RegisterEvent((string)args.NewValue);
        }

        private static bool IsWindowsRuntimeType(Type type)
        {
            return type != null && type.AssemblyQualifiedName.EndsWith("ContentType=WindowsRuntime", StringComparison.Ordinal);
        }
    }
}
