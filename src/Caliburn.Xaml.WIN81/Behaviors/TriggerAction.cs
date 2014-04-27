using Microsoft.Xaml.Interactivity;
using Windows.UI.Xaml;

namespace Caliburn.Xaml
{
    /// <summary>
    /// Represents an attachable object that encapsulates a unit of functionality.
    /// </summary>
    public abstract class TriggerAction : DependencyObject, IAction, IAttachedObject
    {
        private DependencyObject _associatedObject;

        /// <summary>
        /// Attaches to the specified object.
        /// </summary>
        /// <param name="associatedObject">The <see cref="DependencyObject" /> to which the <seealso cref="TriggerAction" /> will be attached.</param>
        public void Attach(DependencyObject associatedObject)
        {
            _associatedObject = associatedObject;
            OnAttached();
        }

        /// <summary>
        /// Detaches this instance from its associated object.
        /// </summary>
        public void Detach()
        {
            OnDetaching();
            _associatedObject = null;
        }

        /// <summary>
        /// Gets the <see cref="DependencyObject" /> to which the <seealso cref="TriggerAction" /> is attached.
        /// </summary>
        public DependencyObject AssociatedObject
        {
            get { return _associatedObject; }
        }

        /// <summary>
        ///  Called after the action is attached to an AssociatedObject.
        /// </summary>
        protected virtual void OnAttached() { }

        /// <summary>
        /// Called when the action is being detached from its AssociatedObject, but before it has actually occurred.
        /// </summary>
        protected virtual void OnDetaching() { }

        /// <summary>
        /// Executes the action.
        /// </summary>
        /// <param name="sender">The object that is passed to the action by the behavior. Generally this is <seealso cref="IBehavior.AssociatedObject" /> or a target object.</param>
        /// <param name="parameter">The value of this parameter is determined by the caller.</param>
        /// <returns>
        /// Returns the result of the action.
        /// </returns>
        public object Execute(object sender, object parameter)
        {
            Invoke(parameter);
            return null;
        }

        /// <summary>
        /// Invokes the action.
        /// </summary>
        /// <param name="parameter">The parameter to the action. If the action does not require a parameter, the parameter may be set to a null reference.</param>
        protected abstract void Invoke(object parameter);
    }
}
