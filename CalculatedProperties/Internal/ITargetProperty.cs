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
        /// Returns the current set of source properties.
        /// </summary>
        ISet<ISourceProperty> Sources { get; }

        /// <summary>
        /// Updates the source properties for this target property (as detected by the <see cref="DependencyTracker"/>).
        /// </summary>
        /// <param name="sourcesToRemove">The set of source properties to remove for this target property.</param>
        /// <param name="sourcesToAdd">The set of source properties to add for this target property. This parameter may be <c>null</c>.</param>
        void UpdateSources(ISet<ISourceProperty> sourcesToRemove, ISet<ISourceProperty> sourcesToAdd);
    }
}
