using Meadow.Hardware;
using Meadow.Peripherals.Sensors;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Meadow.Foundation.Sensors.Hid
{
    public abstract class WiiExtensionBase: ISensor
    {
        protected readonly II2cPeripheral i2cPeripheral;

        protected readonly byte[] readBuffer = new byte[6];

        /// <summary>
        /// Lock for sampling
        /// </summary>
        protected object samplingLock = new object();

        /// <summary>
        /// Sampling cancellation token source
        /// </summary>
        protected CancellationTokenSource? SamplingTokenSource { get; set; }


        public bool IsSampling { get; protected set; } = false;

        public WiiExtensionBase(II2cBus i2cBus, byte address)
        {
            i2cPeripheral = new I2cPeripheral(i2cBus, address);

            Initialize();
        }

        public virtual void Update()
        {
            i2cPeripheral.WriteRegister(0, 0);

            i2cPeripheral.Read(readBuffer);
        }

        public void StartUpdating(TimeSpan? updateInterval)
        {
            // thread safety
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
        /// Stops sampling the joystick position.
        /// </summary>
        public void StopUpdating()
        {
            lock (samplingLock)
            {
                if (!IsSampling) return;

                SamplingTokenSource?.Cancel();

                // state machine
                IsSampling = false;
            }
        }

        protected virtual void Initialize()
        {
            i2cPeripheral.WriteRegister(0xF0, 0x55);
            i2cPeripheral.WriteRegister(0xFB, 0x00);
            Thread.Sleep(100);
        }
    }
}