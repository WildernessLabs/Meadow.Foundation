using Meadow.Hardware;
using Meadow.Peripherals.Sensors.Environmental;
using Meadow.Units;
using System;
using System.Threading.Tasks;

namespace Meadow.Foundation.Sensors.Environmental
{
    /// <summary>
    /// Represents an IonScience MiniPID2 analog photoionisation (PID) Volatile Organic Compounds (VOC) sensor
    /// </summary>
    public partial class MiniPID2 : SamplingSensorBase<Concentration>, IConcentrationSensor, IDisposable
    {
        /// <summary>
        /// Raised when the VOC concentration changes
        /// </summary>
        public event EventHandler<IChangeResult<Concentration>> ConcentrationUpdated = default!;

        /// <summary>
        /// Raised when the VOC concentration changes
        /// </summary>
        public event EventHandler<IChangeResult<Concentration>> VOCConcentrationUpdated = default!;

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
        /// Is the object disposed
        /// </summary>
        public bool IsDisposed { get; private set; }

        /// <summary>
        /// Did we create the port(s) used by the peripheral
        /// </summary>
        readonly bool createdPort = false;

        SensorCalibration[]? calibrations;

        /// <summary>
        /// Create a new MiniPID2 object
        /// </summary>
        /// <param name="analogPin">The analog data pin connected to the sensor</param>
        /// <param name="pid2Type">The MiniPID sensor type</param>
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
            createdPort = true;
        }

        /// <summary>
        /// Create a new MiniPID2 object
        /// </summary>
        /// <param name="analogInputPort">The analog port connected to the sensor</param>
        /// <param name="pid2Type">The MiniPID sensor type</param>
        public MiniPID2(IAnalogInputPort analogInputPort, MiniPID2Type pid2Type)
        {
            MiniPID2DeviceType = pid2Type;
            AnalogInputPort = analogInputPort;
            Initialize();
        }

        /// <summary>
        /// Set the sensor voltage offset 
        /// Useful for compensating for air conditions
        /// </summary>
        /// <param name="offset">Offset voltage</param>
        /// <param name="sensorType">The sensor to change</param>
        public void SetOffsetForSensor(Voltage offset, MiniPID2Type sensorType)
        {
            calibrations![(int)sensorType].Offset = offset;
        }

        /// <summary>
        /// Get the voltage offset for a sensor
        /// </summary>
        /// <param name="sensorType">The sensor</param>
        /// <returns>The offset as voltage</returns>
        public Voltage GetOffsetForSensor(MiniPID2Type sensorType)
            => calibrations![(int)sensorType].Offset;

        /// <summary>
        /// Initialize the sensor
        /// </summary>
        void Initialize()
        {
            static SensorCalibration GetCalibration(int airOffsetLow, int airOffsetHigh, double sensitivity, double minimumPPB)
            {
                return new SensorCalibration(new Voltage((airOffsetLow + airOffsetHigh) / 2, Voltage.UnitType.Millivolts),
                                                                        new Voltage(sensitivity, Voltage.UnitType.Millivolts),
                                                                        new Concentration(minimumPPB, Units.Concentration.UnitType.PartsPerBillion));
            }

            calibrations = new SensorCalibration[(int)MiniPID2Type.count];

            calibrations[(int)MiniPID2Type.PPM] = GetCalibration(51, 65, 65, 100);
            calibrations[(int)MiniPID2Type.PPM_WR] = GetCalibration(51, 64, 40, 500);
            calibrations[(int)MiniPID2Type.PPB] = GetCalibration(51, 80, 30, 1);
            calibrations[(int)MiniPID2Type.PPB_WR] = GetCalibration(51, 80, 5, 20);
            calibrations[(int)MiniPID2Type.HS] = GetCalibration(100, 200, 600, 0.5);
            calibrations[(int)MiniPID2Type._10ev] = GetCalibration(51, 80, 15, 5);
            calibrations[(int)MiniPID2Type._11_7eV] = GetCalibration(51, 90, 1, 100);

            AnalogInputPort.Subscribe
            (
                IAnalogInputPort.CreateObserver(
                    result =>
                    {
                        ChangeResult<Concentration> changeResult = new()
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
            int i = (int)MiniPID2DeviceType;

            var ppm = (voltage.Millivolts - calibrations![i].Offset.Millivolts) / calibrations[i].Sensitivity.Millivolts;

            if (ppm < calibrations[i].MinimumDetectionLimit.PartsPerMillion)
            {
                return calibrations[i].MinimumDetectionLimit;
            }

            return new Concentration(ppm, Units.Concentration.UnitType.PartsPerMillion);
        }

        struct SensorCalibration
        {
            public SensorCalibration(Voltage offset, Voltage sensitivity, Concentration minimumDetectionLimit)
            {
                Offset = offset;
                Sensitivity = sensitivity;
                MinimumDetectionLimit = minimumDetectionLimit;
            }

            /// <summary>
            /// The offset to compensate for air quality/conditions
            /// </summary>
            public Voltage Offset { get; set; }

            /// <summary>
            /// Sensitivity mv/ppm as voltage
            /// </summary>
            public Voltage Sensitivity { get; set; }

            /// <summary>
            /// The minimum concentration returned by the sensor
            /// </summary>
            public Concentration MinimumDetectionLimit { get; set; }
        }

        ///<inheritdoc/>
        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Dispose of the object
        /// </summary>
        /// <param name="disposing">Is disposing</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!IsDisposed)
            {
                if (disposing && createdPort)
                {
                    AnalogInputPort?.Dispose();
                }

                IsDisposed = true;
            }
        }
    }
}