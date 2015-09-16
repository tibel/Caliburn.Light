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
            /// An implementation of <see cref="IConductor"/> that holds on many items but only activates one at a time.
            /// </summary>
            public class OneActive : ConductorBaseWithActiveItem<T>
            {
                private readonly BindableCollection<T> _items = new BindableCollection<T>();

                /// <summary>
                /// Initializes a new instance of the <see cref="Conductor&lt;T&gt;.Collection.OneActive"/> class.
                /// </summary>
                public OneActive()
                {
                    _items.CollectionChanged += (s, e) =>
                    {
                        switch (e.Action)
                        {
                            case NotifyCollectionChangedAction.Add:
                                foreach (var x in e.NewItems.OfType<IChild>()) { x.Parent = this; }
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
                /// Gets the children.
                /// </summary>
                /// <returns>The collection of children.</returns>
                public override IEnumerable<T> GetChildren()
                {
                    return _items;
                }

                /// <summary>
                /// Activates the specified item.
                /// </summary>
                /// <param name="item">The item to activate.</param>
                public override void ActivateItem(T item)
                {
                    if (item != null && ReferenceEquals(item, ActiveItem))
                    {
                        if (IsActive)
                        {
                            ScreenHelper.TryActivate(item);
                            OnActivationProcessed(item, true);
                        }

                        return;
                    }

                    ChangeActiveItem(item, false);
                }

                /// <summary>
                /// Deactivates the specified item.
                /// </summary>
                /// <param name="item">The item to close.</param>
                /// <param name="close">Indicates whether or not to close the item after deactivating it.</param>
                public override async void DeactivateItem(T item, bool close)
                {
                    if (item == null) return;

                    if (!close)
                    {
                        ScreenHelper.TryDeactivate(item, false);
                    }
                    else
                    {
                        var result = await CloseStrategy.ExecuteAsync(new[] {item});
                        if (result.Item1)
                            CloseItemCore(item);
                    }
                }

                private void CloseItemCore(T item)
                {
                    if (ReferenceEquals(item, ActiveItem))
                    {
                        var index = _items.IndexOf(item);
                        var next = DetermineNextItemToActivate(_items, index);

                        ChangeActiveItem(next, true);
                    }
                    else
                    {
                        ScreenHelper.TryDeactivate(item, true);
                    }

                    _items.Remove(item);
                }

                /// <summary>
                /// Determines the next item to activate based on the last active index.
                /// </summary>
                /// <param name="list">The list of possible active items.</param>
                /// <param name="lastIndex">The index of the last active item.</param>
                /// <returns>The next item to activate.</returns>
                /// <remarks>Called after an active item is closed.</remarks>
                protected virtual T DetermineNextItemToActivate(IList<T> list, int lastIndex)
                {
                    var toRemoveAt = lastIndex - 1;

                    if (toRemoveAt == -1 && list.Count > 1)
                    {
                        return list[1];
                    }

                    if (toRemoveAt > -1 && toRemoveAt < list.Count - 1)
                    {
                        return list[toRemoveAt];
                    }

                    return default(T);
                }

                /// <summary>
                /// Called to check whether or not this instance can close.
                /// </summary>
                /// <returns>A task containing the result of the close check.</returns>
                public override async Task<bool> CanCloseAsync()
                {
                    var result = await CloseStrategy.ExecuteAsync(_items.ToArray());

                    var canClose = result.Item1;
                    var closable = result.Item2;

                    if (!canClose && closable.Any())
                    {
                        if (closable.Contains(ActiveItem))
                        {
                            var list = _items.ToList();
                            var next = ActiveItem;
                            do
                            {
                                var previous = next;
                                next = DetermineNextItemToActivate(list, list.IndexOf(previous));
                                list.Remove(previous);
                            } while (closable.Contains(next));

                            var previousActive = ActiveItem;
                            ChangeActiveItem(next, true);
                            _items.Remove(previousActive);

                            var stillToClose = closable.ToList();
                            stillToClose.Remove(previousActive);
                            closable = stillToClose;
                        }

                        foreach (var x in closable.OfType<IDeactivate>()) { x.Deactivate(true); }
                        _items.RemoveRange(closable);
                    }

                    return canClose;
                }

                /// <summary>
                /// Called when activating.
                /// </summary>
                protected override void OnActivate()
                {
                    ScreenHelper.TryActivate(ActiveItem);
                }

                /// <summary>
                /// Called when deactivating.
                /// </summary>
                /// <param name="close">Inidicates whether this instance will be closed.</param>
                protected override void OnDeactivate(bool close)
                {
                    if (close)
                    {
                        foreach (var x in _items.OfType<IDeactivate>()) { x.Deactivate(true); }
                        _items.Clear();
                    }
                    else
                    {
                        ScreenHelper.TryDeactivate(ActiveItem, false);
                    }
                }

                /// <summary>
                /// Ensures that an item is ready to be activated.
                /// </summary>
                /// <param name="newItem"></param>
                /// <returns>The item to be activated.</returns>
                protected override T EnsureItem(T newItem)
                {
                    if (newItem == null)
                    {
                        newItem = DetermineNextItemToActivate(_items, ActiveItem != null ? _items.IndexOf(ActiveItem) : 0);
                    }
                    else
                    {
                        var index = _items.IndexOf(newItem);

                        if (index < 0)
                            _items.Add(newItem);
                        else
                            newItem = _items[index];
                    }

                    return base.EnsureItem(newItem);
                }
            }
        }
    }
}
