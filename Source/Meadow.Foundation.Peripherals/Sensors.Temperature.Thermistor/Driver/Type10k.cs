using Meadow.Hardware;
using Meadow.Units;
using System;
using System.Threading.Tasks;

namespace Meadow.Foundation.Sensors.Temperature
{
    public class Type10k : SteinhartHartCalculatedThermistor
    {
        private Resistance seriesResistance;
        private object betaCoefficient;

        public static Resistance DefaultSeriesResistance = new Resistance(10, Resistance.UnitType.Kiloohms);
        public static double DefaultBetaCoefficient = 3950d;

        public override Resistance ThermistorResistanceAt25C => new Resistance(10, Resistance.UnitType.Kiloohms);

        public override Resistance SeriesResistance => seriesResistance;
        public override double BetaCoefficient => 3950d;

        public Type10k()
        {
            this.seriesResistance = DefaultSeriesResistance;
            this.betaCoefficient = DefaultBetaCoefficient;
        }

        public Type10k(Resistance seriesResistance)
        {
            this.seriesResistance = seriesResistance;
            this.betaCoefficient = DefaultBetaCoefficient;
        }

        public Type10k(double betaCoefficient)
        {
            this.seriesResistance = DefaultSeriesResistance;
            this.betaCoefficient = betaCoefficient;
        }

        public Type10k(Resistance seriesResistance, double betaCoefficient)
        {
            this.seriesResistance = seriesResistance;
            this.betaCoefficient = betaCoefficient;
        }

        public override async Task<Units.Temperature> CalculateTemperature(IAnalogInputPort analog)
        {
            var voltageReading = await analog.Read();
            var seriesResistance = new Resistance(10, Resistance.UnitType.Kiloohms);

            var analogMax = 0xffff >> analog.Channel.Precision;
            var measuredResistance = seriesResistance.Ohms / (analogMax / voltageReading.Volts - 1);

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