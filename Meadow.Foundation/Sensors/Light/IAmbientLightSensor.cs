using System;

namespace Meadow.Foundation.Sensors.Light
{
	interface IAmbientLightSensor
	{
		double Reading { get; }
	}
}