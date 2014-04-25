using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CalculatedProperties.Internal
{
    /// <summary>
    /// Tracks dependencies (sources) for a given target property.
    /// </summary>
    public sealed class DependencyTracker
    {
        private static readonly DependencyTracker SingletonInstance = new DependencyTracker();
        private readonly Stack<StackFrame> _stack;

        private DependencyTracker()
        {
            _stack = new Stack<StackFrame>();
            _stack.Push(null);
        }

        /// <summary>
        /// Gets the singleton instance.
        /// </summary>
        public static DependencyTracker Instance { get { return SingletonInstance; } }

        /// <summary>
        /// Starts tracking dependencies for the specified target property. Stops tracking dependencies when the returned disposable is disposed. Dependencies are tracked using a stack, so this is safe to call for a target property that has other target properties as its source.
        /// </summary>
        /// <param name="targetProperty">The target property.</param>
        public IDisposable StartDependencyTracking(ITargetProperty targetProperty)
        {
            _stack.Push(new StackFrame { TargetProperty = targetProperty });
            return StopDependencyTrackingWhenDisposed.Instance;
        }

        /// <summary>
        /// Registers the specified property as a source for the target property currently being tracked. If no target property is currently being tracked, then this method does nothing.
        /// </summary>
        /// <param name="sourceProperty">The source property.</param>
        public void Register(ISourceProperty sourceProperty)
        {
            var currentFrame = _stack.Peek();
            if (currentFrame == null)
                return;
            currentFrame.SourceProperties.Add(sourceProperty);
        }

        private void StopDependencyTracking()
        {
            var frame = _stack.Pop();
            frame.TargetProperty.UpdateSources(frame.SourceProperties);
        }

        private sealed class StackFrame
        {
            public StackFrame()
            {
                SourceProperties = new HashSet<ISourceProperty>();
            }

            public ITargetProperty TargetProperty { get; set; }
            public HashSet<ISourceProperty> SourceProperties { get; private set; } 
        }

        private sealed class StopDependencyTrackingWhenDisposed : IDisposable
        {
            private static readonly StopDependencyTrackingWhenDisposed SingletonInstance = new StopDependencyTrackingWhenDisposed();

            private StopDependencyTrackingWhenDisposed()
            {
            }

            public static StopDependencyTrackingWhenDisposed Instance { get { return SingletonInstance; } }

            public void Dispose()
            {
                DependencyTracker.Instance.StopDependencyTracking();
            }
        }
    }
}
