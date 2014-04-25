namespace Unit_Tests
{
    public sealed class SimpleViewModel : ViewModelBase
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
}