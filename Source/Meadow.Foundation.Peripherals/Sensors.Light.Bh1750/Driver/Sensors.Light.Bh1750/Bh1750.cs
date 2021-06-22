using Meadow.Hardware;
using Meadow.Peripherals.Sensors.Light;
using Meadow.Units;
using IU = Meadow.Units.Illuminance.UnitType;
using System;
using System.Buffers.Binary;
using System.Threading.Tasks;

namespace Meadow.Foundation.Sensors.Light
{
    public partial class Bh1750 : ByteCommsSensorBase<Illuminance>, ILightSensor
    {
        //==== events
        public event EventHandler<IChangeResult<Illuminance>> LuminosityUpdated = delegate { };

        /// <summary>
        /// BH1750 Light Transmittance (27.20-222.50%)
        /// </summary>
        public double LightTransmittance
        {
            get => lightTransmittance;
            set => SetLightTransmittance(lightTransmittance = value);
        } private double lightTransmittance;

        /// <summary>
        /// BH1750 Measuring Mode
        /// </summary>
        public MeasuringModes MeasuringMode { get; set; }

        public Illuminance? Illuminance => Conditions;

        private const byte DefaultLightTransmittance = 0b_0100_0101;
        private const float MaxTransmittance = 2.225f;
        private const float MinTransmittance = 0.272f;


        /// <summary>
        /// Create a new BH1750 light sensor object using a static reference voltage.
        /// </summary>
        public Bh1750(
            II2cBus i2cBus, byte address,
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
            Peripheral.Write((byte)Commands.PowerOn);
            Peripheral.Write((byte)Commands.Reset);
        }

        protected override Task<Illuminance> ReadSensor()
        {
            return Task.Run(() => {
                if (MeasuringMode == MeasuringModes.OneTimeHighResolutionMode ||
                    MeasuringMode == MeasuringModes.OneTimeHighResolutionMode2 ||
                    MeasuringMode == MeasuringModes.OneTimeLowResolutionMode) {
                    Peripheral.Write((byte)Commands.PowerOn);
                }

                Peripheral.Write((byte)MeasuringMode);
                Peripheral.Read(ReadBuffer.Span[0..2]);

                ushort raw = BinaryPrimitives.ReadUInt16BigEndian(ReadBuffer.Span[0..2]);

                double result = raw / (1.2 * lightTransmittance);

                if (MeasuringMode == MeasuringModes.ContinuouslyHighResolutionMode2 ||
                    MeasuringMode == MeasuringModes.OneTimeHighResolutionMode2) {
                    result *= 2;
                }

                return new Illuminance(result, IU.Lux);
            });
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

            Peripheral.Write((byte)((byte)Commands.MeasurementTimeHigh | (val >> 5)));
            Peripheral.Write((byte)((byte)Commands.MeasurementTimeLow | (val & 0b_0001_1111)));
        }

        protected override void RaiseEventsAndNotify(IChangeResult<Illuminance> changeResult)
        {
            this.LuminosityUpdated?.Invoke(this, changeResult);
            base.RaiseEventsAndNotify(changeResult);
        }
    }
}