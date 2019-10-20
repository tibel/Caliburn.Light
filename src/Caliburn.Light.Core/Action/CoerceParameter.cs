﻿using System;

namespace Caliburn.Light
{
    /// <summary>
    /// The default implementation to coerce a parameter.
    /// </summary>
    /// <typeparam name="TParameter">The type of the command parameter.</typeparam>
    public static class CoerceParameter<TParameter>
    {
        /// <summary>
        /// Default method to coerce a parameter.
        /// </summary>
        /// <param name="parameter">The supplied parameter value.</param>
        /// <returns>The converted parameter value.</returns>
        public static TParameter Default(object parameter)
        {
            if (parameter == null)
                return default(TParameter);

            var specialValue = parameter as ISpecialValue;
            if (specialValue != null)
                parameter = specialValue.Resolve(new CoroutineExecutionContext());

            if (parameter is TParameter)
                return (TParameter)parameter;

            return (TParameter)Convert.ChangeType(parameter, typeof(TParameter));
        }
    }
}
