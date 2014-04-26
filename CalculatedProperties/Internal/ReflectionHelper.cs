using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace CalculatedProperties.Internal
{
    /// <summary>
    /// Provides methods (with caching) to assist with reflection.
    /// </summary>
    public static class ReflectionHelper
    {
        private static Type _iNotifyCollectionChangedType;
        private static Type _notifyCollectionChangedEventArgsType;
        private static Type _notifyCollectionChangedEventHandlerType;
        private static EventInfo _collectionChangedEvent;

        /// <summary>
        /// Provides methods (with caching) to assist with reflection over a specific type.
        /// </summary>
        /// <typeparam name="T">The type being reflected over.</typeparam>
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

            /// <summary>
            /// Adds a <c>INotifyCollectionChanged.CollectionChanged</c> event handler that calls <see cref="IProperty.InvalidateTargets"/> on the specified property. Returns the subscribed delegate, or <c>null</c> if <typeparamref name="T"/> does not implement <c>INotifyCollectionChanged</c> or if <paramref name="value"/> is <c>null</c>.
            /// </summary>
            /// <param name="property">The property whose targets should be invalidated. May not be <c>null</c>.</param>
            /// <param name="value">The value to observe. May be <c>null</c>.</param>
            public static Delegate AddEventHandler(IProperty property, T value)
            {
                // ReSharper disable once CompareNonConstrainedGenericWithNull
                if (!ImplementsINotifyCollectionChanged || value == null)
                    return null;

                var sender = Expression.Parameter(typeof(object), "sender");
                var args = Expression.Parameter(_notifyCollectionChangedEventArgsType, "e");
                var lambda = Expression.Lambda(_notifyCollectionChangedEventHandlerType,
                    Expression.Call(Expression.Constant(property), "InvalidateTargets", null),
                    sender, args);
                var handler = lambda.Compile();
                _collectionChangedEvent.AddEventHandler(value, handler);
                return handler;
            }

            /// <summary>
            /// Removes a <c>INotifyCollectionChanged.CollectionChanged</c> event handler from the specified value. Does nothing if <typeparamref name="T"/> does not implement <c>INotifyCollectionChanged</c> or if <paramref name="value"/> is <c>null</c>.
            /// </summary>
            /// <param name="value">The value being observed. May be <c>null</c>.</param>
            /// <param name="handler">The delegate to be unsubscribed.</param>
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
