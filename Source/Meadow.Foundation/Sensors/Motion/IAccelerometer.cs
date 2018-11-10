using System;

namespace Meadow.Foundation.Sensors.Motion
{
	public interface IAccelerometer
	{
		float XAcceleration { get; }
		float YAcceleration { get; }
		float ZAcceleration { get; }
	}
}