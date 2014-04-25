using System;
using System.Collections.Generic;
using System.ComponentModel;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Unit_Tests
{
    [TestClass]
    public class SimpleUnitTests
    {
        public sealed class ViewModel : ViewModelBase
        {
            public int TriggerValue
            {
                get { return Properties.Get(7); }
                set { Properties.Set(value); }
            }

            public int CalculatedValue
            {
                get
                {
                    return Properties.Calculated(() =>
                    {
                        ++CalculatedValueExecutionCount;
                        return TriggerValue * 2;
                    });
                }
            }

            public int CalculatedValueExecutionCount;
        }

        [TestMethod]
        public void Trigger_UsesSpecifiedDefaultValue()
        {
            var vm = new ViewModel();
            Assert.AreEqual(7, vm.TriggerValue);
        }

        [TestMethod]
        public void Calculated_BeforeRead_DoesNotExecute()
        {
            var vm = new ViewModel();
            Assert.AreEqual(0, vm.CalculatedValueExecutionCount);
        }

        [TestMethod]
        public void Calculated_InitialEvaluation_CalculatesValue()
        {
            var vm = new ViewModel();
            Assert.AreEqual(14, vm.CalculatedValue);
            Assert.AreEqual(1, vm.CalculatedValueExecutionCount);
        }

        [TestMethod]
        public void Calculated_MultipleReads_CachesValue()
        {
            var vm = new ViewModel();
            var value = vm.CalculatedValue;
            value = vm.CalculatedValue;
            Assert.AreEqual(1, vm.CalculatedValueExecutionCount);
        }

        [TestMethod]
        public void TriggerChanged_RaisesPropertyChanged()
        {
            var changes = new List<string>();
            var vm = new ViewModel();
            var value = vm.TriggerValue;
            vm.PropertyChanged += (_, args) => changes.Add(args.PropertyName);
            vm.TriggerValue = 13;
            CollectionAssert.AreEquivalent(new[] { "TriggerValue" }, changes);
        }

        [TestMethod]
        public void TriggerChanged_UpdatesValue()
        {
            var vm = new ViewModel();
            vm.TriggerValue = 13;
            Assert.AreEqual(13, vm.TriggerValue);
        }

        [TestMethod]
        public void TriggerChanged_AfterCalculatedIsRead_RaisesPropertyChangedForCalculated()
        {
            var changes = new List<string>();
            var vm = new ViewModel();
            vm.PropertyChanged += (_, args) => changes.Add(args.PropertyName);
            var originalValue = vm.CalculatedValue;
            vm.TriggerValue = 13;
            CollectionAssert.AreEquivalent(new[] { "TriggerValue", "CalculatedValue" }, changes);
        }

        [TestMethod]
        public void TriggerChanged_AfterCalculatedIsRead_RecalculatesCalculatedValue()
        {
            var vm = new ViewModel();
            var originalValue = vm.CalculatedValue;
            vm.TriggerValue = 13;
            var updatedValue = vm.CalculatedValue;
            Assert.AreEqual(14, originalValue);
            Assert.AreEqual(26, updatedValue);
            Assert.AreEqual(2, vm.CalculatedValueExecutionCount);
        }
    }
}
