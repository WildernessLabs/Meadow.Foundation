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
    /// Represents a Pmsa003I AQI particulate matter sensor
    /// </summary>
    public partial class Pmsa003I :
        ByteCommsSensorBase<(
            Density? StandardParticulateMatter_1micron, //Particulate Matter 1 micron or less
            Density? StandardParticulateMatter_2_5micron, //Particulate Matter 2.5 micron or less
            Density? StandardParticulateMatter_10micron, //Particulate Matter 10 micron or less
            Density? EnvironmentalParticulateMatter_1micron,
            Density? EnvironmentalParticulateMatter_2_5micron,
            Density? EnvironmentalParticulateMatter_10micron,
            Concentration? particles_0_3microns,
            Concentration? particles_0_5microns,
            Concentration? particles_10microns,
            Concentration? particles_25microns,
            Concentration? particles_50microns,
            Concentration? particles_100microns)>,
        IConcentrationSensor
    {
        public Concentration? Concentration { get; }

        /// <summary>
        /// Raised when the Standard particulate matter PM2.5 density changes
        /// </summary>
        public event EventHandler<IChangeResult<Concentration>> ConcentrationUpdated;

        /// <summary>
        /// Raised when the Standard particulate matter PM1.0 density changes
        /// </summary>
        public event EventHandler<IChangeResult<Density>> StandardPM_1micronUpdated = delegate { };

        /// <summary>
        /// Raised when the Standard particulate matter PM2.5 density changes
        /// </summary>
        public event EventHandler<IChangeResult<Density>> StandardPM_2_5micronUpdated = delegate { };

        /// <summary>
        /// Raised when the Standard particulate matter PM10.0 density changes
        /// </summary>
        public event EventHandler<IChangeResult<Density>> StandardPM_10micronUpdated = delegate { };

        /// <summary>
        /// Raised when the Environment particulate matter PM1.0 density changes
        /// </summary>
        public event EventHandler<IChangeResult<Density>> EnvironmentalPM_1micronUpdated = delegate { };

        /// <summary>
        /// Raised when the Environment particulate matter PM2.5 density changes
        /// </summary>
        public event EventHandler<IChangeResult<Density>> EnvironmentalPM_2_5micronUpdated = delegate { };

        /// <summary>
        /// Raised when the Environment particulate matter PM10.0 density changes
        /// </summary>
        public event EventHandler<IChangeResult<Density>> EnvironmentalPM_10micronUpdated = delegate { };

        /// <summary>
        /// Raised when the concentration of 0-0.3 micron particles changes
        /// </summary>
        public event EventHandler<IChangeResult<Concentration>> ConcentrationOf0_3micronParticlesUpdated = delegate { };

        /// <summary>
        /// Raised when the concentration of 0.3-0.5 micron particles changes
        /// </summary>
        public event EventHandler<IChangeResult<Concentration>> ConcentrationOf0_5micronParticlesUpdated = delegate { };

        /// <summary>
        /// Raised when the concentration of 0.5-10 micron particles changes
        /// </summary>
        public event EventHandler<IChangeResult<Concentration>> ConcentrationOf10micronParticlesUpdated = delegate { };

        /// <summary>
        /// Raised when the concentration of 10-25 micron particles changes
        /// </summary>
        public event EventHandler<IChangeResult<Concentration>> ConcentrationOf25micronParticlesUpdated = delegate { };

        /// <summary>
        /// Raised when the concentration of 25-50 micron particles changes
        /// </summary>
        public event EventHandler<IChangeResult<Concentration>> ConcentrationOf50micronParticlesUpdated = delegate { };

        /// <summary>
        /// Raised when the concentration of 50-100 micron particles changes
        /// </summary>
        public event EventHandler<IChangeResult<Concentration>> ConcentrationOf100micronParticlesUpdated = delegate { };

        /// <summary>
        /// Standard particulate matter PM1.0 density
        /// </summary>
        public Density? PM1_0Std => Conditions.StandardParticulateMatter_1micron;

        /// <summary>
        /// Standard particulate matter PM2.5 density
        /// </summary>
        public Density? PM2_5Std => Conditions.StandardParticulateMatter_2_5micron;

        /// <summary>
        /// Standard particulate matter PM10 density
        /// </summary>
        public Density? PM10_0Std => Conditions.StandardParticulateMatter_10micron;

        /// <summary>
        /// Standard particulate matter PM1.0 density
        /// </summary>
        public Density? PM1_0Env => Conditions.EnvironmentalParticulateMatter_1micron;

        /// <summary>
        /// Standard particulate matter PM2.5 density
        /// </summary>
        public Density? PM2_5Env => Conditions.EnvironmentalParticulateMatter_2_5micron;

        /// <summary>
        /// Standard particulate matter PM10 density
        /// </summary>
        public Density? PM10_0Env => Conditions.EnvironmentalParticulateMatter_10micron;

        /// <summary>
        /// Concentration of 0 - 0.3 micron particles
        /// </summary>
        public Concentration? ConcentrationOf0_3micronParticles => Conditions.particles_0_3microns;
        /// <summary>
        /// Concentration of 0.3 - 0.5 micron particles
        /// </summary>
        public Concentration? ConcentrationOf0_5micronParticles => Conditions.particles_0_5microns;
        /// <summary>
        /// Concentration of 0.5 - 10 micron particles
        /// </summary>
        public Concentration? ConcentrationOf10micronParticles => Conditions.particles_10microns;
        /// <summary>
        /// Concentration of 0.5 - 10 micron particles
        /// </summary>
        public Concentration? ConcentrationOf25micronParticles => Conditions.particles_25microns;
        /// <summary>
        /// Concentration of 10 - 50 micron particles
        /// </summary>
        public Concentration? ConcentrationOf50micronParticles => Conditions.particles_50microns;
        /// <summary>
        /// Concentration of 50 - 100 micron particles
        /// </summary>
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

        /// <summary>
        /// Raise change events for subscribers
        /// </summary>
        /// <param name="changeResult">The change result with the current sensor data</param>
        protected override void RaiseEventsAndNotify(
            IChangeResult<(Density? StandardParticulateMatter_1micron,
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
                Concentration? particles_100microns)> changeResult)
        {
            if (changeResult.New.StandardParticulateMatter_1micron is { } SPM0_1)
            {
                StandardPM_1micronUpdated?.Invoke(this, new ChangeResult<Density>(SPM0_1, changeResult.Old.Value.StandardParticulateMatter_1micron));
            }
            if (changeResult.New.StandardParticulateMatter_2_5micron is { } SPM0_2_5)
            {
                StandardPM_1micronUpdated?.Invoke(this, new ChangeResult<Density>(SPM0_2_5, changeResult.Old.Value.StandardParticulateMatter_2_5micron));
            }
            if (changeResult.New.StandardParticulateMatter_10micron is { } SPM0_10)
            {
                StandardPM_1micronUpdated?.Invoke(this, new ChangeResult<Density>(SPM0_10, changeResult.Old.Value.StandardParticulateMatter_10micron));
            }
            if (changeResult.New.EnvironmentalParticulateMatter_1micron is { } EM0_1)
            {
                StandardPM_1micronUpdated?.Invoke(this, new ChangeResult<Density>(EM0_1, changeResult.Old.Value.EnvironmentalParticulateMatter_1micron));
            }
            if (changeResult.New.EnvironmentalParticulateMatter_2_5micron is { } EM0_2_5)
            {
                StandardPM_1micronUpdated?.Invoke(this, new ChangeResult<Density>(EM0_2_5, changeResult.Old.Value.EnvironmentalParticulateMatter_2_5micron));
            }
            if (changeResult.New.EnvironmentalParticulateMatter_10micron is { } EM0_10)
            {
                StandardPM_1micronUpdated?.Invoke(this, new ChangeResult<Density>(EM0_10, changeResult.Old.Value.EnvironmentalParticulateMatter_10micron));
            }

            if (changeResult.New.particles_0_3microns is { } P_0_3)
            {
                ConcentrationOf0_3micronParticlesUpdated?.Invoke(this, new ChangeResult<Concentration>(P_0_3, changeResult.Old.Value.particles_0_3microns));
            }
            if (changeResult.New.particles_0_5microns is { } P_0_5)
            {
                ConcentrationOf0_5micronParticlesUpdated?.Invoke(this, new ChangeResult<Concentration>(P_0_5, changeResult.Old.Value.particles_0_5microns));
            }
            if (changeResult.New.particles_10microns is { } P_10)
            {
                ConcentrationOf10micronParticlesUpdated?.Invoke(this, new ChangeResult<Concentration>(P_10, changeResult.Old.Value.particles_10microns));
                ConcentrationUpdated?.Invoke(this, new ChangeResult<Concentration>(P_10, changeResult.Old.Value.particles_10microns));
            }
            if (changeResult.New.particles_25microns is { } P_25)
            {
                ConcentrationOf25micronParticlesUpdated?.Invoke(this, new ChangeResult<Concentration>(P_25, changeResult.Old.Value.particles_25microns));
            }
            if (changeResult.New.particles_50microns is { } P_50)
            {
                ConcentrationOf50micronParticlesUpdated?.Invoke(this, new ChangeResult<Concentration>(P_50, changeResult.Old.Value.particles_50microns));
            }
            if (changeResult.New.particles_100microns is { } P_100)
            {
                ConcentrationOf100micronParticlesUpdated?.Invoke(this, new ChangeResult<Concentration>(P_100, changeResult.Old.Value.particles_100microns));
            }

            base.RaiseEventsAndNotify(changeResult);
        }

        async Task<Concentration> ISensor<Concentration>.Read()
            => (await Read()).particles_25microns.Value;
    }
}