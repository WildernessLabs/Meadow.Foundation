using Meadow.Hardware;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Meadow.Foundation.Sensors.Power
{
    public class Ina260 : IDisposable
    {
        /// <summary>
        ///     Valid addresses for the sensor.
        /// </summary>
        public enum Addresses : byte
        {
            Address0 = 0x40,
            Address1 = 0x41,
            Default = Address0
        }

        private enum Register : byte
        {
            Config = 0x00,
            Current = 0x01,
            Voltage = 0x02,
            Power = 0x03,
            MaskEnable = 0x06,
            AlertLimit = 0x07,
            ManufacturerID = 0xFE,
            DieID = 0xFF
        }

        public delegate void ValueChangedHandler(float previousValue, float newValue);

        public event ValueChangedHandler VoltageChanged;
        public event ValueChangedHandler CurrentChanged;
        public event ValueChangedHandler PowerChanged;

        private const float MeasurementScale = 0.00125f;
        private float _voltage;
        private float _current;
        private float _power;
        private float? _lastVoltage;
        private float? _lastCurrent;
        private float? _lastPower;
        private TimeSpan _samplePeriod;

        private II2cBus Bus { get; set; }
        private Register RegisterPointer { get; set; }
        private object SyncRoot { get; } = new object();
        private CancellationTokenSource SamplingTokenSource { get; set; }

        public float VoltageChangeThreshold { get; set; }
        public float CurrentChangeThreshold { get; set; }
        public float PowerChangeThreshold { get; set; }
        public bool IsSampling { get; private set; }
        public byte Address { get; private set; }

        public Ina260(II2cBus i2cBus, byte address = (byte)Addresses.Default)
        {
            if (i2cBus == null) throw new ArgumentNullException(nameof(i2cBus));

            switch (address)
            {
                case 0x40:
                case 0x41:
                    // valid;
                    break;
                default:
                    throw new ArgumentOutOfRangeException("INA200 device address must be either 0x40 or 0x41");
            }

            Bus = i2cBus;
            Address = address;
        }

        public Ina260(II2cBus i2cBus, Addresses address)
            : this(i2cBus, (byte)address)
        {
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
                        if (VoltageChanged != null)
                        {
                            RefreshVoltage(true);
                        }
                        if (CurrentChanged != null)
                        {
                            RefreshCurrent(true);
                        }
                        if (PowerChanged != null)
                        {
                            RefreshPower(true);
                        }

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

        /// <summary>
        /// Reads the unique manufacturer identification number
        /// </summary>
        public int ManufacturerID
        {
            get => ReadRegisterUInt16(Register.ManufacturerID);
        }

        /// <summary>
        /// Reads the unique die identification number
        /// </summary>
        public int DieID
        {
            get => ReadRegisterUInt16(Register.ManufacturerID);
        }

        /// <summary>
        /// Reads the value of the current (in Amps) flowing through the shunt resistor
        /// </summary>
        public float Current
        {
            private set => _current = value;
            get
            {
                if (!IsSampling)
                {
                    RefreshCurrent();
                }
                return _current;
            }
        }

        private void RefreshCurrent(bool raiseEvents = false)
        {
            Current = ReadRegisterInt16(Register.Current) * MeasurementScale;

            if (raiseEvents)
            {
                if (!_lastCurrent.HasValue)
                {
                    // raise event
                    CurrentChanged?.Invoke(0f, Current);

                    _lastCurrent = Current;
                }
                else
                {
                    var delta = Math.Abs(Current - _lastCurrent.Value);
                    if (delta > CurrentChangeThreshold)
                    {
                        // raise event
                        CurrentChanged?.Invoke(_lastCurrent.Value, Current);

                        _lastCurrent = Current;
                    }
                }
            }
        }

        /// <summary>
        /// Reads bus voltage measurement (in Volts) data
        /// </summary>
        public float Voltage
        {
            private set => _voltage = value;
            get
            {
                if (!IsSampling)
                {
                    RefreshVoltage();
                }
                return _voltage;
            }
        }

        private void RefreshVoltage(bool raiseEvents = false)
        {
            Voltage = ReadRegisterUInt16(Register.Voltage) * MeasurementScale;

            if (raiseEvents)
            {
                if (!_lastVoltage.HasValue)
                {
                    // raise event
                    VoltageChanged?.Invoke(0f, Voltage);

                    _lastVoltage = Voltage;
                }
                else
                {
                    var delta = Math.Abs(Voltage - _lastVoltage.Value);
                    if (delta > VoltageChangeThreshold)
                    {
                        // raise event
                        VoltageChanged?.Invoke(_lastVoltage.Value, Voltage);

                        _lastVoltage = Voltage;
                    }
                }
            }
        }

        /// <summary>
        /// Reads the value of the calculated power being delivered to the load
        /// </summary>
        public float Power
        {
            private set => _power = value;
            get
            {
                if (!IsSampling)
                {
                    RefreshPower();
                }
                return _power;
            }
        }

        private void RefreshPower(bool raiseEvents = false)
        {
            Power = ReadRegisterUInt16(Register.Power) * 0.01f;

            if (raiseEvents)
            {
                if (!_lastPower.HasValue)
                {
                    // raise event
                    PowerChanged?.Invoke(0f, Power);

                    _lastPower = Power;
                }
                else
                {
                    var delta = Math.Abs(Power - _lastPower.Value);
                    if (delta > PowerChangeThreshold)
                    {
                        // raise event
                        PowerChanged?.Invoke(_lastPower.Value, Power);

                        _lastPower = Power;
                    }
                }
            }
        }

        private ushort ReadRegisterUInt16(Register register)
        {
            lock (SyncRoot)
            {
                if (register != RegisterPointer)
                {
                    // write the pointer
                    Bus.WriteData(Address, (byte)register);
                    RegisterPointer = register;
                }

                var buffer = Bus.ReadData(Address, 2);

                unchecked
                {
                    return (ushort)((buffer[0] << 8) | buffer[1]);
                }
            }
        }

        private short ReadRegisterInt16(Register register)
        {
            lock (SyncRoot)
            {
                if (register != RegisterPointer)
                {
                    // write the pointer
                    Bus.WriteData(Address, (byte)register);
                    RegisterPointer = register;

                    RegisterPointer = register;
                }

                var buffer = Bus.ReadData(Address, 2);

                unchecked
                {
                    return (short)((buffer[0] << 8) | buffer[1]);
                }
            }
        }
    }
}
