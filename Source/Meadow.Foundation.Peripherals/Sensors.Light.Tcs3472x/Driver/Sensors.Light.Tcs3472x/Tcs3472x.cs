using System;
using System.Buffers.Binary;
using System.Threading;
using System.Threading.Tasks;
using Meadow.Hardware;

namespace Meadow.Foundation.Sensors.Light
{
    /// <summary>
    ///     Driver for the Tcs3472x light-to-digital converter.
    /// </summary>
    public partial class Tcs3472x
    {
        I2cPeripheral i2CPeripheral;

        private byte integrationTimeByte;
        private double integrationTime;
        private bool isLongTime;
        private GainType gain;

        /// <summary>
        /// Set/Get the time to wait for the sensor to read the data
        /// Minimum time is 0.0024 s
        /// Maximum time is 7.4 s
        /// Be aware that it is not a linear function
        /// </summary>
        public double IntegrationTime
        {
            get => integrationTime;
            set
            {
                integrationTime = value;
                SetIntegrationTime(integrationTime);
            }
        }

        /// <summary>
        /// Set/Get the gain
        /// </summary>
        public GainType Gain
        {
            get => gain;
            set
            {
                gain = value;
                i2CPeripheral.WriteRegister((byte)Registers.CONTROL, (byte)gain);
            }
        }

        /// <summary>
        /// Get the type of sensor
        /// </summary>
        public DeviceType Device { get; internal set; }

        /// <summary>
        ///     Create a new instance of the Tcs3472x class with the specified I2C address.
        /// </summary>
        /// <remarks>
        ///     By default the sensor will be set to low gain.
        /// <remarks>
        /// <param name="i2cBus">I2C bus.</param>
        public Tcs3472x(II2cBus i2cBus, byte address = 0x29, double integrationTime = 0.700, GainType gain = GainType.Gain60X)
        {
            i2CPeripheral = new I2cPeripheral(i2cBus, address);

            //detect device type
            Device = (DeviceType)i2CPeripheral.ReadRegister((byte)(Registers.COMMAND_BIT | Registers.ID));

            Console.WriteLine($"Device: {Device}");
            
            isLongTime = false;
            IntegrationTime = Math.Clamp(integrationTime, 0.0024, 0.7);

            Console.WriteLine($"Integration time: {IntegrationTime}");

            SetIntegrationTime(integrationTime);
            Gain = gain;
            PowerOn();
        }

        /// <summary>
        /// Get true is there are valid data
        /// </summary>
        public bool IsValidData
        {
            get
            {
                var status = i2CPeripheral.ReadRegister((byte)(Registers.COMMAND_BIT | Registers.STATUS));
                return ((Registers)(status & (byte)Registers.STATUS_AVALID) == Registers.STATUS_AVALID);
            }
        }

        /// <summary>
        /// Get true if RGBC is clear channel interrupt
        /// </summary>
        public bool IsClearInterrupt
        {
            get
            {
                var status = i2CPeripheral.ReadRegister((byte)(Registers.COMMAND_BIT | Registers.STATUS));
                return ((Registers)(status & (byte)Registers.STATUS_AINT) == Registers.STATUS_AINT);
            }
        }

        /// <summary>
        /// Set the integration (sampling) time for the sensor
        /// </summary>
        /// <param name="timeSeconds">Time in seconds for each sample. 0.0024 second(2.4ms) increments.Clipped to the range of 0.0024 to 0.6144 seconds.</param>
        private void SetIntegrationTime(double timeSeconds)
        {
            if (timeSeconds <= 700)
            {
                if (isLongTime)
                {
                    SetConfigLongTime(false);
                }

                isLongTime = false;
                var timeByte = Math.Clamp((int)(0x100 - (timeSeconds / 0.0024)), 0, 255);
                i2CPeripheral.WriteRegister((byte)Registers.ATIME, (byte)timeByte);
                integrationTimeByte = (byte)timeByte;
            }
            else
            {
                if (!isLongTime)
                {
                    SetConfigLongTime(true);
                }

                isLongTime = true;
                var timeByte = (int)(0x100 - (timeSeconds / 0.029));
                timeByte = Math.Clamp(timeByte, 0, 255);
                i2CPeripheral.WriteRegister((byte)Registers.WTIME, (byte)timeByte);
                integrationTimeByte = (byte)timeByte;
            }
        }

        private void SetConfigLongTime(bool setLong)
        {
            i2CPeripheral.WriteRegister((byte)Registers.CONFIG, setLong ? (byte)(Registers.CONFIG_WLONG) : (byte)0x00);
        }

        private void PowerOn()
        {
            i2CPeripheral.WriteRegister((byte)Registers.ENABLE, (byte)Registers.ENABLE_PON);
            Thread.Sleep(3);
            i2CPeripheral.WriteRegister((byte)Registers.ENABLE, (byte)(Registers.ENABLE_PON | Registers.ENABLE_AEN));
        }

        private void PowerOff()
        {
            var powerState = i2CPeripheral.ReadRegister((byte)Registers.ENABLE);
            powerState = (byte)(powerState & ~(byte)(Registers.ENABLE_PON | Registers.ENABLE_AEN));
            i2CPeripheral.WriteRegister((byte)Registers.ENABLE, powerState);
        }

        /// <summary>
        /// Set/Clear the colors and clear interrupts
        /// </summary>
        /// <param name="state">true to set all interrupts, false to clear</param>
        public void SetInterrupt(bool state) =>
            SetInterrupt(InterruptState.All, state);

        /// <summary>
        /// Set/clear a specific interrupt persistence
        /// This is used to have more than 1 cycle before generating an
        /// interruption.
        /// </summary>
        /// <param name="interupt">The percistence cycles</param>
        /// <param name="state">True to set the interrupt, false to clear</param>
        public void SetInterrupt(InterruptState interupt, bool state)
        {
            i2CPeripheral.WriteRegister((byte)Registers.PERS, (byte)interupt);
            var enable = i2CPeripheral.ReadRegister((byte)Registers.ENABLE);

            enable = state
                ? enable |= (byte)Registers.ENABLE_AIEN
                : enable = (byte)(enable & ~(byte)Registers.ENABLE_AIEN);
            i2CPeripheral.WriteRegister((byte)Registers.ENABLE, enable);
        }

        /// <summary>
        /// Get the color
        /// </summary>
        /// <param name="delay">Wait to read the data that the integration time is passed</param>
        /// <returns></returns>
        public async Task<Color> GetColor(bool delay = true)
        {
            // To have a new reading, you need to wait for integration time to happen
            // If you don't wait, then you'll read the previous value
            if (delay)
            {
                await Task.Delay((int)(IntegrationTime * 1000.0));
            }

            var divide = (256 - integrationTimeByte) * 1024.0;

            // If we are in long wait, we'll need to divide even more
            if (isLongTime)
            {
                divide *= 12.0;
            }

            Console.WriteLine($"Red: {I2cRead16(Registers.RDATAL)}");
            Console.WriteLine($"Green: {I2cRead16(Registers.GDATAL)}");
            Console.WriteLine($"Blue: {I2cRead16(Registers.BDATAL)}");

            double r = (I2cRead16(Registers.RDATAL) / divide);
            double g = (I2cRead16(Registers.GDATAL) / divide);
            double b = (I2cRead16(Registers.BDATAL) / divide);
            double a = (I2cRead16(Registers.CDATAL) / divide);

            return Color.FromRgba(r, g, b, a);
        }

        private ushort I2cRead16(Registers reg)
        {
            return i2CPeripheral.ReadUShort((byte)(Registers.COMMAND_BIT | reg), ByteOrder.BigEndian);
        }
    }
}