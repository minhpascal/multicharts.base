using System;
using System.Drawing;
using System.Linq;
using PowerLanguage.Function;
using ATCenterProxy.interop;

namespace PowerLanguage.Strategy
{
    public class henry_EA : SignalObject
    {

        private AverageFC m_MA;
        private AverageFC m_H20;
        private AverageFC m_L20;
        private AverageFC m_H10;
        private AverageFC m_L10;
        private AverageFC m_ATR;
        // private VariableSeries<Double> m_MA0;
        // private VariableObject<Int32> m_Counter;

        private IOrderMarket m_BK;
        private IOrderMarket m_SP;
        private IOrderMarket m_SK;
        private IOrderMarket m_BP;

        public henry_EA(object ctx) :
            base(ctx)
        {
            LongN = 20;  //INPUT
            ShortN = 10;
            AtrN = 20;
        }

        private ISeries<double> Price { get; set; }

        [Input]
        public int LongN { get; set; }

        [Input]
        public int ShortN { get; set; }

        [Input]
        public int AtrN { get; set; }

        protected override void Create()
        {

            //create TA objects
            m_MA = new AverageFC(this);

            //orders
            m_BK =
                OrderCreator.MarketNextBar(new SOrderParameters(Contracts.Default, "BK", EOrderAction.Buy));
            m_SP =
                OrderCreator.MarketNextBar(new SOrderParameters(Contracts.Default, "SP", EOrderAction.Sell,
                                                                      OrderExit.FromAll));
            m_SK =
                OrderCreator.MarketNextBar(new SOrderParameters(Contracts.Default, "SK",
                                                                      EOrderAction.SellShort));
            m_BP =
                OrderCreator.MarketNextBar(new SOrderParameters(Contracts.Default, "BP",
                                                                      EOrderAction.BuyToCover, OrderExit.FromAll));
        }

        protected override void StartCalc()
        {

            //initialize TA
            Price = Bars.Close;   //C
            m_MA.price = Price;   //MA(C,N)
            m_MA.length = LongN;
        }


        protected override void CalcBar()
        {

            if (StrategyInfo.MarketPosition == 0)
            {
                if (PublicFunctions.DoubleGreater(Bars.High[0], Bars.High.Highest(LongN, 1))  //H>H20
                )
                    m_BK.Send();

                if (PublicFunctions.DoubleLess(Bars.Low[0], Bars.Low.Lowest(LongN, 1)) //L<L20
                )
                    m_SK.Send();
            }

            if (StrategyInfo.MarketPosition > 0)
            {
                //SP
                if (PublicFunctions.DoubleLess(Bars.Low[0], Bars.Low.Lowest(ShortN, 1)) //L<L10, STP
                    || PublicFunctions.DoubleLess(Bars.Low[0], this.EntryPrice() - this.AverageTrueRange(AtrN) * 2)) //L<BKP-2*ATR
                    m_SP.Send();

                //BK2
                if (PublicFunctions.DoubleGreater(Bars.High[0], this.EntryPrice() - this.AverageTrueRange(AtrN) * 0.5)  //H>BKP+0.5*ATR
                )
                    m_BK.Send();
            }


            if (StrategyInfo.MarketPosition < 0)
            {
                //BP
                if (PublicFunctions.DoubleGreater(Bars.High[0], Bars.High.Highest(ShortN, 1)) // H>H10, BTP
                    || PublicFunctions.DoubleGreater(Bars.High[0], this.EntryPrice() + this.AverageTrueRange(AtrN) * 2)) //H>SKP+2*ATR
                    m_BP.Send();
                //SK2
                if (PublicFunctions.DoubleLess(Bars.Low[0], this.EntryPrice() - this.AverageTrueRange(AtrN) * 0.5) //L<SKP-0.5*ATR
                )
                    m_SK.Send();


            }
        }
    }
}
