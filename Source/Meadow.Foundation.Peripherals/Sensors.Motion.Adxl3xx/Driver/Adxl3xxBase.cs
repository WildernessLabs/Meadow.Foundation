using System;
using System.Threading;
using System.Threading.Tasks;
using Meadow.Foundation.Spatial;
using Meadow.Hardware;
using Meadow.Peripherals.Sensors;
using Meadow.Peripherals.Sensors.Motion;
using Meadow.Units;

namespace Meadow.Foundation.Sensors.Motion
{
    /// <summary>
    /// Base class for ADXL335, ADXL337, and ADXL377 triple axis accelerometers
    /// </summary>
    public abstract class Adxl3xxBase : SamplingSensorBase<Acceleration3D>, IAccelerometer
    {
        /// <summary>
        /// Raised when the acceleration value changes
        /// </summary>
        public event EventHandler<IChangeResult<Acceleration3D>> Acceleration3DUpdated = delegate { };
        
        /// <summary>
        /// The X analog input port
        /// </summary>
        protected IAnalogInputPort XAnalogIn { get; }

        /// <summary>
        /// The Y analog input port
        /// </summary>
        protected IAnalogInputPort YAnalogIn { get; }

        /// <summary>
        /// The Z analog input port
        /// </summary>
        protected IAnalogInputPort ZAnalogIn { get; }
        
        /// <summary>
        /// Power supply voltage applied to the sensor - this will be set (in the constructor)
        /// to 3.3V by default
        /// </summary>
        protected Voltage SupplyVoltage { get; } = new Voltage(3.3, Voltage.UnitType.Volts);

        /// <summary>
        /// Gravity range
        /// </summary>
        protected double GravityRange { get; }

        /// <summary>
        /// The current acceration value
        /// </summary>
        public Acceleration3D? Acceleration3D => Conditions;

        /// <summary>
        /// Create a new Adxl3xxBase sensor object
        /// </summary>
        /// <param name="device">The device connected to the sensor</param>
        /// <param name="xPin">Analog pin connected to the X axis output from the ADXL335 sensor.</param>
        /// <param name="yPin">Analog pin connected to the Y axis output from the ADXL335 sensor.</param>
        /// <param name="zPin">Analog pin connected to the Z axis output from the ADXL335 sensor.</param>
        /// <param name="gravityRange">The gravity rangy</param>
        /// <param name="supplyVoltage">The supply voltage (typically 3.3V)</param>
        protected Adxl3xxBase(IAnalogInputController device,
            IPin xPin, IPin yPin, IPin zPin,
            int gravityRange, Voltage? supplyVoltage)
        {
            XAnalogIn = device.CreateAnalogInputPort(xPin, 5, TimeSpan.FromMilliseconds(40), supplyVoltage ?? new Voltage(3.3));
            YAnalogIn = device.CreateAnalogInputPort(yPin, 5, TimeSpan.FromMilliseconds(40), supplyVoltage ?? new Voltage(3.3));
            ZAnalogIn = device.CreateAnalogInputPort(zPin, 5, TimeSpan.FromMilliseconds(40), supplyVoltage ?? new Voltage(3.3));
            GravityRange = gravityRange;

            if(supplyVoltage is { } supplyV) 
            { 
                SupplyVoltage = supplyV; 
            }
        }

        /// <summary>
        /// Raise events for subcribers and notify of value changes
        /// </summary>
        /// <param name="changeResult">The updated sensor data</param>
        protected override void RaiseEventsAndNotify(IChangeResult<Acceleration3D> changeResult)
        {
            Acceleration3DUpdated?.Invoke(this, changeResult);
            base.RaiseEventsAndNotify(changeResult);
        }

        /// <summary>
        /// Reads data from the sensor
        /// </summary>
        /// <returns>The latest sensor reading</returns>
        protected override Task<Acceleration3D> ReadSensor()
        {
            return Task.Run(async () => {
                var x = await XAnalogIn.Read();
                var y = await YAnalogIn.Read();
                var z = await ZAnalogIn.Read();

                return new Acceleration3D(VoltageToGravity(x), VoltageToGravity(y), VoltageToGravity(z));
            });
        }

        /// <summary>
        /// Convert voltage to gravity
        /// </summary>
        /// <param name="voltage">The voltage to convert</param>
        /// <returns>Acceleration value</returns>
        protected Acceleration VoltageToGravity(Voltage voltage)
        {
            return new Acceleration((voltage.Volts - (SupplyVoltage.Volts / 2)) / (SupplyVoltage.Volts / GravityRange), Acceleration.UnitType.Gravity);
        }
    }
}