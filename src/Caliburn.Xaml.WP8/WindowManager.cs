using Caliburn.Light;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Animation;
using Weakly;

namespace Caliburn.Xaml
{
    /// <summary>
    /// A service that manages windows.
    /// </summary>
    public class WindowManager : IWindowManager
    {
        /// <summary>
        /// Predicate used to determine whether a page being navigated is actually a system dialog, which should 
        /// cause a temporary dialog disappearance.
        /// </summary>
        /// <remarks>
        /// The default implementation just take into account DatePicker and TimePicker pages from WP7 toolkit.
        /// </remarks>
        public static Func<Uri, bool> IsSystemDialogNavigation = uri =>
            uri != null && uri.ToString().StartsWith("/Microsoft.Phone.Controls.Toolkit");

        /// <summary>
        ///   Shows a modal dialog for the specified model.
        /// </summary>
        /// <param name = "rootModel">The root model.</param>
        /// <param name = "context">The context.</param>
        /// <param name = "settings">The optional dialog settings.</param>
        public virtual void ShowDialog(object rootModel, object context = null,
            IDictionary<string, object> settings = null)
        {
            var navigationSvc = IoC.GetInstance<INavigationService>();

            var host = new DialogHost(navigationSvc);
            var view = ViewLocator.LocateForModel(rootModel, host, context);
            host.Content = view;
            host.SetValue(View.IsGeneratedProperty, true);

            ViewModelBinder.Bind(rootModel, host, null);

            ApplySettings(host, settings);

            var activatable = rootModel as IActivate;
            if (activatable != null)
            {
                activatable.Activate();
            }

            var deactivator = rootModel as IDeactivate;
            if (deactivator != null)
            {
                host.Closed += delegate { deactivator.Deactivate(true); };
            }

            host.Open();
        }

        /// <summary>
        ///   Shows a popup at the current mouse position.
        /// </summary>
        /// <param name = "rootModel">The root model.</param>
        /// <param name = "context">The view context.</param>
        /// <param name = "settings">The optional popup settings.</param>
        public virtual void ShowPopup(object rootModel, object context = null,
            IDictionary<string, object> settings = null)
        {
            var popup = CreatePopup(rootModel, settings);
            var view = ViewLocator.LocateForModel(rootModel, popup, context);

            popup.Child = view;
            popup.SetValue(View.IsGeneratedProperty, true);

            ViewModelBinder.Bind(rootModel, popup, null);

            var activatable = rootModel as IActivate;
            if (activatable != null)
            {
                activatable.Activate();
            }

            var deactivator = rootModel as IDeactivate;
            if (deactivator != null)
            {
                popup.Closed += delegate { deactivator.Deactivate(true); };
            }

            popup.IsOpen = true;
        }

        /// <summary>
        ///   Creates a popup for hosting a popup window.
        /// </summary>
        /// <param name = "rootModel">The model.</param>
        /// <param name = "settings">The optional popup settings.</param>
        /// <returns>The popup.</returns>
        protected virtual Popup CreatePopup(object rootModel, IDictionary<string, object> settings)
        {
            var popup = new Popup();
            ApplySettings(popup, settings);
            return popup;
        }

        private static void ApplySettings(object target, IEnumerable<KeyValuePair<string, object>> settings)
        {
            if (settings == null) return;

            var type = target.GetType();
            foreach (var pair in settings)
            {
                var propertyInfo = type.GetProperty(pair.Key);
                if (propertyInfo != null)
                    propertyInfo.SetValue(target, pair.Value, null);
            }
        }

        private static Transform SafeTransformToVisual(UIElement element, UIElement visual)
        {
            GeneralTransform result;

            try
            {
                result = element.TransformToVisual(visual);
            }
            catch (ArgumentException)
            {
                result = null;
            }

            return result as Transform ?? new TranslateTransform();
        }

        [ContentProperty("Content")]
        internal sealed class DialogHost : FrameworkElement
        {
            private readonly INavigationService _navigationSvc;
            private PhoneApplicationPage _currentPage;

            private Popup _hostPopup;
            private bool _isOpen;
            private ContentControl _viewContainer;
            private Border _pageFreezingLayer;
            private Border _maskingLayer;
            private readonly IElementPlacementAnimator _elementPlacementAnimator;

            private readonly Dictionary<IApplicationBarIconButton, bool> _appBarButtonsStatus =
                new Dictionary<IApplicationBarIconButton, bool>();

            private bool _appBarMenuEnabled;

            public DialogHost(INavigationService navigationSvc)
            {
                _navigationSvc = navigationSvc;

                _currentPage = navigationSvc.CurrentContent as PhoneApplicationPage;
                if (_currentPage == null)
                {
                    throw new InvalidOperationException(
                        string.Format(
                            "In order to use ShowDialog the view currently loaded in the application frame ({0})"
                            + " should inherit from PhoneApplicationPage or one of its descendents.",
                            navigationSvc.CurrentContent.GetType()));
                }

                navigationSvc.Navigating += OnNavigating;
                navigationSvc.Navigated += OnNavigated;

                CreateUIElements();
                _elementPlacementAnimator = CreateElementsAnimator();
            }

            public EventHandler Closed;

            public UIElement Content
            {
                get { return (UIElement) _viewContainer.Content; }
                set { _viewContainer.Content = value; }
            }

            public void Open()
            {
                if (_isOpen)
                {
                    return;
                }

                _isOpen = true;

                if (_currentPage.ApplicationBar != null)
                {
                    DisableAppBar();
                }

                ArrangePlacement();

                _currentPage.BackKeyPress += CurrentPageBackKeyPress;
                _currentPage.OrientationChanged += CurrentPageOrientationChanged;

                _hostPopup.IsOpen = true;
            }

            public void Close()
            {
                Close(false);
            }

            private void Close(bool reopenOnBackNavigation)
            {
                if (!_isOpen)
                {
                    return;
                }

                _isOpen = false;
                _elementPlacementAnimator.Exit(() => { _hostPopup.IsOpen = false; });

                if (_currentPage.ApplicationBar != null)
                {
                    RestoreAppBar();
                }

                _currentPage.BackKeyPress -= CurrentPageBackKeyPress;
                _currentPage.OrientationChanged -= CurrentPageOrientationChanged;

                if (!reopenOnBackNavigation)
                {
                    _navigationSvc.Navigating -= OnNavigating;
                    _navigationSvc.Navigated -= OnNavigated;

                    var handler = Closed;
                    if (handler != null)
                        handler(this, EventArgs.Empty);
                }
            }

            private IElementPlacementAnimator CreateElementsAnimator()
            {
                return new DefaultElementPlacementAnimator(_maskingLayer, _viewContainer);
            }

            private void CreateUIElements()
            {
                _viewContainer = new ContentControl
                {
                    HorizontalContentAlignment = HorizontalAlignment.Stretch,
                    VerticalContentAlignment = VerticalAlignment.Top,
                };
                _maskingLayer = new Border
                {
                    Child = _viewContainer,
                    Background = new SolidColorBrush(Color.FromArgb(170, 0, 0, 0)),
                    VerticalAlignment = VerticalAlignment.Top,
                    HorizontalAlignment = HorizontalAlignment.Left
                };
                _pageFreezingLayer = new Border
                {
                    Background = new SolidColorBrush(Colors.Transparent),
                    Width = Application.Current.Host.Content.ActualWidth,
                    Height = Application.Current.Host.Content.ActualHeight
                };

                var panel = new Canvas();
                panel.Children.Add(_pageFreezingLayer);
                panel.Children.Add(_maskingLayer);

                _hostPopup = new Popup {Child = panel};
            }

            private void DisableAppBar()
            {
                _appBarMenuEnabled = _currentPage.ApplicationBar.IsMenuEnabled;
                _appBarButtonsStatus.Clear();
                _currentPage.ApplicationBar.Buttons.Cast<IApplicationBarIconButton>()
                    .ForEach(b =>
                    {
                        _appBarButtonsStatus.Add(b, b.IsEnabled);
                        b.IsEnabled = false;
                    });

                _currentPage.ApplicationBar.IsMenuEnabled = false;
            }

            private void RestoreAppBar()
            {
                _currentPage.ApplicationBar.IsMenuEnabled = _appBarMenuEnabled;
                _currentPage.ApplicationBar.Buttons.Cast<IApplicationBarIconButton>()
                    .ForEach(b =>
                    {
                        bool status;
                        if (_appBarButtonsStatus.TryGetValue(b, out status))
                            b.IsEnabled = status;
                    });
            }

            private void ArrangePlacement()
            {
                _maskingLayer.Dispatcher.BeginInvoke(() =>
                {
                    var placement = new ElementPlacement
                    {
                        Transform = SafeTransformToVisual(_currentPage, null),
                        Orientation = _currentPage.Orientation,
                        Size = new Size(_currentPage.ActualWidth, _currentPage.ActualHeight)
                    };

                    _elementPlacementAnimator.AnimateTo(placement);
                });
            }

            private Uri _currentPageUri;

            private void OnNavigating(object sender, System.Windows.Navigation.NavigatingCancelEventArgs e)
            {
                if (IsSystemDialogNavigation(e.Uri))
                {
                    _currentPageUri = _navigationSvc.CurrentSource;
                }
            }

            private void OnNavigated(object sender, System.Windows.Navigation.NavigationEventArgs e)
            {
                if (IsSystemDialogNavigation(e.Uri))
                {
                    Close(_currentPageUri != null);
                }
                else if (e.Uri.Equals(_currentPageUri))
                {
                    _currentPageUri = null;
                    //refreshes the page instance
                    _currentPage = (PhoneApplicationPage) _navigationSvc.CurrentContent;

                    Open();
                }
                else
                {
                    Close(reopenOnBackNavigation: false);
                }
            }

            private void CurrentPageBackKeyPress(object sender, CancelEventArgs e)
            {
                e.Cancel = true;
                Close();
            }

            private void CurrentPageOrientationChanged(object sender, OrientationChangedEventArgs e)
            {
                ArrangePlacement();
            }

            public class ElementPlacement
            {
                public Transform Transform;
                public PageOrientation Orientation;
                public Size Size;

                public double AngleFromDefault
                {
                    get
                    {
                        if ((Orientation & PageOrientation.Landscape) == 0)
                        {
                            return 0;
                        }

                        return Orientation == PageOrientation.LandscapeRight ? 90 : -90;
                    }
                }
            }

            public interface IElementPlacementAnimator
            {
                void Enter(ElementPlacement initialPlacement);
                void AnimateTo(ElementPlacement newPlacement);
                void Exit(Action onCompleted);
            }

            public class DefaultElementPlacementAnimator : IElementPlacementAnimator
            {
                private readonly FrameworkElement _maskingLayer;
                private readonly FrameworkElement _viewContainer;
                private readonly Storyboard _storyboard = new Storyboard();
                private ElementPlacement _currentPlacement;

                public DefaultElementPlacementAnimator(FrameworkElement maskingLayer, FrameworkElement viewContainer)
                {
                    _maskingLayer = maskingLayer;
                    _viewContainer = viewContainer;
                }

                public void Enter(ElementPlacement initialPlacement)
                {
                    _currentPlacement = initialPlacement;

                    //size
                    _maskingLayer.Width = _currentPlacement.Size.Width;
                    _maskingLayer.Height = _currentPlacement.Size.Height;

                    //position and orientation
                    _maskingLayer.RenderTransform = _currentPlacement.Transform;

                    //enter animation
                    var projection = new PlaneProjection {CenterOfRotationY = 0.1};
                    _viewContainer.Projection = projection;
                    AddDoubleAnimation(projection, "RotationX", -90, 0, 400);
                    AddDoubleAnimation(_maskingLayer, "Opacity", 0, 1, 400);

                    _storyboard.Begin();
                }

                public void AnimateTo(ElementPlacement newPlacement)
                {
                    _storyboard.Stop();
                    _storyboard.Children.Clear();

                    if (_currentPlacement == null)
                    {
                        Enter(newPlacement);
                        return;
                    }

                    //size
                    AddDoubleAnimation(_maskingLayer, "Width", _currentPlacement.Size.Width, newPlacement.Size.Width,
                        200);
                    AddDoubleAnimation(_maskingLayer, "Height", _currentPlacement.Size.Height, newPlacement.Size.Height,
                        200);

                    //rotation at orientation change
                    var transformGroup = new TransformGroup();
                    var rotation = new RotateTransform
                    {
                        CenterX = Application.Current.Host.Content.ActualWidth/2,
                        CenterY = Application.Current.Host.Content.ActualHeight/2
                    };
                    transformGroup.Children.Add(newPlacement.Transform);
                    transformGroup.Children.Add(rotation);
                    _maskingLayer.RenderTransform = transformGroup;
                    AddDoubleAnimation(rotation, "Angle",
                        newPlacement.AngleFromDefault - _currentPlacement.AngleFromDefault, 0.0);

                    //slight fading at orientation change
                    AddFading(_maskingLayer);

                    _currentPlacement = newPlacement;
                    _storyboard.Begin();
                }

                public void Exit(Action onCompleted)
                {
                    _storyboard.Stop();
                    _storyboard.Children.Clear();

                    //exit animation
                    var projection = new PlaneProjection {CenterOfRotationY = 0.1};
                    _viewContainer.Projection = projection;
                    AddDoubleAnimation(projection, "RotationX", 0, 90, 250);
                    AddDoubleAnimation(_maskingLayer, "Opacity", 1, 0, 350);

                    EventHandler handler = null;
                    handler = (o, e) =>
                    {
                        _storyboard.Completed -= handler;
                        onCompleted();
                        _currentPlacement = null;
                    };
                    _storyboard.Completed += handler;
                    _storyboard.Begin();
                }

                private void AddDoubleAnimation(DependencyObject target, string property, double from, double to,
                    int ms = 500)
                {
                    var timeline = new DoubleAnimation
                    {
                        From = from,
                        To = to,
                        EasingFunction = new ExponentialEase {EasingMode = EasingMode.EaseOut, Exponent = 4},
                        Duration = new Duration(TimeSpan.FromMilliseconds(ms))
                    };

                    Storyboard.SetTarget(timeline, target);
                    Storyboard.SetTargetProperty(timeline, new PropertyPath(property));
                    _storyboard.Children.Add(timeline);
                }

                private void AddFading(FrameworkElement target)
                {
                    var timeline = new DoubleAnimationUsingKeyFrames
                    {
                        Duration = new Duration(TimeSpan.FromMilliseconds(500))
                    };
                    timeline.KeyFrames.Add(new LinearDoubleKeyFrame
                    {
                        Value = 1,
                        KeyTime = KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(0))
                    });
                    timeline.KeyFrames.Add(new LinearDoubleKeyFrame
                    {
                        Value = 0.5,
                        KeyTime = KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(150))
                    });
                    timeline.KeyFrames.Add(new LinearDoubleKeyFrame
                    {
                        Value = 1,
                        KeyTime = KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(300))
                    });

                    Storyboard.SetTarget(timeline, target);
                    Storyboard.SetTargetProperty(timeline, new PropertyPath("Opacity"));
                    _storyboard.Children.Add(timeline);
                }
            }
        }
    }
}
