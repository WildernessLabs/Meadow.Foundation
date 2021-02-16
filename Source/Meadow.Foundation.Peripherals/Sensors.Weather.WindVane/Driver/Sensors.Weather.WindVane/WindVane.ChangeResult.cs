using System;
using Meadow;
using Meadow.Units;

namespace Meadow.Foundation.Sensors.Weather
{
	public partial class WindVane
	{
		public class WindVaneChangeResult : INumericChangeResult<Azimuth>
		{
			public Azimuth New { get; set; }
			public Azimuth Old { get; set; }

			public Azimuth DeltaPercent => (Old > 0) ? Delta / Old : 0;

			public Azimuth Delta => New - Old;

			public WindVaneChangeResult()
			{
			}
		}
	}
}