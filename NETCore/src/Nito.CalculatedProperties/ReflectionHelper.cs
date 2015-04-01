using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Nito.CalculatedProperties;

/// <summary>
/// Provides methods (with caching) to assist with reflection.
/// </summary>
internal static class ReflectionHelper
{
    private static Type _iNotifyCollectionChangedType;
    private static Type _notifyCollectionChangedEventHandlerType;
    private static Type _notifyCollectionChangedEventArgsType;
    private static EventInfo _collectionChangedEvent;

    private static Type _iBindingListType;
    private static Type _listChangedEventHandlerType;
    private static Type _listChangedEventArgsType;
    private static EventInfo _listChangedEvent;

    /// <summary>
    /// Provides methods (with caching) to assist with reflection over a specific type.
    /// </summary>
    /// <typeparam name="T">The type being reflected over.</typeparam>
    public static class For<T>
    {
        // ReSharper disable StaticFieldInGenericType
        private static readonly bool ImplementsINotifyCollectionChanged;
        private static readonly bool ImplementsIBindingList;
        // ReSharper restore StaticFieldInGenericType

        static For()
        {
            var interfaces = typeof(T).GetTypeInfo().ImplementedInterfaces;
            ImplementsINotifyCollectionChanged = DetectINotifyCollectionChanged(interfaces);
            if (!ImplementsINotifyCollectionChanged)
                ImplementsIBindingList = DetectIBindingList(interfaces);
        }

        private static bool DetectINotifyCollectionChanged(IEnumerable<Type> interfaces)
        {
            if (_collectionChangedEvent != null)
                return interfaces.Contains(_iNotifyCollectionChangedType);
            _iNotifyCollectionChangedType = interfaces.FirstOrDefault(x => x.FullName == "System.Collections.Specialized.INotifyCollectionChanged");
            if (_iNotifyCollectionChangedType == null)
                return false;
            var assembly = _iNotifyCollectionChangedType.GetTypeInfo().Assembly;
            _notifyCollectionChangedEventHandlerType = assembly.GetType("System.Collections.Specialized.NotifyCollectionChangedEventHandler");
            _notifyCollectionChangedEventArgsType = assembly.GetType("System.Collections.Specialized.NotifyCollectionChangedEventArgs");
            _collectionChangedEvent = _iNotifyCollectionChangedType.GetTypeInfo().GetDeclaredEvent("CollectionChanged");
            return true;
        }

        private static bool DetectIBindingList(IEnumerable<Type> interfaces)
        {
            if (_collectionChangedEvent != null)
                return interfaces.Contains(_iBindingListType);
            _iBindingListType = interfaces.FirstOrDefault(x => x.FullName == "System.ComponentModel.IBindingList");
            if (_iBindingListType == null)
                return false;
            var assembly = _iBindingListType.GetTypeInfo().Assembly;
            _listChangedEventHandlerType = assembly.GetType("System.ComponentModel.ListChangedEventHandler");
            _listChangedEventArgsType = assembly.GetType("System.ComponentModel.ListChangedEventArgs");
            _listChangedEvent = _iBindingListType.GetTypeInfo().GetDeclaredEvent("ListChanged");
            return true;
        }

        /// <summary>
        /// Adds a <c>INotifyCollectionChanged.CollectionChanged</c> event handler that calls <see cref="IProperty.InvalidateTargets"/> on the specified property. Returns the subscribed delegate, or <c>null</c> if <typeparamref name="T"/> does not implement <c>INotifyCollectionChanged</c> or if <paramref name="value"/> is <c>null</c>.
        /// </summary>
        /// <param name="property">The property whose targets should be invalidated. May not be <c>null</c>.</param>
        /// <param name="value">The value to observe. May be <c>null</c>.</param>
        public static Delegate AddEventHandler(IProperty property, T value)
        {
            // ReSharper disable once CompareNonConstrainedGenericWithNull
            if ((!ImplementsINotifyCollectionChanged && !ImplementsIBindingList) || value == null)
                return null;

            Delegate result;
            if (ImplementsINotifyCollectionChanged)
            {
                // (object sender, NotifyCollectionChangedEventArgs e) => property.InvalidateTargets();
                var sender = Expression.Parameter(typeof(object), "sender");
                var args = Expression.Parameter(_notifyCollectionChangedEventArgsType, "e");
                var lambda = Expression.Lambda(_notifyCollectionChangedEventHandlerType,
                    Expression.Call(Expression.Constant(property), "InvalidateTargets", null),
                    sender, args);
                result = lambda.Compile();

                _collectionChangedEvent.AddEventHandler(value, result);
            }
            else
            {
                // (object sender, ListChangedEventArgsType e) => property.InvalidateTargets();
                var sender = Expression.Parameter(typeof(object), "sender");
                var args = Expression.Parameter(_listChangedEventArgsType, "e");
                var lambda = Expression.Lambda(_listChangedEventHandlerType,
                    Expression.Call(Expression.Constant(property), "InvalidateTargets", null),
                    sender, args);
                result = lambda.Compile();

                _listChangedEvent.AddEventHandler(value, result);
            }

            return result;
        }

        /// <summary>
        /// Removes a <c>INotifyCollectionChanged.CollectionChanged</c> event handler from the specified value. Does nothing if <typeparamref name="T"/> does not implement <c>INotifyCollectionChanged</c> or if <paramref name="value"/> is <c>null</c>.
        /// </summary>
        /// <param name="value">The value being observed. May be <c>null</c>.</param>
        /// <param name="handler">The delegate to be unsubscribed.</param>
        public static void RemoveEventHandler(T value, Delegate handler)
        {
            // ReSharper disable once CompareNonConstrainedGenericWithNull
            if ((!ImplementsINotifyCollectionChanged && !ImplementsIBindingList) || value == null)
                return;
            if (ImplementsINotifyCollectionChanged)
                _collectionChangedEvent.RemoveEventHandler(value, handler);
            else
                _listChangedEvent.RemoveEventHandler(value, handler);
        }
    }
}