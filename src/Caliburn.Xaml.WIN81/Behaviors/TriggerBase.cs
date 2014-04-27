using Microsoft.Xaml.Interactivity;
using Windows.ApplicationModel;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Markup;

namespace Caliburn.Xaml
{
    /// <summary>
    /// Represents an object that can invoke Actions conditionally.
    /// </summary>
    [ContentProperty(Name = "Actions")]
    public abstract class TriggerBase : DependencyObject, IBehavior
    {
        /// <summary>
        /// Identifies the <seealso cref="TriggerBase.Actions"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty ActionsProperty = DependencyProperty.Register("Actions",
            typeof(AttachedCollection<TriggerAction>), typeof(TriggerBase), new PropertyMetadata(null));

        private DependencyObject _associatedObject;

        /// <summary>
        /// Attaches to the specified object.
        /// </summary>
        /// <param name="associatedObject">The <see cref="DependencyObject" /> to which the <seealso cref="TriggerBase" /> will be attached.</param>
        public void Attach(DependencyObject associatedObject)
        {
            if (associatedObject == _associatedObject || DesignMode.DesignModeEnabled)
                return;

            _associatedObject = associatedObject;
            Actions.Attach(associatedObject);
            OnAttached();
        }

        /// <summary>
        /// Detaches this instance from its associated object.
        /// </summary>
        public void Detach()
        {
            OnDetaching();
            _associatedObject = null;
            Actions.Detach();
        }

        /// <summary>
        /// Gets the <see cref="DependencyObject" /> to which the <seealso cref="TriggerBase" /> is attached.
        /// </summary>
        public DependencyObject AssociatedObject
        {
            get { return _associatedObject; }
        }

        /// <summary>
        ///  Called after the trigger is attached to an AssociatedObject.
        /// </summary>
        protected virtual void OnAttached() { }

        /// <summary>
        /// Called when the trigger is being detached from its AssociatedObject, but before it has actually occurred.
        /// </summary>
        protected virtual void OnDetaching() { }

        /// <summary>
        /// Gets the collection of actions associated with the behavior. This is a dependency property.
        /// </summary>
        public AttachedCollection<TriggerAction> Actions
        {
            get
            {
                var actionCollection = (AttachedCollection<TriggerAction>)GetValue(ActionsProperty);
                if (actionCollection == null)
                {
                    actionCollection = new AttachedCollection<TriggerAction>();
                    SetValue(ActionsProperty, actionCollection);
                }
                return actionCollection;
            }
        }
    }
}
