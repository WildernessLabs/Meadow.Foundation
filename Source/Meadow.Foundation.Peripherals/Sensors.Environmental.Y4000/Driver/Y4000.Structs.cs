using Meadow.Units;
using System;
using System.Text;

namespace Meadow.Foundation.Sensors.Environmental
{
    public partial class Y4000
    {
        //pass in the float array
        //check for infinity and switch to zero
        //populate all of the properties 

        enum Measurement
        {
            DissolvedOxygen, //DO
            Turbidity,
            Conductivity, //CT
            PotentialHydrogen, //pH
            Temperature,
            OxidationReductionPotential, //Orp
            Chlorophyl, //Chl
            BlueGreenAlgae, //BGA
        }

        /// <summary>
        /// Struct to hold Y4000 sensor measurement data
        /// </summary>
        public struct Measurements
        {
            /// <summary>
            /// Concentration of dissolved Oxygen in water
            /// </summary>
            public ConcentrationInWater DissolvedOxygen { get; private set; }

            /// <summary>
            /// Turbidity (NTU)
            /// </summary>
            public Turbidity Turbidity { get; private set; }

            /// <summary>
            /// Electrical conductivity (CT)
            /// </summary>
            public Conductivity ElectricalConductivity { get; private set; }

            /// <summary>
            /// PotentialHydrogren (pH)
            /// </summary>
            public PotentialHydrogen PH { get; private set; }

            /// <summary>
            /// ORP or Redox
            /// ORP is a measurement of the net voltage potential of excess oxidizers or reducers present in a liquid
            /// </summary>
            public Voltage OxidationReductionPotential { get; private set; }

            /// <summary>
            /// Chlorophyll Concentration (CHL)
            /// </summary>
            public ConcentrationInWater Chlorophyl { get; private set; }

            /// <summary>
            /// Salination (SAL)
            /// </summary>
            public ConcentrationInWater BlueGreenAlgae { get; private set; }

            /// <summary>
            /// Temperature
            /// </summary>
            public Units.Temperature Temperature { get; private set; }

            /// <summary>
            /// Measurements constructor, converts float array to Y4000 Measurements.
            /// </summary>
            /// <param name="data">8 element float array containing water component measurements</param>
            public Measurements(float[] data)
            {
                if (data.Length != 8)
                {
                    throw new ArgumentException($"Measurements record expects 8 values, received {data.Length}");
                }

                float value = Normalize(data[(int)Measurement.DissolvedOxygen]);
                DissolvedOxygen = new ConcentrationInWater(value, ConcentrationInWater.UnitType.MilligramsPerLiter);

                value = Normalize(data[(int)Measurement.Turbidity]);
                Turbidity = new Turbidity(value);

                value = Normalize(data[(int)Measurement.Conductivity]);
                ElectricalConductivity = new Conductivity(value, Conductivity.UnitType.MilliSiemensPerCentimeter);

                value = Normalize(data[(int)Measurement.PotentialHydrogen]);
                PH = new PotentialHydrogen(value);

                value = Normalize(data[(int)Measurement.OxidationReductionPotential]);
                OxidationReductionPotential = new Voltage(value, Voltage.UnitType.Volts);

                value = Normalize(data[(int)Measurement.Chlorophyl]);
                Chlorophyl = new ConcentrationInWater(value, ConcentrationInWater.UnitType.MicrogramsPerLiter);

                value = Normalize(data[(int)Measurement.BlueGreenAlgae]);
                BlueGreenAlgae = new ConcentrationInWater(value, ConcentrationInWater.UnitType.MilligramsPerLiter);

                value = Normalize(data[(int)Measurement.Temperature]);
                Temperature = new Units.Temperature(value, Units.Temperature.UnitType.Celsius);
            }

            static float Normalize(float value) => float.IsNormal(value) ? value : 0;

            /// <summary>
            /// Returns a string that represents the current Y4000 measurement data.
            /// </summary>
            /// <returns>A string that represents the current Y4000 measurement data.</returns>
            public override string ToString()
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine($"DissolvedOxygen: {DissolvedOxygen} mg/L");
                sb.AppendLine($"Turbidity: {Turbidity} NTU");
                sb.AppendLine($"ElectricalConductivity: {ElectricalConductivity.MilliSiemensPerCentimeter} mS/cm");
                sb.AppendLine($"PH: {PH}");
                sb.AppendLine($"OxidationReductionPotential: {OxidationReductionPotential.Millivolts} mV");
                sb.AppendLine($"Chlorophyll: {Chlorophyl.MicrogramsPerLiter} ug/L");
                sb.AppendLine($"BlueGreenAlgae: {BlueGreenAlgae.PartsPerMillion} ppm");
                sb.AppendLine($"Temperature: {Temperature.Celsius} C");

                return sb.ToString();
            }
        }
    }
}