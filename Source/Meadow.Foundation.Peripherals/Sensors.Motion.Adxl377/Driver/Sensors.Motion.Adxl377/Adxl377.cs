using Meadow.Foundation.Spatial;
using Meadow.Hardware;
using Meadow.Peripherals.Sensors.Motion;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Meadow.Foundation.Sensors.Motion
{
    /// <summary>
    ///     Driver for the ADXL377 triple axis accelerometer.
    ///     +/- 200g
    /// </summary>
    public class Adxl377 : FilterableChangeObservableBase<AccelerationConditionChangeResult, AccelerationConditions>,
        IAccelerometer
    {
        /// <summary>
        ///     Minimum value that can be used for the update interval when the
        ///     sensor is being configured to generate interrupts.
        /// </summary>
        public const ushort MinimumPollingPeriod = 100;

        /// <summary>
        ///     Analog input channel connected to the x axis.
        /// </summary>
        private readonly IAnalogInputPort _xPort;

        /// <summary>
        ///     Analog input channel connected to the x axis.
        /// </summary>
        private readonly IAnalogInputPort _yPort;

        /// <summary>
        ///     Analog input channel connected to the x axis.
        /// </summary>
        private readonly IAnalogInputPort _zPort;

        /// <summary>
        ///     Voltage that represents 0g.  This is the supply voltage / 2.
        /// </summary>
        private float _zeroGVoltage => SupplyVoltage / 2f;

        

        

        /// <summary>
        ///     Acceleration along the X-axis.
        /// </summary>
        /// <remarks>
        ///     This property will only contain valid data after a call to Read or after
        ///     an interrupt has been generated.
        /// </remarks>
        public float AccellerationX => Conditions.XAcceleration.Value;

        /// <summary>
        ///     Acceleration along the Y-axis.
        /// </summary>
        /// <remarks>
        ///     This property will only contain valid data after a call to Read or after
        ///     an interrupt has been generated.
        /// </remarks>
        public float AccelerationY => Conditions.YAcceleration.Value;

        /// <summary>
        ///     Acceleration along the Z-axis.
        /// </summary>
        /// <remarks>
        ///     This property will only contain valid data after a call to Read or after
        ///     an interrupt has been generated.
        /// </remarks>
        public float AccelerationZ => Conditions.ZAcceleration.Value;

        /// <summary>
        ///     Volts per G for the X axis.
        /// </summary>
        public float XVoltsPerG { get; set; }

        /// <summary>
        ///     Volts per G for the X axis.
        /// </summary>
        public float YVoltsPerG { get; set; }

        /// <summary>
        ///     Volts per G for the X axis.
        /// </summary>
        public float ZVoltsPerG { get; set; }

        /// <summary>
        ///     Power supply voltage applied to the sensor.  This will be set (in the constructor)
        ///     to 3.3V by default.
        /// </summary>
        public float SupplyVoltage { get; set; }

        public AccelerationConditions Conditions { get; protected set; } = new AccelerationConditions();

        // internal thread lock
        private object _lock = new object();
        private CancellationTokenSource SamplingTokenSource;

        /// <summary>
        /// Gets a value indicating whether the analog input port is currently
        /// sampling the ADC. Call StartSampling() to spin up the sampling process.
        /// </summary>
        /// <value><c>true</c> if sampling; otherwise, <c>false</c>.</value>
        public bool IsSampling { get; protected set; } = false;

        

        

        public event EventHandler<AccelerationConditionChangeResult> Updated;

        

        

        /// <summary>
        ///     Create a new ADXL337 sensor object.
        /// </summary>
        /// <param name="xPin">Analog pin connected to the X axis output from the ADXL335 sensor.</param>
        /// <param name="yPin">Analog pin connected to the Y axis output from the ADXL335 sensor.</param>
        /// <param name="zPin">Analog pin connected to the Z axis output from the ADXL335 sensor.</param>
        public Adxl377(IIODevice device, IPin xPin, IPin yPin, IPin zPin)
        {
            _xPort = device.CreateAnalogInputPort(xPin);
            _yPort = device.CreateAnalogInputPort(yPin);
            _zPort = device.CreateAnalogInputPort(zPin);
            //
            //  Now set the default calibration data.
            //
            XVoltsPerG = 0.00825f;
            YVoltsPerG = 0.00825f;
            ZVoltsPerG = 0.00825f;
            SupplyVoltage = 3.3f;
        }

        

        

        ///// <summary>
        ///// Convenience method to get the current temperature. For frequent reads, use
        ///// StartSampling() and StopSampling() in conjunction with the SampleBuffer.
        ///// </summary>
        public async Task<AccelerationConditions> Read()
        {
            await Update();

            return Conditions;
        }

        ///// <summary>
        ///// Starts continuously sampling the sensor.
        /////
        ///// This method also starts raising `Changed` events and IObservable
        ///// subscribers getting notified.
        ///// </summary>
        public void StartUpdating(int standbyDuration = 1000)
        {
            // thread safety
            lock (_lock) {
                if (IsSampling) { return; }

                // state muh-cheen
                IsSampling = true;

                SamplingTokenSource = new CancellationTokenSource();
                CancellationToken ct = SamplingTokenSource.Token;

                AccelerationConditions oldConditions;
                AccelerationConditionChangeResult result;
                Task.Factory.StartNew(async () => {
                    while (true) {
                        if (ct.IsCancellationRequested) {
                            // do task clean up here
                            _observers.ForEach(x => x.OnCompleted());
                            break;
                        }
                        // capture history
                        oldConditions = AccelerationConditions.From(Conditions);

                        // read
                        await Update();

                        // build a new result with the old and new conditions
                        result = new AccelerationConditionChangeResult(oldConditions, Conditions);

                        // let everyone know
                        RaiseChangedAndNotify(result);

                        // sleep for the appropriate interval
                        await Task.Delay(standbyDuration);
                    }
                }, SamplingTokenSource.Token);
            }
        }

        protected void RaiseChangedAndNotify(AccelerationConditionChangeResult changeResult)
        {
            Updated?.Invoke(this, changeResult);
            base.NotifyObservers(changeResult);
        }

        ///// <summary>
        ///// Stops sampling the temperature.
        ///// </summary>
        public void StopUpdating()
        {
            lock (_lock) {
                if (!IsSampling) { return; }

                SamplingTokenSource?.Cancel();

                // state muh-cheen
                IsSampling = false;
            }
        }

        /// <summary>
        ///     Read the sensor output and convert the sensor readings into acceleration values.
        /// </summary>
        public async Task Update()
        {
            Conditions.XAcceleration = (await _xPort.Read() - _zeroGVoltage) / XVoltsPerG;
            Conditions.YAcceleration = (await _yPort.Read() - _zeroGVoltage) / YVoltsPerG;
            Conditions.ZAcceleration = (await _zPort.Read() - _zeroGVoltage) / ZVoltsPerG;
        }

        /// <summary>
        ///     Get the raw analog input values from the sensor.
        /// </summary>
        /// <returns>Vector object containing the raw sensor data from the analog pins.</returns>
        public async Task<Vector> GetRawSensorData()
        {
            return new Vector(await _xPort.Read(), await _yPort.Read(), await _zPort.Read());
        }

        
    }
}