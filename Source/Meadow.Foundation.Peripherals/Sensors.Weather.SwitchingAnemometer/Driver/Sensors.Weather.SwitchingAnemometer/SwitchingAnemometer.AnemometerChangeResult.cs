using System;
using Meadow;

namespace Meadow.Foundation.Sensors.Weather
{
	public partial class SwitchingAnemometer
	{
		public class AnemometerChangeResult : INumericChangeResult<float>
		{
			public float New { get; set; }
			public float Old { get; set; }

			public float DeltaPercent => (Old > 0) ? Delta / Old : 0f;

			public float Delta => New - Old;

			public AnemometerChangeResult()
			{
			}
		}
	}
}
