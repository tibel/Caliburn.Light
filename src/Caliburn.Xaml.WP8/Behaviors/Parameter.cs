using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Interactivity;

namespace Caliburn.Light
{
    /// <summary>
    /// Represents a parameter of a TriggerAction.
    /// </summary>
    public class Parameter : DependencyObject, IAttachedObject
    {
        /// <summary>
        /// Identifies the <seealso cref="Parameter.Value"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty ValueProperty = DependencyProperty.Register("Value", typeof (object),
            typeof (Parameter), new PropertyMetadata(null, OnValueChanged));

        private DependencyObject _associatedObject;
        private WeakReference _owner;

        /// <summary>
        /// Gets or sets the value of the parameter.
        /// </summary>
        [Category("Common Properties")]
        public object Value
        {
            get { return GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }

        void IAttachedObject.Attach(DependencyObject dependencyObject)
        {
            _associatedObject = dependencyObject;
        }

        void IAttachedObject.Detach()
        {
            _associatedObject = null;
        }

        DependencyObject IAttachedObject.AssociatedObject
        {
            get { return _associatedObject; }
        }

        /// <summary>
        /// Gets the owner of this instance.
        /// </summary>
        protected IHaveParameters Owner
        {
            get { return _owner == null ? null : _owner.Target as IHaveParameters; }
        }

        /// <summary>
        /// Makes the parameter aware of the <see cref="IHaveParameters"/> that it is attached to.
        /// </summary>
        /// <param name="owner">The owner of the parameter.</param>
        public void MakeAwareOf(IHaveParameters owner)
        {
            _owner = (owner != null) ? new WeakReference(owner) : null;
        }

        private static void OnValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var owner = ((Parameter)d).Owner;
            if (owner != null)
                owner.UpdateEnabledState();
        }
    }
}
