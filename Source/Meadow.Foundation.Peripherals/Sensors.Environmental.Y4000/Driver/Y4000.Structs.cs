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
            DO,
            Turbidity,
            CT,
            pH,
            Temp,
            Orp,
            Chl,
            BGA
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

                float value = float.IsNormal(data[(int)Measurement.DO]) ? data[(int)Measurement.DO] : 0;
                DissolvedOxygen = new ConcentrationInWater(value, ConcentrationInWater.UnitType.MilligramsPerLiter);

                value = float.IsNormal(data[(int)Measurement.Turbidity]) ? data[(int)Measurement.Turbidity] : 0;
                Turbidity = new Turbidity(value);

                value = float.IsNormal(data[(int)Measurement.CT]) ? data[(int)Measurement.CT] : 0;
                ElectricalConductivity = new Conductivity(value, Conductivity.UnitType.MilliSiemensPerCentimeter);

                value = float.IsNormal(data[(int)Measurement.pH]) ? data[(int)Measurement.pH] : 0;
                PH = new PotentialHydrogen(value);

                value = float.IsNormal(data[(int)Measurement.Orp]) ? data[(int)Measurement.Orp] : 0;
                OxidationReductionPotential = new Voltage(value, Voltage.UnitType.Volts);

                value = float.IsNormal(data[(int)Measurement.Chl]) ? data[(int)Measurement.Chl] : 0;
                Chlorophyl = new ConcentrationInWater(value, ConcentrationInWater.UnitType.MicrogramsPerLiter);

                value = float.IsNormal(data[(int)Measurement.BGA]) ? data[(int)Measurement.BGA] : 0;
                BlueGreenAlgae = new ConcentrationInWater(value, ConcentrationInWater.UnitType.MilligramsPerLiter);

                value = float.IsNormal(data[(int)Measurement.Temp]) ? data[(int)Measurement.Temp] : 0;
                Temperature = new Units.Temperature(value, Units.Temperature.UnitType.Celsius);
            }

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