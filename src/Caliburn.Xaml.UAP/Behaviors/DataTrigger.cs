using Microsoft.Xaml.Interactions.Core;
using Microsoft.Xaml.Interactivity;
using System;
using System.Globalization;
using System.Linq;
using Windows.ApplicationModel;
using Windows.UI.Xaml;

namespace Caliburn.Light
{
    /// <summary>
    /// Represents a trigger that performs actions when the bound data meets a specified condition.
    /// </summary>
    public sealed class DataTrigger : TriggerBase
    {
        /// <summary>
        /// The bound object that the DataTrigger will listen to.
        /// </summary>
        public static readonly DependencyProperty BindingProperty = DependencyProperty.Register("Binding",
            typeof (object), typeof (DataTrigger),
            new PropertyMetadata(null, OnValueChanged));

        /// <summary>
        /// Specifies the type of comparison to be performed between <see cref="Binding"/> and <see cref="Value"/>.
        /// </summary>
        public static readonly DependencyProperty ComparisonConditionProperty =
            DependencyProperty.Register("ComparisonCondition", typeof (ComparisonConditionType), typeof (DataTrigger),
                new PropertyMetadata(ComparisonConditionType.Equal, OnValueChanged));

        /// <summary>
        /// Specifies the value to be compared with the value of <see cref="Binding"/>.
        /// </summary>
        public static readonly DependencyProperty ValueProperty = DependencyProperty.Register("Value", typeof (object),
            typeof (DataTrigger), new PropertyMetadata(null, OnValueChanged));

        /// <summary>
        /// Gets or sets the bound object that the DataTrigger will listen to.
        /// </summary>
        public object Binding
        {
            get { return GetValue(BindingProperty); }
            set { SetValue(BindingProperty, value); }
        }

        /// <summary>
        /// Gets or sets the type of comparison to be performed between <see cref="Binding"/> and <see cref="Value"/>.
        /// </summary>
        public ComparisonConditionType ComparisonCondition
        {
            get { return (ComparisonConditionType) GetValue(ComparisonConditionProperty); }
            set { SetValue(ComparisonConditionProperty, value); }
        }

        /// <summary>
        /// Gets or sets the value to be compared with the value of <see cref="Binding"/>.
        /// </summary>
        public object Value
        {
            get { return GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }

        private static bool Compare(object leftOperand, ComparisonConditionType operatorType, object rightOperand)
        {
            if (leftOperand != null && rightOperand != null)
                rightOperand = TypeConverterHelper.Convert(rightOperand.ToString(), leftOperand.GetType().FullName);

            var leftOperand1 = leftOperand as IComparable;
            var rightOperand1 = rightOperand as IComparable;
            if (leftOperand1 != null && rightOperand1 != null)
                return EvaluateComparable(leftOperand1, operatorType, rightOperand1);

            switch (operatorType)
            {
                case ComparisonConditionType.Equal:
                    return Equals(leftOperand, rightOperand);
                case ComparisonConditionType.NotEqual:
                    return !Equals(leftOperand, rightOperand);
                case ComparisonConditionType.LessThan:
                case ComparisonConditionType.LessThanOrEqual:
                case ComparisonConditionType.GreaterThan:
                case ComparisonConditionType.GreaterThanOrEqual:
                    if (leftOperand1 == null && rightOperand1 == null)
                        throw new ArgumentException("Invalid operands.");
                    if (leftOperand1 == null)
                        throw new ArgumentException("Invalid left operand.");
                    throw new ArgumentException("Invalid right operand.");
                default:
                    return false;
            }
        }

        private static bool EvaluateComparable(IComparable leftOperand, ComparisonConditionType operatorType,
            IComparable rightOperand)
        {
            object obj = null;
            try
            {
                obj = Convert.ChangeType(rightOperand, leftOperand.GetType(), CultureInfo.CurrentCulture);
            }
            catch (FormatException)
            {
            }
            catch (InvalidCastException)
            {
            }

            if (obj == null)
                return operatorType == ComparisonConditionType.NotEqual;

            var num = leftOperand.CompareTo(obj);
            switch (operatorType)
            {
                case ComparisonConditionType.Equal:
                    return num == 0;
                case ComparisonConditionType.NotEqual:
                    return num != 0;
                case ComparisonConditionType.LessThan:
                    return num < 0;
                case ComparisonConditionType.LessThanOrEqual:
                    return num <= 0;
                case ComparisonConditionType.GreaterThan:
                    return num > 0;
                case ComparisonConditionType.GreaterThanOrEqual:
                    return num >= 0;
                default:
                    return false;
            }
        }

        private static void OnValueChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs args)
        {
            var dataTrigger = (DataTrigger) dependencyObject;
            if (dataTrigger.AssociatedObject == null)
                return;

            RefreshDataBindingsOnActions(dataTrigger.Actions);
            if (
                !Compare(dataTrigger.Binding, dataTrigger.ComparisonCondition, dataTrigger.Value))
                return;

            if (DesignMode.DesignModeEnabled)
                return;

            foreach (var action in dataTrigger.Actions.Cast<IAction>())
            {
                action.Execute(dataTrigger.AssociatedObject, args);
            }
        }

        private static void RefreshDataBindingsOnActions(TriggerActionCollection actions)
        {
            foreach (var target in actions)
            {
                foreach (var property in BindingHelper.GetDependencyProperties(target.GetType()))
                {
                    BindingHelper.RefreshBinding(target, property);
                }
            }
        }
    }
}
