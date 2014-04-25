using System;
using System.Collections.Generic;
using System.ComponentModel;
using CalculatedProperties;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Unit_Tests
{
    [TestClass]
    public class ChainUnitTests
    {
        public sealed class ViewModel : ViewModelBase
        {
            public int Leaf
            {
                get { return Properties.Get(7); }
                set { Properties.Set(value); }
            }

            public int Branch
            {
                get { return Properties.Get(11); }
                set { Properties.Set(value); }
            }

            public int Intermediate
            {
                get
                {
                    return Properties.Calculated(() =>
                    {
                        ++IntermediateExecutionCount;
                        return Leaf * 2;
                    });
                }
            }

            public int Root
            {
                get
                {
                    return Properties.Calculated(() =>
                    {
                        ++RootExecutionCount;
                        return Intermediate + Branch;
                    });
                }
            }

            public int RootExecutionCount;
            public int IntermediateExecutionCount;
        }
        
        [TestMethod]
        public void Root_InitialValueIsCalculated()
        {
            var vm = new ViewModel();
            Assert.AreEqual(25, vm.Root);
            Assert.AreEqual(1, vm.IntermediateExecutionCount);
            Assert.AreEqual(1, vm.RootExecutionCount);
        }

        [TestMethod]
        public void LeafChanges_RaisesPropertyChangedForAllAffectedProperties()
        {
            var changes = new List<string>();
            var vm = new ViewModel();
            vm.PropertyChanged += (_, args) => changes.Add(args.PropertyName);
            var value = vm.Root;
            vm.Leaf = 13;
            CollectionAssert.AreEquivalent(new[] { "Leaf", "Intermediate", "Root" }, changes);
        }

        [TestMethod]
        public void BranchChanges_RaisesPropertyChangedForAllAffectedProperties()
        {
            var changes = new List<string>();
            var vm = new ViewModel();
            vm.PropertyChanged += (_, args) => changes.Add(args.PropertyName);
            var value = vm.Root;
            vm.Branch = 13;
            CollectionAssert.AreEquivalent(new[] { "Branch", "Root" }, changes);
        }

        [TestMethod]
        public void LeafChanges_NotificationsDeferred_RaisesPropertyChangedForAllAffectedPropertiesAfterNotificationsResumed()
        {
            var changes = new List<string>();
            var vm = new ViewModel();
            vm.PropertyChanged += (_, args) => changes.Add(args.PropertyName);
            var value = vm.Root;
            using (PropertyChangedNotificationManager.Instance.DeferNotifications())
            {
                vm.Leaf = 13;
                CollectionAssert.AreEquivalent(new string[] { }, changes);
            }
            CollectionAssert.AreEquivalent(new[] { "Leaf", "Intermediate", "Root" }, changes);
        }

        [TestMethod]
        public void LeafAndBranchChanges_RaisesPropertyChangedForAllAffectedPropertiesImmediately()
        {
            var changes = new List<string>();
            var vm = new ViewModel();
            vm.PropertyChanged += (_, args) => changes.Add(args.PropertyName);
            var value = vm.Root;
            vm.Leaf = 13;
            vm.Branch = 13;
            CollectionAssert.AreEquivalent(new[] { "Leaf", "Intermediate", "Root", "Branch", "Root" }, changes);
        }

        [TestMethod]
        public void LeafAndBranchChanges_NotificationsDeferred_RaisesPropertyChangedForAllAffectedPropertiesAfterNotificationsResumed_AndCombinesThem()
        {
            var changes = new List<string>();
            var vm = new ViewModel();
            vm.PropertyChanged += (_, args) => changes.Add(args.PropertyName);
            var value = vm.Root;
            using (PropertyChangedNotificationManager.Instance.DeferNotifications())
            {
                vm.Leaf = 13;
                vm.Branch = 13;
                CollectionAssert.AreEquivalent(new string[] { }, changes);
            }
            CollectionAssert.AreEquivalent(new[] { "Leaf", "Intermediate", "Root", "Branch" }, changes);
        }
    }
}
