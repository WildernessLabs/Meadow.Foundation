using Meadow.Hardware;
using Meadow.Units;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Meadow.Foundation.Sensors.Motion
{
    /// <summary>
    /// Represents the APDS9960 Proximity, Light, RGB, and Gesture Sensor
    /// </summary>
    public partial class Apds9960 : ByteCommsSensorBase<(Color? Color, Illuminance? AmbientLight)>,
        II2cPeripheral
    {
        /// <summary>
        /// Raised when the ambient light value changes
        /// </summary>
        public event EventHandler<IChangeResult<Illuminance>> AmbientLightUpdated = default!;

        /// <summary>
        /// Raised when the color value changes
        /// </summary>
        public event EventHandler<IChangeResult<Color>> ColorUpdated = default!;

        /// <summary>
        /// The default I2C address for the peripheral
        /// </summary>
        public byte DefaultI2cAddress => (byte)Addresses.Default;

        private readonly IDigitalInterruptPort? interruptPort;
        private readonly GestureData gestureData;
        private int gestureUdDelta;
        private int gestureLrDelta;
        private int gestureUdCount;
        private int gestureLrCount;
        private int gestureNearCount;
        private int gestureFarCount;
        private States gestureState;
        private Direction gestureDirection;
        private static readonly byte ERROR = 0xFF;
        private static readonly byte FIFO_PAUSE_TIME = 30;      // Wait period (ms) between FIFO reads

        /// <summary>
        /// The current color value
        /// </summary>
        public Color? Color => Conditions.Color;

        /// <summary>
        /// The current ambient light value
        /// </summary>
        public Illuminance? AmbientLight => Conditions.AmbientLight;

        private readonly Memory<byte> readBuffer = new byte[256];

        /// <summary>
        /// Create a new instance of the APDS9960 communicating over the I2C interface.
        /// </summary>
        /// <param name="i2cBus">SI2C bus object</param>
        /// <param name="interruptPin">The interrupt pin</param>
        public Apds9960(II2cBus i2cBus, IPin? interruptPin)
            : base(i2cBus, (byte)Addresses.Default)
        {
            if (interruptPin != null)
            {
                interruptPort = interruptPin.CreateDigitalInterruptPort(InterruptMode.EdgeRising, ResistorMode.Disabled);
                interruptPort.Changed += InterruptPort_Changed;
            }

            gestureData = new GestureData();

            gestureUdDelta = 0;
            gestureLrDelta = 0;

            gestureUdCount = 0;
            gestureLrCount = 0;

            gestureNearCount = 0;
            gestureFarCount = 0;

            gestureState = 0;
            gestureDirection = (int)Direction.NONE;

            Initialize();
        }

        /// <summary>
        /// Reads data from the sensor
        /// </summary>
        /// <returns>The latest sensor reading</returns>
        protected override Task<(Color? Color, Illuminance? AmbientLight)> ReadSensor()
        {
            (Color? Color, Illuminance? AmbientLight) conditions;

            // TODO: before each of these readings, we need to check to see
            // if that feature is enabled, and if it's not, skip it and set
            // the `conditions.[feature] = null;`

            //---- ambient light
            // TODO: someone needs to verify this
            // have no idea if this conversion is correct. the extent of the datasheet documentation is:
            // "RGBC results can be used to calculate ambient light levels (i.e. Lux) and color temperature (i.e. Kelvin)."
            // NOTE: looks correct, actually. reading ~600 lux in my office and went to 4k LUX when i moved the sensor to the window
            var ambient = ReadAmbientLight();
            conditions.AmbientLight = new Illuminance(ambient, Illuminance.UnitType.Lux);

            //---- color
            // TODO: someone needs to verify this.
            var rgbDivisor = 65536 / 256; // come back as 16-bit values (ushorts). need to be byte.
            var r = ReadRedLight() / rgbDivisor;
            var g = ReadGreenLight() / rgbDivisor;
            var b = ReadBlueLight() / rgbDivisor;
            var a = ambient / rgbDivisor;

            conditions.Color = Foundation.Color.FromRgba(r, g, b, a);

            return Task.FromResult(conditions);
        }

        /// <summary>
        /// Raise events for subscribers and notify of value changes
        /// </summary>
        /// <param name="changeResult">The updated sensor data</param>
        protected override void RaiseEventsAndNotify(IChangeResult<(Color? Color, Illuminance? AmbientLight)> changeResult)
        {
            if (changeResult.New.AmbientLight is { } ambient)
            {
                AmbientLightUpdated?.Invoke(this, new ChangeResult<Illuminance>(ambient, changeResult.Old?.AmbientLight));
            }
            if (changeResult.New.Color is { } color)
            {
                ColorUpdated?.Invoke(this, new ChangeResult<Color>(color, changeResult.Old?.Color));
            }
            base.RaiseEventsAndNotify(changeResult);
        }

        private void InterruptPort_Changed(object sender, DigitalPortResult e)
        {
            //    throw new NotImplementedException();
        }

        private void Initialize()
        {
            var id = BusComms.ReadRegister(Registers.APDS9960_ID);

            //  if ((id != APDS9960_ID) && (id != APDS9960_ID_1) && (id != APDS9960_ID_2))
            //     if(id == 0)
            //    {
            Resolver.Log.Info($"Device found {id}");
            //     }

            Resolver.Log.Info("SetMode");
            SetMode(OperatingModes.ALL, BooleanValues.OFF);

            /* Set default values for ambient light and proximity registers */
            BusComms.WriteRegister(Registers.APDS9960_ATIME, DefaultValues.DEFAULT_ATIME);
            BusComms.WriteRegister(Registers.APDS9960_WTIME, DefaultValues.DEFAULT_WTIME);
            BusComms.WriteRegister(Registers.APDS9960_PPULSE, DefaultValues.DEFAULT_PROX_PPULSE);
            BusComms.WriteRegister(Registers.APDS9960_POFFSET_UR, DefaultValues.DEFAULT_POFFSET_UR);
            BusComms.WriteRegister(Registers.APDS9960_POFFSET_DL, DefaultValues.DEFAULT_POFFSET_DL);
            BusComms.WriteRegister(Registers.APDS9960_CONFIG1, DefaultValues.DEFAULT_CONFIG1);
            SetLEDDrive(DefaultValues.DEFAULT_LDRIVE);

            Resolver.Log.Info("SetProximityGain");
            SetProximityGain(DefaultValues.DEFAULT_PGAIN);
            SetAmbientLightGain(DefaultValues.DEFAULT_AGAIN);
            SetProxIntLowThresh(DefaultValues.DEFAULT_PILT);
            SetProxIntHighThresh(DefaultValues.DEFAULT_PIHT);

            SetLightIntLowThreshold(DefaultValues.DEFAULT_AILT);

            SetLightIntHighThreshold(DefaultValues.DEFAULT_AIHT);

            BusComms.WriteRegister(Registers.APDS9960_PERS, DefaultValues.DEFAULT_PERS);

            BusComms.WriteRegister(Registers.APDS9960_CONFIG2, DefaultValues.DEFAULT_CONFIG2);

            BusComms.WriteRegister(Registers.APDS9960_CONFIG3, DefaultValues.DEFAULT_CONFIG3);

            Resolver.Log.Info("SetGestureEnterThresh");
            SetGestureEnterThresh(DefaultValues.DEFAULT_GPENTH);

            SetGestureExitThresh(DefaultValues.DEFAULT_GEXTH);

            BusComms.WriteRegister(Registers.APDS9960_GCONF1, DefaultValues.DEFAULT_GCONF1);

            SetGestureGain(DefaultValues.DEFAULT_GGAIN);

            SetGestureLEDDrive(DefaultValues.DEFAULT_GLDRIVE);

            SetGestureWaitTime(DefaultValues.DEFAULT_GWTIME);

            BusComms.WriteRegister(Registers.APDS9960_GOFFSET_U, DefaultValues.DEFAULT_GOFFSET);
            BusComms.WriteRegister(Registers.APDS9960_GOFFSET_D, DefaultValues.DEFAULT_GOFFSET);
            BusComms.WriteRegister(Registers.APDS9960_GOFFSET_L, DefaultValues.DEFAULT_GOFFSET);
            BusComms.WriteRegister(Registers.APDS9960_GOFFSET_R, DefaultValues.DEFAULT_GOFFSET);
            BusComms.WriteRegister(Registers.APDS9960_GPULSE, DefaultValues.DEFAULT_GPULSE);
            BusComms.WriteRegister(Registers.APDS9960_GCONF3, DefaultValues.DEFAULT_GCONF3);
            SetGestureIntEnable(DefaultValues.DEFAULT_GIEN);
        }

        /**
         * @brief Reads and returns the contents of the ENABLE register
         *
         * @return Contents of the ENABLE register. 0xFF if error.
         */
        private byte GetMode()
        {
            return BusComms.ReadRegister(Registers.APDS9960_ENABLE);
        }

        /**
         * @brief Enables or disables a feature in the APDS-9960
         *
         * @param[in] mode which feature to enable
         * @param[in] enable ON (1) or OFF (0)
         * @return True if operation success. False otherwise.
         */
        private void SetMode(byte mode, byte enable)
        {
            byte reg_val;

            /* Read current ENABLE register */
            reg_val = GetMode();

            if (reg_val == ERROR)
            {
                return; //ToDo exception
            }

            /* Change bit(s) in ENABLE register */
            enable = (byte)(enable & 0x01);
            if (mode >= 0 && mode <= 6)
            {
                if (enable > 0)
                {
                    reg_val |= (byte)(1 << mode);
                }
                else
                {
                    reg_val &= (byte)~(1 << mode);
                }
            }
            else if (mode == OperatingModes.ALL)
            {
                if (enable > 0)
                {
                    reg_val = 0x7F;
                }
                else
                {
                    reg_val = 0x00;
                }
            }

            /* Write value back to ENABLE register */
            BusComms.WriteRegister(Registers.APDS9960_ENABLE, reg_val);
        }

        /// <summary>
        /// Enable light sensor
        /// </summary>
        /// <param name="interrupts">True to enable interrupts for light</param>
        public void EnableLightSensor(bool interrupts)
        {
            /* Set default gain, interrupts, enable power, and enable sensor */
            SetAmbientLightGain(DefaultValues.DEFAULT_AGAIN);

            SetAmbientLightIntEnable(interrupts);

            EnablePower(true);
            SetMode(OperatingModes.AMBIENT_LIGHT, 1);
        }

        /// <summary>
        /// Disable light sensor
        /// </summary>
        public void DisableLightSensor()
        {
            SetAmbientLightIntEnable(false);
            SetMode(OperatingModes.AMBIENT_LIGHT, 0);
        }

        /// <summary>
        /// Enable proximity sensor
        /// </summary>
        /// <param name="interrupts">True to enable interrupts for proximity</param>
        public void EnableProximitySensor(bool interrupts)
        {
            /* Set default gain, LED, interrupts, enable power, and enable sensor */
            SetProximityGain(DefaultValues.DEFAULT_PGAIN);
            SetLEDDrive(DefaultValues.DEFAULT_LDRIVE);

            if (interrupts)
            {
                SetProximityIntEnable(1);
            }
            else
            {
                SetProximityIntEnable(0);
            }
            EnablePower(true);
            SetMode(OperatingModes.PROXIMITY, 1);
        }

        /// <summary>
        /// Disable proximity sensor
        /// </summary>
        public void DisableProximitySensor()
        {
            SetProximityIntEnable(0);
            SetMode(OperatingModes.PROXIMITY, 0);
        }

        /// <summary>
        /// Starts the gesture recognition engine on the APDS-9960
        /// </summary>
        /// <param name="interrupts"></param>
        /// <returns>Enable interrupts for gestures</returns>
        public bool EnableGestureSensor(bool interrupts)
        {
            /* Enable gesture mode
               Set ENABLE to 0 (power off)
               Set WTIME to 0xFF
               Set AUX to LED_BOOST_100
               Enable PON, WEN, PEN, GEN in ENABLE 
            */
            ResetGestureParameters();
            BusComms.WriteRegister(Registers.APDS9960_WTIME, 0xFF);
            BusComms.WriteRegister(Registers.APDS9960_PPULSE, DefaultValues.DEFAULT_GESTURE_PPULSE);

            SetLEDBoost(GainValues.LED_BOOST_100);

            SetGestureIntEnable((byte)(interrupts ? 1 : 0));

            SetGestureMode(1);
            EnablePower(true);
            SetMode(OperatingModes.WAIT, 1);
            SetMode(OperatingModes.PROXIMITY, 1);
            SetMode(OperatingModes.GESTURE, 1);

            return true;
        }

        /// <summary>
        /// Disable gestures
        /// </summary>
        public void DisableGestureSensor()
        {
            ResetGestureParameters();
            SetGestureIntEnable(0);
            SetGestureMode(0);
            SetMode(OperatingModes.GESTURE, 0);
        }

        /// <summary>
        /// Is a gesture reading available
        /// </summary>
        /// <returns>True if available</returns>
        public bool IsGestureAvailable()
        {
            byte val = BusComms.ReadRegister(Registers.APDS9960_GSTATUS);

            /* Shift and mask out GVALID bit */
            val &= BitFields.APDS9960_GVALID;

            /* Return true/false based on GVALID bit */
            return val == 1;
        }

        /// <summary>
        /// Read the current gesture
        /// </summary>
        /// <returns>The direction</returns>
        /// <exception cref="Exception">Throws if reading gesture data failed</exception>
        public Direction ReadGesture()
        {
            /* Make sure that power and gesture is on and data is valid */
            if (!IsGestureAvailable() || (GetMode() & 0b01000001) == 0x0)
            {
                return (int)Direction.NONE;
            }

            /* Keep looping as long as gesture data is valid */
            while (true)
            {
                byte fifo_level;
                byte bytes_read;

                /* Wait some time to collect next batch of FIFO data */
                Thread.Sleep(FIFO_PAUSE_TIME);

                var gstatus = BusComms.ReadRegister(Registers.APDS9960_GSTATUS);

                /* If we have valid data, read in FIFO */
                if ((gstatus & BitFields.APDS9960_GVALID) == BitFields.APDS9960_GVALID)
                {
                    fifo_level = BusComms.ReadRegister(Registers.APDS9960_GFLVL);

                    /* If there's stuff in the FIFO, read it into our data block */
                    if (fifo_level > 0)
                    {
                        byte len = (byte)(fifo_level * 4);

                        BusComms.ReadRegister(Registers.APDS9960_GFIFO_U, readBuffer.Span[0..len]);

                        Resolver.Log.Info(BitConverter.ToString(readBuffer.Span[0..len].ToArray()));

                        bytes_read = len; //ToDo should we have a check> (byte)fifo_data.Length;

                        if (bytes_read < 1)
                        {
                            throw new Exception();
                        }

                        /* If at least 1 set of data, sort the data into U/D/L/R */
                        if (bytes_read >= 4)
                        {
                            for (int i = 0; i < bytes_read; i += 4)
                            {
                                gestureData.UData[gestureData.Index] = readBuffer.Span[i + 0];
                                gestureData.DData[gestureData.Index] = readBuffer.Span[i + 1];
                                gestureData.LData[gestureData.Index] = readBuffer.Span[i + 2];
                                gestureData.RData[gestureData.Index] = readBuffer.Span[i + 3];
                                gestureData.Index++;
                                gestureData.TotalGestures++;
                            }

                            /* Filter and process gesture data. Decode near/far state */
                            if (ProcessGestureData())
                            {
                                if (DecodeGesture())
                                {
                                    //***TODO: U-Turn Gestures
                                }
                            }

                            /* Reset data */
                            gestureData.Index = 0;
                            gestureData.TotalGestures = 0;
                        }
                    }
                }
                else
                {
                    /* Determine best guessed gesture and clean up */
                    Thread.Sleep(FIFO_PAUSE_TIME);
                    DecodeGesture();

                    ResetGestureParameters();
                    return gestureDirection;
                }
            }
        }

        /// <summary>
        /// Enable power
        /// </summary>
        /// <param name="enable">True to enable, false to disable</param>
        public void EnablePower(bool enable)
        {
            SetMode(OperatingModes.POWER, (byte)(enable ? 1 : 0));
        }

        /// <summary>
        /// Read ambient light value
        /// </summary>
        /// <returns></returns>
        protected ushort ReadAmbientLight()
        {
            byte val = BusComms.ReadRegister(Registers.APDS9960_CDATAL);

            byte val_byte = BusComms.ReadRegister(Registers.APDS9960_CDATAH);

            return (ushort)(val + (val_byte << 8));
        }

        /// <summary>
        /// Read red light value
        /// </summary>
        /// <returns></returns>
        protected ushort ReadRedLight()
        {
            byte val = BusComms.ReadRegister(Registers.APDS9960_RDATAL);

            byte val_byte = BusComms.ReadRegister(Registers.APDS9960_RDATAH);

            return (ushort)(val + (val_byte << 8));
        }

        /// <summary>
        /// Read green light value
        /// </summary>
        /// <returns></returns>
        protected ushort ReadGreenLight()
        {
            byte val = BusComms.ReadRegister(Registers.APDS9960_GDATAL);

            byte val_byte = BusComms.ReadRegister(Registers.APDS9960_GDATAH);

            return (ushort)(val + (val_byte << 8));
        }

        /// <summary>
        /// Read blue light value
        /// </summary>
        /// <returns></returns>
        protected ushort ReadBlueLight()
        {
            byte val = BusComms.ReadRegister(Registers.APDS9960_BDATAL);

            byte val_byte = BusComms.ReadRegister(Registers.APDS9960_BDATAH);

            return (ushort)(val + (val_byte << 8));
        }

        /// <summary>
        /// Read proximity
        /// </summary>
        /// <returns></returns>
        public byte ReadProximity()
        {
            return BusComms.ReadRegister(Registers.APDS9960_PDATA);
        }

        /// <summary>
        /// Reset all gesture data parameters
        /// </summary>
        private void ResetGestureParameters()
        {
            gestureData.Index = 0;
            gestureData.TotalGestures = 0;

            gestureUdDelta = 0;
            gestureLrDelta = 0;

            gestureUdCount = 0;
            gestureLrCount = 0;

            gestureNearCount = 0;
            gestureFarCount = 0;

            gestureState = 0;
            gestureDirection = (int)Direction.NONE;
        }

        /// <summary>
        /// Processes the raw gesture data to determine swipe direction
        /// </summary>
        /// <returns>True if near or far state seen, false otherwise</returns>
        private bool ProcessGestureData()
        {
            byte u_first = 0;
            byte d_first = 0;
            byte l_first = 0;
            byte r_first = 0;
            byte u_last = 0;
            byte d_last = 0;
            byte l_last = 0;
            byte r_last = 0;
            int ud_ratio_first;
            int lr_ratio_first;
            int ud_ratio_last;
            int lr_ratio_last;
            int ud_delta;
            int lr_delta;
            int i;

            /* If we have less than 4 total gestures, that's not enough */
            if (gestureData.TotalGestures <= 4)
            {
                return false;
            }

            /* Check to make sure our data isn't out of bounds */
            if ((gestureData.TotalGestures <= 32) &&
                (gestureData.TotalGestures > 0))
            {
                /* Find the first value in U/D/L/R above the threshold */
                for (i = 0; i < gestureData.TotalGestures; i++)
                {
                    if ((gestureData.UData[i] > GestureParameters.GESTURE_THRESHOLD_OUT) &&
                        (gestureData.DData[i] > GestureParameters.GESTURE_THRESHOLD_OUT) &&
                        (gestureData.LData[i] > GestureParameters.GESTURE_THRESHOLD_OUT) &&
                        (gestureData.RData[i] > GestureParameters.GESTURE_THRESHOLD_OUT))
                    {

                        u_first = gestureData.UData[i];
                        d_first = gestureData.DData[i];
                        l_first = gestureData.LData[i];
                        r_first = gestureData.RData[i];
                        break;
                    }
                }

                /* If one of the _first values is 0, then there is no good data */
                if ((u_first == 0) || (d_first == 0) ||
                    (l_first == 0) || (r_first == 0))
                {
                    return false;
                }
                /* Find the last value in U/D/L/R above the threshold */
                for (i = gestureData.TotalGestures - 1; i >= 0; i--)
                {
                    if ((gestureData.UData[i] > GestureParameters.GESTURE_THRESHOLD_OUT) &&
                        (gestureData.DData[i] > GestureParameters.GESTURE_THRESHOLD_OUT) &&
                        (gestureData.LData[i] > GestureParameters.GESTURE_THRESHOLD_OUT) &&
                        (gestureData.RData[i] > GestureParameters.GESTURE_THRESHOLD_OUT))
                    {

                        u_last = gestureData.UData[i];
                        d_last = gestureData.DData[i];
                        l_last = gestureData.LData[i];
                        r_last = gestureData.RData[i];
                        break;
                    }
                }
            }

            /* Calculate the first vs. last ratio of up/down and left/right */
            ud_ratio_first = (u_first - d_first) * 100 / (u_first + d_first);
            lr_ratio_first = (l_first - r_first) * 100 / (l_first + r_first);
            ud_ratio_last = (u_last - d_last) * 100 / (u_last + d_last);
            lr_ratio_last = (l_last - r_last) * 100 / (l_last + r_last);

            /* Determine the difference between the first and last ratios */
            ud_delta = ud_ratio_last - ud_ratio_first;
            lr_delta = lr_ratio_last - lr_ratio_first;

            /* Accumulate the UD and LR delta values */
            gestureUdDelta += ud_delta;
            gestureLrDelta += lr_delta;

            /* Determine U/D gesture */
            if (gestureUdDelta >= GestureParameters.GESTURE_SENSITIVITY_1)
            {
                gestureUdCount = 1;
            }
            else if (gestureUdDelta <= -GestureParameters.GESTURE_SENSITIVITY_1)
            {
                gestureUdCount = -1;
            }
            else
            {
                gestureUdCount = 0;
            }

            /* Determine L/R gesture */
            if (gestureLrDelta >= GestureParameters.GESTURE_SENSITIVITY_1)
            {
                gestureLrCount = 1;
            }
            else if (gestureLrDelta <= -GestureParameters.GESTURE_SENSITIVITY_1)
            {
                gestureLrCount = -1;
            }
            else
            {
                gestureLrCount = 0;
            }

            /* Determine Near/Far gesture */
            if ((gestureUdCount == 0) && (gestureLrCount == 0))
            {
                if ((Math.Abs(ud_delta) < GestureParameters.GESTURE_SENSITIVITY_2) &&
                    (Math.Abs(lr_delta) < GestureParameters.GESTURE_SENSITIVITY_2))
                {

                    if ((ud_delta == 0) && (lr_delta == 0))
                    {
                        gestureNearCount++;
                    }
                    else if ((ud_delta != 0) || (lr_delta != 0))
                    {
                        gestureFarCount++;
                    }

                    if ((gestureNearCount >= 10) && (gestureFarCount >= 2))
                    {
                        if ((ud_delta == 0) && (lr_delta == 0))
                        {
                            gestureState = States.NEAR_STATE;
                        }
                        else if ((ud_delta != 0) && (lr_delta != 0))
                        {
                            gestureState = States.FAR_STATE;
                        }
                        return true;
                    }
                }
            }
            else
            {
                if ((Math.Abs(ud_delta) < GestureParameters.GESTURE_SENSITIVITY_2) && (Math.Abs(lr_delta) < GestureParameters.GESTURE_SENSITIVITY_2))
                {

                    if ((ud_delta == 0) && (lr_delta == 0))
                    {
                        gestureNearCount++;
                    }

                    if (gestureNearCount >= 10)
                    {
                        gestureUdCount = 0;
                        gestureLrCount = 0;
                        gestureUdDelta = 0;
                        gestureLrDelta = 0;
                    }
                }
            }

            return false;
        }

        /**
         * @brief Determines swipe direction or near/far state
         *
         * @return True if near/far event. False otherwise.
         */
        private bool DecodeGesture()
        {
            /* Return if near or far event is detected */
            if (gestureState == States.NEAR_STATE)
            {
                gestureDirection = Direction.NEAR;
                return true;
            }
            if (gestureState == States.FAR_STATE)
            {
                gestureDirection = Direction.FAR;
                return true;
            }

            /* Determine swipe direction */
            if ((gestureUdCount == -1) && (gestureLrCount == 0))
            {
                gestureDirection = Direction.UP;
            }
            else if ((gestureUdCount == 1) && (gestureLrCount == 0))
            {
                gestureDirection = Direction.DOWN;
            }
            else if ((gestureUdCount == 0) && (gestureLrCount == 1))
            {
                gestureDirection = Direction.RIGHT;
            }
            else if ((gestureUdCount == 0) && (gestureLrCount == -1))
            {
                gestureDirection = Direction.LEFT;
            }
            else if ((gestureUdCount == -1) && (gestureLrCount == 1))
            {
                if (Math.Abs(gestureUdDelta) > Math.Abs(gestureLrDelta))
                {
                    gestureDirection = Direction.UP;
                }
                else
                {
                    gestureDirection = Direction.RIGHT;
                }
            }
            else if ((gestureUdCount == 1) && (gestureLrCount == -1))
            {
                if (Math.Abs(gestureUdDelta) > Math.Abs(gestureLrDelta))
                {
                    gestureDirection = Direction.DOWN;
                }
                else
                {
                    gestureDirection = Direction.LEFT;
                }
            }
            else if ((gestureUdCount == -1) && (gestureLrCount == -1))
            {
                if (Math.Abs(gestureUdDelta) > Math.Abs(gestureLrDelta))
                {
                    gestureDirection = Direction.UP;
                }
                else
                {
                    gestureDirection = Direction.LEFT;
                }
            }
            else if ((gestureUdCount == 1) && (gestureLrCount == 1))
            {
                if (Math.Abs(gestureUdDelta) > Math.Abs(gestureLrDelta))
                {
                    gestureDirection = Direction.DOWN;
                }
                else
                {
                    gestureDirection = Direction.RIGHT;
                }
            }
            else
            {
                return false;
            }

            return true;
        }

        /*******************************************************************************
         * Getters and setters for register values
         ******************************************************************************/

        /**
         * @brief Returns the lower threshold for proximity detection
         *
         * @return lower threshold
         */
        public byte GetProxIntLowThresh()
        {
            return BusComms.ReadRegister(Registers.APDS9960_PILT);
        }

        /**
         * @brief Sets the lower threshold for proximity detection
         *
         * @param[in] threshold the lower proximity threshold
         * @return True if operation successful. False otherwise.
         */
        public void SetProxIntLowThresh(byte threshold)
        {
            BusComms.WriteRegister(Registers.APDS9960_PILT, threshold);
        }

        /**
         * @brief Returns the high threshold for proximity detection
         *
         * @return high threshold
         */
        public byte GetProxIntHighThresh()
        {
            return BusComms.ReadRegister(Registers.APDS9960_PIHT);
        }

        /**
         * @brief Sets the high threshold for proximity detection
         *
         * @param[in] threshold the high proximity threshold
         * @return True if operation successful. False otherwise.
         */
        public void SetProxIntHighThresh(byte threshold)
        {
            BusComms.WriteRegister(Registers.APDS9960_PIHT, threshold);
        }

        /**
         * @brief Returns LED drive strength for proximity and ALS
         *
         * Value    LED Current
         *   0        100 mA
         *   1         50 mA
         *   2         25 mA
         *   3         12.5 mA
         *
         * @return the value of the LED drive strength. 0xFF on failure.
         */
        public byte GetLEDDrive()
        {
            byte val = BusComms.ReadRegister(Registers.APDS9960_CONTROL);

            /* Shift and mask out LED drive bits */
            val = (byte)((val >> 6) & 0b00000011);

            return val;
        }

        /**
         * @brief Sets the LED drive strength for proximity and ALS
         *
         * Value    LED Current
         *   0        100 mA
         *   1         50 mA
         *   2         25 mA
         *   3         12.5 mA
         *
         * @param[in] drive the value (0-3) for the LED drive strength
         * @return True if operation successful. False otherwise.
         */
        public bool SetLEDDrive(byte drive)
        {
            byte val = BusComms.ReadRegister(Registers.APDS9960_CONTROL);

            /* Set bits in register to given value */
            drive &= 0b00000011;
            drive = (byte)(drive << 6);
            val &= 0b00111111;
            val |= drive;

            /* Write register value back into CONTROL register */
            BusComms.WriteRegister(Registers.APDS9960_CONTROL, val);

            return true;
        }

        /**
         * @brief Returns receiver gain for proximity detection
         *
         * Value    Gain
         *   0       1x
         *   1       2x
         *   2       4x
         *   3       8x
         *
         * @return the value of the proximity gain. 0xFF on failure.
         */
        public byte GetProximityGain()
        {
            byte val = BusComms.ReadRegister(Registers.APDS9960_CONTROL);

            /* Shift and mask out PDRIVE bits */
            val = (byte)((val >> 2) & 0b00000011);

            return val;
        }

        /**
         * @brief Sets the receiver gain for proximity detection
         *
         * Value    Gain
         *   0       1x
         *   1       2x
         *   2       4x
         *   3       8x
         *
         * @param[in] drive the value (0-3) for the gain
         * @return True if operation successful. False otherwise.
         */
        public void SetProximityGain(byte drive)
        {
            byte val = BusComms.ReadRegister(Registers.APDS9960_CONTROL);

            /* Set bits in register to given value */
            drive &= 0b00000011;
            drive = (byte)(drive << 2);
            val &= 0b11110011;
            val |= drive;

            /* Write register value back into CONTROL register */
            BusComms.WriteRegister(Registers.APDS9960_CONTROL, val);
        }

        /**
         * @brief Returns receiver gain for the ambient light sensor (ALS)
         *
         * Value    Gain
         *   0        1x
         *   1        4x
         *   2       16x
         *   3       64x
         *
         * @return the value of the ALS gain. 0xFF on failure.
         */
        private byte GetAmbientLightGain()
        {
            byte val = BusComms.ReadRegister(Registers.APDS9960_CONTROL);

            /* Shift and mask out ADRIVE bits */
            val &= 0b00000011;

            return val;
        }

        /**
         * @brief Sets the receiver gain for the ambient light sensor (ALS)
         *
         * Value    Gain
         *   0        1x
         *   1        4x
         *   2       16x
         *   3       64x
         *
         * @param[in] drive the value (0-3) for the gain
         * @return True if operation successful. False otherwise.
         */
        private void SetAmbientLightGain(byte drive)
        {
            byte val = BusComms.ReadRegister(Registers.APDS9960_CONTROL);

            drive &= 0b00000011;
            val &= 0b11111100;
            val |= drive;

            BusComms.WriteRegister(Registers.APDS9960_CONTROL, val);
        }

        /**
         * @brief Get the current LED boost value
         * 
         * Value  Boost Current
         *   0        100%
         *   1        150%
         *   2        200%
         *   3        300%
         *
         * @return The LED boost value. 0xFF on failure.
         */
        private byte GetLEDBoost()
        {
            byte val = BusComms.ReadRegister(Registers.APDS9960_CONTROL);

            /* Shift and mask out LED_BOOST bits */
            val = (byte)((val >> 4) & 0b00000011);

            return val;
        }

        /**
         * @brief Sets the LED current boost value
         *
         * Value  Boost Current
         *   0        100%
         *   1        150%
         *   2        200%
         *   3        300%
         *
         * @param[in] drive the value (0-3) for current boost (100-300%)
         * @return True if operation successful. False otherwise.
         */
        private void SetLEDBoost(byte boost)
        {
            byte val = BusComms.ReadRegister(Registers.APDS9960_CONFIG2);

            /* Set bits in register to given value */
            boost &= 0b00000011;
            boost = (byte)(boost << 4);
            val &= 0b11001111;
            val |= boost;

            BusComms.WriteRegister(Registers.APDS9960_CONFIG2, val);
        }

        /**
         * @brief Gets proximity gain compensation enable
         *
         * @return 1 if compensation is enabled. 0 if not. 0xFF on error.
         */
        private byte GetProxGainCompEnable()
        {
            byte val = BusComms.ReadRegister(Registers.APDS9960_CONFIG3);

            /* Shift and mask out PCMP bits */
            val = (byte)((val >> 5) & 0b00000001);

            return val;
        }

        /**
         * @brief Sets the proximity gain compensation enable
         *
         * @param[in] enable 1 to enable compensation. 0 to disable compensation.
         * @return True if operation successful. False otherwise.
         */
        private void SetProxGainCompEnable(byte enable)
        {
            byte val = BusComms.ReadRegister(Registers.APDS9960_CONFIG3);

            /* Set bits in register to given value */
            enable &= 0b00000001;
            enable = (byte)(enable << 5);
            val &= 0b11011111;
            val |= enable;

            /* Write register value back into CONFIG3 register */
            BusComms.WriteRegister(Registers.APDS9960_CONFIG3, val);
        }

        /**
         * @brief Gets the current mask for enabled/disabled proximity photodiodes
         *
         * 1 = disabled, 0 = enabled
         * Bit    Photodiode
         *  3       UP
         *  2       DOWN
         *  1       LEFT
         *  0       RIGHT
         *
         * @return Current proximity mask for photodiodes. 0xFF on error.
         */
        private byte GetProxPhotoMask()
        {
            byte val = BusComms.ReadRegister(Registers.APDS9960_CONFIG3);

            /* Mask out photodiode enable mask bits */
            val &= 0b00001111;

            return val;
        }

        /**
         * @brief Sets the mask for enabling/disabling proximity photodiodes
         *
         * 1 = disabled, 0 = enabled
         * Bit    Photodiode
         *  3       UP
         *  2       DOWN
         *  1       LEFT
         *  0       RIGHT
         *
         * @param[in] mask 4-bit mask value
         * @return True if operation successful. False otherwise.
         */
        private void SetProxPhotoMask(byte mask)
        {
            byte val = BusComms.ReadRegister(Registers.APDS9960_CONFIG3);

            /* Set bits in register to given value */
            mask &= 0b00001111;
            val &= 0b11110000;
            val |= mask;

            BusComms.WriteRegister(Registers.APDS9960_CONFIG3, val);
        }

        /**
         * @brief Gets the entry proximity threshold for gesture sensing
         *
         * @return Current entry proximity threshold.
         */
        private byte GetGestureEnterThresh()
        {
            byte val = BusComms.ReadRegister(Registers.APDS9960_GPENTH);

            return val;
        }

        /**
         * @brief Sets the entry proximity threshold for gesture sensing
         *
         * @param[in] threshold proximity value needed to start gesture mode
         * @return True if operation successful. False otherwise.
         */
        private void SetGestureEnterThresh(byte threshold)
        {
            BusComms.WriteRegister(Registers.APDS9960_GPENTH, threshold);
        }

        /**
         * @brief Gets the exit proximity threshold for gesture sensing
         *
         * @return Current exit proximity threshold.
         */
        private byte GetGestureExitThresh()
        {
            byte val = BusComms.ReadRegister(Registers.APDS9960_GEXTH);

            return val;
        }

        /**
         * @brief Sets the exit proximity threshold for gesture sensing
         *
         * @param[in] threshold proximity value needed to end gesture mode
         * @return True if operation successful. False otherwise.
         */
        private void SetGestureExitThresh(byte threshold)
        {
            BusComms.WriteRegister(Registers.APDS9960_GEXTH, threshold);
        }

        /**
         * @brief Gets the gain of the photodiode during gesture mode
         *
         * Value    Gain
         *   0       1x
         *   1       2x
         *   2       4x
         *   3       8x
         *
         * @return the current photodiode gain. 0xFF on error.
         */
        private byte GetGestureGain()
        {
            byte val = BusComms.ReadRegister(Registers.APDS9960_GCONF2);

            /* Shift and mask out GGAIN bits */
            val = (byte)((val >> 5) & 0b00000011);

            return val;
        }

        /**
         * @brief Sets the gain of the photodiode during gesture mode
         *
         * Value    Gain
         *   0       1x
         *   1       2x
         *   2       4x
         *   3       8x
         *
         * @param[in] gain the value for the photodiode gain
         * @return True if operation successful. False otherwise.
         */
        private void SetGestureGain(byte gain)
        {
            byte val = BusComms.ReadRegister(Registers.APDS9960_GCONF2);

            /* Set bits in register to given value */
            gain &= 0b00000011;
            gain = (byte)(gain << 5);
            val &= 0b10011111;
            val |= gain;

            /* Write register value back into GCONF2 register */
            BusComms.WriteRegister(Registers.APDS9960_GCONF2, val);
        }

        /**
         * @brief Gets the drive current of the LED during gesture mode
         *
         * Value    LED Current
         *   0        100 mA
         *   1         50 mA
         *   2         25 mA
         *   3         12.5 mA
         *
         * @return the LED drive current value. 0xFF on error.
         */
        private byte GetGestureLEDDrive()
        {
            /* Read value from GCONF2 register */
            byte val = BusComms.ReadRegister(Registers.APDS9960_GCONF2);

            /* Shift and mask out GLDRIVE bits */
            val = (byte)((val >> 3) & 0b00000011);

            return val;
        }

        /**
         * @brief Sets the LED drive current during gesture mode
         *
         * Value    LED Current
         *   0        100 mA
         *   1         50 mA
         *   2         25 mA
         *   3         12.5 mA
         *
         * @param[in] drive the value for the LED drive current
         * @return True if operation successful. False otherwise.
         */
        private void SetGestureLEDDrive(byte drive)
        {
            byte val = BusComms.ReadRegister(Registers.APDS9960_GCONF2);

            /* Set bits in register to given value */
            drive &= 0b00000011;
            drive = (byte)(drive << 3);
            val &= 0b11100111;
            val |= drive;

            BusComms.WriteRegister(Registers.APDS9960_GCONF2, val);
        }

        /**
         * @brief Gets the time in low power mode between gesture detections
         *
         * Value    Wait time
         *   0          0 ms
         *   1          2.8 ms
         *   2          5.6 ms
         *   3          8.4 ms
         *   4         14.0 ms
         *   5         22.4 ms
         *   6         30.8 ms
         *   7         39.2 ms
         *
         * @return the current wait time between gestures. 0xFF on error.
         */
        private byte GetGestureWaitTime()
        {
            byte val = BusComms.ReadRegister(Registers.APDS9960_GCONF2);

            /* Mask out GWTIME bits */
            val &= 0b00000111;

            return val;
        }

        /**
         * @brief Sets the time in low power mode between gesture detections
         *
         * Value    Wait time
         *   0          0 ms
         *   1          2.8 ms
         *   2          5.6 ms
         *   3          8.4 ms
         *   4         14.0 ms
         *   5         22.4 ms
         *   6         30.8 ms
         *   7         39.2 ms
         *
         * @param[in] the value for the wait time
         * @return True if operation successful. False otherwise.
         */
        private void SetGestureWaitTime(byte time)
        {
            byte val = BusComms.ReadRegister(Registers.APDS9960_GCONF2);

            /* Set bits in register to given value */
            time &= 0b00000111;
            val &= 0b11111000;
            val |= time;

            BusComms.WriteRegister(Registers.APDS9960_GCONF2, val);
        }

        /**
         * @brief Gets the low threshold for ambient light interrupts
         *
         * @param[out] threshold current low threshold stored on the APDS-9960
         * @return True if operation successful. False otherwise.
         */
        private ushort GetLightIntLowThreshold()
        {
            var threshold = BusComms.ReadRegister(Registers.APDS9960_AILTL);

            var val_byte = BusComms.ReadRegister(Registers.APDS9960_AILTH);

            return (byte)(threshold + (val_byte << 8));
        }

        /**
         * @brief Sets the low threshold for ambient light interrupts
         *
         * @param[in] threshold low threshold value for interrupt to trigger
         * @return True if operation successful. False otherwise.
         */
        private void SetLightIntLowThreshold(ushort threshold)
        {
            byte val_low;
            byte val_high;

            /* Break 16-bit threshold into 2 8-bit values */
            val_low = (byte)(threshold & 0x00FF);
            val_high = (byte)((threshold & 0xFF00) >> 8);

            BusComms.WriteRegister(Registers.APDS9960_AILTL, val_low);
            BusComms.WriteRegister(Registers.APDS9960_AILTL, val_high);
        }

        /**
         * @brief Gets the high threshold for ambient light interrupts
         *
         * @param[out] threshold current low threshold stored on the APDS-9960
         * @return True if operation successful. False otherwise.
         */
        private ushort GetLightIntHighThreshold()
        {
            var threshold = BusComms.ReadRegister(Registers.APDS9960_AIHTL);

            var val_byte = BusComms.ReadRegister(Registers.APDS9960_AIHTH);

            return (byte)(threshold + (val_byte << 8));
        }

        /**
         * @brief Sets the high threshold for ambient light interrupts
         *
         * @param[in] threshold high threshold value for interrupt to trigger
         * @return True if operation successful. False otherwise.
         */
        private void SetLightIntHighThreshold(ushort threshold)
        {
            /* Break 16-bit threshold into 2 8-bit values */
            byte val_low = (byte)(threshold & 0x00FF);
            byte val_high = (byte)((threshold & 0xFF00) >> 8);

            BusComms.WriteRegister(Registers.APDS9960_AIHTL, val_low);
            BusComms.WriteRegister(Registers.APDS9960_AIHTH, val_high);
        }

        /**
         * @brief Gets the low threshold for proximity interrupts
         *
         * @param[out] threshold current low threshold stored on the APDS-9960
         * @return True if operation successful. False otherwise.
         */
        private byte GetProximityIntLowThreshold()
        {
            return BusComms.ReadRegister(Registers.APDS9960_PILT);
        }

        /**
         * @brief Sets the low threshold for proximity interrupts
         *
         * @param[in] threshold low threshold value for interrupt to trigger
         * @return True if operation successful. False otherwise.
         */
        private void SetProximityIntLowThreshold(byte threshold)
        {
            BusComms.WriteRegister(Registers.APDS9960_PILT, threshold);
        }

        /**
         * @brief Gets the high threshold for proximity interrupts
         *
         * @param[out] threshold current low threshold stored on the APDS-9960
         * @return True if operation successful. False otherwise.
         */
        private byte GetProximityIntHighThreshold()
        {
            return BusComms.ReadRegister(Registers.APDS9960_PIHT);
        }

        /**
         * @brief Sets the high threshold for proximity interrupts
         *
         * @param[in] threshold high threshold value for interrupt to trigger
         * @return True if operation successful. False otherwise.
         */
        private void SetProximityIntHighThreshold(byte threshold)
        {
            BusComms.WriteRegister(Registers.APDS9960_PIHT, threshold);
        }

        /**
         * @brief Gets if ambient light interrupts are enabled or not
         *
         * @return 1 if interrupts are enabled, 0 if not. 0xFF on error.
         */
        private byte GetAmbientLightIntEnable()
        {
            byte val = BusComms.ReadRegister(Registers.APDS9960_ENABLE);

            /* Shift and mask out AIEN bit */
            val = (byte)((val >> 4) & 0b00000001);

            return val;
        }

        /**
         * @brief Turns ambient light interrupts on or off
         *
         * @param[in] enable 1 to enable interrupts, 0 to turn them off
         * @return True if operation successful. False otherwise.
         */
        private void SetAmbientLightIntEnable(bool enable)
        {
            byte val = BusComms.ReadRegister(Registers.APDS9960_ENABLE);

            /* Set bits in register to given value */
            byte data = (byte)(enable ? 0x1 : 0x0);
            data &= 0b00000001;
            data = (byte)(data << 4);
            val &= 0b11101111;
            val |= data;

            BusComms.WriteRegister(Registers.APDS9960_ENABLE, val);
        }

        /**
         * @brief Gets if proximity interrupts are enabled or not
         *
         * @return 1 if interrupts are enabled, 0 if not. 0xFF on error.
         */
        private byte GetProximityIntEnable()
        {
            byte val = BusComms.ReadRegister(Registers.APDS9960_ENABLE);

            /* Shift and mask out PIEN bit */
            val = (byte)((val >> 5) & 0b00000001);

            return val;
        }

        /**
         * @brief Turns proximity interrupts on or off
         *
         * @param[in] enable 1 to enable interrupts, 0 to turn them off
         * @return True if operation successful. False otherwise.
         */
        private void SetProximityIntEnable(byte enable)
        {
            byte val = BusComms.ReadRegister(Registers.APDS9960_ENABLE);

            /* Set bits in register to given value */
            enable &= 0b00000001;
            enable = (byte)(enable << 5);
            val &= 0b11011111;
            val |= enable;

            BusComms.WriteRegister(Registers.APDS9960_ENABLE, val);
        }

        /**
         * @brief Gets if gesture interrupts are enabled or not
         *
         * @return 1 if interrupts are enabled, 0 if not. 0xFF on error.
         */
        private byte GetGestureIntEnable()
        {
            byte val = BusComms.ReadRegister(Registers.APDS9960_GCONF4);

            /* Shift and mask out GIEN bit */
            val = (byte)((val >> 1) & 0b00000001);

            return val;
        }

        /**
         * @brief Turns gesture-related interrupts on or off
         *
         * @param[in] enable 1 to enable interrupts, 0 to turn them off
         * @return True if operation successful. False otherwise.
         */
        private void SetGestureIntEnable(byte enable)
        {
            byte val = BusComms.ReadRegister(Registers.APDS9960_GCONF4);

            /* Set bits in register to given value */
            enable &= 0b00000001;
            enable = (byte)(enable << 1);
            val &= 0b11111101;
            val |= enable;

            BusComms.WriteRegister(Registers.APDS9960_GCONF4, val);
        }

        /**
         * @brief Clears the ambient light interrupt
         *
         * @return True if operation completed successfully. False otherwise.
         */
        private void ClearAmbientLightInt()
        {
            BusComms.WriteRegister(Registers.APDS9960_AICLEAR, 0);
        }

        /**
         * @brief Clears the proximity interrupt
         *
         * @return True if operation completed successfully. False otherwise.
         */
        private void ClearProximityInt()
        {
            BusComms.WriteRegister(Registers.APDS9960_PICLEAR, 0);
        }

        /**
         * @brief Tells if the gesture state machine is currently running
         *
         * @return 1 if gesture state machine is running, 0 if not. 0xFF on error.
         */
        private byte GetGestureMode()
        {
            byte val = BusComms.ReadRegister(Registers.APDS9960_GCONF4);

            /* Mask out GMODE bit */
            val &= 0b00000001;

            return val;
        }

        /**
         * @brief Tells the state machine to either enter or exit gesture state machine
         *
         * @param[in] mode 1 to enter gesture state machine, 0 to exit.
         * @return True if operation successful. False otherwise.
         */
        private void SetGestureMode(byte mode)
        {
            byte val = BusComms.ReadRegister(Registers.APDS9960_GCONF4);

            /* Set bits in register to given value */
            mode &= 0b00000001;
            val &= 0b11111110;
            val |= mode;

            BusComms.WriteRegister(Registers.APDS9960_GCONF4, val);
        }

        /* Container for gesture data */
        private class GestureData
        {
            public byte[] UData { get; set; } = new byte[32];
            public byte[] DData { get; set; } = new byte[32];
            public byte[] LData { get; set; } = new byte[32];
            public byte[] RData { get; set; } = new byte[32];
            public byte Index { get; set; }
            public byte TotalGestures { get; set; }
            public byte InThreshold { get; set; }
            public byte OutThreshold { get; set; }
        }
    }
}