using System;
using System.Threading;
using Meadow.Hardware;

namespace Meadow.Foundation.Sensors.LoadCell
{
    /// <summary>
    /// 24-Bit Dual-Channel ADC For Bridge Sensors
    /// </summary>
    /// <remarks><b>Work in progress.  This driver is not yet functional.</b></remarks>
    public class Nau7802 : IDisposable //Todo: FilterableObservableBase<FloatChangeResult, float>,  IDisposable
    {
        /// <summary>
        /// Valid addresses for the sensor.
        /// </summary>
        public enum Addresses : byte
        {
            Address0 = 0x2A,
            Default = Address0
        }

        private enum Register : byte
        {
            PU_CTRL = 0x00,
            CTRL1 = 0x01,
            CTRL2 = 0x02,
            OCAL1_B2 = 0x03,
            OCAL1_B1 = 0x04,
            OCAL1_B0 = 0x05,
            GCAL1_B3 = 0x06,
            GCAL1_B2 = 0x07,
            GCAL1_B1 = 0x08,
            GCAL1_B0 = 0x09,
            OCAL2_B2 = 0x0a,
            OCAL2_B1 = 0x0b,
            OCAL2_B0 = 0x0c,
            GCAL2_B3 = 0x0d,
            GCAL2_B2 = 0x0e,
            GCAL2_B1 = 0x0f,
            GCAL2_B0 = 0x10,
            I2C_CTRL = 0x11,
            ADCO_B2 = 0x12,
            ADCO_B1 = 0x13,
            ADCO_B0 = 0x14,
            OTP_B1 = 0x15,
            OTP_B0 = 0x16,
            PGA = 0x1B,
            PGA_PWR = 0x1C,
            DEV_REV = 0x1f
        }

        [Flags]
        private enum PU_CTRL_BITS
        {
            /// <summary>
            /// Register Reset
            /// </summary>
            RR = 1 << 0,
            /// <summary>
            /// Power up digital
            /// </summary>
            PUD = 1 << 1,
            /// <summary>
            /// Power-up analog
            /// </summary>
            PUA = 1 << 2,
            /// <summary>
            /// Power-up ready
            /// </summary>
            PUR = 1 << 3,
            /// <summary>
            /// Cycle start
            /// </summary>
            CS = 1 << 4,
            /// <summary>
            /// Cycle ready
            /// </summary>
            CR = 1 << 5,
            /// <summary>
            /// System clock source select
            /// </summary>
            OSCS = 1 << 6,
            /// <summary>
            /// AVDD source select
            /// </summary>
            AVDDS = 1 << 7
        }

        private enum LdoVoltage
        {
            LDO_2V4 = 0b111,
            LDO_2V7 = 0b110,
            LDO_3V0 = 0b101,
            LDO_3V3 = 0b100,
            LDO_3V6 = 0b011,
            LDO_3V9 = 0b010,
            LDO_4V2 = 0b001,
            LDO_4V5 = 0b000,
        }

        private enum AdcGain
        {
            Gain0 = 0b000,
            Gain2 = 0b001,
            Gain4 = 0b010,
            Gain8 = 0b011,
            Gain16 = 0b100,
            Gain32 = 0b101,
            Gain64 = 0b110,
            Gain128 = 0b111,
        }

        private enum ConversionRate
        {
            SamplePerSecond10 = 0b000,
            SamplePerSecond20 = 0b001,
            SamplePerSecond40 = 0b010,
            SamplePerSecond80 = 0b011,
            SamplePerSecond320 = 0b111,
        }



        private II2cBus Device { get; }
        private object SyncRoot { get; } = new object();
        private decimal gramsPerAdcUnit = 0;
        private PU_CTRL_BITS currentPU_CTRL;
        private int tareValue;

        /// <summary>
        /// The peripheral's address on the I2C Bus
        /// </summary>
        public byte Address { get; private set; }

        /// <summary>
        /// Creates an instance of the NAU7802 Driver class
        /// </summary>
        /// <param name="bus"></param>
        public Nau7802(II2cBus bus)
        {
            Device = bus;
            Initialize((byte)Addresses.Default);
        }

        protected virtual void Dispose(bool disposing)
        {
        }

        /// <summary>
        /// Dispose managed resources
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
        }

        private void Initialize(byte address)
        {
            switch (address)
            {
                case (byte)Addresses.Default:
                    // valid;
                    break;
                default:
                    throw new ArgumentOutOfRangeException($"NAU7802 device supports only address {(int)Addresses.Default}");
            }

            Address = address;

            PowerOn();

            // let the ADCs settle
            Thread.Sleep(500);
        }

        private void WriteRegister(Register register, PU_CTRL_BITS value)
        {
            WriteRegister(register, (byte)value);
        }

        private void WriteRegister(Register register, byte value)
        {
            lock (SyncRoot)
            {
                Span<byte> buffer = stackalloc byte[2];

                buffer[0] = (byte)register;
                buffer[1] = value;

                Device.WriteData(Address, buffer);
            }
        }

        private byte ReadRegister(Register register)
        {
            lock (SyncRoot)
            {
                Span<byte> write = stackalloc byte[1];
                Span<byte> read = stackalloc byte[1];

                write[0] = (byte)register;

                Device.WriteReadData(Address, write, read);

                return read[0];
            }
        }

        private int ReadADC()
        {
            lock (SyncRoot)
            {
                Span<byte> write = stackalloc byte[1];
                Span<byte> read = stackalloc byte[3];

                write[0] = (byte)Register.ADCO_B2;

                Device.WriteReadData(Address, write, read);

                return read[0] << 16 | read[1] << 8 | read[2];
            }
        }

        /// <summary>
        /// Tares the sensor, effectively setting the current weight reading to relative zero. 
        /// </summary>
        public void Tare()
        {
            while(!IsConversionComplete())
            {
                Thread.Sleep(1);
            }

            tareValue = ReadADC();
            Console.WriteLine($"Tare base = {tareValue}");
        }

        private void PowerOn()
        {
            Console.WriteLine($"Powering up...");

            // read the control register
            currentPU_CTRL = (PU_CTRL_BITS)ReadRegister(Register.PU_CTRL);

            Console.WriteLine($"PU_CTRL: 0x{currentPU_CTRL:x}");

            // Set the RR bit to 1 in R0x00, to guarantee a reset of all register values
            currentPU_CTRL |= PU_CTRL_BITS.RR;
            WriteRegister(Register.PU_CTRL, currentPU_CTRL);

            // Set the RR bit to 0 and PUD bit 1, in R0x00, to enter normal operation 
            currentPU_CTRL &= ~PU_CTRL_BITS.RR;
            currentPU_CTRL |= (PU_CTRL_BITS.PUD | PU_CTRL_BITS.PUA);
            WriteRegister(Register.PU_CTRL, currentPU_CTRL);

            // After about 200 microseconds, the PWRUP bit will be Logic = 1 indicating the device is ready for the remaining programming setup. 
            do
            {
                Thread.Sleep(1);
                currentPU_CTRL = (PU_CTRL_BITS)ReadRegister(Register.PU_CTRL);
            } while ((currentPU_CTRL & PU_CTRL_BITS.PUR) == 0);

            // At this point, all appropriate device selections and configuration can be made
            //  a.For example R0x00 = 0xAE 
            //  b.R0x15 = 0x30 

            Console.WriteLine($"Configuring...");

            SetLDO(LdoVoltage.LDO_3V3);
            SetGain(AdcGain.Gain128);
            SetConversionRate(ConversionRate.SamplePerSecond80);
            WriteRegister(Register.OTP_B1, 0x30); // turn off CLK_CHP
            EnableCh2DecouplingCap();

            CalibrateAdc();

            // No conversion will take place until the R0x00 bit 4 “CS” is set Logic = 1 
            currentPU_CTRL |= PU_CTRL_BITS.CS;
            WriteRegister(Register.PU_CTRL, currentPU_CTRL);

            // Enter the low power standby condition by setting PUA and PUD bits to 0, in R0x00 
            // Resume operation by setting PUA and PUD bits to 1, in R0x00.This sequence is the same for powering up from the standby condition, except that from standby all of the information in the configuration and calibration registers will be retained if the power supply is stable.Depending on conditions and the application, it may be desirable to perform calibration again to update the calibration registers for the best possible accuracy.


        }

        private bool IsConversionComplete()
        {
            var puctrl = (PU_CTRL_BITS)ReadRegister(Register.PU_CTRL);
            return (puctrl & PU_CTRL_BITS.CR) == PU_CTRL_BITS.CR;
        }

        private void EnableCh2DecouplingCap()
        {
            // app note - enable ch2 decoupling cap
            var pga_pwr = ReadRegister(Register.PGA_PWR);
            pga_pwr |= 1 << 3;
            WriteRegister(Register.PGA_PWR, pga_pwr);
        }

        private void SetLDO(LdoVoltage value)
        {
            var ctrl1 = ReadRegister(Register.CTRL1);
            ctrl1 &= 0b11000111; // clear LDO
            ctrl1 |= (byte)((byte)value << 3);
            WriteRegister(Register.CTRL1, ctrl1);
            currentPU_CTRL |= PU_CTRL_BITS.AVDDS;
            WriteRegister(Register.PU_CTRL, currentPU_CTRL); // enable internal LDO
        }

        private void SetGain(AdcGain value)
        {
            var ctrl1 = ReadRegister(Register.CTRL1);
            ctrl1 &= 0b11111000; // clear gain
            ctrl1 |= (byte)value;
            WriteRegister(Register.CTRL1, ctrl1);
        }

        private void SetConversionRate(ConversionRate value)
        {
            var ctrl2 = ReadRegister(Register.CTRL2);
            ctrl2 &= 0b10001111; // clear gain
            ctrl2 |= (byte)((byte)value << 4);
            WriteRegister(Register.CTRL2, ctrl2);
        }

        private void CalibrateAdc()
        {
            var ctrl2 = ReadRegister(Register.CTRL2);
            ctrl2 |= (byte)((byte)1 << 7);
            WriteRegister(Register.CTRL2, ctrl2);

            do
            {
                ctrl2 = ReadRegister(Register.CTRL2);
                if ((ctrl2 & (1 << 3)) != 0)
                {
                    // calibration error
                    throw new Exception("Calibration error");
                }
                if ((ctrl2 & (1 << 2)) == 0)
                {
                    // cal complete
                    break;
                }
            } while (true);

        }

        /// <summary>
        /// Calculates the calibration factor of the load cell.  Call this method with a known weight on the sensor, and then use the returned value in a call to <see cref="SetCalibrationFactor(int, Weight)"/> before using the sensor.
        /// </summary>
        /// <returns></returns>
        public int CalculateCalibrationFactor()
        {
            // do a few reads, then return the difference between tare (zero) and this value.
            var reads = 5;
            var sum = 0;

            for(int i = 0; i < reads; i++)
            {
                sum += DoConversion();
                Thread.Sleep(200);
            }

            return sum / reads;
        }

        /// <summary>
        /// Sets the sensor's calibration factor based on a factor calculated with a know weight by calling <see cref="CalculateCalibrationFactor"/>.
        /// </summary>
        /// <param name="factor"></param>
        /// <param name="knownValue"></param>
        public void SetCalibrationFactor(int factor, Weight knownValue)
        {
            gramsPerAdcUnit = knownValue.ConvertTo(WeightUnits.Grams) / (decimal)factor;
        }

        /// <summary>
        /// Gets the current sensor weight
        /// </summary>
        /// <returns></returns>
        public Weight GetWeight()
        {
            if(gramsPerAdcUnit == 0)
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
            return new Weight(grams, WeightUnits.Grams);
        }

        private int DoConversion()
        {
            /*
            Console.WriteLine($"Starting conversion...");
            WriteRegister(Register.PU_CTRL, (byte)(_currentPU_CTRL | PU_CTRL_BITS.CS));

            // wait for ready
            do
            {
                _currentPU_CTRL = (PU_CTRL_BITS)ReadRegister(Register.PU_CTRL);
            } while ((_currentPU_CTRL & PU_CTRL_BITS.CR) == 0);
            */

            if(!IsConversionComplete())
            {
                Console.WriteLine("ADC is busy");
                return 0;
            }

            //read
            Console.WriteLine("Reading ADC...");
            var adc = ReadADC();
            Console.WriteLine($"ADC = 0x{adc:x}");

            // convert based on gain, etc.
            return adc;
        }
    }
}