using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace CalculatedProperties.Internal
{
    /// <summary>
    /// An internal interface for an object responsible for deferring (and consolidating) of <see cref="INotifyPropertyChanged.PropertyChanged"/> events.
    /// </summary>
    public interface IPropertyChangedNotificationManager
    {
        /// <summary>
        /// Registers the specified property with the <c>PropertyChanged</c> notification manager. When the notification manager is done deferring, it will call <see cref="IProperty.InvokeOnPropertyChanged"/>.
        /// </summary>
        /// <param name="property">The property to register.</param>
        void Register(IProperty property);
    }
}
