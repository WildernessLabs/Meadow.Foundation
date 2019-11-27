using Meadow.Hardware;

namespace Meadow.Foundation.Sensors.Motion
{
    public class Mpu6050
    {
        private enum Registers : byte
        {
            PowerManagement = 0x6b,
            AccelerometerX = 0x3b,
            AccelerometerY = 0x3d,
            AccelerometerZ = 0x3f,
            Temperature = 0x41,
            GyroX = 0x43,
            GyroY = 0x45,
            GyroZ = 0x47
        }

        private II2cBus _bus;

        public byte Address { get; }

        public Mpu6050(II2cBus bus, byte address = 0x68)
        {
            Address = address;
            _bus = bus;
        }

        public void Wake()
        {
            _bus.WriteData(Address, (byte)Registers.PowerManagement, 0);
        }

        public void Refresh()
        {
            // tell it to send us 14 bytes (each value is 2-bytes), starting at 0x3b
            var data = _bus.WriteReadData(Address, 14, (byte)Registers.AccelerometerX);

            AccelerationX = data[0] << 8 | data[1];
            AccelerationY = data[2] << 8 | data[3];
            AccelerationZ = data[4] << 8 | data[5];
            Temperature = data[6] << 8 | data[7];
            GyroX = data[8] << 8 | data[9];
            GyroY = data[10] << 8 | data[11];
            GyroZ = data[12] << 8 | data[13];
        }

        public int AccelerationX { get; private set; }
        public int AccelerationY { get; private set; }
        public int AccelerationZ { get; private set; }
        public int Temperature { get; private set; }
        public int GyroX { get; private set; }
        public int GyroY { get; private set; }
        public int GyroZ { get; private set; }
    }
}
