using Meadow.Hardware;

namespace Meadow.Foundation.Sensors.Motion
{
    public class Mpu6050
    {
        /// <summary>
        ///     Valid addresses for the sensor.
        /// </summary>
        public enum Addresses : byte
        {
            Address0 = 0x68,
            Address1 = 0x69,
            Default = Address0
        }

        private enum Register : byte
        {
            Config = 0x1a,
            GyroConfig = 0x1b,
            AccelConfig = 0x1c,
            InterruptConfig = 0x37,
            InterruptEnable = 0x38,
            InterruptStatus = 0x3a,
            PowerManagement = 0x6b,
            AccelerometerX = 0x3b,
            AccelerometerY = 0x3d,
            AccelerometerZ = 0x3f,
            Temperature = 0x41,
            GyroX = 0x43,
            GyroY = 0x45,
            GyroZ = 0x47
        }

        private const float GyroScaleBase = 131f;
        private const float AccelScaleBase = 16384f;

        private int GyroScale { get; set; }
        private int AccelerometerScale { get; set; }
        private object SyncRoot { get; } = new object();
        public byte Address { get; }
        private II2cBus Bus { get; set; }
        /// <summary>
        /// Accelerometer X measurement, in g
        /// </summary>
        public float AccelerationX { get; private set; }
        /// <summary>
        /// Accelerometer Y measurement, in g
        /// </summary>
        public float AccelerationY { get; private set; }
        /// <summary>
        /// Accelerometer Z measurement, in g
        /// </summary>
        public float AccelerationZ { get; private set; }
        public float TemperatureC { get; private set; }
        /// <summary>
        /// Gyroscope X measurement, in degrees per second
        /// </summary>
        public float GyroX { get; private set; }
        /// <summary>
        /// Gyroscope Y measurement, in degrees per second
        /// </summary>
        public float GyroY { get; private set; }
        /// <summary>
        /// Gyroscope Z measurement, in degrees per second
        /// </summary>
        public float GyroZ { get; private set; }

        public Mpu6050(II2cBus bus, byte address = 0x68)
        {
            Address = address;
            Bus = bus;
        }

        public Mpu6050(II2cBus bus, Addresses address = Addresses.Default)
            : this(bus, (byte)address)
        {
        }

        public void Wake()
        {
            Bus.WriteData(Address, (byte)Register.PowerManagement, 0);
            LoadConfiguration();
        }

        private void LoadConfiguration()
        {
            // read all 3 config bytes
            var data = Bus.WriteReadData(Address, 3, (byte)Register.Config);

            GyroScale = (data[1] & 0b00011000) >> 3;
            AccelerometerScale = (data[2] & 0b00011000) >> 3;
        }

        public void Refresh()
        {
            lock (SyncRoot)
            {
                // tell it to send us 14 bytes (each value is 2-bytes), starting at 0x3b
                var data = Bus.WriteReadData(Address, 14, (byte)Register.AccelerometerX);

                var a_scale = (1 << AccelerometerScale) / AccelScaleBase;
                var g_scale = (1 << GyroScale) / GyroScaleBase;
                AccelerationX = ScaleAndOffset(data, 0, a_scale);
                AccelerationY = ScaleAndOffset(data, 2, a_scale);
                AccelerationZ = ScaleAndOffset(data, 4, a_scale);
                TemperatureC = ScaleAndOffset(data, 6, 1 / 340f, 36.53f);
                GyroX = ScaleAndOffset(data, 8, g_scale);
                GyroY = ScaleAndOffset(data, 10, g_scale);
                GyroZ = ScaleAndOffset(data, 12, g_scale);
            }
        }

        private float ScaleAndOffset(byte[] data, int index, float scale, float offset = 0)
        {
            // convert to a signed number
            unchecked
            {
                var s = (short)(data[index] << 8 | data[index + 1]);
                return (s * scale) + offset;
            }
        }
    }
}
