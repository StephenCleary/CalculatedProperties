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
        }
        
        [TestMethod]
        public void InitialValueIsCalculated()
        {
            var vm = new ViewModel();
            Assert.AreEqual(13, vm.FirstOr13);
        }

        [TestMethod]
        public void CalculatedValueReadAndLeafUpdated_ValueIsRecalculated()
        {
            var vm = new ViewModel();
            var value = vm.FirstOr13;
            vm.Leaf.Add(7);
            Assert.AreEqual(7, vm.FirstOr13);
        }
    }
}
