using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Unit_Tests
{
    [TestClass]
    public class MemoryLeakUnitTests
    {
        [TestMethod]
        public void LongLivingPublisherAllowsToGarbageCollectShortLivingSubscribers()
        {
            FullGarbageCollection();

            var longLivingVm = new LongLivingVm();

            var notifications = new List<string>();
            Action transientScopeAction = () =>
            {
                var transientObject = new ShortLivingViewModel(longLivingVm);
                transientObject.PropertyChanged += (sender, args) => notifications.Add(args.PropertyName);
                // emulate read from UI
                var tmp = transientObject.FullName;
                Assert.IsTrue(notifications.Count == 0);

                longLivingVm.FirstName = "Mister";
                Assert.IsTrue(notifications.Count == 1 && notifications[0] == nameof(ShortLivingViewModel.FullName));

                // we have one instance of short living object
                Assert.IsTrue(ShortLivingViewModel.InstanceCount == 1);
                // scope finished: eligible for GC
            };
            transientScopeAction();

            FullGarbageCollection();

            notifications.Clear();
            longLivingVm.FirstName = "Twister";

            // transient listener has been GCed and didn't receive any notifications
            Assert.IsTrue(notifications.Count == 0);
            Assert.IsTrue(ShortLivingViewModel.InstanceCount == 0);
        }

        private static void FullGarbageCollection()
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
        }
    }

    class ShortLivingViewModel : ViewModelBase
    {
        public static int InstanceCount = 0;

        ~ShortLivingViewModel()
        {
            InstanceCount--;
        }

        public ShortLivingViewModel(LongLivingVm longLivingVm)
        {
            InstanceCount++;
            _longLivingVm = longLivingVm;
        }

        private readonly LongLivingVm _longLivingVm;

        public string FullName => Properties.Calculated(() => $"{_longLivingVm.FirstName} {_longLivingVm.LastName}");
    }

    class LongLivingVm : ViewModelBase
    {
        public string FirstName
        {
            get { return Properties.Get((string)null); }
            set { Properties.Set(value); }
        }

        public string LastName
        {
            get { return Properties.Get((string)null); }
            set { Properties.Set(value); }
        }
    }
}
