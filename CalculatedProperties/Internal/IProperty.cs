using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace CalculatedProperties.Internal
{
    /// <summary>
    /// A property instance.
    /// </summary>
    public interface IProperty
    {
        /// <summary>
        /// Raises <see cref="INotifyPropertyChanged.PropertyChanged"/> for this property.
        /// </summary>
        void InvokeOnPropertyChanged();

        /// <summary>
        /// Queues <see cref="INotifyPropertyChanged.PropertyChanged"/> and invalidates this property and the transitive closure of all its target properties. If notifications are not deferred, then this method will raise <see cref="INotifyPropertyChanged.PropertyChanged"/> for all affected properties before returning.
        /// </summary>
        void Invalidate();

        /// <summary>
        /// Invalidates this property and the transitive closure of all its target properties. If notifications are not deferred, then this method will raise <see cref="INotifyPropertyChanged.PropertyChanged"/> for all affected properties before returning. <see cref="INotifyPropertyChanged.PropertyChanged"/> is not raised for this property.
        /// </summary>
        void InvalidateTargets();
    }
}
