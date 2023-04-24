using Meadow.Hardware;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Meadow.Foundation.Sensors.Hid
{
    /// <summary>
    /// Abstract base class that represents 
    /// Nintendo Wiimote I2C extension controllers 
    /// </summary>
    public abstract partial class WiiExtensionControllerBase
    {
        /// <summary>
        /// Default I2C bus speed (400kHz)
        /// </summary>
        public static I2cBusSpeed DefaultSpeed => I2cBusSpeed.Fast;

        /// <summary>
        /// I2C Communication bus used to communicate with the peripheral
        /// </summary>
        protected readonly II2cCommunications i2cComms;

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
        protected CancellationTokenSource SamplingTokenSource { get; set; }

        /// <summary>
        /// Are we actively reading data from the extension controller
        /// </summary>
        public bool IsSampling { get; protected set; } = false;

        /// <summary>
        /// Base constructor
        /// </summary>
        /// <param name="i2cBus">the I2C bus connected to the extension controller</param>
        /// <param name="address">The extension controller address</param>
        public WiiExtensionControllerBase(II2cBus i2cBus, byte address)
        {
            i2cComms = new I2cCommunications(i2cBus, address);

            Initialize();
        }

        /// <summary>
        /// Initialize the extension controller
        /// </summary>
        protected virtual void Initialize()
        {
            i2cComms.WriteRegister(0xF0, 0x55);
            i2cComms.WriteRegister(0xFB, 0x00);
            Thread.Sleep(100);
        }

        /// <summary>
        /// Get the latest sensor data from the device
        /// </summary>
        public virtual void Update()
        {
            i2cComms.Write(0);

            i2cComms.Read(readBuffer[..6]);
        }

        /// <summary>
        /// Gets the device ID
        /// </summary>
        /// <returns>The ID as a byte</returns>
        public byte[] GetIdentification()
        {
            i2cComms.Write(0xFA);
            i2cComms.Read(readBuffer[..6]);

            Resolver.Log.Info(BitConverter.ToString(readBuffer[..6].ToArray()));

            return readBuffer[..6].ToArray();
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

                var t = new Task(() =>
                {
                    while (ct.IsCancellationRequested == false)
                    {
                        Update();
                        Thread.Sleep(updateInterval.Value);
                    }
                }, ct, TaskCreationOptions.LongRunning);
                t.Start();
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