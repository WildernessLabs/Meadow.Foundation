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

        [Flags]
        public enum IntegrationTimes : byte
        {
            Time_100Ms = 0x00, // 100 milliseconds
            Time_200Ms = 0x01, // 200 milliseconds
            Time_300Ms = 0x02, // 300 milliseconds
            Time_400Ms = 0x03, // 400 milliseconds
            Time_500Ms = 0x04, // 500 milliseconds
            Time_600Ms = 0x05  // 600 milliseconds
        }

        [Flags]
        public enum GainFactor : byte
        {
            Low = 0x00,     /// Low gain (1x)
            Medium = 0x10,  /// Medium gain (25x)
            High = 0x20,    /// High gain (428x)
            Maximum = 0x30  /// Maximum gain (9876x)
        }

        [Flags]
        public enum EnableStates : byte
        {
            PowerOff = 0x00,
            PowerOn = 0x01,
            Aen = 0x02,
            Aien = 0x10,
            Npien = 0x80
        }

        public delegate void ValueChangedHandler(int previousValue, int newValue);

        public event ValueChangedHandler Channel0Changed;
        public event ValueChangedHandler Channel1Changed;

        private int _ch0;
        private int _ch1;
        private int? _lastCh0;
        private int? _lastCh1;
        private TimeSpan _samplePeriod;
        private IntegrationTimes _integrationTime;
        private GainFactor _gain;

        private II2cBus i2cBus { get; set; }
        private object SyncRoot { get; } = new object();
        private CancellationTokenSource SamplingTokenSource { get; set; }

        public int ChangeThreshold { get; set; }

        public bool IsSampling { get; private set; }
        public byte Address { get; private set; }

        public Tsl2591(II2cBus bus, byte address = (byte) Addresses.Default)
        {
            i2cBus = bus;
            Address = address;
            Gain = GainFactor.Medium;
            IntegrationTime = IntegrationTimes.Time_100Ms;
            PowerOn();
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

        /// <summary>
        /// Start sampling from the sensor.
        /// </summary>
        /// <remarks>
        /// If the requested sampling period is less than the minimum sampling period as defined by the
        /// <seealso cref="IntegrationPeriod"/> then the sampling period will be set to a period based
        /// upon the <seealso cref="IntegrationPeriod"/>.
        /// </remarks>
        /// <param name="samplePeriod">Requested sampling period.</param>
        public void StartSampling(TimeSpan samplePeriod)
        {
            lock (SyncRoot)
            {
                TimeSpan minimumSamplePeriod = TimeSpan.FromMilliseconds(IntegrationTimeInMilliseconds(IntegrationTime) + 20);
                if (_samplePeriod < minimumSamplePeriod)
                {
                    _samplePeriod = minimumSamplePeriod;
                }
                else
                {
                    // allow subsequent calls to StartSampling to just change the sample period
                    _samplePeriod = samplePeriod;
                }

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
        /// Gain of the sensor.
        /// </summary>
        public GainFactor Gain
        {
            get { return (_gain); }
            set
            {
                PowerOff();
                _gain = value;
                WriteRegister(Register.Command | Register.Config, (byte) ((byte) _integrationTime | (byte) _gain));
                PowerOn();
            }
        }

        /// <summary>
        /// Integration time for the sensor.
        /// </summary>
        public IntegrationTimes IntegrationTime
        {
            get { return (_integrationTime); }
            set
            {
                PowerOff();
                _integrationTime = value;
                WriteRegister(Register.Command | Register.Config, (byte) ((byte) _integrationTime | (byte) _gain));
                PowerOn();
            }
        }

        /// <summary>
        /// Full spectrum luminosity (visible and infrared light combined).
        /// </summary>
        public int FullSpectrumLuminosity { get; private set; }

        /// <summary>
        /// Infrared light luminosity.
        /// </summary>
        public int InfraredLuminosity { get; private set; }

        /// <summary>
        /// Visible light luminosity.
        /// </summary>
        public int VisibleLightLuminosity { get; private set; }

        /// <summary>
        /// Visible lux.
        /// </summary>
        /// <remarks>
        /// A Lux value of -1 indicates that no reading has been made or that the sensor is overloaded.
        /// </remarks>
        public double Lux { get; private set; }

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

        /// <summary>
        /// Convert the integration time into milliseconds.
        /// </summary>
        /// <param name="integrationTime">Integration time</param>
        /// <returns>Integration time in milliseconds.</returns>
        private double IntegrationTimeInMilliseconds(IntegrationTimes integrationTime)
        {
            double it = 100;            // Default value, 100ms.
            switch (IntegrationTime)
            {
                case IntegrationTimes.Time_100Ms:
                    it = 100;
                    break;
                case IntegrationTimes.Time_200Ms:
                    it = 200;
                    break;
                case IntegrationTimes.Time_300Ms:
                    it = 300;
                    break;
                case IntegrationTimes.Time_400Ms:
                    it = 400;
                    break;
                case IntegrationTimes.Time_500Ms:
                    it = 500;
                    break;
                case IntegrationTimes.Time_600Ms:
                    it = 600;
                    break;
            }

            return (it);
        }

        /// <summary>
        /// Multiplication factor for the sensor reading.
        /// </summary>
        /// <param name="gain">Gain level to be translated.</param>
        /// <returns>Multiplication factor for the specified gain.</returns>
        private double GainMultiplier(GainFactor gain)
        {
            double g = 1.0;             // Default gain = 1.
            switch (gain)
            {
                case GainFactor.Low:
                    g = 1;
                    break;
                case GainFactor.Medium:
                    g = 25;
                    break;
                case GainFactor.High:
                    g = 428;
                    break;
                case GainFactor.Maximum:
                    g = 9876;
                    break;

            }
            return (g);
        }

        private void CalculateSensorValues()
        {
            FullSpectrumLuminosity = Channel0;
            InfraredLuminosity = Channel1;
            VisibleLightLuminosity = Channel0 - Channel1;

            double countsPerLux;

            if ((Channel0 == 0xffff) || (Channel1 == 0xffff))
            {
                Lux = -1;
                return;
            }
            countsPerLux = (IntegrationTimeInMilliseconds(IntegrationTime) * GainMultiplier(Gain)) / 408.0;
            Lux = (Channel0 - Channel1) * (1 - (Channel1 / Channel0)) / countsPerLux;
        }

        public void RefreshChannels(bool raiseEvents = false)
        {
            // data sheet indicates you should always read all 4 bytes, in order, for valid data
            Channel0 = ReadRegisterUInt16(Register.CH0DataL | Register.Command);
            Channel1 = ReadRegisterUInt16(Register.CH1DataL | Register.Command);
            CalculateSensorValues();

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
                i2cBus.WriteData(Address, 2, (byte) register, value);
            }
        }

        private byte ReadRegisterByte(Register register)
        {
            lock (SyncRoot)
            {
                var data = i2cBus.WriteReadData(Address, 1, (byte) register);

                return data[0];
            }
        }

        private ushort ReadRegisterUInt16(Register register)
        {
            lock (SyncRoot)
            {
                var data = i2cBus.WriteReadData(Address, 2, (byte) register);

                unchecked
                {
                    return (ushort)((data[0] << 8) | data[1]);
                }
            }
        }
    }
}