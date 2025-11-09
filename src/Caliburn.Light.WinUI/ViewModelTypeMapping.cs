using Microsoft.UI.Xaml;
using System;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;

namespace Caliburn.Light.WinUI
{
    /// <summary>
    /// Describes a view view-model mapping.
    /// </summary>
    public readonly struct ViewModelTypeMapping
    {
        /// <summary>
        /// The view type.
        /// </summary>
        public Type ViewType { get; }

        /// <summary>
        /// The view-model type.
        /// </summary>
        public Type ModelType { get; }

        /// <summary>
        /// The context instance (or null).
        /// </summary>
        public string? Context { get; }

        private ViewModelTypeMapping(Type viewType, Type modelType, string? context)
        {
            ViewType = viewType;
            ModelType = modelType;
            Context = context;
        }

        /// <summary>
        /// Creates a view view-model mapping.
        /// </summary>
        /// <typeparam name="TView">The view type.</typeparam>
        /// <typeparam name="TViewModel">The view-model type.</typeparam>
        /// <param name="context">The context instance (or null).</param>
        public static ViewModelTypeMapping Create<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)] TView, TViewModel>(string? context = null)
           where TView : UIElement
           where TViewModel : INotifyPropertyChanged
        {
            return new ViewModelTypeMapping(typeof(TView), typeof(TViewModel), context);
        }
    }
}
