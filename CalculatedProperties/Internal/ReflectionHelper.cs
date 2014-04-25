using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace CalculatedProperties.Internal
{
    public static class ReflectionHelper
    {
        private static Type _iNotifyCollectionChangedType;
        private static Type _notifyCollectionChangedEventArgsType;
        private static Type _notifyCollectionChangedEventHandlerType;
        private static EventInfo _collectionChangedEvent;

        public static class For<T>
        {
            // ReSharper disable once StaticFieldInGenericType
            private static readonly bool ImplementsINotifyCollectionChanged;

            static For()
            {
                if (_collectionChangedEvent == null)
                {
                    _iNotifyCollectionChangedType = typeof(T).GetInterfaces().FirstOrDefault(x => x.FullName == "System.Collections.Specialized.INotifyCollectionChanged");
                    if (_iNotifyCollectionChangedType == null)
                        return;

                    ImplementsINotifyCollectionChanged = true;
                    var assembly = _iNotifyCollectionChangedType.Assembly;
                    _notifyCollectionChangedEventArgsType = assembly.GetType("System.Collections.Specialized.NotifyCollectionChangedEventArgs");
                    _notifyCollectionChangedEventHandlerType = assembly.GetType("System.Collections.Specialized.NotifyCollectionChangedEventHandler");
                    _collectionChangedEvent = _iNotifyCollectionChangedType.GetEvent("CollectionChanged");
                }
                else
                {
                    ImplementsINotifyCollectionChanged = typeof (T).GetInterfaces().Contains(_iNotifyCollectionChangedType);
                }
            }

            public static Delegate AddEventHandler(object property, T value)
            {
                // ReSharper disable once CompareNonConstrainedGenericWithNull
                if (!ImplementsINotifyCollectionChanged || value == null)
                    return null;

                var sender = Expression.Parameter(typeof(object), "sender");
                var args = Expression.Parameter(_notifyCollectionChangedEventArgsType, "e");
                var lambda = Expression.Lambda(_notifyCollectionChangedEventHandlerType,
                    Expression.Call(Expression.Constant(property, property.GetType()), "InvalidateTargets", null),
                    sender, args);
                var handler = lambda.Compile();
                _collectionChangedEvent.AddEventHandler(value, handler);
                return handler;
            }

            public static void RemoveEventHandler(T value, Delegate handler)
            {
                // ReSharper disable once CompareNonConstrainedGenericWithNull
                if (!ImplementsINotifyCollectionChanged || value == null)
                    return;
                _collectionChangedEvent.RemoveEventHandler(value, handler);
            }
        }
    }
}
