using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Unit_Tests
{
    [TestClass]
    public class BindingListUnitTests
    {
        public sealed class ViewModel : ViewModelBase
        {
            public BindingList<int> Leaf
            {
                get { return Properties.Get(new BindingList<int>()); }
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

            public BindingList<int> Intermediate
            {
                get
                {
                    return Properties.Calculated(() =>
                    {
                        var ret = new BindingList<int>();
                        foreach (var element in Leaf)
                            ret.Add(element);
                        return ret;
                    });
                }
            }

            public int Calculated
            {
                get { return Properties.Calculated(() => Intermediate.Count); }
            }

            public BindingList<ChildViewModel> Children
            {
                get { return Properties.Get(new BindingList<ChildViewModel>()); }
                set { Properties.Set(value); }
            }

            public string FirstChildName
            {
                get
                {
                    return Properties.Calculated(() =>
                    {
                        if (Children.Count == 0)
                            return null;
                        return Children[0].Name;
                    });
                }
            }

            public sealed class ChildViewModel : INotifyPropertyChanged
            {
                private string _name;

                public string Name
                {
                    get { return _name; }
                    set { _name = value; OnPropertyChanged(); }
                }

                public event PropertyChangedEventHandler PropertyChanged;

                private void OnPropertyChanged([CallerMemberName] string propertyName = null)
                {
                    PropertyChangedEventHandler handler = PropertyChanged;
                    if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
                }
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
            var newValue = new BindingList<int>();
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
            vm.Leaf = new BindingList<int>();
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
            var newValue = new BindingList<int>();
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
            vm.Leaf = new BindingList<int>();
            Assert.AreEqual(0, vm.Calculated);

            var changes = new List<string>();
            vm.PropertyChanged += (_, args) => changes.Add(args.PropertyName);

            oldValue.Add(11);
            Assert.AreEqual(0, vm.Calculated);
            CollectionAssert.AreEquivalent(new string[] { }, changes);
        }

        [TestMethod]
        public void ItemPropertyUpdated_DoesRecalculate()
        {
            var vm = new ViewModel();
            vm.Children.Add(new ViewModel.ChildViewModel { Name = "Steve" });
            Assert.AreEqual("Steve", vm.FirstChildName);
            var changes = new List<string>();
            vm.PropertyChanged += (_, args) => changes.Add(args.PropertyName);

            vm.Children[0].Name = "Bob";

            Assert.AreEqual("Bob", vm.FirstChildName);
            CollectionAssert.AreEquivalent(new[] { "FirstChildName" }, changes);
        }
    }
}
