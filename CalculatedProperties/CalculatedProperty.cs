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
    /// A calculated property: a property whose value is determined by a delegate. Calculated properties are target properties and may also be source properties.
    /// </summary>
    /// <typeparam name="T">The type of the property value returned by the delegate.</typeparam>
    public sealed class CalculatedProperty<T> : SourcePropertyBase, ITargetProperty
    {
        private readonly Func<T> _calculateValue;
        private readonly HashSet<ISourceProperty> _sources;
        private T _value;
        private bool _valueIsValid;

        /// <summary>
        /// Creates the calculated property.
        /// </summary>
        /// <param name="onPropertyChanged">A method that raises <see cref="INotifyPropertyChanged.PropertyChanged"/>.</param>
        /// <param name="calculateValue">The delegate used to calculate the property value.</param>
        public CalculatedProperty(Action<PropertyChangedEventArgs> onPropertyChanged, Func<T> calculateValue)
            : base(onPropertyChanged)
        {
            _calculateValue = calculateValue;
            _sources = new HashSet<ISourceProperty>();
        }

        /// <summary>
        /// Gets the value of the property. If the value has already been calculated and is valid, then the cached value is returned. Otherwise, a valid value is calculated (using dependency tracking) and returned.
        /// </summary>
        /// <param name="propertyName">The name of the property.</param>
        public T GetValue([CallerMemberName] string propertyName = null)
        {
            SetPropertyName(propertyName);
            DependencyTracker.Instance.Register(this);
            if (_valueIsValid)
                return _value;
            using (DependencyTracker.Instance.StartDependencyTracking(this))
            {
                _value = _calculateValue();
                _valueIsValid = true;
                return _value;
            }
        }

        /// <summary>
        /// Queues <see cref="INotifyPropertyChanged.PropertyChanged"/> and invalidates this property and the transitive closure of all its target properties. If notifications are not deferred, then this method will raise <see cref="INotifyPropertyChanged.PropertyChanged"/> for all affected properties before returning.
        /// </summary>
        public override void Invalidate()
        {
            _valueIsValid = false;
            _value = default(T);
            base.Invalidate();
        }

        ISet<ISourceProperty> ITargetProperty.Sources
        {
            get { return _sources; }
        }

        void ITargetProperty.UpdateSources(ISet<ISourceProperty> sourcesToRemove, ISet<ISourceProperty> sourcesToAdd)
        {
            _sources.ExceptWith(sourcesToRemove);
            if (sourcesToAdd != null)
                _sources.UnionWith(sourcesToAdd);
        }
    }
}
