using Weakly;
using System.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
#if !NETFX_CORE
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interactivity;
using System.Windows.Markup;
#else
using Windows.UI.Core;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Markup;
#endif

namespace Caliburn.Light
{
    /// <summary>
    /// Calls a method on a specified object when invoked.
    /// It also maintains the Enabled state of the target element based on a guard method/property.
    /// </summary>
#if !NETFX_CORE
    [ContentProperty("Parameters")]
#else
    [ContentProperty(Name = "Parameters")]
#endif
    public class CallMethodAction : TriggerAction<DependencyObject>, IHaveParameters
    {
        /// <summary>
        /// Identifies the <seealso cref="CallMethodAction.TargetObject"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty TargetObjectProperty = DependencyProperty.Register("TargetObject",
            typeof (object), typeof (CallMethodAction), new PropertyMetadata(null, OnTargetObjectChanged));

        /// <summary>
        /// Identifies the <seealso cref="CallMethodAction.MethodName"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty MethodNameProperty = DependencyProperty.Register("MethodName",
            typeof(string), typeof(CallMethodAction), new PropertyMetadata(null, OnMethodNameChanged));

        /// <summary>
        /// Identifies the <seealso cref="CallMethodAction.Parameters"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty ParametersProperty = DependencyProperty.Register("Parameters",
            typeof(AttachedCollection<Parameter>), typeof(CallMethodAction), new PropertyMetadata(null));

        /// <summary>
        /// The object that exposes the method of interest. This is a dependency property.
        /// </summary>
        public object TargetObject
        {
            get { return GetValue(TargetObjectProperty); }
            set { SetValue(TargetObjectProperty, value); }
        }

        /// <summary>
        /// The name of the method to invoke. This is a dependency property.
        /// </summary>
        public string MethodName
        {
            get { return (string) GetValue(MethodNameProperty); }
            set { SetValue(MethodNameProperty, value); }
        }

        /// <summary>
        /// Gets the collection of parameters associated with the action. This is a dependency property.
        /// </summary>
        public AttachedCollection<Parameter> Parameters
        {
            get
            {
                var parametersCollection = (AttachedCollection<Parameter>) GetValue(ParametersProperty);
                if (parametersCollection == null)
                {
                    parametersCollection = new AttachedCollection<Parameter>();
                    SetValue(ParametersProperty, parametersCollection);
                }
                return parametersCollection;
            }
        }

        private static void OnMethodNameChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            var callMethodAction = (CallMethodAction)sender;
            callMethodAction.UpdateMethodInfo();
            callMethodAction.UpdateEnabledState();
        }

        private static void OnTargetObjectChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            var callMethodAction = (CallMethodAction)sender;
            callMethodAction.UpdateMethodInfo();
            callMethodAction.UpdateEnabledState();
        }

        /// <summary>
        /// Called after the action is attached to an AssociatedObject.
        /// </summary>
        protected override void OnAttached()
        {
            base.OnAttached();
            Parameters.Attach(AssociatedObject);
            Parameters.OfType<Parameter>().ForEach(x => x.MakeAwareOf(this));

            UpdateMethodInfo();
            UpdateEnabledState();
        }

        /// <summary>
        /// Called when the action is being detached from its AssociatedObject, but before it has actually occurred.
        /// </summary>
        protected override void OnDetaching()
        {
            Parameters.Detach();
            ResetMethodInfo();
            base.OnDetaching();
        }

        private object Target
        {
            get { return TargetObject ?? AssociatedObject; }
        }

        private MethodInfo _method;
        private MethodInfo _guard;
        private string _guardName;
        private IDisposable _propertyChangedRegistration;

        private void ResetMethodInfo()
        {
            if (_propertyChangedRegistration != null)
            {
                _propertyChangedRegistration.Dispose();
                _propertyChangedRegistration = null;
            }

            _method = null;
            _guard = null;
            _guardName = null;
        }

        private void UpdateMethodInfo()
        {
            ResetMethodInfo();

            if (Target == null || string.IsNullOrEmpty(MethodName)) return;

            _method = ParameterBinder.FindBestMethod(Target, MethodName, Parameters);
            if (_method == null) return;

            _guardName = "Can" + _method.Name;

            _guard = ParameterBinder.FindGuardMethod(Target, _method);
            if (_guard != null) return;

            var inpc = Target as INotifyPropertyChanged;
            if (inpc == null) return;

            var property = Target.GetType().GetRuntimeProperty(_guardName);
            if (property == null) return;

            _guard = property.GetMethod;
            _propertyChangedRegistration = WeakEventHandler.Register<PropertyChangedEventArgs>(inpc, "PropertyChanged", OnPropertyChanged);
        }

        private void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (string.IsNullOrEmpty(e.PropertyName) || e.PropertyName == _guardName)
            {
#if NETFX_CORE
                if (Dispatcher.HasThreadAccess)
                    UpdateEnabledState();
                else
                {
                    var dummy = Dispatcher.RunAsync(CoreDispatcherPriority.Normal, UpdateEnabledState);
                }
#else
                if (Dispatcher.CheckAccess())
                    UpdateEnabledState();
                else
                    Dispatcher.BeginInvoke(new Action(UpdateEnabledState));
#endif
            }
        }

        /// <summary>
        /// Forces an update of the IsEnabled state.
        /// </summary>
        public void UpdateEnabledState()
        {
            if (AssociatedObject == null) return;

            var canExecute = true;
            if (_guard != null)
            {
                var guardFunction = DynamicDelegate.From(_guard);
                canExecute = (bool)guardFunction(Target, ParameterBinder.DetermineParameters(Parameters, _guard.GetParameters()));
            }

#if SILVERLIGHT || NETFX_CORE
            var control = AssociatedObject as Control;
#else
            var control = AssociatedObject as UIElement;
#endif

            if (control != null)
                control.IsEnabled = canExecute;
        }

        /// <summary>
        /// Invokes the action.
        /// </summary>
        /// <param name="parameter">The parameter to the action. If the action does not require a parameter, the parameter may be set to a null reference.</param>
        protected override void Invoke(object parameter)
        {
            if (Target == null) return;

            var parameterValues = ParameterBinder.DetermineParameters(Parameters, _method.GetParameters());
            var returnValue = DynamicDelegate.From(_method)(Target, parameterValues);
            if (returnValue == null) return;

            var enumerable = returnValue as IEnumerable<ICoTask>;
            if (enumerable != null)
                returnValue = enumerable.GetEnumerator();

            var enumerator = returnValue as IEnumerator<ICoTask>;
            if (enumerator != null)
                returnValue = enumerator.AsCoTask();

            var coTask = returnValue as ICoTask;
            if (coTask != null)
            {
                var context = new CoroutineExecutionContext
                {
                    Source = this,
                    Target = Target,
                };

                coTask.ExecuteAsync(context);
            }
        }
    }
}
