using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CalculatedProperties.Internal
{
    /// <summary>
    /// A property that can act as a data source for other properties.
    /// </summary>
    public interface ISourceProperty : IProperty
    {
        /// <summary>
        /// Adds the target property to this property's set of targets.
        /// </summary>
        /// <param name="targetProperty">The target property to add.</param>
        void AddTarget(ITargetProperty targetProperty);

        /// <summary>
        /// Removes the target property from this property's set of targets.
        /// </summary>
        /// <param name="targetProperty">The target property to remove.</param>
        void RemoveTarget(ITargetProperty targetProperty);
    }
}
