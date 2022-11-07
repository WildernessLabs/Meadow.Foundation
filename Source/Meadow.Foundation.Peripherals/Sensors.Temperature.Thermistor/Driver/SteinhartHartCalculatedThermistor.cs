using Meadow.Hardware;
using Meadow.Units;
using System;
using System.Threading.Tasks;

namespace Meadow.Foundation.Sensors.Temperature
{
    public class SteinhartHartCalculatedThermistor : Thermistor
    {
        public const double DefaultBetaCoefficient = 3950d;

        public double BetaCoefficient { get; } = DefaultBetaCoefficient;
        public Resistance SeriesResistance { get; }
        public override Resistance ThermistorResistanceAt25C { get; }

        public SteinhartHartCalculatedThermistor(IAnalogInputPort analog, Resistance nominalResistanceAt25C)
            : base(analog)
        {
            ThermistorResistanceAt25C = nominalResistanceAt25C;
            SeriesResistance = nominalResistanceAt25C;
        }

        public SteinhartHartCalculatedThermistor(IAnalogInputPort analog, Resistance nominalResistanceAt25C, Resistance seriesResisance)
            : base(analog)
        {
            ThermistorResistanceAt25C = nominalResistanceAt25C;
            SeriesResistance = seriesResisance;
        }

        public SteinhartHartCalculatedThermistor(IAnalogInputPort analog, Resistance nominalResistanceAt25C, Resistance seriesResisance, double betaCoefficient)
            : base(analog)
        {
            ThermistorResistanceAt25C = nominalResistanceAt25C;
            SeriesResistance = seriesResisance;
            BetaCoefficient = betaCoefficient;
        }

        public SteinhartHartCalculatedThermistor(IAnalogInputPort analog, Resistance nominalResistanceAt25C, double betaCoefficient)
            : base(analog)
        {
            ThermistorResistanceAt25C = nominalResistanceAt25C;
            SeriesResistance = nominalResistanceAt25C;
            BetaCoefficient = betaCoefficient;
        }

        protected override async Task<Units.Temperature> ReadSensor()
        {
            var voltageReading = await AnalogInput.Read();

            var analogMax = 0xffff >> AnalogInput.Channel.Precision;
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