using System;
using System.Drawing;
using System.Linq;
using PowerLanguage.Function;
using ATCenterProxy.interop;

namespace PowerLanguage.Strategy {
	public class henry_EA : SignalObject {
		
	  	private AverageFC m_MA;
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
            ShortN = 1;
        }

        private ISeries<double> Price { get; set; }

        [Input]
        public int LongN { get; set; }

        [Input]
        public int ShortN { get; set; }

        protected override void Create(){

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

        protected override void StartCalc(){
            
            //initialize TA
            Price = Bars.Close;   //C
            m_MA.price = Price;   //MA(C,N)
            m_MA.length = LongN;
        }


        protected override void CalcBar(){

            if (PublicFunctions.DoubleLess(Bars.Low[0], Bars.Low.Lowest(ShortN, 1)) //C<LL, buy low
                && PublicFunctions.DoubleGreater(Bars.Close[0], Bars.Close[1]))  //C>C-1
                m_BK.Send();
            if (PublicFunctions.DoubleGreater(Bars.High[0], Bars.High.Highest(ShortN, 1))  //C>HH, sell high
                && PublicFunctions.DoubleLess(Bars.Close[0], Bars.Close[1]))  //C<C-1
                m_SK.Send();

            if (PublicFunctions.DoubleGreater(Bars.High[0], Bars.High.Highest(ShortN, 1))
              && PublicFunctions.DoubleLess(Bars.Close[0], Bars.Close[1]))
                m_SP.Send();

            if (PublicFunctions.DoubleLess(Bars.Low[0], Bars.Low.Lowest(ShortN, 1))
             && PublicFunctions.DoubleGreater(Bars.Close[0], Bars.Close[1]))
                m_BP.Send();
			
        }
	}
}