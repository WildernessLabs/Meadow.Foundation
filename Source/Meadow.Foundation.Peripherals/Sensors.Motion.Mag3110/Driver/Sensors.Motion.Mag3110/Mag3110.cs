using Meadow.Devices;
using Meadow.Hardware;
using Meadow.Peripherals.Sensors.Motion;
using Meadow.Units;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Meadow.Foundation.Sensors.Motion
{
    public class Mag3110:
        FilterableChangeObservableBase<MagneticField3d>
       // IMagnetometer
    {
        /// <summary>
        /// Register addresses in the sensor.
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

        public MagneticField3d MagneticField3d { get; protected set; } = new MagneticField3d();

        // internal thread lock
        private object _lock = new object();
        private CancellationTokenSource SamplingTokenSource;

        /// <summary>
        /// Gets a value indicating whether the analog input port is currently
        /// sampling the ADC. Call StartSampling() to spin up the sampling process.
        /// </summary>
        /// <value><c>true</c> if sampling; otherwise, <c>false</c>.</value>
        public bool IsSampling { get; protected set; } = false;

        public event EventHandler<IChangeResult<MagneticField3d>> Updated;
        public event EventHandler<IChangeResult<MagneticField3d>> MagneticField3dUpdated;

        /// <summary>
        /// MAG3110 object.
        /// </summary>
        private readonly II2cPeripheral i2cPeripheral;

        /// <summary>
        /// Interrupt port used to detect then end of a conversion.
        /// </summary>
        private readonly IDigitalInputPort interruptPort;

        /// <summary>
        /// Temperature of the sensor die.
        /// </summary>
        public Units.Temperature Temperature => new Units.Temperature((sbyte)i2cPeripheral.ReadRegister(Registers.Temperature), Units.Temperature.UnitType.Celsius);

        /// <summary>
        /// Change or get the standby status of the sensor.
        /// </summary>
        public bool Standby
        {
            get
            {
                var controlRegister = i2cPeripheral.ReadRegister((byte) Registers.Control1);
                return (controlRegister & 0x03) == 0;
            }
            set
            {
                var controlRegister = i2cPeripheral.ReadRegister(Registers.Control1);
                if (value)
                {
                    controlRegister &= 0xfc; // ~0x03
                }
                else
                {
                    controlRegister |= 0x01;
                }
                i2cPeripheral.WriteRegister(Registers.Control1, controlRegister);
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
            get { return(i2cPeripheral.ReadRegister(Registers.DRStatus) & 0x08) > 0; }
        }

        /// <summary>
        /// Enable or disable interrupts.
        /// </summary>
        /// <remarks>
        /// Interrupts can be triggered when a conversion completes (see section 4.2.5
        /// of the datasheet).  The interrupts are tied to the ZYXDR bit in the DR Status
        /// register.
        /// </remarks>
        private bool _digitalInputsEnabled;

        public bool DigitalInputsEnabled
        {
            get { return _digitalInputsEnabled; }
            set
            {
                Standby = true;
                var cr2 = i2cPeripheral.ReadRegister(Registers.Control2);
                if (value)
                {
                    cr2 |= 0x80;
                }
                else
                {
                    cr2 &= 0x7f;
                }
                i2cPeripheral.WriteRegister(Registers.Control2, cr2);
                _digitalInputsEnabled = value;
            }
        }

        /// <summary>
        /// Create a new MAG3110 object using the default parameters for the component.
        /// </summary>
        /// <param name="device">IO Device.</param>
        /// <param name="interruptPin">Interrupt pin used to detect end of conversions.</param>
        /// <param name="address">Address of the MAG3110 (default = 0x0e).</param>
        /// <param name="speed">Speed of the I2C bus (default = 400 KHz).</param>        
        public Mag3110(IMeadowDevice device, II2cBus i2cBus, IPin interruptPin = null, byte address = 0x0e, ushort speed = 400) :
            this (i2cBus, device.CreateDigitalInputPort(interruptPin, InterruptMode.EdgeRising, ResistorMode.Disabled), address) { }

        /// <summary>
        /// Create a new MAG3110 object using the default parameters for the component.
        /// </summary>
        /// <param name="interruptPort">Interrupt port used to detect end of conversions.</param>
        /// <param name="address">Address of the MAG3110 (default = 0x0e).</param>
        /// <param name="i2cBus">I2C bus object - default = 400 KHz).</param>        
        public Mag3110(II2cBus i2cBus, IDigitalInputPort interruptPort = null, byte address = 0x0e)
        {
            i2cPeripheral = new I2cPeripheral(i2cBus, address);

            var deviceID = i2cPeripheral.ReadRegister((byte) Registers.WhoAmI);
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
            i2cPeripheral.WriteRegister(Registers.Control1, 0x00);
            i2cPeripheral.WriteRegister(Registers.Control2, 0x80);
            i2cPeripheral.WriteRegisters(Registers.XOffsetMSB, new byte[] { 0, 0, 0, 0, 0, 0 });
        }

        ///// <summary>
        ///// Convenience method to get the current temperature. For frequent reads, use
        ///// StartSampling() and StopSampling() in conjunction with the SampleBuffer.
        ///// </summary>
        public MagneticField3d Read()
        {
            Update();

            return MagneticField3d;
        }

        ///// <summary>
        ///// Starts continuously sampling the sensor.
        /////
        ///// This method also starts raising `Changed` events and IObservable
        ///// subscribers getting notified.
        ///// </summary>
        public void StartUpdating(int standbyDuration = 1000)
        {
            // thread safety
            lock (_lock)
            {
                if (IsSampling) { return; }

                // state muh-cheen
                IsSampling = true;

                SamplingTokenSource = new CancellationTokenSource();
                CancellationToken ct = SamplingTokenSource.Token;

                MagneticField3d oldConditions;
                ChangeResult<MagneticField3d> result;
                Task.Factory.StartNew(async () =>
                {
                    while (true)
                    {
                        if (ct.IsCancellationRequested)
                        {   // do task clean up here
                            observers.ForEach(x => x.OnCompleted());
                            break;
                        }
                        // capture history
                        oldConditions = MagneticField3d;

                        // read
                        Update();

                        // build a new result with the old and new conditions
                        result = new ChangeResult<MagneticField3d>(MagneticField3d, oldConditions);

                        // let everyone know
                        RaiseChangedAndNotify(result);

                        // sleep for the appropriate interval
                        await Task.Delay(standbyDuration);
                    }
                }, SamplingTokenSource.Token);
            }
        }

        protected void RaiseChangedAndNotify(IChangeResult<MagneticField3d> changeResult)
        {
            Updated?.Invoke(this, changeResult);
            MagneticField3dUpdated?.Invoke(this, changeResult);
            base.NotifyObservers(changeResult);
        }

        /// <summary>
        /// Read data from the sensor 
        /// </summary>
        public void Update()
        {
            var controlRegister = i2cPeripheral.ReadRegister(Registers.Control1);
            controlRegister |= 0x02;
            i2cPeripheral.WriteRegister(Registers.Control1, controlRegister);
            var data = i2cPeripheral.ReadRegisters(Registers.XMSB, 6);

            MagneticField3d = new MagneticField3d(
                new MagneticField((short)((data[0] << 8) | data[1]), MagneticField.UnitType.MicroTesla),
                new MagneticField((short)((data[2] << 8) | data[3]), MagneticField.UnitType.MicroTesla),
                new MagneticField((short)((data[4] << 8) | data[5]), MagneticField.UnitType.MicroTesla)
                );
        }

        /// <summary>
        /// Interrupt for the MAG3110 conversion complete interrupt.
        /// </summary>
        private void DigitalInputPortChanged(object sender, DigitalInputPortChangeResult e)
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