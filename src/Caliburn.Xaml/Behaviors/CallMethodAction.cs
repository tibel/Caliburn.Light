using Weakly;
using System;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
#if !NETFX_CORE
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interactivity;
using System.Windows.Markup;
#else
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
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
            typeof(ParameterCollection), typeof(CallMethodAction), new PropertyMetadata(null));

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
        public ParameterCollection Parameters
        {
            get { return (ParameterCollection)GetValue(ParametersProperty); }
        }

        /// <summary>
        /// Creates an instance of <see cref="CallMethodAction"/>.
        /// </summary>
        public CallMethodAction()
        {
            SetValue(ParametersProperty, new ParameterCollection());
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
            Parameters.ForEach(x => ((Parameter)x).MakeAwareOf(this));

            UpdateMethodInfo();
            UpdateEnabledState();
        }

        /// <summary>
        /// Called when the action is being detached from its AssociatedObject, but before it has actually occurred.
        /// </summary>
        protected override void OnDetaching()
        {
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

            _method = ParameterBinder.FindBestMethod(Target, MethodName, Parameters.Count);
            if (_method == null) return;

            _guardName = "Can" + _method.Name;

            _guard = ParameterBinder.FindGuardMethod(Target, _method);
            if (_guard != null) return;

            var inpc = Target as INotifyPropertyChanged;
            if (inpc == null) return;

            var property = Target.GetType().GetRuntimeProperty(_guardName);
            if (property == null) return;

            _guard = property.GetMethod;
            _propertyChangedRegistration = inpc.RegisterPropertyChangedWeak(this, (t, s, e) => t.OnPropertyChanged(e));
        }

        private void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            if (string.IsNullOrEmpty(e.PropertyName) || e.PropertyName == _guardName)
            {
#if NETFX_CORE
                if (Dispatcher.HasThreadAccess)
                    UpdateEnabledState();
                else
                    Dispatcher.RunAsync(CoreDispatcherPriority.Normal, UpdateEnabledState).AsTask().ObserveException();
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
                var parameterValues = DetermineParameters(_guard);
                canExecute = (bool)_guard.Invoke(Target, parameterValues);
            }

#if SILVERLIGHT || NETFX_CORE
            var control = AssociatedObject as Control;
            if (control != null && !BindingHelper.IsDataBound(control, Control.IsEnabledProperty))
                control.IsEnabled = canExecute;
#else
            var control = AssociatedObject as FrameworkElement;
            if (control != null && !BindingHelper.IsDataBound(control, UIElement.IsEnabledProperty))
                control.IsEnabled = canExecute;
#endif
        }

        /// <summary>
        /// Invokes the action.
        /// </summary>
        /// <param name="parameter">The parameter to the action. If the action does not require a parameter, the parameter may be set to a null reference.</param>
        protected override void Invoke(object parameter)
        {
            if (Target == null)
                throw new InvalidOperationException(string.Format("No target found for method {0}.", MethodName));
            if (_method == null)
                throw new InvalidOperationException(string.Format("Method {0} not found on target of type {1}.", MethodName, Target.GetType()));

            var parameterValues = DetermineParameters(_method, parameter);
            var returnValue = _method.Invoke(Target, parameterValues);

            var task = returnValue as Task;
            if (task != null)
                task.ObserveException().Watch();
        }

        private object[] DetermineParameters(MethodBase method, object eventArgs = null)
        {
            var requiredParameters = method.GetParameters();
            if (requiredParameters.Length == 0)
                return new object[0];

            var parameterValues = Parameters.Select(x => ((Parameter)x).Value).ToArray();
            if (requiredParameters.Length != parameterValues.Length)
                throw new InvalidOperationException(string.Format("Inconsistent number of parameters for method {0}.", method.Name));

            var context = (CoroutineExecutionContext) null;
            for (var i = 0; i < requiredParameters.Length; i++)
            {
                var parameterType = requiredParameters[i].ParameterType;
                var parameterValue = parameterValues[i];

                var specialValue = parameterValue as ISpecialValue;
                if (specialValue != null)
                {
                    if (context == null)
                        context = new CoroutineExecutionContext
                        {
                            Source = AssociatedObject,
                            Target = Target,
                            EventArgs = eventArgs,
                        };

                    parameterValue = specialValue.Resolve(context);
                }

                parameterValues[i] = ParameterBinder.CoerceValue(parameterType, parameterValue);
            }

            return parameterValues;
        }
    }
}
