using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CalculatedProperties.Internal
{
    /// <summary>
    /// A property that acts as a data target for other property sources.
    /// </summary>
    public interface ITargetProperty : IProperty
    {
        /// <summary>
        /// Updates the source properties for this target property (as detected by the <see cref="DependencyTracker"/>).
        /// </summary>
        /// <param name="sources">The new collection of source properties.</param>
        void UpdateSources(ISet<ISourceProperty> sources);
    }
}
