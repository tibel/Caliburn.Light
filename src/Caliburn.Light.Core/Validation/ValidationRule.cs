using System;

namespace Caliburn.Light
{
    /// <summary>
    /// Provides a way to create a custom rule in order to check the validity of an object.
    /// </summary>
    /// <typeparam name="T">The type of the object the rule applies to.</typeparam>
    public abstract class ValidationRule<T>
    {
        private readonly string _propertyName;
        private readonly string _errorMessage;

        /// <summary>
        /// Initializes a new instance of the <see cref="ValidationRule&lt;T&gt;"/> class.
        /// </summary>
        /// <param name="propertyName">The name of the property this instance applies to.</param>
        /// <param name="errorMessage">The error message if the rules fails.</param>
        protected ValidationRule(string propertyName, string errorMessage)
        {
            if (string.IsNullOrEmpty(propertyName))
                throw new ArgumentNullException(nameof(propertyName));
            if (string.IsNullOrEmpty(errorMessage))
                throw new ArgumentNullException(nameof(errorMessage));

            _propertyName = propertyName;
            _errorMessage = errorMessage;
        }

        /// <summary>
        /// Gets the name of the property this instance applies to.
        /// </summary>
        public string PropertyName
        {
            get { return _propertyName; }
        }

        /// <summary>
        /// Gets the error message if the rules fails.
        /// </summary>
        public string ErrorMessage
        {
            get { return _errorMessage; }
        }

        /// <summary>
        /// Applies the rule to the specified object.
        /// </summary>
        /// <param name="obj">The object to apply the rule to.</param>
        /// <returns>
        /// <c>true</c> if the object satisfies the rule, otherwise <c>false</c>.
        /// </returns>
        public abstract bool Apply(T obj);
    }
}
