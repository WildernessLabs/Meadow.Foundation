using Meadow.Hardware;
using Meadow.Peripherals.Sensors;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Meadow.Foundation.Sensors.Hid
{
    /// <summary>
    /// Abstract base class that represents 
    /// Nintendo Wiimote I2C extension controllers 
    /// </summary>
    public abstract partial class WiiExtensionBase : ISensor
    {
        /// <summary>
        /// Default I2C bus speed (400kHz)
        /// </summary>
        public static I2cBusSpeed DefaultSpeed => I2cBusSpeed.Fast;

        /// <summary>
        /// The I2C peripheral object for the extension
        /// </summary>
        protected readonly II2cPeripheral i2cPeripheral;

        /// <summary>
        /// Data buffer returned by the controller
        /// </summary>
        protected Span<byte> readBuffer => _readBuffer;
        byte[] _readBuffer = new byte[8];

        /// <summary>
        /// Lock for sampling
        /// </summary>
        protected object samplingLock = new object();

        /// <summary>
        /// Sampling cancellation token source
        /// </summary>
        protected CancellationTokenSource? SamplingTokenSource { get; set; }

        /// <summary>
        /// Are we actively reading data from the extension controller
        /// </summary>
        public bool IsSampling { get; protected set; } = false;

        /// <summary>
        /// Base ctor constructor
        /// </summary>
        /// <param name="i2cBus">the I2C bus connected to the extension controller</param>
        /// <param name="address">The extension controller address</param>
        public WiiExtensionBase(II2cBus i2cBus, byte address)
        {
            i2cPeripheral = new I2cPeripheral(i2cBus, address);

            Initialize();
        }

        /// <summary>
        /// Initialize the extension controller
        /// </summary>
        protected virtual void Initialize()
        {
            i2cPeripheral.WriteRegister(0xF0, 0x55);
            i2cPeripheral.WriteRegister(0xFB, 0x00);
            Thread.Sleep(100);
        }

        /// <summary>
        /// Get the latest sensor data from the device
        /// </summary>
        public virtual void Update()
        {
            i2cPeripheral.WriteRegister(0, 0);

            i2cPeripheral.Read(readBuffer);
        }

        /// <summary>
        /// Starts continuously sampling the sensor
        /// </summary>
        /// <param name="updateInterval">interval between samples</param>
        public void StartUpdating(TimeSpan? updateInterval)
        {
            lock (samplingLock)
            {
                if (IsSampling) return;
                IsSampling = true;

                SamplingTokenSource = new CancellationTokenSource();
                CancellationToken ct = SamplingTokenSource.Token;

                Task.Run(() =>
                {
                    while(ct.IsCancellationRequested == false)
                    {
                        Update();
                        Thread.Sleep(updateInterval.Value);
                    }
                });
            }
        }

        /// <summary>
        /// Stops sampling the extension controller
        /// </summary>
        public void StopUpdating()
        {
            lock (samplingLock)
            {
                if (!IsSampling) return;

                SamplingTokenSource?.Cancel();

                IsSampling = false;
            }
        }
    }
}