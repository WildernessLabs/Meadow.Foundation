using System;
using System.Threading;
using System.Threading.Tasks;
using Meadow.Hardware;
using Meadow.Peripherals.Sensors;
using Meadow.Peripherals.Sensors.Environmental;
using Meadow.Units;

namespace Meadow.Foundation.Sensors.Environmental
{
    /// <summary>
    /// Represnts an ENS160 Digital Metal-Oxide Multi-Gas Sensor
    /// </summary>
    public partial class Ens160 : ByteCommsSensorBase<Concentration>, IConcentrationSensor
    {
        /// <summary>
        /// Raised when the concentration changes
        /// </summary>
        public event EventHandler<IChangeResult<Concentration>> ConcentrationUpdated = delegate { };

        /// <summary>
        /// The current C02 concentration value
        /// </summary>
        public Concentration? Concentration => Concentration;

        /// <summary>
        /// Create a new ENS160 object
        /// </summary>
        /// <remarks>
        /// The constructor sends the stop periodic updates method otherwise 
        /// the sensor may not respond to new commands
        /// </remarks>
        /// <param name="i2cBus">The I2C bus</param>
        /// <param name="address">The I2C address</param>
        public Ens160(II2cBus i2cBus, byte address = (byte)Addresses.Default)
            : base(i2cBus, address, readBufferSize: 9, writeBufferSize: 9)
        {
            //perfect 96 & 1
            Console.WriteLine(Peripheral.ReadRegister(0x00)); 
            Console.WriteLine(Peripheral.ReadRegister(0x01));
        }
        
        /// <summary>
        /// Set the sensor operating mode
        /// </summary>
        public void SetOperatingMode()
        {

        }

        /// <summary>
        /// Starts updating the sensor on the updateInterval frequency specified
        /// </summary>
        public override void StartUpdating(TimeSpan? updateInterval = null)
        {
         
            base.StartUpdating(updateInterval);
        }

        /// <summary>
        /// Stop updating the sensor
        /// The sensor will not respond to commands for 500ms 
        /// The call will delay the calling thread for 500ms
        /// </summary>
        public override void StopUpdating()
        {
         
            base.StopUpdating();
        }

        /// <summary>
        /// Get Scdx40 C02 Gas Concentration and
        /// Update the Concentration property
        /// </summary>
        protected override async Task<Concentration> ReadSensor()
        {
            return await Task.Run(() =>
            { 
                Concentration conditions = new Units.Concentration(0);

                return conditions;
            });
        }

        /// <summary>
        /// Raise change events for subscribers
        /// </summary>
        /// <param name="changeResult">The change result with the current sensor data</param>
        protected void RaiseChangedAndNotify(IChangeResult<Concentration> changeResult)
        {
          //  ConcentrationUpdated?.Invoke(this, new ChangeResult<Concentration>(Concentration, changeResult.Old?.Concentration));
            base.RaiseEventsAndNotify(changeResult);
        }
    }
}