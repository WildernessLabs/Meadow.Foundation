using Meadow.Hardware;
using Meadow.Units;
using System.Threading.Tasks;

namespace Meadow.Foundation.Sensors.Temperature
{
    public interface IThermistorType
    {
        Resistance ThermistorResistanceAt25C { get; }
        Task<Units.Temperature> CalculateTemperature(IAnalogInputPort analog);
    }
}