using System;

namespace Netduino.Foundation.Sensors.Light
{
	interface IAmbientLightSensor
	{
		double Reading { get; }
	}
}