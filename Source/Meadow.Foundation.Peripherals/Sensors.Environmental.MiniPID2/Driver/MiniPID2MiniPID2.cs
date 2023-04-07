using Meadow.Hardware;
using Meadow.Peripherals.Sensors.Environmental;
using Meadow.Units;
using System;
using System.Threading.Tasks;

namespace Meadow.Foundation.Sensors.Environmental
{
    /// <summary>
    /// Represents an IonScience MiniPID2 analog photoionisation (PID) VOC sensor
    /// </summary>
    public partial class MiniPID2 : SamplingSensorBase<Concentration>, IConcentrationSensor
    {
        /// <summary>
        /// Raised when the VOC concentration changes
        /// </summary>
        public event EventHandler<IChangeResult<Concentration>> ConcentrationUpdated = delegate { };

        /// <summary>
        /// Raised when the VOC concentration changes
        /// </summary>
        public event EventHandler<IChangeResult<Concentration>> VOCConcentrationUpdated = delegate { };

        /// <summary>
        /// The current VOC concentration value
        /// </summary>
        public Concentration? Concentration { get; protected set; }

        /// <summary>
        /// The MiniPID2 device type
        /// </summary>
        public MiniPID2Type MiniPID2DeviceType { get; protected set; }

        ///<Summary>
        /// AnalogInputPort connected to temperature sensor
        ///</Summary>
        protected IAnalogInputPort AnalogInputPort { get; }

        /// <summary>
        /// Create a new MiniPID2 object
        /// </summary>
        /// <param name="analogPin">The analog data pin connected to the sensor</param>
        /// <param name="sampleCount">How many samples to take during a given reading.
        /// These are automatically averaged to reduce noise</param>
        /// <param name="sampleInterval">The time between sample readings</param>
        public MiniPID2(IPin analogPin,
                        MiniPID2Type pid2Type,
                        int sampleCount = 5,
                        TimeSpan? sampleInterval = null) :
            this(analogPin.CreateAnalogInputPort(sampleCount,
                sampleInterval ?? TimeSpan.FromMilliseconds(40),
                new Voltage(3.3, Voltage.UnitType.Volts)), pid2Type)
        {
        }

        /// <summary>
        /// Create a new MiniPID2 object
        /// </summary>
        /// <param name="analogInputPort">The analog port connected to the sensor</param>
        public MiniPID2(IAnalogInputPort analogInputPort, MiniPID2Type pid2Type)
        {
            MiniPID2DeviceType = pid2Type;
            AnalogInputPort = analogInputPort;
            Initialize();
        }

        /// <summary>
        /// Initialize the sensor
        /// </summary>
        void Initialize()
        {
            AnalogInputPort.Subscribe
            (
                IAnalogInputPort.CreateObserver(
                    result =>
                    {
                        ChangeResult<Concentration> changeResult = new ChangeResult<Concentration>()
                        {
                            New = VoltageToConcentration(result.New),
                            Old = Concentration
                        };
                        Concentration = changeResult.New;
                        RaiseEventsAndNotify(changeResult);
                    }
                )
           );
        }

        /// <summary>
        /// Convenience method to get the current concentration. For frequent reads, use
        /// StartSampling() and StopSampling() in conjunction with the SampleBuffer.
        /// </summary>
        /// <returns>The concentration averages of the given sample count</returns>
        protected override async Task<Concentration> ReadSensor()
        {
            var voltage = await AnalogInputPort.Read();
            var newConcentration = VoltageToConcentration(voltage);
            Concentration = newConcentration;
            return newConcentration;
        }

        /// <summary>
        /// Set ambient temperature
        /// </summary>
        /// <param name="ambientTemperature"></param>
        public void SetTemperature(Units.Temperature ambientTemperature)
        {

        }

        /// <summary>
        /// Set relative humidity
        /// </summary>
        /// <param name="humidity"></param>
        public void SetHumidity(RelativeHumidity humidity)
        {

        }

        /// <summary>
        /// Starts updating the sensor on the updateInterval frequency specified
        /// </summary>
        public override void StartUpdating(TimeSpan? updateInterval = null)
        {
            lock (samplingLock)
            {
                if (IsSampling) { return; }
                IsSampling = true;
                AnalogInputPort.StartUpdating(updateInterval);
            }
        }

        /// <summary>
        /// Stops sampling the concentration
        /// </summary>
        public override void StopUpdating()
        {
            lock (samplingLock)
            {
                if (!IsSampling) { return; }
                IsSampling = false;
                AnalogInputPort.StopUpdating();
            }
        }

        /// <summary>
        /// Method to notify subscribers to ConcentrationUpdated event handler
        /// </summary>
        /// <param name="changeResult"></param>
        protected override void RaiseEventsAndNotify(IChangeResult<Concentration> changeResult)
        {
            ConcentrationUpdated?.Invoke(this, changeResult);
            base.RaiseEventsAndNotify(changeResult);
        }

        /// <summary>
        /// Converts voltage to Concentration
        /// </summary>
        /// <param name="voltage"></param>
        /// <returns>Concentration</returns>
        protected Concentration VoltageToConcentration(Voltage voltage)
        {
            switch (MiniPID2DeviceType)
            {
                case MiniPID2Type.PPB:
                    Console.WriteLine(voltage.ToString());
                    var ppb = (voltage.Millivolts - (51 + 65) / 2.0) / 30.0;
                    return new Concentration(ppb, Units.Concentration.UnitType.PartsPerBillion);

            }

            return new Concentration(0);
        }
    }
}