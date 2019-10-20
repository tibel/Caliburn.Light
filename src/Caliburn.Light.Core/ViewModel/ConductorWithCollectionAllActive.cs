using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;

namespace Caliburn.Light
{
    public partial class Conductor<T>
    {
        /// <summary>
        /// An implementation of <see cref="IConductor"/> that holds on many items.
        /// </summary>
        public partial class Collection
        {
            /// <summary>
            /// An implementation of <see cref="IConductor"/> that holds on to many items which are all activated.
            /// </summary>
            public class AllActive : ConductorBase<T>
            {
                private readonly BindableCollection<T> _items = new BindableCollection<T>();

                /// <summary>
                /// Initializes a new instance of the <see cref="Conductor&lt;T&gt;.Collection.AllActive"/> class.
                /// </summary>
                public AllActive()
                {
                    _items.CollectionChanged += (s, e) =>
                    {
                        switch (e.Action)
                        {
                            case NotifyCollectionChangedAction.Add:
                                foreach(var x in e.NewItems.OfType<IChild>()) { x.Parent = this; }
                                break;
                            case NotifyCollectionChangedAction.Remove:
                                foreach (var x in e.OldItems.OfType<IChild>()) { x.Parent = null; }
                                break;
                            case NotifyCollectionChangedAction.Replace:
                                foreach (var x in e.NewItems.OfType<IChild>()) { x.Parent = this; }
                                foreach (var x in e.OldItems.OfType<IChild>()) { x.Parent = null; }
                                break;
                            case NotifyCollectionChangedAction.Reset:
                                foreach (var x in _items.OfType<IChild>()) { x.Parent = this; }
                                break;
                        }
                    };
                }

                /// <summary>
                /// Gets the items that are currently being conducted.
                /// </summary>
                public IBindableCollection<T> Items
                {
                    get { return _items; }
                }

                /// <summary>
                /// Called when activating.
                /// </summary>
                protected override void OnActivate()
                {
                    foreach(var x in _items.OfType<IActivate>()) { x.Activate(); }
                }

                /// <summary>
                /// Called when deactivating.
                /// </summary>
                /// <param name="close">Indicates whether this instance will be closed.</param>
                protected override void OnDeactivate(bool close)
                {
                    foreach (var x in _items.OfType<IDeactivate>()) { x.Deactivate(close); }
                    if (close)
                    {
                        _items.Clear();
                    }
                }

                /// <summary>
                /// Called to check whether or not this instance can close.
                /// </summary>
                /// <returns>A task containing the result of the close check.</returns>
                public override async Task<bool> CanCloseAsync()
                {
                    var result = await CloseStrategy.ExecuteAsync(_items.ToArray());

                    var canClose = result.CanClose;
                    var closeables = result.Closeables;

                    if (!canClose && closeables.Any())
                    {
                        foreach (var x in closeables.OfType<IDeactivate>()) { x.Deactivate(true); }
                        _items.RemoveRange(closeables);
                    }

                    return canClose;
                }

                /// <summary>
                /// Activates the specified item.
                /// </summary>
                /// <param name="item">The item to activate.</param>
                public override void ActivateItem(T item)
                {
                    if (item == null) return;

                    item = EnsureItem(item);

                    if (IsActive)
                    {
                        ScreenHelper.TryActivate(item);
                    }

                    OnActivationProcessed(item, true);
                }

                /// <summary>
                /// Deactivates the specified item.
                /// </summary>
                /// <param name="item">The item to close.</param>
                /// <param name="close">Indicates whether or not to close the item after deactivating it.</param>
                public override async void DeactivateItem(T item, bool close)
                {
                    if (item == null)
                        return;

                    if (!close)
                    {
                        ScreenHelper.TryDeactivate(item, false);
                    }
                    else
                    {
                        var result = await CloseStrategy.ExecuteAsync(new[] {item});
                        if (result.CanClose)
                            CloseItemCore(item);
                    }
                }

                /// <summary>
                /// Gets the children.
                /// </summary>
                /// <returns>The collection of children.</returns>
                public override IEnumerable<T> GetChildren()
                {
                    return _items;
                }

                private void CloseItemCore(T item)
                {
                    ScreenHelper.TryDeactivate(item, true);
                    _items.Remove(item);
                }

                /// <summary>
                /// Ensures that an item is ready to be activated.
                /// </summary>
                /// <param name="newItem"></param>
                /// <returns>The item to be activated.</returns>
                protected override T EnsureItem(T newItem)
                {
                    var index = _items.IndexOf(newItem);

                    if (index < 0)
                    {
                        _items.Add(newItem);
                    }
                    else
                    {
                        newItem = _items[index];
                    }

                    return base.EnsureItem(newItem);
                }
            }
        }
    }
}
