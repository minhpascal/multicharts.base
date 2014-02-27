namespace PowerLanguage.Strategy
{
    [IOGMode(IOGMode.Disabled)]
    public class Key_Reversal_LE : SignalObject
    {
        private IOrderMarket m_KeyRevLE;

        public Key_Reversal_LE(object ctx) :
            base(ctx)
        {
            Length = 1;
        }

        [Input]
        public int Length { get; set; }

        protected override void Create(){
            m_KeyRevLE =
                OrderCreator.MarketNextBar(new SOrderParameters(Contracts.Default, "KeyRevLE", EOrderAction.Buy));
        }


        protected override void CalcBar(){
            if (PublicFunctions.DoubleLess(Bars.Low[0], Bars.Low.Lowest(Length, 1)) //C<LL, buy low
                && PublicFunctions.DoubleGreater(Bars.Close[0], Bars.Close[1]))  //C>C-1
              m_KeyRevLE.Send();
        }
    }
}