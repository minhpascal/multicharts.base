using PowerLanguage.Function;

namespace PowerLanguage.Strategy {

	
	public class optimize_by_sharp_ratio : SignalObject {
		
		private SharpeRatio m_SharpeRatio;
		
		public optimize_by_sharp_ratio(object ctx) : 
				base(ctx) {
		}

	    [Input]
	    public SharpeRatio.EPeriod Period{ get; set; }
		
		protected override void Create() {
			m_SharpeRatio = new SharpeRatio(this);
		}
		
		protected override void StartCalc() {
            m_SharpeRatio.Period = Period;
		}
		
		protected override void CalcBar() {
			if (Bars.LastBarOnChart)
                CustomFitnessValue = m_SharpeRatio.Value;
		}
	}
}