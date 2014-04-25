using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Unit_Tests
{
    public sealed class BranchViewModel : ViewModelBase
    {
        public bool UseB
        {
            get { return Properties.Get(false); }
            set { Properties.Set(value); }
        }

        public int A
        {
            get { return Properties.Get(7); }
            set { Properties.Set(value); }
        }

        public int B
        {
            get { return Properties.Get(11); }
            set { Properties.Set(value); }
        }

        public int CalculatedValue
        {
            get
            {
                return Properties.Calculated(() =>
                {
                    ++CalculatedValueExecutionCount;
                    return UseB ? B : A;
                });
            }
        }

        public int CalculatedValueExecutionCount;
    }
}
