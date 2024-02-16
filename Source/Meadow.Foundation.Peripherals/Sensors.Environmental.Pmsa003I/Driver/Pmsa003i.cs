﻿using Meadow.Hardware;
using Meadow.Units;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Meadow.Foundation.Sensors.Environmental
{
    /// <summary>
    /// Represents a Pmsa003i AQI particulate matter sensor
    /// </summary>
    public partial class Pmsa003i :
        ByteCommsSensorBase<(
            Density? StandardParticulateMatter_1micron, //Particulate Matter 1 micron or less
            Density? StandardParticulateMatter_2_5micron, //Particulate Matter 2.5 micron or less
            Density? StandardParticulateMatter_10micron, //Particulate Matter 10 micron or less
            Density? EnvironmentalParticulateMatter_1micron,
            Density? EnvironmentalParticulateMatter_2_5micron,
            Density? EnvironmentalParticulateMatter_10micron,
            ParticleDensity? ParticleDensity_0_3microns,
            ParticleDensity? ParticleDensity_0_5microns,
            ParticleDensity? ParticleDensity_10microns,
            ParticleDensity? ParticleDensity_25microns,
            ParticleDensity? ParticleDensity_50microns,
            ParticleDensity? ParticleDensity_100microns)>,
        II2cPeripheral
    {
        /// <summary>
        /// Raised when the Standard particulate matter PM1.0 density changes
        /// </summary>
        public event EventHandler<IChangeResult<Density>> StandardPM_1micronUpdated = default!;

        /// <summary>
        /// Raised when the Standard particulate matter PM2.5 density changes
        /// </summary>
        public event EventHandler<IChangeResult<Density>> StandardPM_2_5micronUpdated = default!;

        /// <summary>
        /// Raised when the Standard particulate matter PM10.0 density changes
        /// </summary>
        public event EventHandler<IChangeResult<Density>> StandardPM_10micronUpdated = default!;

        /// <summary>
        /// Raised when the Environment particulate matter PM1.0 density changes
        /// </summary>
        public event EventHandler<IChangeResult<Density>> EnvironmentalPM_1micronUpdated = default!;

        /// <summary>
        /// Raised when the Environment particulate matter PM2.5 density changes
        /// </summary>
        public event EventHandler<IChangeResult<Density>> EnvironmentalPM_2_5micronUpdated = default!;

        /// <summary>
        /// Raised when the Environment particulate matter PM10.0 density changes
        /// </summary>
        public event EventHandler<IChangeResult<Density>> EnvironmentalPM_10micronUpdated = default!;

        /// <summary>
        /// Raised when the number of 0-0.3 micron particles (in 0.1 liters of air) changes
        /// </summary>
        public event EventHandler<IChangeResult<ParticleDensity>> CountOf0_3micronParticlesUpdated = default!;

        /// <summary>
        /// Raised when the number of 0.3-0.5 micron particles (in 0.1 liters of air) changes
        /// </summary>
        public event EventHandler<IChangeResult<ParticleDensity>> CountOf0_5micronParticlesUpdated = default!;

        /// <summary>
        /// Raised when the number of 0.5-10 micron particles changes
        /// </summary>
        public event EventHandler<IChangeResult<ParticleDensity>> CountOf10micronParticlesUpdated = default!;

        /// <summary>
        /// Raised when the number of 10-25 micron particles (in 0.1 liters of air) changes
        /// </summary>
        public event EventHandler<IChangeResult<ParticleDensity>> CountOf25micronParticlesUpdated = default!;

        /// <summary>
        /// Raised when the number of 25-50 micron particles (in 0.1 liters of air) changes
        /// </summary>
        public event EventHandler<IChangeResult<ParticleDensity>> CountOf50micronParticlesUpdated = default!;

        /// <summary>
        /// Raised when the number of 50-100 micron particles (in 0.1 liters of air) changes
        /// </summary>
        public event EventHandler<IChangeResult<ParticleDensity>> CountOf100micronParticlesUpdated = default!;

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
        /// Particle density of 0 - 0.3 micron particles in air
        /// </summary>
        public ParticleDensity? CountOf0_3micronParticles => Conditions.ParticleDensity_0_3microns;
        /// <summary>
        /// Particle density of 0.3 - 0.5 micron particles in air
        /// </summary>
        public ParticleDensity? CountOf0_5micronParticles => Conditions.ParticleDensity_0_5microns;
        /// <summary>
        /// Particle density of 0.5 - 10 micron particles in air
        /// </summary>
        public ParticleDensity? CountOf10micronParticles => Conditions.ParticleDensity_10microns;
        /// <summary>
        /// Particle density of 0.5 - 10 micron particles in air
        /// </summary>
        public ParticleDensity? CountOf25micronParticles => Conditions.ParticleDensity_25microns;
        /// <summary>
        /// Particle density of 10 - 50 micron particles in air
        /// </summary>
        public ParticleDensity? CountOf50micronParticles => Conditions.ParticleDensity_50microns;
        /// <summary>
        /// Particle density of 50 - 100 micron particles in  air
        /// </summary>
        public ParticleDensity? CountOf100micronParticles => Conditions.ParticleDensity_100microns;

        /// <summary>
        /// The default I2C address for the peripheral
        /// </summary>
        public byte DefaultI2cAddress => (byte)Addresses.Default;

        /// <summary>
        /// Create a new PMSA003I sensor object
        /// </summary>
        /// <remarks></remarks>
        /// <param name="i2cBus">The I2C bus</param>
        public Pmsa003i(II2cBus i2cBus)
            : base(i2cBus, (byte)Addresses.Default)
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

        /// <summary>
        /// Read data from the sensor
        /// </summary>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        protected override Task<(
           Density? StandardParticulateMatter_1micron,
           Density? StandardParticulateMatter_2_5micron,
           Density? StandardParticulateMatter_10micron,
           Density? EnvironmentalParticulateMatter_1micron,
           Density? EnvironmentalParticulateMatter_2_5micron,
           Density? EnvironmentalParticulateMatter_10micron,
           ParticleDensity? ParticleDensity_0_3microns,
           ParticleDensity? ParticleDensity_0_5microns,
           ParticleDensity? ParticleDensity_10microns,
           ParticleDensity? ParticleDensity_25microns,
           ParticleDensity? ParticleDensity_50microns,
           ParticleDensity? ParticleDensity_100microns)> ReadSensor()
        {
            var buffer = new byte[32];
            BusComms.Read(buffer);
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

            // this is in big-endian format, so we need to reverse things...
            var pm10Standard = new Density(BitConverter.ToUInt16(span[26..28]), Density.UnitType.MicroGramsPerMetersCubed);
            var pm25Standard = new Density(BitConverter.ToUInt16(span[24..26]), Density.UnitType.MicroGramsPerMetersCubed);
            var pm100Standard = new Density(BitConverter.ToUInt16(span[22..24]), Density.UnitType.MicroGramsPerMetersCubed);
            var pm10Environmental = new Density(BitConverter.ToUInt16(span[20..22]), Density.UnitType.MicroGramsPerMetersCubed);
            var pm25Environmental = new Density(BitConverter.ToUInt16(span[18..20]), Density.UnitType.MicroGramsPerMetersCubed);
            var pm100Environmental = new Density(BitConverter.ToUInt16(span[16..18]), Density.UnitType.MicroGramsPerMetersCubed);
            var p03um = new ParticleDensity(BitConverter.ToUInt16(span[14..16]), ParticleDensity.UnitType.ParticlesPerCentiliter);
            var p05um = new ParticleDensity(BitConverter.ToUInt16(span[12..14]), ParticleDensity.UnitType.ParticlesPerCentiliter);
            var p10um = new ParticleDensity(BitConverter.ToUInt16(span[10..12]), ParticleDensity.UnitType.ParticlesPerCentiliter);
            var p25um = new ParticleDensity(BitConverter.ToUInt16(span[8..10]), ParticleDensity.UnitType.ParticlesPerCentiliter);
            var p50um = new ParticleDensity(BitConverter.ToUInt16(span[6..8]), ParticleDensity.UnitType.ParticlesPerCentiliter);
            var p100um = new ParticleDensity(BitConverter.ToUInt16(span[4..6]), ParticleDensity.UnitType.ParticlesPerCentiliter);

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
                ParticleDensity? ParticleDensity_0_3microns,
                ParticleDensity? ParticleDensity_0_5microns,
                ParticleDensity? ParticleDensity_10microns,
                ParticleDensity? ParticleDensity_25microns,
                ParticleDensity? ParticleDensity_50microns,
                ParticleDensity? ParticleDensity_100microns)> changeResult)
        {
            if (changeResult.New.StandardParticulateMatter_1micron is { } SPM0_1)
            {
                StandardPM_1micronUpdated?.Invoke(this, new ChangeResult<Density>(SPM0_1, changeResult.Old!.Value.StandardParticulateMatter_1micron));
            }
            if (changeResult.New.StandardParticulateMatter_2_5micron is { } SPM0_2_5)
            {
                StandardPM_2_5micronUpdated?.Invoke(this, new ChangeResult<Density>(SPM0_2_5, changeResult.Old!.Value.StandardParticulateMatter_2_5micron));
            }
            if (changeResult.New.StandardParticulateMatter_10micron is { } SPM0_10)
            {
                StandardPM_10micronUpdated?.Invoke(this, new ChangeResult<Density>(SPM0_10, changeResult.Old!.Value.StandardParticulateMatter_10micron));
            }
            if (changeResult.New.EnvironmentalParticulateMatter_1micron is { } EM0_1)
            {
                EnvironmentalPM_1micronUpdated?.Invoke(this, new ChangeResult<Density>(EM0_1, changeResult.Old!.Value.EnvironmentalParticulateMatter_1micron));
            }
            if (changeResult.New.EnvironmentalParticulateMatter_2_5micron is { } EM0_2_5)
            {
                EnvironmentalPM_2_5micronUpdated?.Invoke(this, new ChangeResult<Density>(EM0_2_5, changeResult.Old!.Value.EnvironmentalParticulateMatter_2_5micron));
            }
            if (changeResult.New.EnvironmentalParticulateMatter_10micron is { } EM0_10)
            {
                EnvironmentalPM_10micronUpdated?.Invoke(this, new ChangeResult<Density>(EM0_10, changeResult.Old!.Value.EnvironmentalParticulateMatter_10micron));
            }

            if (changeResult.New.ParticleDensity_0_3microns is { } P_0_3)
            {
                CountOf0_3micronParticlesUpdated?.Invoke(this, new ChangeResult<ParticleDensity>(P_0_3, changeResult.Old!.Value.ParticleDensity_0_3microns));
            }
            if (changeResult.New.ParticleDensity_0_5microns is { } P_0_5)
            {
                CountOf0_5micronParticlesUpdated?.Invoke(this, new ChangeResult<ParticleDensity>(P_0_5, changeResult.Old!.Value.ParticleDensity_0_5microns));
            }
            if (changeResult.New.ParticleDensity_10microns is { } P_10)
            {
                CountOf10micronParticlesUpdated?.Invoke(this, new ChangeResult<ParticleDensity>(P_10, changeResult.Old!.Value.ParticleDensity_10microns));
            }
            if (changeResult.New.ParticleDensity_25microns is { } P_25)
            {
                CountOf25micronParticlesUpdated?.Invoke(this, new ChangeResult<ParticleDensity>(P_25, changeResult.Old!.Value.ParticleDensity_25microns));
            }
            if (changeResult.New.ParticleDensity_50microns is { } P_50)
            {
                CountOf50micronParticlesUpdated?.Invoke(this, new ChangeResult<ParticleDensity>(P_50, changeResult.Old!.Value.ParticleDensity_50microns));
            }
            if (changeResult.New.ParticleDensity_100microns is { } P_100)
            {
                CountOf100micronParticlesUpdated?.Invoke(this, new ChangeResult<ParticleDensity>(P_100, changeResult.Old!.Value.ParticleDensity_100microns));
            }

            base.RaiseEventsAndNotify(changeResult);
        }
    }
}