using Meadow.Devices;
using Meadow.Hardware;
using Meadow.Peripherals.Sensors;
using Meadow.Peripherals.Sensors.Motion;
using Meadow.Units;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Meadow.Foundation.Sensors.Motion
{
    // TODO: Sensor is fully converted but data isn't right:
    // Accel: [X:429.00,Y:-45.00,Z:-1,682.00 (m/s^2)]
    // Temp: 16.00C

    // TODO: Interrupt handling is commented out
    public partial class Mag3110 :
        ByteCommsSensorBase<(MagneticField3D? MagneticField3D, Units.Temperature? Temperature)>,
        ITemperatureSensor
        //IMagnetometer
    {
        //==== events
        public event EventHandler<IChangeResult<MagneticField3D>> MagneticField3dUpdated = delegate { };
        public event EventHandler<IChangeResult<Units.Temperature>> TemperatureUpdated = delegate { };

        //==== internals
        /// <summary>
        /// Interrupt port used to detect then end of a conversion.
        /// </summary>
        protected readonly IDigitalInputPort interruptPort;

        //==== properties
        public MagneticField3D? MagneticField3d => Conditions.MagneticField3D;

        /// <summary>
        /// Temperature of the sensor die.
        /// </summary>
        public Units.Temperature? Temperature => Conditions.Temperature;

        /// <summary>
        /// Change or get the standby status of the sensor.
        /// </summary>
        public bool Standby
        {
            get
            {
                var controlRegister = Peripheral.ReadRegister((byte) Registers.CONTROL_1);
                return (controlRegister & 0x03) == 0;
            }
            set
            {
                var controlRegister = Peripheral.ReadRegister(Registers.CONTROL_1);
                if (value)
                {
                    controlRegister &= 0xfc; // ~0x03
                }
                else
                {
                    controlRegister |= 0x01;
                }
                Peripheral.WriteRegister(Registers.CONTROL_1, controlRegister);
            }
        }

        /// <summary>
        /// Indicate if there is any data ready for reading (x, y or z).
        /// </summary>
        /// <remarks>
        /// See section 5.1.1 of the datasheet.
        /// </remarks>
        public bool IsDataReady
        {
            get { return(Peripheral.ReadRegister(Registers.DR_STATUS) & 0x08) > 0; }
        }

        /// <summary>
        /// Enable or disable interrupts.
        /// </summary>
        /// <remarks>
        /// Interrupts can be triggered when a conversion completes (see section 4.2.5
        /// of the datasheet).  The interrupts are tied to the ZYXDR bit in the DR Status
        /// register.
        /// </remarks>
        public bool DigitalInputsEnabled
        {
            get { return digitalInputsEnabled; }
            set
            {
                Standby = true;
                var cr2 = Peripheral.ReadRegister(Registers.CONTROL_2);
                if (value)
                {
                    cr2 |= 0x80;
                }
                else
                {
                    cr2 &= 0x7f;
                }
                Peripheral.WriteRegister(Registers.CONTROL_2, cr2);
                digitalInputsEnabled = value;
            }
        } protected bool digitalInputsEnabled;

        //==== ctors

        /// <summary>
        /// Create a new MAG3110 object using the default parameters for the component.
        /// </summary>
        /// <param name="device">IO Device.</param>
        /// <param name="interruptPin">Interrupt pin used to detect end of conversions.</param>
        /// <param name="address">Address of the MAG3110 (default = 0x0e).</param>
        /// <param name="speed">Speed of the I2C bus (default = 400 KHz).</param>        
        public Mag3110(IMeadowDevice device, II2cBus i2cBus, IPin interruptPin = null, byte address = Addresses.Mag3110, ushort speed = 400) :
                this (i2cBus, device.CreateDigitalInputPort(interruptPin, InterruptMode.EdgeRising, ResistorMode.Disabled), address)
        { }

        /// <summary>
        /// Create a new MAG3110 object using the default parameters for the component.
        /// </summary>
        /// <param name="interruptPort">Interrupt port used to detect end of conversions.</param>
        /// <param name="address">Address of the MAG3110 (default = 0x0e).</param>
        /// <param name="i2cBus">I2C bus object - default = 400 KHz).</param>        
        public Mag3110(II2cBus i2cBus, IDigitalInputPort interruptPort = null, byte address = Addresses.Mag3110)
            : base (i2cBus, address)
        {
            var deviceID = Peripheral.ReadRegister((byte) Registers.WHO_AM_I);
            if (deviceID != 0xc4)
            {
                throw new Exception("Unknown device ID, " + deviceID + " retruend, 0xc4 expected");
            }
 
            if (interruptPort != null)
            {
                this.interruptPort = interruptPort;
                this.interruptPort.Changed += DigitalInputPortChanged;
            }
            Reset();
        }

        /// <summary>
        /// Reset the sensor.
        /// </summary>
        public void Reset()
        {
            Standby = true;
            Peripheral.WriteRegister(Registers.CONTROL_1, 0x00);
            Peripheral.WriteRegister(Registers.CONTROL_2, 0x80);
            WriteBuffer.Span[0] = Registers.X_OFFSET_MSB;
            WriteBuffer.Span[1] = WriteBuffer.Span[2] = WriteBuffer.Span[3] = 0;
            WriteBuffer.Span[4] = WriteBuffer.Span[5] = WriteBuffer.Span[6] = 0;
            //Peripheral.WriteRegisters(Registers.X_OFFSET_MSB, new byte[] { 0, 0, 0, 0, 0, 0 });
            Peripheral.Write(WriteBuffer.Span[0..7]);
        }

        protected override void RaiseEventsAndNotify(IChangeResult<(MagneticField3D? MagneticField3D, Units.Temperature? Temperature)> changeResult)
        {
            if (changeResult.New.MagneticField3D is { } mag) {
                MagneticField3dUpdated?.Invoke(this, new ChangeResult<MagneticField3D>(mag, changeResult.Old?.MagneticField3D));
            }
            if (changeResult.New.Temperature is { } temp) {
                TemperatureUpdated?.Invoke(this, new ChangeResult<Units.Temperature>(temp, changeResult.Old?.Temperature));
            }
            base.RaiseEventsAndNotify(changeResult);
        }

        protected override Task<(MagneticField3D? MagneticField3D, Units.Temperature? Temperature)> ReadSensor()
        {
            return Task.Run(() => 
            {
                (MagneticField3D? MagneticField3D, Units.Temperature? Temperature) conditions;

                var controlRegister = Peripheral.ReadRegister(Registers.CONTROL_1);
                controlRegister |= 0x02;
                Peripheral.WriteRegister(Registers.CONTROL_1, controlRegister);
                //var data = Peripheral.ReadRegisters(Registers.X_MSB, 6);
                Peripheral.ReadRegister(Registers.X_MSB, ReadBuffer.Span[0..6]);

                conditions.MagneticField3D = new MagneticField3D(
                    new MagneticField((short)((ReadBuffer.Span[0] << 8) | ReadBuffer.Span[1]), MagneticField.UnitType.MicroTesla),
                    new MagneticField((short)((ReadBuffer.Span[2] << 8) | ReadBuffer.Span[3]), MagneticField.UnitType.MicroTesla),
                    new MagneticField((short)((ReadBuffer.Span[4] << 8) | ReadBuffer.Span[5]), MagneticField.UnitType.MicroTesla)
                    );

                conditions.Temperature = new Units.Temperature((sbyte)Peripheral.ReadRegister(Registers.TEMPERATURE), Units.Temperature.UnitType.Celsius);

                return conditions;
            });
        }

        /// <summary>
        /// Interrupt for the MAG3110 conversion complete interrupt.
        /// </summary>
        private void DigitalInputPortChanged(object sender, DigitalPortResult e)
        {
            /*
            if (OnReadingComplete != null)
            {
                Read();
                var readings = new SensorReading();
                readings.X = X;
                readings.Y = Y;
                readings.Z = Z;
                OnReadingComplete(readings);
            }*/
        }
    }
}