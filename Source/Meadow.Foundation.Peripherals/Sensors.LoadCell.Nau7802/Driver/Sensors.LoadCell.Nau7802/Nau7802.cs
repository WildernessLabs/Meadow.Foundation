using System;
using System.Threading;
using Meadow.Hardware;
using Meadow.Units;

namespace Meadow.Foundation.Sensors.LoadCell
{
    /// <summary>
    /// 24-Bit Dual-Channel ADC For Bridge Sensors
    /// </summary>
    public partial class Nau7802 :
        FilterableChangeObservableI2CPeripheral<CompositeChangeResult<Units.Mass>, Units.Mass>,
        IDisposable

    {
        //==== internals
        private II2cBus Device { get; }
        private object SyncRoot { get; } = new object();
        private double _gramsPerAdcUnit = 0;
        private PU_CTRL_BITS _currentPU_CTRL;
        private int _tareValue;

        /// <summary>
        /// Creates an instance of the NAU7802 Driver class
        /// </summary>
        /// <param name="bus"></param>
        public Nau7802(II2cBus bus)
            : base(bus, (byte)Addresses.Default)
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

            _tareValue = ReadADC();
            Output.WriteLine($"Tare base = {_tareValue}");
        }

        private void PowerOn()
        {
            Output.WriteLine($"Powering up...");

            // read the control register
            _currentPU_CTRL = (PU_CTRL_BITS)ReadRegister(Register.PU_CTRL);

            Output.WriteLine($"PU_CTRL: 0x{_currentPU_CTRL:x}");

            // Set and clear the RR bit in 0x00, to guarantee a reset of all register values
            _currentPU_CTRL |= PU_CTRL_BITS.RR;
            WriteRegister(Register.PU_CTRL, _currentPU_CTRL);
            Thread.Sleep(1); // make sure it has time to do it's thing
            _currentPU_CTRL &= ~PU_CTRL_BITS.RR;
            WriteRegister(Register.PU_CTRL, _currentPU_CTRL);

            // turn on the analog and digital power
            _currentPU_CTRL |= (PU_CTRL_BITS.PUD | PU_CTRL_BITS.PUA);
            WriteRegister(Register.PU_CTRL, _currentPU_CTRL);
            Thread.Sleep(10); // make sure it has time to do it's thing


            Output.WriteLine($"Configuring...");

            SetLDO(LdoVoltage.LDO_3V3);
            SetGain(AdcGain.Gain128);
            SetConversionRate(ConversionRate.SamplePerSecond80);
            WriteRegister(Register.OTP_ADC, 0x30); // turn off CLK_CHP
            EnableCh2DecouplingCap();

            if (!CalibrateAdc())
            {
                throw new Exception("Calibration error");
            }

            // No conversion will take place until the R0x00 bit 4 “CS” is set Logic = 1 
            _currentPU_CTRL |= PU_CTRL_BITS.CS;
            WriteRegister(Register.PU_CTRL, _currentPU_CTRL);

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
            pga_pwr |= 1 << 7;
            WriteRegister(Register.PGA_PWR, pga_pwr);
        }

        private void SetLDO(LdoVoltage value)
        {
            var ctrl1 = ReadRegister(Register.CTRL1);
            ctrl1 &= 0b11000111; // clear LDO
            ctrl1 |= (byte)((byte)value << 3);
            WriteRegister(Register.CTRL1, ctrl1);
            _currentPU_CTRL |= PU_CTRL_BITS.AVDDS;
            WriteRegister(Register.PU_CTRL, _currentPU_CTRL); // enable internal LDO
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

        private bool CalibrateAdc()
        {
            // read ctrl2
            var ctrl2 = ReadRegister(Register.CTRL2);

            // turn on the calibration bit
            ctrl2 |= (byte)CTRL2_BITS.CALS;
            WriteRegister(Register.CTRL2, ctrl2);

            // now wiat for either completion or error
            do
            {
                ctrl2 = ReadRegister(Register.CTRL2);
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
        public void SetCalibrationFactor(int factor, Mass knownValue)
        {
            _gramsPerAdcUnit = knownValue.Grams / (double)factor;
        }

        /// <summary>
        /// Gets the current sensor weight
        /// </summary>
        /// <returns></returns>
        public Mass GetWeight()
        {
            if(_gramsPerAdcUnit == 0)
            {
                throw new Exception("Calibration factor has not been set");
            }

            // get an ADC conversion
            var c = DoConversion();
            // subtract the tare
            var adc = c - _tareValue;
            // convert to grams
            var grams = adc * _gramsPerAdcUnit;
            // convert to desired units
            return new Mass(grams, Mass.UnitType.Grams);
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
    }
}