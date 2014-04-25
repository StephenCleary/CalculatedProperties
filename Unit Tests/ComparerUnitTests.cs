using System;
using System.Collections.Generic;
using System.ComponentModel;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Unit_Tests
{
    [TestClass]
    public class ComparerUnitTests
    {
        public sealed class EvenOddComparer : IEqualityComparer<int>
        {
            public bool Equals(int x, int y)
            {
                return (x % 2).Equals(y % 2);
            }

            public int GetHashCode(int obj)
            {
                return (obj % 2).GetHashCode();
            }

            public static readonly EvenOddComparer Instance = new EvenOddComparer();
        }

        public sealed class ViewModel : ViewModelBase
        {
            public int A
            {
                get { return Properties.Get(7, EvenOddComparer.Instance); }
                set { Properties.Set(value, EvenOddComparer.Instance); }
            }

            public int Calculated
            {
                get { return Properties.Calculated(() => A * 2); }
            }
        }

        [TestMethod]
        public void ChangesToEquivalentValue_SavesNewValue()
        {
            var vm = new ViewModel();
            var value = vm.A;
            vm.A = 13;
            Assert.AreEqual(13, vm.A);
        }
        
        [TestMethod]
        public void ChangesToEquivalentValue_DoesNotRaisePropertyChanged()
        {
            var changes = new List<string>();
            var vm = new ViewModel();
            vm.PropertyChanged += (_, args) => changes.Add(args.PropertyName);
            var value = vm.A;
            vm.A = 13;
            CollectionAssert.AreEquivalent(new string[] { }, changes);
        }

        [TestMethod]
        public void ChangesToEquivalentValue_DoesNotNotifyCalculatedValue()
        {
            var changes = new List<string>();
            var vm = new ViewModel();
            vm.PropertyChanged += (_, args) => changes.Add(args.PropertyName);
            var original = vm.Calculated;
            vm.A = 13;
            CollectionAssert.AreEquivalent(new string[] { }, changes);
            Assert.AreEqual(original, vm.Calculated);
        }
    }
}
