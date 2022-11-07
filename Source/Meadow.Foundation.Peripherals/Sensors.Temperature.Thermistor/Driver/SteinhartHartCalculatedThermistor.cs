using Meadow.Hardware;
using Meadow.Units;
using System;
using System.Threading.Tasks;

namespace Meadow.Foundation.Sensors.Temperature
{
    public abstract class SteinhartHartCalculatedThermistor : ThermistorTypeBase
    {
        public abstract double BetaCoefficient { get; }
        public abstract Resistance SeriesResistance { get; }

        public override async Task<Units.Temperature> CalculateTemperature(IAnalogInputPort analog)
        {
            var voltageReading = await analog.Read();

            var analogMax = 0xffff >> analog.Channel.Precision;
            var measuredResistance = SeriesResistance.Ohms / (analogMax / voltageReading.Volts - 1);

            double steinhart;
            steinhart = measuredResistance / ThermistorResistanceAt25C.Ohms;     // (R/Ro)
            steinhart = Math.Log(steinhart);                  // ln(R/Ro)
            steinhart /= BetaCoefficient;                   // 1/B * ln(R/Ro)
            steinhart += 1.0 / NominalTemperature.Kelvin; // + (1/To)
            steinhart = 1.0 / steinhart;                 // Invert

            return new Units.Temperature(steinhart, Units.Temperature.UnitType.Kelvin);

        }
    }
}