using Meadow.Units;
using System.Threading.Tasks;

namespace Meadow.Foundation.Sensors.Environmental;

public class SimulatedKellerTransducer : IKellerTransducer
{
    public Task<Pressure> ReadPressure(PressureChannel channel)
    {
        return Task.FromResult(new Pressure(123, Pressure.UnitType.Millibar));
    }

    public Task<Units.Temperature> ReadTemperature(TemperatureChannel channel)
    {
        return Task.FromResult(new Units.Temperature(42.1, Units.Temperature.UnitType.Fahrenheit));
    }
}
