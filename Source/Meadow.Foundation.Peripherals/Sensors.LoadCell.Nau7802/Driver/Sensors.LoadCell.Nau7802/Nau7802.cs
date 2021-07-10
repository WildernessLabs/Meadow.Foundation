using System;
using System.Threading;
using System.Threading.Tasks;
using Meadow.Hardware;
using Meadow.Peripherals.Sensors;
using Meadow.Units;

namespace Meadow.Foundation.Sensors.LoadCell
{
    /// <summary>
    /// 24-Bit Dual-Channel ADC For Bridge Sensors
    /// </summary>
    public partial class Nau7802 :
        ByteCommsSensorBase<Units.Mass>,
        IMassSensor,
        IDisposable

    {
        //==== events
        public event EventHandler<IChangeResult<Mass>> MassUpdated = delegate { };

        //==== internals
        private byte[] _read = new byte[3];
        private double _gramsPerAdcUnit = 0;
        private PU_CTRL_BITS _currentPU_CTRL;
        private int _tareValue;

        //==== Properties
        public TimeSpan DefaultSamplePeriod { get; } = TimeSpan.FromSeconds(1);

        /// <summary>
        /// The last read Mass.
        /// </summary>
        public Mass? Mass { get; private set; }

        /// <summary>
        /// Creates an instance of the NAU7802 Driver class
        /// </summary>
        /// <param name="bus"></param>
        public Nau7802(II2cBus bus, int updateIntervalMs = 1000)
            : base(bus, (byte)Addresses.Default, updateIntervalMs)
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
                    throw new ArgumentOutOfRangeException($"NAU7802 device supports only address {(int)Addresses.Default}");
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

            //Bus.ReadRegisterBytes((byte)Register.ADCO_B2, _read);
            Peripheral.ReadRegister((byte)Register.ADCO_B2, _read);
            return _read[0] << 16 | _read[1] << 8 | _read[2];
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
            Output.WriteLine($"Tare base = {_tareValue:x}");
        }

        private void PowerOn()
        {
            Output.WriteLine($"Powering up...");

            // Set and clear the RR bit in 0x00, to guarantee a reset of all register values
            _currentPU_CTRL = PU_CTRL_BITS.RR;
            //Bus.WriteRegister((byte)Register.PU_CTRL, (byte)_currentPU_CTRL);
            Peripheral.WriteRegister((byte)Register.PU_CTRL, (byte)_currentPU_CTRL);
            Thread.Sleep(1); // make sure it has time to do it's thing
            _currentPU_CTRL &= ~PU_CTRL_BITS.RR;
            //Bus.WriteRegister((byte)Register.PU_CTRL, (byte)_currentPU_CTRL);
            Peripheral.WriteRegister((byte)Register.PU_CTRL, (byte)_currentPU_CTRL);

            // turn on the analog and digital power
            _currentPU_CTRL |= (PU_CTRL_BITS.PUD | PU_CTRL_BITS.PUA);
            //Bus.WriteRegister((byte)Register.PU_CTRL, (byte)_currentPU_CTRL);
            Peripheral.WriteRegister((byte)Register.PU_CTRL, (byte)_currentPU_CTRL);
            // wait for power-up ready
            var timeout = 100;
            do
            {
                if(timeout-- <= 0)
                {
                    Output.WriteLine("Timeout powering up");
                    throw new Exception("Timeout powering up");
                }
                Thread.Sleep(10);
                //_currentPU_CTRL = (PU_CTRL_BITS)Bus.ReadRegisterByte((byte)Register.PU_CTRL);
                _currentPU_CTRL = (PU_CTRL_BITS)Peripheral.ReadRegister((byte)Register.PU_CTRL);
            } while ((_currentPU_CTRL & PU_CTRL_BITS.PUR) != PU_CTRL_BITS.PUR);


            Output.WriteLine($"Configuring...");

            SetLDO(LdoVoltage.LDO_3V3);
            SetGain(AdcGain.Gain128);
            SetConversionRate(ConversionRate.SamplePerSecond80);
            //Bus.WriteRegister((byte)Register.OTP_ADC, 0x30); // turn off CLK_CHP
            Peripheral.WriteRegister((byte)Register.OTP_ADC, 0x30); // turn off CLK_CHP
            EnableCh2DecouplingCap();

            if (!CalibrateAdc())
            {
                throw new Exception("Calibration error");
            }

            // turn on cycle start
            //_currentPU_CTRL = (PU_CTRL_BITS)Bus.ReadRegisterByte((byte)Register.PU_CTRL);
            _currentPU_CTRL = (PU_CTRL_BITS)Peripheral.ReadRegister((byte)Register.PU_CTRL);
            _currentPU_CTRL |= PU_CTRL_BITS.CS;
            //Bus.WriteRegister((byte)Register.PU_CTRL, (byte)_currentPU_CTRL);
            Peripheral.WriteRegister((byte)Register.PU_CTRL, (byte)_currentPU_CTRL);


            Output.WriteLine($"PU_CTRL: {_currentPU_CTRL}"); // 0xBE

            // Enter the low power standby condition by setting PUA and PUD bits to 0, in R0x00 
            // Resume operation by setting PUA and PUD bits to 1, in R0x00.This sequence is the same for powering up from the standby condition, except that from standby all of the information in the configuration and calibration registers will be retained if the power supply is stable.Depending on conditions and the application, it may be desirable to perform calibration again to update the calibration registers for the best possible accuracy.

        }

        private bool IsConversionComplete()
        {
            //var puctrl = (PU_CTRL_BITS)Bus.ReadRegisterByte((byte)Register.PU_CTRL);
            var puctrl = (PU_CTRL_BITS)Peripheral.ReadRegister((byte)Register.PU_CTRL);
            return (puctrl & PU_CTRL_BITS.CR) == PU_CTRL_BITS.CR;
        }

        private void EnableCh2DecouplingCap()
        {
            // app note - enable ch2 decoupling cap
            //var pga_pwr = Bus.ReadRegisterByte((byte)Register.PGA_PWR);
            var pga_pwr = Peripheral.ReadRegister((byte)Register.PGA_PWR);
            pga_pwr |= 1 << 7;
            //Bus.WriteRegister((byte)Register.PGA_PWR, pga_pwr);
            Peripheral.WriteRegister((byte)Register.PGA_PWR, pga_pwr);
        }

        private void SetLDO(LdoVoltage value)
        {
            //var ctrl1 = Bus.ReadRegisterByte((byte)Register.CTRL1);
            var ctrl1 = Peripheral.ReadRegister((byte)Register.CTRL1);
            ctrl1 &= 0b11000111; // clear LDO
            ctrl1 |= (byte)((byte)value << 3);
            //Bus.WriteRegister((byte)Register.CTRL1, ctrl1);
            Peripheral.WriteRegister((byte)Register.CTRL1, ctrl1);
            _currentPU_CTRL |= PU_CTRL_BITS.AVDDS;
            //Bus.WriteRegister((byte)Register.PU_CTRL, (byte)_currentPU_CTRL); // enable internal LDO
            Peripheral.WriteRegister((byte)Register.PU_CTRL, (byte)_currentPU_CTRL); // enable internal LDO
        }

        private void SetGain(AdcGain value)
        {
            //var ctrl1 = Bus.ReadRegisterByte((byte)Register.CTRL1);
            var ctrl1 = Peripheral.ReadRegister((byte)Register.CTRL1);
            ctrl1 &= 0b11111000; // clear gain
            ctrl1 |= (byte)value;
            //Bus.WriteRegister((byte)Register.CTRL1, ctrl1);
            Peripheral.WriteRegister((byte)Register.CTRL1, ctrl1);
        }

        private void SetConversionRate(ConversionRate value)
        {
            //var ctrl2 = Bus.ReadRegisterByte((byte)Register.CTRL2);
            var ctrl2 = Peripheral.ReadRegister((byte)Register.CTRL2);
            ctrl2 &= 0b10001111; // clear gain
            ctrl2 |= (byte)((byte)value << 4);
            //Bus.WriteRegister((byte)Register.CTRL2, ctrl2);
            Peripheral.WriteRegister((byte)Register.CTRL2, ctrl2);
        }

        private bool CalibrateAdc()
        {
            // read ctrl2
            //var ctrl2 = Bus.ReadRegisterByte((byte)Register.CTRL2);
            var ctrl2 = Peripheral.ReadRegister((byte)Register.CTRL2);

            // turn on the calibration bit
            ctrl2 |= (byte)CTRL2_BITS.CALS;
            //Bus.WriteRegister((byte)Register.CTRL2, ctrl2);
            Peripheral.WriteRegister((byte)Register.CTRL2, ctrl2);

            // now wiat for either completion or error
            do
            {
                //ctrl2 = Bus.ReadRegisterByte((byte)Register.CTRL2);
                ctrl2 = Peripheral.ReadRegister((byte)Register.CTRL2);
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
            Console.WriteLine($"SetCalibrationFactor: knownValue.Grams: {knownValue.Grams:N1}");
            _gramsPerAdcUnit = knownValue.Grams / (double)factor;
        }

        private int DoConversion()
        {
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

        protected override async Task<Mass> ReadSensor()
        {
            return await Task.Run(() => {
                if (_gramsPerAdcUnit == 0)
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
                return new Mass(grams, Units.Mass.UnitType.Grams);
            });
        }


        /// <summary>
        /// Inheritance-safe way to raise events and notify observers.
        /// </summary>
        /// <param name="changeResult"></param>
        protected override void RaiseEventsAndNotify(IChangeResult<Mass> changeResult)
        {
            try
            {
                MassUpdated?.Invoke(this, changeResult);
                base.RaiseEventsAndNotify(changeResult);
            }
            catch(Exception ex)
            {
                Console.WriteLine($"NAU7802 event handler threw: {ex.Message}");
                throw;
            }
        }
    }
}