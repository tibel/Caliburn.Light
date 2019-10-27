using System;
using System.Linq.Expressions;
using System.Reflection;

namespace Caliburn.Light
{
    ///<summary>
    /// Provides support for extracting property information based on a property expression.
    ///</summary>
    public static class PropertySupport
    {
        /// <summary>
        /// Extracts the property name from a property expression.
        /// </summary>
        /// <param name="propertyExpression">The property expression (e.g. p => p.PropertyName)</param>
        /// <returns>The name of the property.</returns>
        public static string ExtractPropertyName<TProperty>(Expression<Func<TProperty>> propertyExpression)
        {
            return ExtractPropertyName((LambdaExpression)propertyExpression);
        }
        
        /// <summary>
        /// Extracts the property name from a property expression.
        /// </summary>
        /// <param name="propertyExpression">The property expression (e.g. p => p.PropertyName)</param>
        /// <returns>The name of the property.</returns>
        public static string ExtractPropertyName(LambdaExpression propertyExpression)
        {
            if (propertyExpression is null)
                throw new ArgumentNullException(nameof(propertyExpression));

            return GetMemberInfo(propertyExpression).Name;
        }

        private static MemberInfo GetMemberInfo(Expression expression)
        {
            expression = ((LambdaExpression)expression).Body;

            if (expression.NodeType == ExpressionType.Convert)
            {
                expression = ((UnaryExpression)expression).Operand;
            }

            var memberExpression = (MemberExpression)expression;
            return memberExpression.Member;
        }
    }
}
