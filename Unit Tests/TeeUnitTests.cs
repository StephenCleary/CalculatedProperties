using System;
using System.Collections.Generic;
using System.ComponentModel;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Unit_Tests
{
    [TestClass]
    public class TeeUnitTests
    {
        public sealed class ViewModel : ViewModelBase
        {
            public int Trigger
            {
                get { return Properties.Get(7); }
                set { Properties.Set(value); }
            }

            public int AddTwo
            {
                get
                {
                    return Properties.Calculated(() => Trigger + 2);
                }
            }

            public int MultiplyByTwo
            {
                get
                {
                    return Properties.Calculated(() => Trigger * 2);
                }
            }
        }
        
        [TestMethod]
        public void LeafChanges_RaisesPropertyChangedForAllAffectedProperties()
        {
            var changes = new List<string>();
            var vm = new ViewModel();
            vm.PropertyChanged += (_, args) => changes.Add(args.PropertyName);
            var value = vm.AddTwo;
            value = vm.MultiplyByTwo;
            vm.Trigger = 13;
            CollectionAssert.AreEquivalent(new[] { "Trigger", "AddTwo", "MultiplyByTwo" }, changes);
        }
    }
}
