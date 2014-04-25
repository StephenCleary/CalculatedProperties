using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
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
            Attach(_value);
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
            Detach(_value);
            _value = value;
            Attach(_value);
            if (!equal)
                Invalidate();
        }

        // todo: figure out a way to move this to SourcePropertyBase; better performance (static lookups, etc)
        private Delegate _handler;
        private void Attach(T value)
        {
            if (value == null)
                return;
            
            // todo: move to static (and shared!)
            var tType = typeof (T);
            var nccInterfaceType = tType.GetInterfaces().FirstOrDefault(x => x.FullName == "System.Collections.Specialized.INotifyCollectionChanged");
            if (nccInterfaceType == null)
                return;
            var ccEvent = nccInterfaceType.GetEvent("CollectionChanged");
            if (ccEvent == null)
                return;
            var assembly = nccInterfaceType.Assembly;
            var argsType = assembly.GetType("System.Collections.Specialized.NotifyCollectionChangedEventArgs");
            var lambdaType = assembly.GetType("System.Collections.Specialized.NotifyCollectionChangedEventHandler");
            var r = new Reflection();
            var sender = Expression.Parameter(typeof (object), "sender");
            var args = Expression.Parameter(argsType, "e");
            var lambda = r.Lambda(lambdaType, r.Call(r.Constant(GetType(), this), "InvalidateTargets"), sender, args);
            _handler = lambda.Compile();
            ccEvent.AddEventHandler(value, _handler);
        }

        private void Detach(T value)
        {
            if (value == null || _handler == null)
                return;

            var tType = typeof(T);
            var nccInterfaceType = tType.GetInterfaces().FirstOrDefault(x => x.FullName == "System.Collections.Specialized.INotifyCollectionChanged");
            if (nccInterfaceType == null)
                return;
            var ccEvent = nccInterfaceType.GetEvent("CollectionChanged");
            if (ccEvent == null)
                return;
            ccEvent.RemoveEventHandler(value, _handler);
        }
    }
}
