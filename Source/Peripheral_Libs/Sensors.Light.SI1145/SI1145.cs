using System;
using Meadow.Hardware;

namespace Meadow.Foundation.Sensors.Light
{
    public class SI1145
    {
        #region Classes and structures

        /// <summary>
        ///     Registers as defined in section 4.5 of the datasheet.
        /// </summary>
        private class Registers
        {
            /// <summary>
            ///     Part ID.
            /// </summary>
            public static readonly byte PartID = 0x00;
            
            /// <summary>
            ///     
            /// </summary>
            public static readonly byte RevisionID = 0x01;
            
            /// <summary>
            ///     
            /// </summary>
            public static readonly byte SequencerRevision = 0x02;
            
            /// <summary>
            ///     Interrupt configuration.
            /// </summary>
            public static readonly byte InterruptConfiguration = 0x03;
            
            /// <summary>
            ///     Interrupt enable register
            /// </summary>
            public static readonly byte InterruptEnable = 0x04;
            
            /// <summary>
            ///     Hardware key, the system must write 0x17 for normal operation.
            /// </summary>
            public static readonly byte HardwareKey = 0x07;
            
            /// <summary>
            ///     Measuremeant rate 0
            /// </summary>
            /// <remarks>
            ///     MEAS_RATE1 and MEAS_RATE0 together form a 16-bit value: MEAS_RATE [15:0].
            ///     The 16-bit value, when multiplied by 31.25 us, represents the time duration between
            ///     wake-up periods where measurements are made. Once the device wakes up, all
            ///     measurements specified in CHLIST are made.
            /// 
            ///     Note that for the Si1145/6/7 with SEQ_ID=0x01, there is a code error that places
            ///     MEAS_RATE0 at 0x0A with MEAS_RATE1 at 0x08 instead. This will be fixed in
            ///     future revisions of the Si1145/6/7.
            /// </remarks>
            public static readonly byte MeasureRate0 = 0x08;
            
            /// <summary>
            ///     Measuremeant rate 1
            /// </summary>
            /// <remarks>
            ///     MEAS_RATE1 and MEAS_RATE0 together form a 16-bit value: MEAS_RATE [15:0].
            ///     The 16-bit value, when multiplied by 31.25 us, represents the time duration between
            ///     wake-up periods where measurements are made. Once the device wakes up, all
            ///     measurements specified in CHLIST are made.
            /// 
            ///     Note that for the Si1145/6/7 with SEQ_ID=0x01, there is a code error that places
            ///     MEAS_RATE0 at 0x0A with MEAS_RATE1 at 0x08 instead. This will be fixed in
            ///     future revisions of the Si1145/6/7.
            /// </remarks>
            public static readonly byte MeasurementRate1 = 0x09;
            
            /// <summary>
            ///     Proximity sensor LED 1 and 2.
            /// </summary>
            public static readonly byte ProximitySensorLED21 = 0x0f;
            
            /// <summary>
            ///     Proximity sensor LED 3.
            /// </summary>
            public static readonly byte ProximitySensorLED3 = 0x10;
            
            /// <summary>
            ///     Parameter mailbox - used to write values to the sensor.
            /// </summary>
            public static readonly byte ParameterMailbox = 0x17;
            
            /// <summary>
            ///     Command register.
            /// </summary>
            public static readonly byte Command = 0x18;
            
            /// <summary>
            ///     Response register.
            /// </summary>
            public static readonly byte Response = 0x20;
            
            /// <summary>
            ///     Interrupt status.
            /// </summary>
            public static readonly byte InterruptStatus = 0x21;
            
            /// <summary>
            ///     Visible light reading (LSB)
            /// </summary>
            public static readonly byte VisibleLightLSB = 0x22;
            
            /// <summary>
            ///     Visible Light reading (MSB).
            /// </summary>
            public static readonly byte VisibleLightMSB = 0x23;
            
            /// <summary>
            ///     Infrared reading (LSB).
            /// </summary>
            public static readonly byte InfraredLSB = 0x24;
            
            /// <summary>
            ///     Infrared reading (MSB).
            /// </summary>
            public static readonly byte InfraredMSB = 0x25;
            
            /// <summary>
            ///     Ultraviolet light index (LSB).
            /// </summary>
            public static readonly byte UltravioletLightLSB = 0x2c;
            
            /// <summary>
            ///     Ultraviolet light index (MSB).
            /// </summary>
            public static readonly byte UltravioletLightMSB = 0x2d;
            
            /// <summary>
            ///     Chip status (running, suspended, sleep).
            /// </summary>
            public static readonly byte Status = 0x30;
        }

        #endregion Classes and structures

        #region Member variables and fields

        /// <summary>
        ///     Command bus object used to communicate with the SI1145 sensor.
        /// </summary>
        private II2cPeripheral _si1145;

        #endregion Member variables and fields

        #region Properties

        /// <summary>
        ///     Ultraviolet light level.
        /// </summary>
        public double Ultraviolet
        {
            get
            {
                byte[] data = _si1145.ReadRegisters(Registers.UltravioletLightLSB, 2);
                int result = (data[1] << 8) | data[0];
                return(result / 100.0);
            }
        }

        /// <summary>
        ///     Infrared light level.
        /// </summary>
        public double Infrared
        {
            get
            {
                byte[] data = _si1145.ReadRegisters(Registers.InfraredLSB, 2);
                int result = (data[1] << 8) | data[0];
                return(result);
            }
        }

        /// <summary>
        ///     Visible + infrared light level.
        /// </summary>
        public double Visible
        {
            get
            {
                byte[] data = _si1145.ReadRegisters(Registers.VisibleLightLSB, 2);
                int result = (data[1] << 8) | data[0];
                return(result);
            }
        }

        #endregion Properties

        #region Constructors

        /// <summary>
        ///     Default constructor (private to prevent this being called).
        /// </summary>
        private SI1145()
        {
        }

        /// <summary>
        ///     Create a new SI1145 sensor object.
        /// </summary>
        /// <param name="address">Address of the chip on the I2C bus (default to 0x60).</param>
        /// <param name="iscBus">I2cBus (default to 400 KHz).</param>
        public SI1145(II2cBus i2cBus, byte address = 0x60)
        {
            _si1145 = new I2cPeripheral(i2cBus, address);
            if (_si1145.ReadRegister(Registers.PartID) != 0x45)
            {
                throw new Exception("Invalid part ID");
            }
        }

        #endregion Constructors
    }
}
