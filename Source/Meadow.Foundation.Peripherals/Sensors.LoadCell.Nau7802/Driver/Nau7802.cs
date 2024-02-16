using Meadow.Hardware;
using Meadow.Peripherals.Sensors.Mass;
using Meadow.Units;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Meadow.Foundation.Sensors.LoadCell
{
    /// <summary>
    /// 24-Bit Dual-Channel ADC For Bridge Sensors
    /// </summary>
    public partial class Nau7802 : ByteCommsSensorBase<Mass>, IMassSensor, II2cPeripheral
    {
        private readonly byte[] readBuffer = new byte[3];
        private double gramsPerAdcUnit = 0;
        private PU_CTRL_BITS currentPuCTRL;
        private int tareValue;

        /// <summary>
        /// Default sample period
        /// </summary>
        public TimeSpan DefaultSamplePeriod => TimeSpan.FromSeconds(1);

        /// <summary>
        /// The last read Mass
        /// </summary>
        public Mass? Mass { get; private set; }

        /// <summary>
        /// The default I2C address for the peripheral
        /// </summary>
        public byte DefaultI2cAddress => (byte)Addresses.Default;

        /// <summary>
        /// Creates an instance of the NAU7802 Driver class
        /// </summary>
        /// <param name="i2cBus">The I2C bus</param>
        public Nau7802(II2cBus i2cBus)
            : base(i2cBus, (byte)Addresses.Default)
        {
            Initialize((byte)Addresses.Default);
        }

        private void Initialize(byte address)
        {
            switch (address)
            {
                case (byte)Addresses.Default:
                    // valid;
                    break;
                default:
                    throw new ArgumentOutOfRangeException($"NAU7802 device supports only address {(byte)Addresses.Default}");
            }

            PowerOn();

            // let the ADCs settle
            Thread.Sleep(500);
        }

        private int ReadADC()
        {
            while (!IsConversionComplete())
            {
                Thread.Sleep(1);
            }

            BusComms?.ReadRegister((byte)Register.ADCO_B2, readBuffer);
            return readBuffer[0] << 16 | readBuffer[1] << 8 | readBuffer[2];
        }

        /// <summary>
        /// Tares the sensor, effectively setting the current weight reading to relative zero. 
        /// </summary>
        public void Tare()
        {
            while (!IsConversionComplete())
            {
                Thread.Sleep(1);
            }

            tareValue = ReadADC();
            Output.WriteLine($"Tare base = {tareValue:x}");
        }

        private void PowerOn()
        {
            Output.WriteLine($"Powering up...");

            // Set and clear the RR bit in 0x00, to guarantee a reset of all register values
            currentPuCTRL = PU_CTRL_BITS.RR;
            BusComms?.WriteRegister((byte)Register.PU_CTRL, (byte)currentPuCTRL);
            Thread.Sleep(1); // make sure it has time to do it's thing
            currentPuCTRL &= ~PU_CTRL_BITS.RR;

            BusComms?.WriteRegister((byte)Register.PU_CTRL, (byte)currentPuCTRL);

            // turn on the analog and digital power
            currentPuCTRL |= (PU_CTRL_BITS.PUD | PU_CTRL_BITS.PUA);

            BusComms?.WriteRegister((byte)Register.PU_CTRL, (byte)currentPuCTRL);
            // wait for power-up ready
            var timeout = 100;
            do
            {
                if (timeout-- <= 0)
                {
                    Output.WriteLine("Timeout powering up");
                    throw new Exception("Timeout powering up");
                }
                Thread.Sleep(10);
                currentPuCTRL = (PU_CTRL_BITS)(BusComms?.ReadRegister((byte)Register.PU_CTRL) ?? 0);
            } while ((currentPuCTRL & PU_CTRL_BITS.PUR) != PU_CTRL_BITS.PUR);


            Output.WriteLine($"Configuring...");

            SetLDO(LdoVoltage.LDO_3V3);
            SetGain(AdcGain.Gain128);
            SetConversionRate(ConversionRate.SamplePerSecond80);
            BusComms?.WriteRegister((byte)Register.OTP_ADC, 0x30); // turn off CLK_CHP
            EnableCh2DecouplingCap();

            if (!CalibrateAdc())
            {
                throw new Exception("Calibration error");
            }

            // turn on cycle start
            currentPuCTRL = (PU_CTRL_BITS)(BusComms?.ReadRegister((byte)Register.PU_CTRL) ?? 0);
            currentPuCTRL |= PU_CTRL_BITS.CS;

            BusComms?.WriteRegister((byte)Register.PU_CTRL, (byte)currentPuCTRL);


            Output.WriteLine($"PU_CTRL: {currentPuCTRL}"); // 0xBE

            // Enter the low power standby condition by setting PUA and PUD bits to 0, in R0x00 
            // Resume operation by setting PUA and PUD bits to 1, in R0x00.This sequence is the same for powering up from the standby condition, except that from standby all of the information in the configuration and calibration registers will be retained if the power supply is stable.Depending on conditions and the application, it may be desirable to perform calibration again to update the calibration registers for the best possible accuracy.

        }

        private bool IsConversionComplete()
        {
            var puctrl = (PU_CTRL_BITS)(BusComms?.ReadRegister((byte)Register.PU_CTRL) ?? 0);
            return (puctrl & PU_CTRL_BITS.CR) == PU_CTRL_BITS.CR;
        }

        private void EnableCh2DecouplingCap()
        {   // app note - enable ch2 decoupling cap
            var pga_pwr = BusComms?.ReadRegister((byte)Register.PGA_PWR) ?? 0;
            pga_pwr |= 1 << 7;

            BusComms?.WriteRegister((byte)Register.PGA_PWR, pga_pwr);
        }

        private void SetLDO(LdoVoltage value)
        {
            var ctrl1 = BusComms?.ReadRegister((byte)Register.CTRL1) ?? 0;
            ctrl1 &= 0b11000111; // clear LDO
            ctrl1 |= (byte)((byte)value << 3);

            BusComms?.WriteRegister((byte)Register.CTRL1, ctrl1);
            currentPuCTRL |= PU_CTRL_BITS.AVDDS;

            BusComms?.WriteRegister((byte)Register.PU_CTRL, (byte)currentPuCTRL); // enable internal LDO
        }

        private void SetGain(AdcGain value)
        {
            var ctrl1 = BusComms?.ReadRegister((byte)Register.CTRL1) ?? 0;

            ctrl1 &= 0b11111000; // clear gain
            ctrl1 |= (byte)value;
            //Bus.WriteRegister((byte)Register.CTRL1, ctrl1);
            BusComms?.WriteRegister((byte)Register.CTRL1, ctrl1);
        }

        void SetConversionRate(ConversionRate value)
        {
            var ctrl2 = BusComms?.ReadRegister((byte)Register.CTRL2) ?? 0;
            ctrl2 &= 0b10001111; // clear gain
            ctrl2 |= (byte)((byte)value << 4);

            BusComms?.WriteRegister((byte)Register.CTRL2, ctrl2);
        }

        bool CalibrateAdc()
        {
            var ctrl2 = BusComms?.ReadRegister((byte)Register.CTRL2) ?? 0;

            // turn on the calibration bit
            ctrl2 |= (byte)CTRL2_BITS.CALS;

            BusComms?.WriteRegister((byte)Register.CTRL2, ctrl2);

            do
            {
                ctrl2 = BusComms?.ReadRegister((byte)Register.CTRL2) ?? 0;
                if ((ctrl2 & (byte)CTRL2_BITS.CAL_ERROR) != 0)
                {
                    // calibration error
                    return false;
                }
                if ((ctrl2 & (byte)CTRL2_BITS.CALS) == 0)
                {
                    // cal complete
                    break;
                }
                Thread.Sleep(1);
            } while (true);

            return true;
        }

        /// <summary>
        /// Calculates the calibration factor of the load cell
        /// Call this method with a known weight on the sensor, and then use the returned value in a call before using the sensor
        /// </summary>
        /// <returns>The calibration factor as an int</returns>
        public int CalculateCalibrationFactor()
        {
            var reads = 5;
            var sum = 0;

            for (int i = 0; i < reads; i++)
            {
                sum += DoConversion();
                Thread.Sleep(200);
            }

            return sum / reads;
        }

        /// <summary>
        /// Sets the sensor's calibration factor based on a factor calculated with a know weight by calling <see cref="CalculateCalibrationFactor"/>.
        /// </summary>
        public void SetCalibrationFactor(int factor, Mass knownValue)
        {
            Resolver.Log.Info($"SetCalibrationFactor: knownValue.Grams: {knownValue.Grams:N1}");
            gramsPerAdcUnit = knownValue.Grams / factor;
        }

        int DoConversion()
        {
            if (!IsConversionComplete())
            {
                Output.WriteLine("ADC is busy");
                return 0;
            }

            //read
            Output.WriteLine("Reading ADC...");
            var adc = ReadADC();
            Output.WriteLine($"ADC = 0x{adc:x}");

            // convert based on gain, etc.
            return adc;
        }

        /// <summary>
        /// Reads data from the sensor
        /// </summary>
        /// <returns>The latest sensor reading</returns>
        protected override Task<Mass> ReadSensor()
        {
            if (gramsPerAdcUnit == 0)
            {
                throw new Exception("Calibration factor has not been set");
            }

            // get an ADC conversion
            var c = DoConversion();
            // subtract the tare
            var adc = c - tareValue;
            // convert to grams
            var grams = adc * gramsPerAdcUnit;
            // convert to desired units
            return Task.FromResult(new Mass(grams, Units.Mass.UnitType.Grams));
        }
    }
}