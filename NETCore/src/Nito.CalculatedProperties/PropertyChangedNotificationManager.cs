using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;

namespace Nito.CalculatedProperties
{
    /// <summary>
    /// Manages deferring (and consolidating) of <see cref="INotifyPropertyChanged.PropertyChanged"/> events.
    /// </summary>
    [DebuggerTypeProxy(typeof(DebugView))]
    [DebuggerDisplay("{DebuggerDisplay,nq}")]
    public sealed class PropertyChangedNotificationManager : IPropertyChangedNotificationManager
    {
        private static readonly PropertyChangedNotificationManager SingletonInstance = new PropertyChangedNotificationManager();
        private readonly HashSet<IProperty> _propertiesRequiringNotification = new HashSet<IProperty>(); 
        private int _referenceCount;

        private PropertyChangedNotificationManager()
        {
        }

        /// <summary>
        /// Gets the singleton instance.
        /// </summary>
        public static PropertyChangedNotificationManager Instance { get { return SingletonInstance; } }

        /// <summary>
        /// Defers <see cref="INotifyPropertyChanged.PropertyChanged"/> events until the returned disposable is disposed. Deferrals are reference counted, so they are safe to nest. Do not dispose the returned object more than once.
        /// </summary>
        public IDisposable DeferNotifications()
        {
            ++_referenceCount;
            return ResumeOnDispose.Instance;
        }

        private void ResumeNotifications()
        {
            --_referenceCount;
            if (_referenceCount != 0)
                return;
            var properties = _propertiesRequiringNotification.ToArray();
            _propertiesRequiringNotification.Clear();
            foreach (var property in properties)
                property.InvokeOnPropertyChanged();
        }

        void IPropertyChangedNotificationManager.Register(IProperty property)
        {
            _propertiesRequiringNotification.Add(property);
        }

        private sealed class ResumeOnDispose : IDisposable
        {
            public static ResumeOnDispose Instance = new ResumeOnDispose();

            public void Dispose()
            {
                PropertyChangedNotificationManager.Instance.ResumeNotifications();
            }
        }

        [DebuggerNonUserCode]
        private string DebuggerDisplay
        {
            get
            {
                if (_referenceCount == 0)
                    return "Not deferred";
                return "Deferred; notification count: " + _propertiesRequiringNotification.Count + ", defer refcount: " + _referenceCount;
            }
        }

        [DebuggerNonUserCode]
        private sealed class DebugView
        {
            private readonly PropertyChangedNotificationManager _value;

            public DebugView(PropertyChangedNotificationManager value)
            {
                _value = value;
            }

            public int ReferenceCount { get { return _value._referenceCount; } }

            public HashSet<IProperty> DeferredNotifications { get { return _value._propertiesRequiringNotification; } }
        }
    }
}
