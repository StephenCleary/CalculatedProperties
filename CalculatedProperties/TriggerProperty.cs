using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using CalculatedProperties.Internal;

namespace CalculatedProperties
{
    /// <summary>
    /// A trigger property: a source property that invalidates its targets when set.
    /// </summary>
    /// <typeparam name="T">The type of the property value.</typeparam>
    public sealed class TriggerProperty<T> : SourcePropertyBase
    {
        private readonly IEqualityComparer<T> _comparer;
        private T _value;

        /// <summary>
        /// Creates the trigger property.
        /// </summary>
        /// <param name="onPropertyChanged">A method that raises <see cref="INotifyPropertyChanged.PropertyChanged"/>.</param>
        /// <param name="initialValue">The optional initial value of the property.</param>
        /// <param name="comparer">The optional comparer used to determine when the value of the property has changed.</param>
        public TriggerProperty(Action<PropertyChangedEventArgs> onPropertyChanged, T initialValue = default(T), IEqualityComparer<T> comparer = null)
            : base(onPropertyChanged)
        {
            _comparer = comparer ?? EqualityComparer<T>.Default;
            _value = initialValue;
        }

        /// <summary>
        /// Gets the value of the property. If dependency tracking is active, then this property is registered as a source for the target property being tracked.
        /// </summary>
        /// <param name="propertyName">The name of the property.</param>
        public T GetValue([CallerMemberName] string propertyName = null)
        {
            SetPropertyName(propertyName);
            DependencyTracker.Instance.Register(this);
            return _value;
        }

        /// <summary>
        /// Sets the value of the property. The internal value is always set to the new value. If the old value and new value are different (as defined by the comparer passed to the constructor), then this property and the transitive closure of all its target properties are invalidated. If notifications are not deferred, then this method will raise <see cref="INotifyPropertyChanged.PropertyChanged"/> for all affected properties before returning.
        /// </summary>
        /// <param name="value">The new value of the property.</param>
        /// <param name="propertyName">The name of the property.</param>
        public void SetValue(T value, [CallerMemberName] string propertyName = null)
        {
            SetPropertyName(propertyName);
            var equal = _comparer.Equals(_value, value);
            _value = value;
            if (!equal)
                Invalidate();
        }
    }
}
