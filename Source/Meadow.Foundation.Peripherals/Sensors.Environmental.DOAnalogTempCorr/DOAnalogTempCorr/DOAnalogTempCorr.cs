/*  
 *  /************************* Dissolved Oxygen Meter with Temperature Correction *************************
 * Designed to work with an O2 sensor that uses a galvanic dissolved oxygen probe with amplification and
 * ouputs analog voltage, such as the Surveyor (formerly Gravity) dissolved Oxygen Meter from Atlas Scientific
 * https://atlas-scientific.com/embedded-solutions/dissolved-oxygen-meter/
 * Atlas recommends using the setup only to calculate percent saturation of oxygen relative to the
 * partial pressure of oxygen in the atmosphere. Here, we set temperature and then
 * do some temperature correction calulations to estimate dissolved oxygen in mg/L.Note that we ignore two 
 * other factors than can change the relationship between measured voltage and O2 concentration, namely
 * atmospheric pressure and salinity.
 */

using System;
using Meadow.Foundation;
using Meadow.Hardware;
using Meadow.Units;
using System.Threading.Tasks;


using Meadow;
using Meadow.Devices;
using Meadow.Peripherals.Sensors;
using TempCorrectedDOSensorContract;


namespace DOAnalogTempCorr
{
    public class AnalogDO_Temp : PollingSensorBase<ConcentrationInWater>, ITempCorrectedDOsensor
    {
        /// <summary>
        /// Concentration of Oxygen in water
        /// </summary>
        public ConcentrationInWater Concentration { get; set; }

        /// <summary>
        /// Water temperature, used to correct sensor voltage
        /// </summary>
        public Temperature WaterTemperature { get; set; }

        /// <summary>
        /// The analog input port used to get raw sensor voltage
        /// </summary>
        private IAnalogInputPort AnalogInputPort { get; }

        /// <summary>
        /// last voltage read at the sensor
        /// </summary>
        public Voltage Voltage { get; protected set; }


        /// <summary>
        /// default constructor, private so it does not get called accidentally
        /// </summary>
        private AnalogDO_Temp() { }

        /// <summary>
        /// Creates a new AnalogDO_Temp object with provided analog port and starting temp
        /// </summary>
        /// <param name="analogInput">Analog input port used to read sensor</param>
        /// <param name="temperature">Initial water temperature</param>
        public AnalogDO_Temp(IAnalogInputPort analogInputPort, Temperature temperature)
        {
            this.AnalogInputPort = analogInputPort;
            this.WaterTemperature = temperature;

            _ = AnalogInputPort.Subscribe
            (
                IAnalogInputPort.CreateObserver(
                    result =>
                    {
                        this.Voltage = result.New;
                        ChangeResult<ConcentrationInWater> changeResult = new ChangeResult<ConcentrationInWater>()
                        {
                            New = VoltageToConcentration(result.New, this.WaterTemperature),
                            Old = Concentration,
                        };
                        this.Concentration = changeResult.New;
                        RaiseEventsAndNotify(changeResult);
                    }
                )
           );
        }

        /// <summary>
        /// Gets concentration with an updated temperature. For use when updating
        /// </summary>
        /// <param name="temperature">New Water Temperature</param>
        /// <returns>concentration calculated with new temperature</returns>
        public ConcentrationInWater GetConcentrationWithTemp(Temperature temperature)
        {
            this.WaterTemperature = temperature;
            return VoltageToConcentration(this.Voltage, temperature);
        }

        /// <summary>
        /// Public methos that Reads the analog voltage from sensor and returns O2 concentration calculated for current temperature
        /// </summary>
        /// <returns></returns>
        public override async Task<ConcentrationInWater> Read ()
        {
            return await ReadSensor();
        }

        /// <summary>
        /// Reads the analog voltage from sensor and returns O2 concentration calculated for current temperature
        /// </summary>
        /// <returns>Dissolved Oxygen concentration</returns>
        protected override async Task<ConcentrationInWater> ReadSensor()
        {
            Voltage = await AnalogInputPort.Read();
            Concentration = VoltageToConcentration(this.Voltage, this.WaterTemperature);
            return Concentration;
        }

     /// <summary>
     /// Calculates O2 concentration with updated temperature
     /// </summary>
     /// <param name="voltage"></param>
     /// <param name="temperature"></param>
     /// <returns></returns>
        private ConcentrationInWater VoltageToConcentration(Voltage voltage, Temperature temperature)
        {
            double milligramsPerVolt = Math.Exp(Sat_Offset + Sat_Mult * temperature.Celsius);
            return new ConcentrationInWater((voltage.Volts * milligramsPerVolt), ConcentrationInWater.UnitType.MilligramsPerLiter);

        }


        public const byte MODE_MEASURE = 0;
        public const byte MODE_CAL_ZERO = 1;
        public const byte MODE_CAL_SAT = 2;
        public byte calState;


        /// <summary>
        /// The calibration values for the sensor, log linear fits of voltage vs temperature
        /// for no oxygen condition,and for (mg/l)/V for saturated O2 condition
        /// </summary>
        public double Zero_Offset { get; set; } = -3.5382; // or a big negative number like -1E6 if no zero cal is available
        public double Zero_Mult { get; set; } = -0.086818; // // or 0 if no zero cal is available
        public double Sat_Offset { get; set; } = 4.6834;
        public double Sat_Mult { get; set; } = -0.086818;

        private double SumXi, SumYi, SumXiYi, SumSqXi, Nreads;

        /// <summary>
        /// Constants for 3rd order poly for DO saturation (mg/L) with °C temperature,
        /// at 1 atm pressure for fresh water. Obtained from the literature
        /// good from 0 to 30 °C
        /// satuaration conc = DO_Sat_K0 + (DO_Sat_K1 * °C) + (DO_Sat_K1 * °C^2)
        /// </summary>
        readonly double DO_Sat_K0 = 14.502;
        readonly double DO_Sat_K1 = -0.35984;
        readonly double DO_Sat_K2 = 0.0043703;


        public void StartCal(byte calMode)
        {
            SumXi = 0;
            SumYi = 0;
            SumXiYi = 0;
            SumSqXi = 0;
            Nreads = 0;
            calState = calMode;
            StopUpdating();
        }

        public void FinishCal(byte calMode)
        {
            if ((calMode == MODE_CAL_ZERO) || (calMode == MODE_CAL_SAT))
            {
                double mult = ((Nreads * SumXiYi) - (SumXi * SumYi)) / ((Nreads * SumSqXi) - (SumXi * SumXi));
                double offset = ((SumYi * SumSqXi) - (SumXi * SumXiYi)) / ((Nreads * SumSqXi) - (SumXi * SumXi));
                if (calMode == MODE_CAL_ZERO)
                {
                    Zero_Mult = mult;
                    Zero_Offset = offset;
                } else
                {
                    Sat_Mult = mult;
                    Sat_Offset = offset;
                }

            }
            calState = MODE_MEASURE;
        }

        public async Task AddCalAsync (Temperature temperature)
        {
            WaterTemperature = temperature;
            Voltage = await AnalogInputPort.Read();
            double Yi;
            if ((calState == MODE_CAL_ZERO) || (calState == MODE_CAL_SAT))
            {
                if (calState == MODE_CAL_ZERO)
                {
                    // get the natuaral log of the sensor voltage
                    Yi = Math.Log(this.Voltage.Volts);
                }
                else
                {
                    // get the sensor voltage offset, i.e., the zero voltage at this temperature
                    Voltage zeroVoltage = new Voltage(Math.Exp(Zero_Offset + WaterTemperature.Celsius * Zero_Mult), Voltage.UnitType.Volts);
                    // get the saturated oxgen concentration value at this temperature from 3deg poly fit
                    double milligramsPerL = DO_Sat_K0 + (DO_Sat_K1 * WaterTemperature.Celsius) + (DO_Sat_K2 * WaterTemperature.Celsius * WaterTemperature.Celsius);
                    // calculate (milligrams/L)/V from this sensor reading, and take the natural log
                    Yi = Math.Log(milligramsPerL / ((this.Voltage - zeroVoltage).Volts));
                }
                // add to the running sums of the needed values for the log linear fits
                SumXi += WaterTemperature.Celsius;
                SumSqXi += (WaterTemperature.Celsius * WaterTemperature.Celsius);
                SumXiYi = WaterTemperature.Celsius * Yi;
                SumYi = Yi;
                Nreads += 1;
            }
        }
    }
}