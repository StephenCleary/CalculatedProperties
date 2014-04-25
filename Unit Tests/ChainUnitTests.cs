using System;
using System.Collections.Generic;
using System.ComponentModel;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Unit_Tests
{
    [TestClass]
    public class ChainUnitTests
    {
        public sealed class ChainViewModel : ViewModelBase
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
            var vm = new ChainViewModel();
            Assert.AreEqual(25, vm.Root);
            Assert.AreEqual(1, vm.IntermediateExecutionCount);
            Assert.AreEqual(1, vm.RootExecutionCount);
        }

        [TestMethod]
        public void LeafChanges_RaisesPropertyChangedForAllAffectedProperties()
        {
            var changes = new List<string>();
            var vm = new ChainViewModel();
            vm.PropertyChanged += (_, args) => changes.Add(args.PropertyName);
            var value = vm.Root;
            vm.Leaf = 13;
            CollectionAssert.AreEquivalent(new[] { "Leaf", "Intermediate", "Root" }, changes);
        }
    }
}
