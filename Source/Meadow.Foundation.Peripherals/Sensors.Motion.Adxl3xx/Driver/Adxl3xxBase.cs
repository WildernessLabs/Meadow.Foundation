using Meadow.Hardware;
using Meadow.Peripherals.Sensors.Motion;
using Meadow.Units;
using System;
using System.Threading.Tasks;

namespace Meadow.Foundation.Sensors.Motion
{
    /// <summary>
    /// Base class for ADXL335, ADXL337, and ADXL377 triple axis accelerometers
    /// </summary>
    public abstract class Adxl3xxBase : PollingSensorBase<Acceleration3D>, IAccelerometer, IDisposable
    {
        /// <summary>
        /// Raised when the acceleration value changes
        /// </summary>
        public event EventHandler<IChangeResult<Acceleration3D>> Acceleration3DUpdated = default!;

        /// <summary>
        /// The X analog input port
        /// </summary>
        protected IAnalogInputPort XAnalogInputPort { get; }

        /// <summary>
        /// The Y analog input port
        /// </summary>
        protected IAnalogInputPort YAnalogInputPort { get; }

        /// <summary>
        /// The Z analog input port
        /// </summary>
        protected IAnalogInputPort ZAnalogInputPort { get; }

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
        /// The current acceleration value
        /// </summary>
        public Acceleration3D? Acceleration3D => Conditions;

        /// <summary>
        /// Is the object disposed
        /// </summary>
        public bool IsDisposed { get; private set; }

        /// <summary>
        /// Did we create the port(s) used by the peripheral
        /// </summary>
        readonly bool createdPorts = false;

        /// <summary>
        /// Create a new Adxl3xxBase sensor object
        /// </summary>
        /// <param name="xPin">Analog pin connected to the X axis output from the ADXL335 sensor.</param>
        /// <param name="yPin">Analog pin connected to the Y axis output from the ADXL335 sensor.</param>
        /// <param name="zPin">Analog pin connected to the Z axis output from the ADXL335 sensor.</param>
        /// <param name="gravityRange">The gravity rangy</param>
        /// <param name="supplyVoltage">The supply voltage (typically 3.3V)</param>
        protected Adxl3xxBase(
            IPin xPin, IPin yPin, IPin zPin,
            int gravityRange, Voltage? supplyVoltage)
        {
            createdPorts = true;

            XAnalogInputPort = xPin.CreateAnalogInputPort(5, TimeSpan.FromMilliseconds(40), supplyVoltage ?? new Voltage(3.3));
            YAnalogInputPort = yPin.CreateAnalogInputPort(5, TimeSpan.FromMilliseconds(40), supplyVoltage ?? new Voltage(3.3));
            ZAnalogInputPort = zPin.CreateAnalogInputPort(5, TimeSpan.FromMilliseconds(40), supplyVoltage ?? new Voltage(3.3));
            GravityRange = gravityRange;

            if (supplyVoltage is { } supplyV)
            {
                SupplyVoltage = supplyV;
            }
        }

        /// <summary>
        /// Raise events for subscribers and notify of value changes
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
        protected async override Task<Acceleration3D> ReadSensor()
        {
            var x = await XAnalogInputPort.Read();
            var y = await YAnalogInputPort.Read();
            var z = await ZAnalogInputPort.Read();

            return new Acceleration3D(VoltageToGravity(x), VoltageToGravity(y), VoltageToGravity(z));
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
                if (disposing && createdPorts)
                {
                }

                IsDisposed = true;
            }
        }
    }
}