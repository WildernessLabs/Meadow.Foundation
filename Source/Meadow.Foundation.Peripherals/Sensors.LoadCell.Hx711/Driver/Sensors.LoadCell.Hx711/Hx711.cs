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
    public class Hx711 : SamplingSensorBase<Mass>, IMassSensor, IDisposable
    {
        //==== events
        public event EventHandler<IChangeResult<Mass>> MassUpdated = delegate { };

        //==== internals
        // TODO: move into `Hx711.AdcGainOptions.cs` and rename
        public enum AdcGain
        {
            Gain128 = 1,
            Gain32 = 2,
            Gain64 = 3,
        }

        private const uint GPIO_BASE = 0x40020000;
        private const uint IDR_OFFSET = 0x10;
        private const uint BSSR_OFFSET = 0x18;
        private const int timing_iterations = 3;

        private uint _sck_address;
        private int _sck_pin;
        private uint _dout_address;
        private uint _sck_set;
        private uint _sck_clear;
        private uint _dout_mask;
        private uint _tareValue = 0;
        private double _gramsPerAdcUnit;
        private bool _createdPorts = false;

        private IDigitalOutputPort SCK { get; }
        private IDigitalInputPort DOUT { get; }
        //private object SyncRoot { get; } = new object();


        //==== properties
        public bool IsDisposed { get; private set; }
        public bool IsSleeping { get; private set; }
        public AdcGain Gain { get; private set; } = AdcGain.Gain128;

        /// <summary>
        /// Creates an instance of the NAU7802 Driver class
        /// </summary>
        /// <param name="bus"></param>
        public Hx711(IDigitalInputOutputController device, IPin sck, IPin dout)
        {
            this.SCK = device.CreateDigitalOutputPort(sck);
            this.DOUT = device.CreateDigitalInputPort(dout);
            _createdPorts = true; // we need to dispose what we create

            CalculateRegisterValues(sck, dout);
            Start();
        }

        /// <summary>
        /// Creates an instance of the NAU7802 Driver class
        /// </summary>
        /// <param name="bus"></param>
        public Hx711(IDigitalOutputPort sck, IDigitalInputPort dout)
        {
            this.SCK = sck;
            this.DOUT = dout;

            CalculateRegisterValues(sck.Pin, dout.Pin);
            Start();
        }

        private void Start()
        {
            ClockLow(); // this resets the chip
            IsSleeping = false;

            // auto-tare
            Thread.Sleep(20);
            Tare();
        }

        /// <summary>
        /// Puts the device into low-power sleep mode
        /// </summary>
        public void Sleep()
        {
            if (IsSleeping) return;

            lock (samplingLock)
            {
                ClockHigh();
                IsSleeping = true;
            }
        }

        /// <summary>
        /// Takes the device out of low-power sleep mode
        /// </summary>
        public void Wake()
        {
            if (!IsSleeping) return;

            lock (samplingLock)
            {
                ClockHigh();
                IsSleeping = false;
            }
        }

        /// <summary>
        /// Tares the sensor, effectively setting the current weight reading to relative zero. 
        /// </summary>
        public void Tare()
        {
            _tareValue = ReadADC();
            Console.WriteLine($"Tare base = {_tareValue}");
        }

        /// <summary>
        /// Calculates the calibration factor of the load cell.  Call this method with a known weight on the sensor, and then use the returned value in a call to <see cref="SetCalibrationFactor(int, Weight)"/> before using the sensor.
        /// </summary>
        /// <returns></returns>
        public int CalculateCalibrationFactor()
        {
            // do a few reads, then return the difference between tare (zero) and this value.
            var reads = 5;
            var sum = 0u;

            for (int i = 0; i < reads; i++)
            {
                sum += ReadADC();
            }

            return (int)((sum / reads) - _tareValue);
        }

        /// <summary>
        /// Sets the sensor's calibration factor based on a factor calculated with a know weight by calling <see cref="CalculateCalibrationFactor"/>.
        /// </summary>
        /// <param name="factor"></param>
        /// <param name="knownValue"></param>
        public void SetCalibrationFactor(int factor, Mass knownValue)
        {
            _gramsPerAdcUnit = (knownValue.Grams / factor);
        }

        /// <summary>
        /// Gets the current sensor weight
        /// </summary>
        /// <returns></returns>
        protected override Task<Mass> ReadSensor()
        {
            if (_gramsPerAdcUnit == 0)
            {
                throw new Exception("Calibration factor has not been set");
            }

            // get an ADC conversion
            var raw = ReadADC();
            // subtract the tare
            var adc = raw - _tareValue;

            // two's complement
            int value;
            if ((raw & 0x800000) != 0)
            {
                value = (int)(~adc + 1) * -1;
            }
            else
            {
                value = (int)adc;
            }

            // convert to grams
            var grams = value * _gramsPerAdcUnit;

            // convert to desired units
            return Task.FromResult(new Mass(grams, Units.Mass.UnitType.Grams));
        }

        private void CalculateRegisterValues(IPin sck, IPin dout)
        {
            // GPIO_BASE = 0x4002 0000
            // BSSR = 0x18
            // Bits 31:16 clear
            // Bits 15:0  set
            // Port offset = 0x0400 * index (with A being index 0)
            int gpio_port = sck.Key.ToString()[1] - 'A';
            _sck_pin = int.Parse(sck.Key.ToString().Substring(2));
            _sck_address = GPIO_BASE | (0x400u * (uint)gpio_port) | BSSR_OFFSET;
            _sck_set = 1u << _sck_pin;
            _sck_clear = 1u << (16 + _sck_pin);

            gpio_port = dout.Key.ToString()[1] - 'A';
            var gpio_pin = int.Parse(dout.Key.ToString().Substring(2));
            _dout_address = GPIO_BASE | (0x400u * (uint)gpio_port) | IDR_OFFSET;
            _dout_mask = 1u << gpio_pin;
        }

        private unsafe void ClockLow()
        {
            // this seems convoluted, but it is intentionally so to keep the compiler from optimizing out out timing.
            // A single call takes roughly 0.2us, but the part requires a minimum of 0.25us for the ADC to settle.  
            // We don't have a simple micro-sleep, so we simply make multiple calls to assert state to suck up the required timing
            for (int i = 0; i < timing_iterations; i++)
            {
                var val = 1u << (16 + _sck_pin); // low
                * (uint*)_sck_address = val;
            }
        }

        private unsafe void ClockHigh()
        {
            for (int i = 0; i < timing_iterations; i++)
            {
                var val = 1u << _sck_pin; // high
                *(uint*)_sck_address = val;
            }
        }

        private unsafe uint ReadADC()
        {
            uint count = 0;

            lock (samplingLock)
            {
                // data line low indicates ready
                while((*(uint*)_dout_address & _dout_mask) != 0)
                {
                    Thread.Sleep(0);
                }

                for (int i = 0; i < 24; i++) // 24 bits of data
                {
                    ClockHigh();
                    count = count << 1;
                    ClockLow();

                    if ((*(uint*)_dout_address & _dout_mask) != 0) // read DOUT state
                    {
                        count++;
                    }
                }
                count ^= 0x800000;

                //set the gain for the next reading
                for (int i = 0; i < (int)Gain; i++)
                {
                    ClockHigh();
                    ClockLow();
                }
            }

            return count;
        }

        public TimeSpan DefaultSamplePeriod { get; } = TimeSpan.FromSeconds(1);

        /// <summary>
        /// The last read Mass.
        /// </summary>
        public Mass? Mass { get; private set; }

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
            catch (Exception ex)
            {
                Console.WriteLine($"HX711 event handler threw: {ex.Message}");
                throw;
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_createdPorts)
            {
                SCK.Dispose();
                DOUT.Dispose();
            }
            IsDisposed = true;
        }

        /// <summary>
        /// Dispose managed resources
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
        }
    }
}