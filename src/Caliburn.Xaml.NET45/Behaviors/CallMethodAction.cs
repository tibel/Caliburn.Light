﻿using Caliburn.Light;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Windows;
using System.Windows.Interactivity;
using System.Windows.Markup;
using Weakly;

namespace Caliburn.Xaml
{
    /// <summary>
    /// Calls a method on a specified object when invoked.
    /// </summary>
    [ContentProperty("Parameters")]
    public class CallMethodAction : TriggerAction<UIElement>, IHaveParameters
    {
        /// <summary>
        /// Identifies the <seealso cref="CallMethodAction.TargetObject"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty TargetObjectProperty = DependencyProperty.Register("TargetObject",
            typeof (object), typeof (CallMethodAction), new PropertyMetadata(OnTargetObjectChanged));

        /// <summary>
        /// Identifies the <seealso cref="CallMethodAction.MethodName"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty MethodNameProperty = DependencyProperty.Register("MethodName",
            typeof(string), typeof(CallMethodAction), new PropertyMetadata(OnMethodNameChanged));

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
            callMethodAction.UpdateAvailability();
        }

        private static void OnTargetObjectChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            var callMethodAction = (CallMethodAction)sender;
            callMethodAction.UpdateMethodInfo();
            callMethodAction.UpdateAvailability();
        }

        /// <summary>
        /// Called after the action is attached to an AssociatedObject.
        /// </summary>
        protected override void OnAttached()
        {
            base.OnAttached();
            Parameters.Attach(AssociatedObject);
            Parameters.ForEach(x => x.MakeAwareOf(this));

            UpdateMethodInfo();
            UpdateAvailability();
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

            _guard = Target.GetType().GetRuntimeProperty(_guardName).GetMethod;
            if (_guard == null) return;

            _propertyChangedRegistration = WeakEventHandler.Register<PropertyChangedEventArgs>(inpc, "PropertyChanged", OnPropertyChanged);
        }

        private void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (string.IsNullOrEmpty(e.PropertyName) || e.PropertyName == _guardName)
            {
                if (Dispatcher.CheckAccess())
                    UpdateAvailability();
                else
                    Dispatcher.BeginInvoke(new Action(UpdateAvailability));
            }
        }

        /// <summary>
        /// Forces an update of the IsEnabled state.
        /// </summary>
        public void UpdateAvailability()
        {
            if (AssociatedObject == null) return;

            if (_guard == null)
            {
                AssociatedObject.IsEnabled = true;
                return;
            }

            var guardFunction = DynamicDelegate.From(_guard);
            var canExecute = (bool) guardFunction(Target, ParameterBinder.DetermineParameters(Parameters, _guard.GetParameters()));
            AssociatedObject.IsEnabled = canExecute;
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