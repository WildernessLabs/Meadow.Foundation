using System;
using System.Threading;
using System.Threading.Tasks;
using Meadow.Hardware;
using Meadow.Peripherals.Sensors;
using Meadow.Units;
using TU = Meadow.Units.Temperature.UnitType;
using PU = Meadow.Units.Pressure.UnitType;

namespace Meadow.Foundation.Sensors.Atmospheric
{
    public partial class Mpl115a2 :
        ByteCommsSensorBase<(Units.Temperature? Temperature, Pressure? Pressure)>,
        ITemperatureSensor, IBarometricPressureSensor
    {
        public event EventHandler<IChangeResult<Units.Temperature>> TemperatureUpdated = delegate { };
        public event EventHandler<IChangeResult<Pressure>> PressureUpdated = delegate { };

        /// <summary>
        /// Structure holding the doubleing point equivalent of the compensation
        /// coefficients for the sensor.
        /// </summary>
        private struct Coefficients
        {
            public double A0;
            public double B1;
            public double B2;
            public double C12;
        }

        /// <summary>
        /// The temperature, in degrees celsius (°C), from the last reading.
        /// </summary>
        public Units.Temperature? Temperature => Conditions.Temperature;

        /// <summary>
        /// The pressure, in hectopascals (hPa), from the last reading. 1 hPa
        /// is equal to one millibar, or 1/10th of a kilopascal (kPa)/centibar.
        /// </summary>
        public Pressure? Pressure => Conditions.Pressure;

        /// <summary>
        /// doubling point variants of the compensation coefficients from the sensor.
        /// </summary>
        private Coefficients coefficients;

        /// <summary>
        /// Create a new MPL115A2 temperature and humidity sensor object.
        /// </summary>
        /// <param name="address">Sensor address (default to 0x60).</param>
        /// <param name="i2cBus">I2CBus (default to 100 KHz).</param>
        public Mpl115a2(II2cBus i2cBus, byte address = 0x60, int updateIntervalMs = 1000)
            : base(i2cBus, address, updateIntervalMs)
        {
            //var device = new I2cPeripheral(i2cBus, address);
            //mpl115a2 = device;
            //
            //  Update the compensation data from the sensor.  The location and format of the
            //  compensation data can be found on pages 5 and 6 of the datasheet.
            //
            Peripheral.ReadRegister(Registers.A0MSB, ReadBuffer.Span);
            var a0 = (short)(ushort)((ReadBuffer.Span[0] << 8) | ReadBuffer.Span[1]);
            var b1 = (short)(ushort)((ReadBuffer.Span[2] << 8) | ReadBuffer.Span[3]);
            var b2 = (short)(ushort)((ReadBuffer.Span[4] << 8) | ReadBuffer.Span[5]);
            var c12 = (short)(ushort)(((ReadBuffer.Span[6] << 8) | ReadBuffer.Span[7]) >> 2);
            //
            //  Convert the raw compensation coefficients from the sensor into the
            //  doubleing point equivalents to speed up the calculations when readings
            //  are made.
            //
            //  Datasheet, section 3.1
            //  a0 is signed with 12 integer bits followed by 3 fractional bits so divide by 2^3 (8)
            //
            coefficients.A0 = (double)a0 / 8;
            //
            //  b1 is 2 integer bits followed by 7 fractional bits.  The lower bits are all 0
            //  so the format is:
            //      sign i1 I0 F12...F0
            //
            //  So we need to divide by 2^13 (8192)
            //
            coefficients.B1 = (double)b1 / 8192;
            //
            //  b2 is signed integer (1 bit) followed by 14 fractional bits so divide by 2^14 (16384).
            //
            coefficients.B2 = (double)b2 / 16384;
            //
            //  c12 is signed with no integer bits but padded with 9 zeroes:
            //      sign 0.000 000 000 f12...f0
            //
            //  So we need to divide by 2^22 (4,194,304) - 13 doubleing point bits 
            //  plus 9 leading zeroes.
            //
            coefficients.C12 = (double)c12 / 4194304;
        }

        /// <summary>
        /// Inheritance-safe way to raise events and notify observers.
        /// </summary>
        /// <param name="changeResult"></param>
        protected override void RaiseEventsAndNotify(IChangeResult<(Units.Temperature? Temperature, Pressure? Pressure)> changeResult)
        {
            //Updated?.Invoke(this, changeResult);
            if (changeResult.New.Temperature is { } temp) {
                TemperatureUpdated?.Invoke(this, new ChangeResult<Units.Temperature>(temp, changeResult.Old?.Temperature));
            }
            if (changeResult.New.Pressure is { } pressure) {
                PressureUpdated?.Invoke(this, new ChangeResult<Units.Pressure>(pressure, changeResult.Old?.Pressure));
            }
            base.RaiseEventsAndNotify(changeResult);
        }

        /// <summary>
        /// Update the temperature and pressure from the sensor and set the Pressure property.
        /// </summary>
        protected override async Task<(Units.Temperature? Temperature, Pressure? Pressure)> ReadSensor()
        {
            return await Task.Run(async () => {
                (Units.Temperature Temperature, Pressure Pressure) conditions;

                //
                //  Tell the sensor to take a temperature and pressure reading, wait for
                //  3ms (see section 2.2 of the datasheet) and then read the ADC values.
                //
                WriteBuffer.Span[0] = Registers.StartConversion;
                WriteBuffer.Span[1] = 0x00;
                Peripheral.Write(WriteBuffer.Span[0..2]);
                //Peripheral.WriteBytes(new byte[] { Registers.StartConversion, 0x00 });

                await Task.Delay(5);

                //var data = Peripheral.ReadRegisters(Registers.PressureMSB, 4);
                Peripheral.ReadRegister(Registers.PressureMSB, ReadBuffer.Span[0..4]);
                //
                //  Extract the sensor data, note that this is a 10-bit reading so move
                //  the data right 6 bits (see section 3.1 of the datasheet).
                //
                var pAdc = (ushort)(((ReadBuffer.Span[0] << 8) + ReadBuffer.Span[1]) >> 6);
                var tAdc = (ushort)(((ReadBuffer.Span[2] << 8) + ReadBuffer.Span[3]) >> 6);
                conditions.Temperature = new Units.Temperature((float)((tAdc - 498.0) / -5.35) + 25, TU.Celsius);
                //
                //  Now use the calculations in section 3.2 to determine the
                //  current pressure reading.
                //
                const double PRESSURE_CONSTANT = 65.0 / 1023.0;

                // from section 3.2
                // The 10-bit compensated pressure output, Pcomp, is calculated as follows:
                // Pcomp = a0 + ((b1 + (c12 * Tadc)) * Padc) + (b2 * Tadc)
                // where:
                //   Padc is the raw pressure reading
                //   Tadc is the raw temperature reading
                var compensatedPressure = coefficients.A0 + ((coefficients.B1 + (coefficients.C12 * (double)tAdc))
                                                              * (double)pAdc) + (coefficients.B2 * (double)tAdc);

                // Pcomp will produce a value of 0 with an input pressure of 50
                // kPa and will produce a full-scale value of 1023 with an input pressure of 115 kPa.
                // kPa = Pcom * (65/1023) + 50
                conditions.Pressure = new Pressure(((float)(PRESSURE_CONSTANT * compensatedPressure) + 50d), PU.KiloPascal);

                return conditions;
            });
        }
    }
}