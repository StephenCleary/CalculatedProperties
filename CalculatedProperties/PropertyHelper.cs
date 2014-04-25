using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using CalculatedProperties.Internal;

namespace CalculatedProperties
{
    public sealed class PropertyHelper
    {
        private readonly Action<PropertyChangedEventArgs> _onPropertyChanged;
        private readonly Dictionary<string, IProperty> _properties = new Dictionary<string, IProperty>();

        public PropertyHelper(Action<PropertyChangedEventArgs> onPropertyChanged)
        {
            _onPropertyChanged = onPropertyChanged;
        }

        public TriggerProperty<T> RetrieveTriggerProperty<T>(T initialValue = default(T), IEqualityComparer<T> comparer = null, [CallerMemberName] string propertyName = null)
        {
            IProperty result;
            if (!_properties.TryGetValue(propertyName, out result))
            {
                result = new TriggerProperty<T>(_onPropertyChanged, initialValue, comparer);
                _properties.Add(propertyName, result);
            }

            return result as TriggerProperty<T>;
        }

        public CalculatedProperty<T> RetrieveCalculatedProperty<T>(Func<T> calculateValue, [CallerMemberName] string propertyName = null)
        {
            IProperty result;
            if (!_properties.TryGetValue(propertyName, out result))
            {
                result = new CalculatedProperty<T>(_onPropertyChanged, calculateValue);
                _properties.Add(propertyName, result);
            }

            return result as CalculatedProperty<T>;
        }

        public T Get<T>(T initialValue, IEqualityComparer<T> comparer = null, [CallerMemberName] string propertyName = null)
        {
            return RetrieveTriggerProperty(initialValue, comparer, propertyName).GetValue(propertyName);
        }

        public void Set<T>(T value, IEqualityComparer<T> comparer = null, [CallerMemberName] string propertyName = null)
        {
            RetrieveTriggerProperty(value, comparer, propertyName).SetValue(value, propertyName);
        }

        public T Calculated<T>(Func<T> calculateValue, [CallerMemberName] string propertyName = null)
        {
            return RetrieveCalculatedProperty(calculateValue, propertyName).GetValue(propertyName);
        }
    }
}
