using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using Weakly;

namespace Caliburn.Light
{
    /// <summary>
    /// A base class for objects of which the properties must be observable.
    /// </summary>
    public class BindableObject : IBindableObject
    {
        private int _suspensionCount;

        /// <summary>
        /// Occurs when a property value changes.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Suspends the notifications.
        /// </summary>
        /// <returns></returns>
        public IDisposable SuspendNotifications()
        {
            _suspensionCount++;
            return new DisposableAction(ResumeNotifications);
        }

        private void ResumeNotifications()
        {
            _suspensionCount--;
        }

        /// <summary>
        /// Raises a change notification indicating that all bindings should be refreshed.
        /// </summary>
        public void Refresh()
        {
            RaisePropertyChanged(string.Empty);
        }

        /// <summary>
        /// Determines wether notifications are suspended.
        /// </summary>
        /// <returns></returns>
        protected bool AreNotificationsSuspended()
        {
            return _suspensionCount > 0;
        }

        /// <summary>
        /// Raises the PropertyChanged event if needed.
        /// </summary>
        /// <param name="propertyName">The name of the property thatchanged.</param>
        protected virtual void RaisePropertyChanged([CallerMemberName] string propertyName = null)
        {
            if (AreNotificationsSuspended()) return;

            var handler = PropertyChanged;
            if (handler != null)
                handler(this, new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        /// Raises the PropertyChanged event if needed.
        /// </summary>
        /// <typeparam name = "TProperty">The type of the property.</typeparam>
        /// <param name = "property">The property expression.</param>
        protected void RaisePropertyChanged<TProperty>(Expression<Func<TProperty>> property)
        {
            RaisePropertyChanged(property.GetMemberInfo().Name);
        }

        /// <summary>
        /// Assigns a new value to the property. Then, raises the PropertyChanged event if needed. 
        /// </summary>
        /// <typeparam name="T">The type of the property that changed.</typeparam>
        /// <param name="field">The field storing the property's value.</param>
        /// <param name="newValue">The property's value after the change occurred.</param>
        /// <param name="propertyName">The name of the property that changed.</param>
        /// <returns>True if the PropertyChanged event has been raised, false otherwise. 
        /// The event is not raised if the old value is equal to the new value.</returns>
        protected bool Set<T>(ref T field, T newValue, [CallerMemberName] string propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, newValue))
                return false;

            field = newValue;
            RaisePropertyChanged(propertyName);
            return true;
        }
    }
}
