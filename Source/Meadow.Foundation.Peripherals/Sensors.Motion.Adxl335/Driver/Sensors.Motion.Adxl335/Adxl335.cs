using System;
using System.Threading;
using System.Threading.Tasks;
using Meadow.Foundation.Spatial;
using Meadow.Hardware;
using Meadow.Peripherals.Sensors.Motion;
using Meadow.Units;

namespace Meadow.Foundation.Sensors.Motion
{
    /// <summary>
    ///     Driver for the ADXL335 triple axis accelerometer.
    ///     +/- 3g
    /// </summary>
    public class Adxl335 : SamplingSensorBase<Acceleration3D>, IAccelerometer
    {
        //==== events
        public event EventHandler<IChangeResult<Acceleration3D>> Acceleration3DUpdated = delegate { };

        /// <summary>
        /// Minimum value that can be used for the update interval when the
        /// sensor is being configured to generate interrupts.
        /// </summary>
        public const ushort MinimumPollingPeriod = 100;

        /// <summary>
        /// Analog input channel connected to the x axis.
        /// </summary>
        protected readonly IAnalogInputPort xPort;

        /// <summary>
        /// Analog input channel connected to the x axis.
        /// </summary>
        protected readonly IAnalogInputPort yPort;

        /// <summary>
        /// Analog input channel connected to the x axis.
        /// </summary>
        protected readonly IAnalogInputPort zPort;

        /// <summary>
        /// Voltage that represents 0g.  This is the supply voltage / 2.
        /// </summary>
        protected float ZeroGVoltage => SupplyVoltage / 2f;

        /// <summary>
        /// Volts per G for the X axis.
        /// </summary>
        public float XVoltsPerG { get; set; }

        /// <summary>
        /// Volts per G for the X axis.
        /// </summary>
        public float YVoltsPerG { get; set; }

        /// <summary>
        /// Volts per G for the X axis.
        /// </summary>
        public float ZVoltsPerG { get; set; }

        /// <summary>
        /// Power supply voltage applied to the sensor.  This will be set (in the constructor)
        /// to 3.3V by default.
        /// </summary>
        public float SupplyVoltage { get; set; }

        public Acceleration3D? Acceleration3D => Conditions;

        /// <summary>
        ///     Create a new ADXL335 sensor object.
        /// </summary>
        /// <param name="xPin">Analog pin connected to the X axis output from the ADXL335 sensor.</param>
        /// <param name="yPin">Analog pin connected to the Y axis output from the ADXL335 sensor.</param>
        /// <param name="zPin">Analog pin connected to the Z axis output from the ADXL335 sensor.</param>
        public Adxl335(IAnalogInputController device, IPin xPin, IPin yPin, IPin zPin)
        {
            xPort = device.CreateAnalogInputPort(xPin);
            yPort = device.CreateAnalogInputPort(yPin);
            zPort = device.CreateAnalogInputPort(zPin);
            //
            //  Now set the default calibration data.
            //
            XVoltsPerG = 0.325f;
            YVoltsPerG = 0.325f;
            ZVoltsPerG = 0.550f;
            SupplyVoltage = 3.3f;
        }

        protected override void RaiseEventsAndNotify(IChangeResult<Acceleration3D> changeResult)
        {
            Acceleration3DUpdated?.Invoke(this, changeResult);
            base.RaiseEventsAndNotify(changeResult);
        }

        protected override Task<Acceleration3D> ReadSensor()
        { 
            return Task.Run(async () => {
                var x = await xPort.Read();
                var y = await yPort.Read();
                var z = await zPort.Read();
                return new Acceleration3D(
                    new Acceleration((x.Volts - ZeroGVoltage) / XVoltsPerG, Acceleration.UnitType.Gravity),
                    new Acceleration((y.Volts - ZeroGVoltage) / YVoltsPerG, Acceleration.UnitType.Gravity),
                    new Acceleration((z.Volts - ZeroGVoltage) / ZVoltsPerG, Acceleration.UnitType.Gravity)
                    );
            });
        }

        ///// <summary>
        ///// Get the raw analog input values from the sensor.
        ///// </summary>
        ///// <returns>Vector object containing the raw sensor data from the analog pins.</returns>
        //public async Task<(Voltage XVolts, Voltage YVolts, Voltage ZVolts)> GetRawSensorData()
        //{
        //    var x = await _xPort.Read();
        //    var y = await _yPort.Read();
        //    var z = await _zPort.Read();

        //    return (x, y, z);
        //}
    }
}