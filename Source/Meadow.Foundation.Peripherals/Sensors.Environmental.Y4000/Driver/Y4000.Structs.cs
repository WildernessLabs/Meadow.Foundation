using Meadow.Units;
using System;
using System.Text;

namespace Meadow.Foundation.Sensors.Environmental
{
    public partial class Y4000
    {
        //pass in the float array
        //check for infinity and switch to zero
        //polulate all of the properties 

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
            /// Chlorophyl Concentration (CHL)
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

            public Measurements(float[] data)
            {
                if(data.Length != 8)
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

            public override string ToString()
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine($"DissolvedOxygen: {DissolvedOxygen} mg/L");
                sb.AppendLine($"Turbidity: {Turbidity} NTU");
                sb.AppendLine($"ElectricalConductivity: {ElectricalConductivity.MilliSiemensPerCentimeter} mS/cm");
                sb.AppendLine($"PH: {PH}");
                sb.AppendLine($"OxidationReductionPotential: {OxidationReductionPotential.Millivolts} mV");
                sb.AppendLine($"Chlorophyl: {Chlorophyl.MicrogramsPerLiter} ug/L");
                sb.AppendLine($"BlueGreenAlgae: {BlueGreenAlgae.PartsPerMillion} ppm");
                sb.AppendLine($"Temperature: {Temperature.Celsius} C");

                return sb.ToString();
            }
        }
    }
}