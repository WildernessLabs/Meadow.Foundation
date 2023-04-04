using Meadow.Hardware;
using Meadow.Peripherals.Sensors;
using Meadow.Peripherals.Sensors.Environmental;
using Meadow.Units;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Meadow.Foundation.Sensors.Environmental
{
    /// <summary>
    /// Represents a Pmsa003I 
    /// </summary>
    public partial class Pmsa003I :
        ByteCommsSensorBase<(
            Density? StandardParticulateMatter_1micron, //Particulate Matter 1 micro or less
            Density? StandardParticulateMatter_2_5micron, //Particulate Matter 2.5 micro or less
            Density? StandardParticulateMatter_10micron, //Particulate Matter 10 micro or less
            Density? EnvironmentalParticulateMatter_1micron,
            Density? EnvironmentalParticulateMatter_2_5micron,
            Density? EnvironmentalParticulateMatter_10micron,
            Concentration? particles_0_3microns,//P03um,
            Concentration? particles_0_5microns,
            Concentration? particles_10microns,
            Concentration? particles_25microns,
            Concentration? particles_50microns,
            Concentration? particles_100microns)>,
        IConcentrationSensor
    {
        public Concentration? Concentration { get; }
        public event EventHandler<IChangeResult<Concentration>> ConcentrationUpdated;

        /// <summary>
        /// Raised when the Standard PM1.0 concentration changes
        /// </summary>
        public event EventHandler<IChangeResult<Density>> StandardParticulateMatter_1micronUpdated = delegate { };

        /// <summary>
        /// Raised when the Standard PM2.5 concentration changes
        /// </summary>
        public event EventHandler<IChangeResult<Density>> StandardParticulateMatter_2_5micronUpdated = delegate { };

        /// <summary>
        /// Raised when the Standard PM10.0 concentration changes
        /// </summary>
        public event EventHandler<IChangeResult<Density>> StandardParticulateMatter_10micron = delegate { };

        /// <summary>
        /// Raised when the Environment PM1.0 concentration changes
        /// </summary>
        public event EventHandler<IChangeResult<Density>> EnvironmentalParticulateMatter_1micron = delegate { };

        /// <summary>
        /// Raised when the Environment PM2.5 concentration changes
        /// </summary>
        public event EventHandler<IChangeResult<Density>> EnvironmentalParticulateMatter_2_5micron = delegate { };

        /// <summary>
        /// Raised when the Environment PM10.0 concentration changes
        /// </summary>
        public event EventHandler<IChangeResult<Density>> EnvironmentalParticulateMatter_10micron = delegate { };

        public Density? PM1_0Std => Conditions.StandardParticulateMatter_1micron;
        public Density? PM2_5Std => Conditions.StandardParticulateMatter_2_5micron;
        public Density? PM10_0Std => Conditions.StandardParticulateMatter_10micron;
        public Density? PM1_0Env => Conditions.EnvironmentalParticulateMatter_1micron;
        public Density? PM2_5Env => Conditions.EnvironmentalParticulateMatter_2_5micron;
        public Density? PM10_0Env => Conditions.EnvironmentalParticulateMatter_10micron;
        public Concentration? ConcentrationOf0_3micronParticles => Conditions.particles_0_3microns;
        public Concentration? ConcentrationOf0_5micronParticles => Conditions.particles_0_5microns;
        public Concentration? ConcentrationOf10micronParticles => Conditions.particles_10microns;
        public Concentration? ConcentrationOf25micronParticles => Conditions.particles_25microns;
        public Concentration? ConcentrationOf50micronParticles => Conditions.particles_50microns;
        public Concentration? ConcentrationOf100micronParticles => Conditions.particles_100microns;

        /// <summary>
        /// Create a new PMSA003I sensor object
        /// </summary>
        /// <remarks></remarks>
        /// <param name="i2cBus">The I2C bus</param>
        public Pmsa003I(II2cBus i2cBus)
            : base(i2cBus, (byte)Addresses.Address_0x12)
        { }

        /// <summary>
        /// Starts updating the sensor on the updateInterval frequency specified
        /// </summary>
        public override void StartUpdating(TimeSpan? updateInterval = null)
        {
            base.StartUpdating(updateInterval);
        }

        /// <summary>
        /// Stop updating the sensor
        /// The sensor will not respond to commands for 500ms 
        /// The call will delay the calling thread for 500ms
        /// </summary>
        public override void StopUpdating()
        {
            base.StopUpdating();
        }

        protected override Task<(
           Density? StandardParticulateMatter_1micron,
           Density? StandardParticulateMatter_2_5micron,
           Density? StandardParticulateMatter_10micron,
           Density? EnvironmentalParticulateMatter_1micron,
           Density? EnvironmentalParticulateMatter_2_5micron,
           Density? EnvironmentalParticulateMatter_10micron,
           Concentration? particles_0_3microns,
           Concentration? particles_0_5microns,
           Concentration? particles_10microns,
           Concentration? particles_25microns,
           Concentration? particles_50microns,
           Concentration? particles_100microns)> ReadSensor()
        {
            var buffer = new byte[32];
            Peripheral.Read(buffer);
            var span = buffer.AsSpan();
            span.Reverse();
            if (buffer[30..32].SequenceEqual(Preamble) == false)
            {
                throw new Exception("Preamble mismatch!");
            }
            var messageLength = BitConverter.ToUInt16(span[28..30]);
            if (messageLength != 28)
            {
                throw new Exception("Message is corrupt or has invalid length");
            }

            var checksum = BitConverter.ToUInt16(buffer[2..4]);
            // this is in big-endian format, so we need to reverse things...
            var pm10Standard = new Density(BitConverter.ToUInt16(span[26..28]), Density.UnitType.MicroGramsPerMetersCubed);
            var pm25Standard = new Density(BitConverter.ToUInt16(span[24..26]), Density.UnitType.MicroGramsPerMetersCubed);
            var pm100Standard = new Density(BitConverter.ToUInt16(span[22..24]), Density.UnitType.MicroGramsPerMetersCubed);
            var pm10Environmental = new Density(BitConverter.ToUInt16(span[20..22]), Density.UnitType.MicroGramsPerMetersCubed);
            var pm25Environmental = new Density(BitConverter.ToUInt16(span[18..20]), Density.UnitType.MicroGramsPerMetersCubed);
            var pm100Environmental = new Density(BitConverter.ToUInt16(span[16..18]), Density.UnitType.MicroGramsPerMetersCubed);
            var p03um = new Concentration(BitConverter.ToUInt16(span[14..16]), Units.Concentration.UnitType.PartsPerMillion);
            var p05um = new Concentration(BitConverter.ToUInt16(span[12..14]), Units.Concentration.UnitType.PartsPerMillion);
            var p10um = new Concentration(BitConverter.ToUInt16(span[10..12]), Units.Concentration.UnitType.PartsPerMillion);
            var p25um = new Concentration(BitConverter.ToUInt16(span[8..10]), Units.Concentration.UnitType.PartsPerMillion);
            var p50um = new Concentration(BitConverter.ToUInt16(span[6..8]), Units.Concentration.UnitType.PartsPerMillion);
            var p100um = new Concentration(BitConverter.ToUInt16(span[4..6]), Units.Concentration.UnitType.PartsPerMillion);

            Conditions = (pm10Standard, pm25Standard, pm100Standard, pm10Environmental, pm25Environmental,
                          pm100Environmental, p03um, p05um, p10um, p25um, p50um, p100um);

            return Task.FromResult(Conditions);
        }

        async Task<Concentration> ISensor<Concentration>.Read()
            => (await Read()).particles_0_3microns.Value;
    }
}