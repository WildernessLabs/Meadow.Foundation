using Meadow.Hardware;
using System;

namespace Meadow.Foundation.Sensors.Motion
{
    public class MAG3110
    {
        #region Structures

        /// <summary>
        ///     Sensor readings to be passed back when an interrupt is generated.
        /// </summary>
        public struct SensorReading
        {
            public short X;
            public short Y;
            public short Z;
        }

        /// <summary>
        ///     Register addresses in the sensor.
        /// </summary>
        private static class Registers
        {
            public static readonly byte DRStatus = 0x00;
            public static readonly byte XMSB = 0x01;
            public static readonly byte XLSB = 0x02;
            public static readonly byte YMSB = 0x03;
            public static readonly byte YLSB = 0x04;
            public static readonly byte ZMSB = 0x05;
            public static readonly byte ZLSB = 0x06;
            public static readonly byte WhoAmI = 0x07;
            public static readonly byte SystemMode = 0x08;
            public static readonly byte XOffsetMSB = 0x09;
            public static readonly byte XOffsetLSB = 0x0a;
            public static readonly byte YOffsetMSB = 0x0b;
            public static readonly byte YOffsetLSB = 0x0c;
            public static readonly byte ZOffsetMSB = 0x0d;
            public static readonly byte ZOffsetLSB = 0x0e;
            public static readonly byte Temperature = 0x0f;
            public static readonly byte Control1 = 0x10;
            public static readonly byte Control2 = 0x11;
        }

        #endregion Structures

        #region Delegates and events

        /// <summary>
        ///     Delegate for the OnDataReceived event.
        /// </summary>
        /// <param name="sensorReading">Sensor readings from the MAG3110.</param>
        public delegate void ReadingComplete(SensorReading sensorReading);

        /// <summary>
        ///     Generated when the sensor indicates that data is ready for processing.
        /// </summary>
        public event ReadingComplete OnReadingComplete;

        #endregion Delegates and events

        #region Member variables / fields

        /// <summary>
        ///     MAG3110 object.
        /// </summary>
        private readonly II2cPeripheral _mag3110;

        /// <summary>
        ///     Interrupt port used to detect then end of a conversion.
        /// </summary>
        private readonly IDigitalInputPort _digitalInputPort;

        #endregion Member variables / fields

        #region Properties

        /// <summary>
        ///     Reading from the X axis.
        /// </summary>
        /// <remarks>
        ///     Data in this property is only current after a call to Read.
        /// </remarks>
        public short X { get; private set; }

        /// <summary>
        ///     Reading from the Y axis.
        /// </summary>
        /// <remarks>
        ///     Data in this property is only current after a call to Read.
        /// </remarks>
        public short Y { get; private set; }

        /// <summary>
        ///     Reading from the Z axis.
        /// </summary>
        /// <remarks>
        ///     Data in this property is only current after a call to Read.
        /// </remarks>
        public short Z { get; private set; }

        /// <summary>
        ///     Temperature of the sensor die.
        /// </summary>
        public sbyte Temperature
        {
            get { return(sbyte) _mag3110.ReadRegister((byte) Registers.Temperature); }
        }

        /// <summary>
        ///     Change or get the standby status of the sensor.
        /// </summary>
        public bool Standby
        {
            get
            {
                var controlRegister = _mag3110.ReadRegister((byte) Registers.Control1);
                return (controlRegister & 0x03) == 0;
            }
            set
            {
                var controlRegister = _mag3110.ReadRegister((byte) Registers.Control1);
                if (value)
                {
                    controlRegister &= 0xfc; // ~0x03
                }
                else
                {
                    controlRegister |= 0x01;
                }
                _mag3110.WriteRegister((byte) Registers.Control1, controlRegister);
            }
        }

        /// <summary>
        ///     Indicate if there is any data ready for reading (x, y or z).
        /// </summary>
        /// <remarks>
        ///     See section 5.1.1 of the datasheet.
        /// </remarks>
        public bool DataReady
        {
            get { return(_mag3110.ReadRegister((byte) Registers.DRStatus) & 0x08) > 0; }
        }

        /// <summary>
        ///     Enable or disable interrupts.
        /// </summary>
        /// <remarks>
        ///     Interrupts can be triggered when a conversion completes (see section 4.2.5
        ///     of the datasheet).  The interrupts are tied to the ZYXDR bit in the DR Status
        ///     register.
        /// </remarks>
        private bool _digitalInputsEnabled;

        public bool DigitalInputsEnabled
        {
            get { return _digitalInputsEnabled; }
            set
            {
                Standby = true;
                var cr2 = _mag3110.ReadRegister((byte) Registers.Control2);
                if (value)
                {
                    cr2 |= 0x80;
                }
                else
                {
                    cr2 &= 0x7f;
                }
                _mag3110.WriteRegister((byte) Registers.Control2, cr2);
                _digitalInputsEnabled = value;
            }
        }

        #endregion

        #region Constructors

        /// <summary>
        ///     Default constructor is private to prevent the developer from calling it.
        /// </summary>
        private MAG3110()
        {
        }

        /// <summary>
        /// Create a new MAG3110 object using the default parameters for the component.
        /// </summary>
        /// <param name="device">IO Device.</param>
        /// <param name="interruptPin">Interrupt pin used to detect end of conversions.</param>
        /// <param name="address">Address of the MAG3110 (default = 0x0e).</param>
        /// <param name="speed">Speed of the I2C bus (default = 400 KHz).</param>        
        public MAG3110(IIODevice device, II2cBus i2cBus, IPin interruptPin = null, byte address = 0x0e, ushort speed = 400) :
            this (i2cBus, device.CreateDigitalInputPort(interruptPin, InterruptMode.EdgeRising, ResistorMode.Disabled), address) { }

        /// <summary>
        /// Create a new MAG3110 object using the default parameters for the component.
        /// </summary>
        /// <param name="interruptPort">Interrupt port used to detect end of conversions.</param>
        /// <param name="address">Address of the MAG3110 (default = 0x0e).</param>
        /// <param name="i2cBus">I2C bus object - default = 400 KHz).</param>        
        public MAG3110(II2cBus i2cBus, IDigitalInputPort interruptPort = null, byte address = 0x0e)
        {
            _mag3110 = new I2cPeripheral(i2cBus, address);

            var deviceID = _mag3110.ReadRegister((byte) Registers.WhoAmI);
            if (deviceID != 0xc4)
            {
                throw new Exception("Unknown device ID, " + deviceID + " retruend, 0xc4 expected");
            }
 
            if (interruptPort != null)
            {
                _digitalInputPort = interruptPort;
                _digitalInputPort.Changed += DigitalInputPortChanged;
            }
            Reset();
        }

        #endregion Constructors

        #region Methods

        /// <summary>
        ///     Reset the sensor.
        /// </summary>
        public void Reset()
        {
            Standby = true;
            _mag3110.WriteRegister((byte) Registers.Control1, 0x00);
            _mag3110.WriteRegister((byte) Registers.Control2, 0x80);
            _mag3110.WriteRegisters((byte) Registers.XOffsetMSB, new byte[] { 0, 0, 0, 0, 0, 0 });
        }

        /// <summary>
        ///     Force the sensor to make a reading and update the relevanyt properties.
        /// </summary>
        public void Read()
        {
            var controlRegister = _mag3110.ReadRegister((byte) Registers.Control1);
            controlRegister |= 0x02;
            _mag3110.WriteRegister((byte) Registers.Control1, controlRegister);
            var data = _mag3110.ReadRegisters((byte) Registers.XMSB, 6);
            X = (short) ((data[0] << 8) | data[1]);
            Y = (short) ((data[2] << 8) | data[3]);
            Z = (short) ((data[4] << 8) | data[5]);
        }

        #endregion Methods

        #region Event handlers

        /// <summary>
        ///     Interrupt from the MAG3110 conversion complete interrupt.
        /// </summary>
        private void DigitalInputPortChanged(object sender, DigitalInputPortEventArgs e)
        {
            if (OnReadingComplete != null)
            {
                Read();
                var readings = new SensorReading();
                readings.X = X;
                readings.Y = Y;
                readings.Z = Z;
                OnReadingComplete(readings);
            }
        }

        #endregion Event handlers
    }
}