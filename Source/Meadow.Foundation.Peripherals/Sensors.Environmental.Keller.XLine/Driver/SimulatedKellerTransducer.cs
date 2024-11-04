using Meadow.Units;
using System;
using System.Threading.Tasks;

namespace Meadow.Foundation.Sensors.Environmental;

public class SimulatedKellerTransducer : IKellerTransducer
{
    public Task<Pressure> ReadPressure(PressureChannel channel)
    {
        throw new NotImplementedException();
    }

    public Task<Units.Temperature> ReadTemperature(TemperatureChannel channel)
    {
        throw new NotImplementedException();
    }
}
