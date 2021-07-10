using System;
using System.Collections;
using System.Threading;
using System.Threading.Tasks;
using Meadow.Hardware;
using Meadow.Peripherals.Sensors;
using Meadow.Units;
using Meadow.Utilities;

namespace Meadow.Foundation.Sensors.Atmospheric
{
    /// <summary>
    /// Device driver for the Adafruit MPRLS Ported Pressure Sensor Breakout
    /// https://www.adafruit.com/product/3965
    /// Device datasheets also available here: https://sensing.honeywell.com/micropressure-mpr-series
    /// </summary>
    public class AdafruitMPRLS : ByteCommsSensorBase<(Pressure? Pressure, Pressure? RawPsiMeasurement)>, IBarometricPressureSensor
    {
        //==== events
        public event EventHandler<IChangeResult<Pressure>> PressureUpdated = delegate { };

        //==== internals
        //Defined in section 6.6.1 of the datasheet.
        private readonly byte[] mprlsMeasurementCommand = { 0xAA, 0x00, 0x00 };

        private int psiMin => 0;
        private int psiMax => 25;

        //This value is set by the manufacturer and can't be changed.
        public const byte Address = 0x18;

        //==== properties
        /// <summary>
        /// Set by the sensor, to tell us it has power.
        /// </summary>
        public bool IsDevicePowered { get; set; }

        /// <summary>
        /// Set by the sensor, to tell us it's busy.
        /// </summary>
        public bool IsDeviceBusy { get; set; }

        /// <summary>
        /// Set by the sensor, to tell us whether or not there's an issue with its own memory.
        /// </summary>
        public bool HasMemoryIntegrityFailed { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public Pressure? RawPsiMeasurement => Conditions.RawPsiMeasurement;

        public Pressure? Pressure => Conditions.Pressure;

        //Tells us that the sensor has reached its pressure limit.
        public bool InternalMathSaturated { get; set; }


        protected override void RaiseEventsAndNotify(IChangeResult<(Pressure? Pressure, Pressure? RawPsiMeasurement)> changeResult)
        {
            if (changeResult.New.Pressure is { } pressure) {
                PressureUpdated?.Invoke(this, new ChangeResult<Pressure>(pressure, changeResult.Old?.Pressure));
            }
            base.RaiseEventsAndNotify(changeResult);
        }

        public AdafruitMPRLS(II2cBus i2cbus, int psiMin = 0, int psiMax = 25)
            : base(i2cbus, Address)
        {
        }

        protected override async Task<(Pressure? Pressure, Pressure? RawPsiMeasurement)> ReadSensor()
        {
            return await Task.Run(async () => {
                //Send the command to the sensor to tell it to do the thing.
                Peripheral.Write(mprlsMeasurementCommand);

                //Datasheet says wait 5 ms.
                await Task.Delay(5);

                while (true) {
                    Peripheral.Read(ReadBuffer.Span[0..1]);

                    //From section 6.5 of the datasheet.
                    IsDevicePowered = BitHelpers.GetBitValue(ReadBuffer.Span[0], 6);
                    IsDeviceBusy = BitHelpers.GetBitValue(ReadBuffer.Span[0], 5);
                    HasMemoryIntegrityFailed = BitHelpers.GetBitValue(ReadBuffer.Span[0], 2);
                    InternalMathSaturated = BitHelpers.GetBitValue(ReadBuffer.Span[0], 0);

                    if (InternalMathSaturated) {
                        throw new InvalidOperationException("Sensor pressure has exceeded max value!");
                    }

                    if (HasMemoryIntegrityFailed) {
                        throw new InvalidOperationException("Sensor internal memory integrity check failed!");
                    }

                    if (!(IsDeviceBusy)) {
                        break;
                    }
                }

                Peripheral.Read(ReadBuffer.Span[0..4]);

                var rawPSIMeasurement = (ReadBuffer.Span[1] << 16) | (ReadBuffer.Span[2] << 8) | ReadBuffer.Span[3];
                //Console.WriteLine(RawPSIMeasurement);

                //From Section 8.0 of the datasheet.
                var calculatedPSIMeasurement = (rawPSIMeasurement - 1677722) * (psiMax - psiMin);
                //Console.WriteLine(CalculatedPSIMeasurement);

                calculatedPSIMeasurement /= 15099494 - 1677722;
                //Console.WriteLine(CalculatedPSIMeasurement);

                calculatedPSIMeasurement += psiMin;
                //Console.WriteLine(CalculatedPSIMeasurement);

                (Pressure? Pressure, Pressure? RawPsiMeasurement) conditions;

                conditions.RawPsiMeasurement = new Pressure(rawPSIMeasurement, Units.Pressure.UnitType.Psi);
                conditions.Pressure = new Pressure(calculatedPSIMeasurement, Units.Pressure.UnitType.Psi);

                return conditions;
            });

        }
    }
}