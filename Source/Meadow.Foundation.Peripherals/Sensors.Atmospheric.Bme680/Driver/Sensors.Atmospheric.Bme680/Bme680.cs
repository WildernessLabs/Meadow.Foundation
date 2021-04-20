using System;
using Meadow.Hardware;
using Meadow.Utilities;

namespace Meadow.Foundation.Sensors.Atmospheric
{
    public partial class Bme680 : I2cPeripheral
    {
        private SensorSettings _settings;
        private readonly object _lock;
        private bool _initialized;
        public bool IsSampling { get; private set; }
        

        public Bme680(II2cBus bus, byte address = 0x77, SensorSettings sensorSettings = null) : base(bus, address)
        {
            if (sensorSettings == null)
                sensorSettings = new SensorSettings();
            _settings = sensorSettings;
            _lock = new object();
            _initialized = false;
        }

        private void Initialize()
        {
            lock (_lock)
            {
                if (_initialized)
                    return;
                Console.WriteLine("Initializing Temperature and Pressure");
                // Init the temp and pressure registers
                // Clear the registers so they're in a known state.
                var status = (byte) ((((byte) _settings.TemperatureOversample << 5) & 0xe0) |
                                     (((byte) _settings.PressureOversample << 2) & 0x1c));
                WriteRegister(RegisterAddresses.ControlTemperatureAndPressure, status);

                // Init the humidity registers
                Console.WriteLine("Initializing Humidity");
                status = (byte) ((byte)_settings.HumidityOversample & 0x07);
                WriteRegister(RegisterAddresses.ControlHumidity, status);

                Console.WriteLine("Finished initializing.");
                _initialized = true;
            }
        }

        public void UpdateSensorSettings(SensorSettings sensorSettings)
        {
            _settings = sensorSettings;
            Initialize();
        }

        public SensorReading Read()
        {
            Initialize();
            try
            {
                return SensorReading.CreateFromDevice(this, _settings);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            return null;
        }

        private byte ReadRegister(Register register)
        {
            return ReadRegister(register.Address);
        }

        private byte[] ReadRegisters(Register register)
        {
            return ReadRegisters(register.Address, register.Length);
        }

        private void WriteRegister(Register register, byte data)
        {
            WriteRegister(register.Address, data);
        }
    }
}
