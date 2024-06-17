using Meadow.Hardware;
using Meadow.Peripherals.Sensors;
using Meadow.Units;
using System;
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

        Units.Temperature? Temperature { get; set; }

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
            AnalogInputPort.Subscribe(IAnalogInputPort.CreateObserver(handler: HandleAnalogUpdate,filter: FilterAnalogUpdate));
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
            AnalogInputPort.Subscribe(IAnalogInputPort.CreateObserver(handler: HandleAnalogUpdate, filter: FilterAnalogUpdate));

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
            AnalogInputPort.Subscribe(IAnalogInputPort.CreateObserver(handler: HandleAnalogUpdate, filter: FilterAnalogUpdate));
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
            AnalogInputPort.Subscribe(IAnalogInputPort.CreateObserver(handler: HandleAnalogUpdate, filter: FilterAnalogUpdate));
        }

        /// <summary>
        /// Reads the sensor voltage, and updates the Temperature property 
        /// </summary>
        public override Task<Units.Temperature> Read()
        {
            return this.ReadSensor();
        }

        /// <summary>
        /// Reads the sensor voltage, and updates the Temperature property 
        /// </summary>
        protected override async Task<Units.Temperature> ReadSensor()
        {
            Units.Temperature temperature = VoltageToTemperature(await AnalogInputPort.Read());
            this.Temperature = temperature;
            return temperature;
        }

        /// <summary>
        /// This filter prevents Temperature calculations for changes in sensor voltage < 10 mV
        /// </summary>
        /// <param name="result"></param>
        /// <returns></returns>
        private bool FilterAnalogUpdate(IChangeResult<Voltage> result)
        {
            bool filter = false;
            if (result.Old is { } old) //if (result.Old is not null) new Voltage old = result.old.Value
            {
                filter = Math.Abs(result.New.Millivolts - old.Millivolts) > 10;
                // Resolver.Log.Info($"Old:{old.Millivolts:N1} mV");
            }

            // Resolver.Log.Info($"New:{result.New.Millivolts:N1} mV");
            // Resolver.Log.Info($"Filter was:{filter}");

            return filter;
        }

        /// <summary>
        /// Update function calculates new temperature from voltage when filter Voltage threshold is met
        /// </summary>
        /// <param name="result">Voltgae change result, old and new</param>
        protected void HandleAnalogUpdate(IChangeResult<Voltage> result)
        {
            Units.Temperature temperature = VoltageToTemperature(result.New);
            ChangeResult<Units.Temperature> TemperatureChangeResult = new()
            {
                Old = this.Temperature,
                New = temperature,
            };
            this.Temperature = temperature;
            // Resolver.Log.Info($"New Temp Up:{this.Temperature.Value.Celsius:N1}  °C");
            base.RaiseEventsAndNotify(TemperatureChangeResult);
        }


        /// <summary>
        /// Example of a function that could be set in User code to run on updates 
        /// </summary>
        /// <param name="sender">Thermistor object that generated update</param>
        /// <param name="e"></param>a structure of two temperatures, old and new
        void HandleTemperatureUpdate(object sender, IChangeResult<Meadow.Units.Temperature> e)
        {
             Resolver.Log.Info($"Handle Result: {e.New.Celsius:N1} °C");
        }
    
        /// <summary>
        /// Concertes sensor voltage to a Temperature 
        /// </summary>
        /// <param name="voltageReading">Voltage read from the sensor</param>
        /// <returns></returns>
        protected Units.Temperature VoltageToTemperature(Voltage voltageReading)
        {
            // calculate resistance, in ohms, from voltage divider circuit
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