using Meadow.Hardware;
using Meadow.Units;
using System.Threading.Tasks;

namespace Meadow.Foundation.Sensors.Temperature
{
    public abstract class ThermistorTypeBase : IThermistorType
    {
        public abstract Resistance ThermistorResistanceAt25C { get; }
        public abstract Task<Units.Temperature> CalculateTemperature(IAnalogInputPort analog);
        public virtual Units.Temperature NominalTemperature => new Units.Temperature(25, Units.Temperature.UnitType.Celsius);
    }
}