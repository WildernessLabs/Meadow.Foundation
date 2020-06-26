using System;
using System.Threading;
using System.Threading.Tasks;
using Meadow.Hardware;

namespace Meadow.Foundation.Sensors.Light
{
    /// <summary>
    ///     Driver for the TSL2591 light-to-digital converter.
    /// </summary>
    public class Tsl2591 : IDisposable
    {
        /// <summary>
        ///     Valid addresses for the sensor.
        /// </summary>
        public enum Addresses : byte
        {
            Address0 = 0x29,
            Default = Address0
        }

        [Flags]
        private enum Register : byte
        {
            Command = 0xA0,
            Enable = 0x00,
            Config = 0x01,
            ALSInterruptLowL = 0x04,
            ALSInterruptLowH = 0x05,
            ALSInterruptHighL = 0x06,
            ALSInterruptHighH = 0x07,
            NPAILTL = 0x08,
            NPAILTH = 0x09,
            NPAIHTL = 0x0A,
            NPAIHTH = 0x0B,
            Persist = 0x0C,
            PackageID = 0x11,
            DeviceID = 0x12,
            Status = 0x13,
            CH0DataL = 0x14,
            CH0DataH = 0x15,
            CH1DataL = 0x16,
            CH1DataH = 0x17
        }

        public delegate void ValueChangedHandler(int previousValue, int newValue);

        public event ValueChangedHandler Channel0Changed;
        public event ValueChangedHandler Channel1Changed;

        private int _ch0;
        private int _ch1;
        private int? _lastCh0;
        private int? _lastCh1;
        private TimeSpan _samplePeriod;

        private II2cBus i2cBus { get; set; }
        private object SyncRoot { get; } = new object();
        private CancellationTokenSource SamplingTokenSource { get; set; }

        public int ChangeThreshold { get; set; }

        public bool IsSampling { get; private set; }
        public byte Address { get; private set; }

        public Tsl2591(II2cBus bus, byte address = (byte)Addresses.Default)
        {
            i2cBus = bus;
            Address = address;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                StopSampling();
            }
        }

        /// <summary>
        /// Dispose managed resources
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
        }

        public void StartSampling(TimeSpan samplePeriod)
        {
            lock (SyncRoot)
            {
                // allow subsequent calls to StartSampling to just change the sample period
                _samplePeriod = samplePeriod;

                if (IsSampling)
                {
                    return;
                }

                SamplingTokenSource = new CancellationTokenSource();
                var ct = SamplingTokenSource.Token;

                Task.Factory.StartNew(async () =>
                {
                    IsSampling = true;

                    while (true)
                    {
                        // check for stop
                        if (ct.IsCancellationRequested)
                        {
                            IsSampling = false;
                            break;
                        }

                        // do reads
                        RefreshChannels(true);

                        await Task.Delay(_samplePeriod);
                    }

                    IsSampling = false;
                });
            }
        }

        public void StopSampling()
        {
            lock (SyncRoot)
            {
                if (!IsSampling) return;
                SamplingTokenSource.Cancel();
            }
        }

        public void PowerOn()
        {
            WriteRegister(Register.Enable | Register.Command, 3);
        }

        public void PowerOff()
        {
            WriteRegister(Register.Enable | Register.Command, 0);
        }

        public int PackageID
        {
            get => ReadRegisterByte(Register.PackageID | Register.Command);
        }

        public int DeviceID
        {
            get => ReadRegisterByte(Register.DeviceID | Register.Command);
        }

        /// <summary>
        /// Reads the value of ADC Channel 0
        /// </summary>
        public int Channel0
        {
            private set => _ch0 = value;
            get
            {
                if (!IsSampling)
                {
                    RefreshChannels();
                }
                return _ch0;
            }
        }

        /// <summary>
        /// Reads the value of ADC Channel 1
        /// </summary>
        public int Channel1
        {
            private set => _ch1 = value;
            get
            {
                if (!IsSampling)
                {
                    RefreshChannels();
                }
                return _ch1;
            }
        }

        private void RefreshChannels(bool raiseEvents = false)
        {
            // data sheet indicates you should always read all 4 bytes, in order, for valid data
            Channel0 = ReadRegisterUInt16(Register.CH0DataL | Register.Command);
            Channel1 = ReadRegisterUInt16(Register.CH1DataL | Register.Command);

            if (raiseEvents)
            {
                if (!_lastCh0.HasValue)
                {
                    // raise event
                    Channel0Changed?.Invoke(0, Channel0);

                    _lastCh0 = Channel0;
                }
                else
                {
                    var delta = Math.Abs(Channel0 - _lastCh0.Value);
                    if (delta > ChangeThreshold)
                    {
                        // raise event
                        Channel0Changed?.Invoke(_lastCh0.Value, Channel0);

                        _lastCh0 = Channel0;
                    }
                }

                if (!_lastCh1.HasValue)
                {
                    // raise event
                    Channel1Changed?.Invoke(1, Channel1);

                    _lastCh1 = Channel1;
                }
                else
                {
                    var delta = Math.Abs(Channel1 - _lastCh1.Value);
                    if (delta > ChangeThreshold)
                    {
                        // raise event
                        Channel1Changed?.Invoke(_lastCh1.Value, Channel1);

                        _lastCh1 = Channel1;
                    }
                }
            }
        }

        private void WriteRegister(Register register, byte value)
        {
            lock (SyncRoot)
            {
                i2cBus.WriteData(Address, 2, (byte)register, value);
            }
        }

        private byte ReadRegisterByte(Register register)
        {
            lock (SyncRoot)
            {
                var data = i2cBus.WriteReadData(Address, 1, (byte)register);

                return data[0];
            }
        }

        private ushort ReadRegisterUInt16(Register register)
        {
            lock (SyncRoot)
            {
                var data = i2cBus.WriteReadData(Address, 2, (byte)register);

                unchecked
                {
                    return (ushort)((data[0] << 8) | data[1]);
                }
            }
        }
    }
}