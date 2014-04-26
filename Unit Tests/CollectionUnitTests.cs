using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Unit_Tests
{
    [TestClass]
    public class CollectionUnitTests
    {
        public sealed class ViewModel : ViewModelBase
        {
            public ObservableCollection<int> Leaf
            {
                get { return Properties.Get(new ObservableCollection<int>()); }
                set { Properties.Set(value); }
            }

            public int FirstOr13
            {
                get
                {
                    return Properties.Calculated(() =>
                    {
                        if (Leaf.Count == 0)
                            return 13;
                        return Leaf.First();
                    });
                }
            }

            public ObservableCollection<int> Intermediate
            {
                get { return Properties.Calculated(() => new ObservableCollection<int>(Leaf)); }
            }

            public int Calculated
            {
                get { return Properties.Calculated(() => Intermediate.Count); }
            }
        }
        
        [TestMethod]
        public void TriggerCollection_InitialValueIsCalculated()
        {
            var vm = new ViewModel();
            Assert.AreEqual(13, vm.FirstOr13);
        }

        [TestMethod]
        public void TriggerCollectionUpdated_ValueIsRecalculated()
        {
            var vm = new ViewModel();
            var value = vm.FirstOr13;
            vm.Leaf.Add(7);
            Assert.AreEqual(7, vm.FirstOr13);
        }

        [TestMethod]
        public void TriggerCollectionReplaced_NewCollectionIsSubscribedTo()
        {
            var vm = new ViewModel();
            vm.Leaf.Add(7);
            Assert.AreEqual(7, vm.FirstOr13);
            var newValue = new ObservableCollection<int>();
            vm.Leaf = newValue;
            Assert.AreEqual(13, vm.FirstOr13);
            newValue.Add(11);
            Assert.AreEqual(11, vm.FirstOr13);
        }

        [TestMethod]
        public void TriggerCollectionReplaced_OldCollectionIsNotSubscribedTo()
        {
            var vm = new ViewModel();
            vm.Leaf.Add(7);
            Assert.AreEqual(7, vm.FirstOr13);
            var oldValue = vm.Leaf;
            vm.Leaf = new ObservableCollection<int>();
            Assert.AreEqual(13, vm.FirstOr13);

            var changes = new List<string>();
            vm.PropertyChanged += (_, args) => changes.Add(args.PropertyName);

            oldValue.Add(11);
            Assert.AreEqual(13, vm.FirstOr13);
            CollectionAssert.AreEquivalent(new string[] { }, changes);
        }

        [TestMethod]
        public void CalculatedCollectionUpdated_DependenciesAreRecalculated()
        {
            var vm = new ViewModel();
            Assert.AreEqual(0, vm.Calculated);
            vm.Intermediate.Add(13);
            Assert.AreEqual(1, vm.Calculated);
        }

        [TestMethod]
        public void TriggerCollectionUpdated_CalculatedCollectionIsRecalculated()
        {
            var vm = new ViewModel();
            Assert.AreEqual(0, vm.Calculated);
            vm.Intermediate.Add(13);
            vm.Leaf.Add(11);
            Assert.AreEqual(1, vm.Calculated);
        }

        [TestMethod]
        public void TriggerCollectionUpdated_NewCollectionIsSubscribedTo()
        {
            var vm = new ViewModel();
            Assert.AreEqual(0, vm.Calculated);
            var newValue = new ObservableCollection<int>();
            vm.Leaf = newValue;
            Assert.AreEqual(0, vm.Calculated);
            newValue.Add(11);
            Assert.AreEqual(1, vm.Calculated);
        }

        [TestMethod]
        public void TriggerCollectionUpdated_OldCollectionIsNotSubscribedTo()
        {
            var vm = new ViewModel();
            vm.Leaf.Add(11);
            Assert.AreEqual(1, vm.Calculated);
            var oldValue = vm.Leaf;
            vm.Leaf = new ObservableCollection<int>();
            Assert.AreEqual(0, vm.Calculated);

            var changes = new List<string>();
            vm.PropertyChanged += (_, args) => changes.Add(args.PropertyName);

            oldValue.Add(11);
            Assert.AreEqual(0, vm.Calculated);
            CollectionAssert.AreEquivalent(new string[] { }, changes);
        }
    }
}
