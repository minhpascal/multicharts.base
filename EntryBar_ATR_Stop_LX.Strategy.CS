using PowerLanguage.Function;

namespace PowerLanguage.Strategy
{
    [IOGMode(IOGMode.Disabled)]
    public class EntryBar_ATR_Stop_LX : SignalObject
    {
        private IOrderPriced m_EbAtrSX;

        public EntryBar_ATR_Stop_LX(object ctx) :
            base(ctx){
            NumAtrs = 1;
            AtrLength = 5;
        }

        [Input]
        public int AtrLength { get; set; }

        [Input]
        public int NumAtrs { get; set; }

        protected override void Create(){
            m_EbAtrSX =
                OrderCreator.Stop(new SOrderParameters(Contracts.Default, "EbAtrSX", EOrderAction.BuyToCover,
                                                             OrderExit.FromAll));
        }

        protected override void CalcBar(){
            if (StrategyInfo.MarketPosition >= 0)
            {
                m_EbAtrSX.Send(Bars.Close[0] + this.AverageTrueRange(AtrLength)*NumAtrs);
            }
        }
    }
}