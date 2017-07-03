using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Unit_Tests
{
    [TestClass]
    public class PerformanceUnitTests
    {
        [TestMethod]
        public void TriggerPropertyWithThousandsOfTargetsHasAcceptableRewiringTime()
        {
            SourceViewModel source = new SourceViewModel();
            
            CreateAndRewireManyTargets(source);

            // GC can collect all targets which went out of scope

            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();

            Assert.IsTrue(TargetViewModel.InstanceCount == 0);
        }

        private static void CreateAndRewireManyTargets(SourceViewModel source)
        {
            const int size = 50000;

            // populate source.BaseValue collection of targets is populated
            Stopwatch sw = new Stopwatch();
            sw.Start();

            TargetViewModel[] manyTargets = new TargetViewModel[2 * size];
            for (int i = 0; i < size; i++)
            {
                TargetViewModel target = new TargetViewModel {Source = source};
                var tmp = target.DerivedValue;
                manyTargets[i] = target;
            }
            sw.Stop();
            Console.WriteLine("Initial wiring time: " + sw.Elapsed);

            Assert.IsTrue(TargetViewModel.InstanceCount == size);

            // emulate massive rewiring: adding new targets and resetting old
            sw.Restart();

            for (int i = 0; i < size; i++)
            {
                // unplug a target
                manyTargets[i].Source = null;
                var tmp = manyTargets[i].DerivedValue;

                // plug a new target and keep it alive in array
                TargetViewModel target = new TargetViewModel {Source = source};
                manyTargets[i + size] = target;
                tmp = target.DerivedValue;
            }
            sw.Stop();
            Console.WriteLine("Rewiring time: " + sw.Elapsed);

            // rewiring should take under a second on fast dev machine but let's say two seconds
            Assert.IsTrue(sw.Elapsed.TotalSeconds < 2);
        }

        class SourceViewModel : ViewModelBase
        {
            public int BaseValue
            {
                get { return Properties.Get(0); }
                set { Properties.Set(value); }
            }
        }

        class TargetViewModel : ViewModelBase
        {
            public static int InstanceCount = 0;

            ~TargetViewModel()
            {
                Interlocked.Decrement(ref InstanceCount);
            }

            public TargetViewModel()
            {
                Interlocked.Increment(ref InstanceCount);
            }

            public SourceViewModel Source
            {
                get { return Properties.Get((SourceViewModel)null); }
                set { Properties.Set(value); }
            }

            public int DerivedValue => Properties.Calculated(() => Source?.BaseValue ?? 0 + 5);
        }
    }
}
