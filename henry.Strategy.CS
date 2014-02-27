using System;
using System.Drawing;
using System.Linq;
using PowerLanguage.Function;
using ATCenterProxy.interop;

namespace PowerLanguage.Strategy {
	public class henry : SignalObject {
		
		//MD
	  	private AverageFC m_AverageFC;
        private VariableSeries<Double> m_Avg;
        private VariableObject<Int32> m_Counter;

		//BK
        private IOrderMarket m_MACrossLE;
		//SP
        private IOrderMarket m_MACrossLX;
		//SK
        private IOrderMarket m_MACrossSE;
		//BP
        private IOrderMarket m_MACrossSX;

        public henry(object ctx) :
            base(ctx)
        {
            Length = 9;  //INPUT
            ConfirmBars = 1;
        }

        private ISeries<double> Price { get; set; }

        [Input]
        public int Length { get; set; }

        [Input]
        public int ConfirmBars { get; set; }

        protected override void Create(){
            m_AverageFC = new AverageFC(this);
            m_Avg = new VariableSeries<Double>(this);
		    m_Counter = new VariableObject<int>(this);
				
            m_MACrossLE =
                OrderCreator.MarketNextBar(new SOrderParameters(Contracts.Default, "MACrossLE", EOrderAction.Buy));
            m_MACrossLX =
                OrderCreator.MarketNextBar(new SOrderParameters(Contracts.Default, "MACrossLX", EOrderAction.Sell,
                                                                      OrderExit.FromAll));
			m_MACrossSE =
                OrderCreator.MarketNextBar(new SOrderParameters(Contracts.Default, "MACrossSE",
                                                                      EOrderAction.SellShort));
            m_MACrossSX =
                OrderCreator.MarketNextBar(new SOrderParameters(Contracts.Default, "MACrossSX",
                                                                      EOrderAction.BuyToCover, OrderExit.FromAll));
        }

        protected override void StartCalc(){
            Price = Bars.Close;
            m_AverageFC.price = Price;
            m_AverageFC.length = Length;
        }


        protected override void CalcBar(){
            double m_Avg = m_AverageFC[0];
			//BK
            if (PublicFunctions.DoubleGreater(Price[0], m_Avg)){// C>MA
                ++m_Counter.Value;
            }
            else{
                m_Counter.Value = 0;
            }
			
			//trade actions
            if (PublicFunctions.DoubleGreater(Bars.CurrentBar, ConfirmBars) 
                 && m_Counter.Value == ConfirmBars)
            {
                m_MACrossLE.Send();
            }
			else
            if (Bars.CurrentBar > 1 && Bars.Close.CrossesUnder(m_Avg, ExecInfo.MaxBarsBack)) //CROSSDOWN(C,MA)
            {
                m_MACrossLX.Send();
            }
			//SK
            if (PublicFunctions.DoubleLess(Price[0], m_Avg)){
                ++m_Counter.Value;
            }
            else{
                m_Counter.Value = 0;
            }
            if (PublicFunctions.DoubleGreater(Bars.CurrentBar, ConfirmBars)
                 && m_Counter.Value == ConfirmBars)
            {
                m_MACrossSE.Send();
            }
			
			//BP
			if (Bars.CurrentBar > 1 && Bars.Close.CrossesOver(m_Avg, ExecInfo.MaxBarsBack))
            {
                m_MACrossSX.Send();
            }
        }
	}
}