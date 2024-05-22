using Meadow.Hardware;
using Meadow.Units;
using System;
using System.Reflection;
using System.Threading.Tasks;

namespace Meadow.Foundation.Sensors.Temperature
{
    /// <summary>
    /// Thermistor temperature sensor object using the Steinhart-Hart equation to determine temperature
    /// </summary>
    public class SteinhartHartCalculatedThermistor : Thermistor
    {
        /// <summary>
        /// The common default beta coefficient (3950) for many thermistors
        /// </summary>
        public const double DefaultBetaCoefficient = 3950d;

        /// <summary>
        /// Gets the beta coefficient of the thermistor used in the Steinhart-Hart equation
        /// </summary>
        public double BetaCoefficient { get; } = DefaultBetaCoefficient;

        /// <summary>
        /// Gets the resistance of the fixed-value series resistor in your voltage divider circuit
        /// </summary>
        public Resistance SeriesResistance { get; }

        /// <summary>
        /// Gets the nominal resistance of the thermistor (e.g. 10kOhm for a 10k thermistor)
        /// </summary>
        public override Resistance NominalResistance { get; }

        /// <summary>
        /// observer for setting conditions
        /// </summary>
        FilterableChangeObserver<Voltage> VoltageObserver;


        /// <summary>
        /// Creates a new SteinhartHartCalculatedThermistor object using the provided analog input and the thermistor's nominal resistance (i.e. 10kOhm)
        /// </summary>
        /// <param name="analogInput">The analog input reading the thermistor voltage divider output</param>
        /// <param name="nominalResistance">The nominal resistance of the thermistor (e.g. 10kOhm for a 10k thermistor)</param>
        /// <remarks>The fixed resistor value will be assumed to match the thermistor's nominal resistance</remarks>
        public SteinhartHartCalculatedThermistor(IAnalogInputPort analogInput, Resistance nominalResistance)
            : base(analogInput)
        {
            NominalResistance = nominalResistance;
            SeriesResistance = nominalResistance;

            VoltageObserver = AddListener();
        }

        /// <summary>
        /// Creates a new SteinhartHartCalculatedThermistor object using the provided analog input and the thermistor's nominal resistance (i.e. 10kOhm) and the fixed resistor value of the voltage divider circuit.
        /// </summary>
        /// <param name="analogInput">The analog input reading the thermistor voltage divider output</param>
        /// <param name="nominalResistance">The nominal resistance of the thermistor (e.g. 10kOhm for a 10k thermistor)</param>
        /// <param name="seriesResistance">The resistance of the fixed-value series resistor in your voltage divider circuit</param>
        public SteinhartHartCalculatedThermistor(IAnalogInputPort analogInput, Resistance nominalResistance, Resistance seriesResistance)
            : base(analogInput)
        {
            NominalResistance = nominalResistance;
            SeriesResistance = seriesResistance;
            VoltageObserver = AddListener();

        }

        /// <summary>
        /// Creates a new SteinhartHartCalculatedThermistor object using the provided analog input and the thermistor's nominal resistance (i.e. 10kOhm) and the fixed resistor value of the voltage divider circuit.
        /// </summary>
        /// <param name="analogInput">The analog input reading the thermistor voltage divider output</param>
        /// <param name="nominalResistance">The nominal resistance of the thermistor (e.g. 10kOhm for a 10k thermistor)</param>
        /// <param name="seriesResistance">The resistance of the fixed-value series resistor in your voltage divider circuit</param>
        /// <param name="betaCoefficient">The beta coefficient of the thermistor used in the Steinhart-Hart equation</param>
        public SteinhartHartCalculatedThermistor(IAnalogInputPort analogInput, Resistance nominalResistance, Resistance seriesResistance, double betaCoefficient)
            : base(analogInput)
        {
            NominalResistance = nominalResistance;
            SeriesResistance = seriesResistance;
            BetaCoefficient = betaCoefficient;
            VoltageObserver = AddListener();
        }

        /// <summary>
        /// Creates a new SteinhartHartCalculatedThermistor object using the provided analog input and the thermistor's nominal resistance (i.e. 10kOhm) and the fixed resistor value of the voltage divider circuit.
        /// </summary>
        /// <param name="analogInput">The analog input reading the thermistor voltage divider output</param>
        /// <param name="nominalResistance">The nominal resistance of the thermistor (e.g. 10kOhm for a 10k thermistor)</param>
        /// <param name="betaCoefficient">The beta coefficient of the thermistor used in the Steinhart-Hart equation</param>
        /// <remarks>The fixed resistor value will be assumed to match the thermistor's nominal resistance</remarks>
        public SteinhartHartCalculatedThermistor(IAnalogInputPort analogInput, Resistance nominalResistance, double betaCoefficient)
            : base(analogInput)
        {
            NominalResistance = nominalResistance;
            SeriesResistance = nominalResistance;
            BetaCoefficient = betaCoefficient;
            VoltageObserver = AddListener();
        }

        /// <summary>
        /// Update the Temperature property
        /// </summary>
        protected override async Task<Units.Temperature> ReadSensor()
        {
            Units.Temperature temp = VoltageToTemperature(await AnalogInputPort.Read());
            Conditions = temp;
            return Conditions;
        }


        protected FilterableChangeObserver<Voltage> AddListener()
        {
            AnalogInputPort.Subscribe
           (
                
                VoltageObserver = IAnalogInputPort.CreateObserver(
                   result =>
                   {
                       ChangeResult<Units.Temperature> changeResult = new ChangeResult<Units.Temperature>()
                       {
                           New = VoltageToTemperature(result.New),
                           Old = Temperature,
                       };
                       RaiseEventsAndNotify(changeResult);
                   }
            )
           );
            Updated += HandleResult;
                /*(object sender, IChangeResult<Meadow.Units.Temperature> e) =>
            {
                //Resolver.Log.Info($"Temperature Updated: {e.New.Fahrenheit:N1}F/{e.New.Celsius:N1}C");
                Conditions = e.New;
            }; */
            return VoltageObserver;
        }

        
        void HandleResult(object sender, IChangeResult<Meadow.Units.Temperature> e)
        {
            Conditions = e.New;
           // Resolver.Log.Info($"Handler: {Conditions.Celsius:N1} °C");
         }


        protected Units.Temperature VoltageToTemperature(Voltage voltageReading)
        {
            // ohms
            var measuredResistance = (SeriesResistance.Ohms * voltageReading.Volts) / (AnalogInputPort.ReferenceVoltage.Volts - voltageReading.Volts);

            double steinhart;
            steinhart = measuredResistance / NominalResistance.Ohms;
            steinhart = Math.Log(steinhart);
            steinhart /= BetaCoefficient;
            steinhart += 1.0 / NominalTemperature.Kelvin;
            steinhart = 1.0 / steinhart;
            return new Units.Temperature(steinhart, Units.Temperature.UnitType.Kelvin);
        }
    }
}