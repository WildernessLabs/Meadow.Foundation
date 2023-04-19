using Meadow.Units;
using System;

namespace Meadow.Foundation.Sensors.Environmental
{
    /// <summary>
    /// A collection of particulate density readings
    /// </summary>
    public class ParticulateReading
    {
        private byte[] _data;

        internal ParticulateReading(byte[] rawData, int offset)
        {
            _data = new byte[12];
            Array.Copy(rawData, offset, _data, 0, 12);
        }

        /// <summary>
        /// Count of 1-micron particles
        /// </summary>
        public ParticleDensity CountOf1micronParticles => new ParticleDensity(_data[0] << 8 | _data[1], ParticleDensity.UnitType.ParticlesPerLiter);
        /// <summary>
        /// Count of 2.5 micron particles
        /// </summary>
        public ParticleDensity CountOf2_5micronParticles => new ParticleDensity(_data[2] << 8 | _data[3], ParticleDensity.UnitType.ParticlesPerLiter);
        /// <summary>
        /// Count of 10 micron particles
        /// </summary>
        public ParticleDensity CountOf10micronParticles => new ParticleDensity(_data[4] << 8 | _data[5], ParticleDensity.UnitType.ParticlesPerLiter);
        /// <summary>
        /// Density of 1 micron particles
        /// </summary>
        public Density EnvironmentalPM_1micron => new Density((_data[6] << 8 | _data[7]) / 10d, Density.UnitType.MicroGramsPerMetersCubed);
        /// <summary>
        /// Density of 2.5 micron particles
        /// </summary>
        public Density EnvironmentalPM_2_5micron => new Density((_data[8] << 8 | _data[9]) / 10d, Density.UnitType.MicroGramsPerMetersCubed);
        /// <summary>
        /// Density of 10 micron particles
        /// </summary>
        public Density EnvironmentalPM_10micron => new Density((_data[10] << 8 | _data[11]) / 10d, Density.UnitType.MicroGramsPerMetersCubed);
    }
}