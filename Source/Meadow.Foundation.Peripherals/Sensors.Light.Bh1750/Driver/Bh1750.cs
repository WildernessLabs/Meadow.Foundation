using Meadow.Hardware;
using Meadow.Peripherals.Sensors.Light;
using Meadow.Units;
using System;
using System.Buffers.Binary;
using System.Threading.Tasks;
using IU = Meadow.Units.Illuminance.UnitType;

namespace Meadow.Foundation.Sensors.Light
{
    /// <summary>
    /// Represents a BH1750 ambient light sensor
    /// </summary>
    public partial class Bh1750 : ByteCommsSensorBase<Illuminance>, ILightSensor, II2cPeripheral
    {
        /// <summary>
        /// BH1750 Light Transmittance (27.20-222.50%)
        /// </summary>
        public double LightTransmittance
        {
            get => lightTransmittance;
            set => SetLightTransmittance(lightTransmittance = value);
        }
        private double lightTransmittance;

        /// <summary>
        /// BH1750 Measuring Mode
        /// </summary>
        public MeasuringModes MeasuringMode { get; set; }

        /// <summary>
        /// The current illuminance read by the sensor
        /// </summary>
        public Illuminance? Illuminance => Conditions;

        private const byte DefaultLightTransmittance = 0b_0100_0101;
        private const float MaxTransmittance = 2.225f;
        private const float MinTransmittance = 0.272f;

        /// <summary>
        /// The default I2C address for the peripheral
        /// </summary>
        public byte DefaultI2cAddress => (byte)Addresses.Default;

        /// <summary>
        /// Create a new BH1750 light sensor object using a static reference voltage.
        /// </summary>
        public Bh1750(
            II2cBus i2cBus, byte address = (byte)Addresses.Default,
            MeasuringModes measuringMode = MeasuringModes.ContinuouslyHighResolutionMode,
            double lightTransmittance = 1)
                : base(i2cBus, address)
        {
            LightTransmittance = lightTransmittance;
            MeasuringMode = measuringMode;

            Initialize();
        }

        private void Initialize()
        {
            BusComms.Write((byte)Commands.PowerOn);
            BusComms.Write((byte)Commands.Reset);
        }

        /// <summary>
        /// Read the current luminosity 
        /// </summary>
        /// <returns>The current Illuminance value</returns>
        protected override async Task<Illuminance> ReadSensor()
        {
            if (MeasuringMode == MeasuringModes.OneTimeHighResolutionMode ||
                MeasuringMode == MeasuringModes.OneTimeHighResolutionMode2 ||
                MeasuringMode == MeasuringModes.OneTimeLowResolutionMode)
            {
                BusComms.Write((byte)Commands.PowerOn);
            }

            BusComms.Write((byte)MeasuringMode);

            //wait for the measurement to complete before reading
            await Task.Delay(GetMeasurementTime(MeasuringMode));

            BusComms.Read(ReadBuffer.Span[0..2]);

            ushort raw = BinaryPrimitives.ReadUInt16BigEndian(ReadBuffer.Span[0..2]);

            double result = raw / (1.2 * lightTransmittance);

            if (MeasuringMode == MeasuringModes.ContinuouslyHighResolutionMode2 ||
                MeasuringMode == MeasuringModes.OneTimeHighResolutionMode2)
            {
                result *= 2;
            }

            return new Illuminance(result, IU.Lux);
        }

        private TimeSpan GetMeasurementTime(MeasuringModes mode)
        {
            return mode switch
            {   //high res modes are 120ms, low res 16ms
                MeasuringModes.ContinuouslyLowResolutionMode => TimeSpan.FromMilliseconds(16),
                MeasuringModes.OneTimeLowResolutionMode => TimeSpan.FromMilliseconds(16),
                MeasuringModes.ContinuouslyHighResolutionMode => TimeSpan.FromMilliseconds(120),
                MeasuringModes.ContinuouslyHighResolutionMode2 => TimeSpan.FromMilliseconds(120),
                MeasuringModes.OneTimeHighResolutionMode => TimeSpan.FromMilliseconds(120),
                MeasuringModes.OneTimeHighResolutionMode2 => TimeSpan.FromMilliseconds(120),
                _ => TimeSpan.FromMilliseconds(120)
            };
        }

        /// <summary>
        /// Set BH1750FVI Light Transmittance
        /// </summary>
        /// <param name="transmittance">Light Transmittance, from 27.20% to 222.50%</param>
        private void SetLightTransmittance(double transmittance)
        {
            if (transmittance > MaxTransmittance || transmittance < MinTransmittance)
            {
                throw new ArgumentOutOfRangeException(nameof(transmittance), $"{nameof(transmittance)} needs to be in the range of 27.20% to 222.50%.");
            }

            byte val = (byte)(DefaultLightTransmittance / transmittance);

            BusComms.Write((byte)((byte)Commands.MeasurementTimeHigh | (val >> 5)));
            BusComms.Write((byte)((byte)Commands.MeasurementTimeLow | (val & 0b_0001_1111)));
        }

        /// <summary>
        /// Raise events for subscribers and notify of value changes
        /// </summary>
        /// <param name="changeResult">The updated sensor data</param>
        protected override void RaiseEventsAndNotify(IChangeResult<Illuminance> changeResult)
        {
            base.RaiseEventsAndNotify(changeResult);
        }
    }
}