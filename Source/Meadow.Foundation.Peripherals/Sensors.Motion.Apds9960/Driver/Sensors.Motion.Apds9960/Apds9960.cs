using Meadow.Devices;
using Meadow.Hardware;
using Meadow.Units;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Meadow.Foundation.Sensors.Motion
{
    // TODO: the light stuff seems to work. not sure on the RGB conversion though.
    //  haven't tested any of the gesture stuff.
    //  need to add distance

    public partial class Apds9960 : ByteCommsSensorBase<(Color? Color, Illuminance? AmbientLight)>
    {
        //==== events
        public event EventHandler<IChangeResult<Illuminance>> AmbientLightUpdated = delegate { };
        public event EventHandler<IChangeResult<Color>> ColorUpdated = delegate { };

        //==== internals
        IDigitalInputPort interruptPort;

        GestureData gestureData;
        int gestureUdDelta;
        int gestureLrDelta;
        int gestureUdCount;
        int gestureLrCount;
        int gestureNearCount;
        int gestureFarCount;
        States gestureState;
        Direction gestureDirection;

        /* Error code for returned values */
        static readonly byte ERROR = 0xFF;

        /* Misc parameters */
        static readonly byte FIFO_PAUSE_TIME = 30;      // Wait period (ms) between FIFO reads

        //==== properties

        public Color? Color => Conditions.Color;
        public Illuminance? AmbientLight => Conditions.AmbientLight;

        //==== ctors

        /// <summary>
        /// Create a new instance of the APDS9960 communicating over the I2C interface.
        /// </summary>
        /// <param name="address">Address of the I2C sensor</param>
        /// <param name="i2cBus">SI2C bus object</param>
        public Apds9960(IMeadowDevice device, II2cBus i2cBus, IPin interruptPin)
            : base(i2cBus, 0x39)
        {
            if (interruptPin != null)
            {
                interruptPort = device.CreateDigitalInputPort(interruptPin, InterruptMode.EdgeRising, ResistorMode.Disabled);
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

        //==== Sensor overrides

        protected override Task<(Color? Color, Illuminance? AmbientLight)> ReadSensor()
        {
            return Task.Run(() => {

                (Color? Color, Illuminance? AmbientLight) conditions;

                // TODO: before each of these readings, we need to check to see
                // if that feature is enabled, and if it's not, skip it and set
                // the `conditions.[feature] = null;`

                //---- ambient light
                // TODO: someone needs to verify this
                // have no idea if this conversion is correct. the exten of the datasheet documentation is:
                // "RGBC results can be used to calculate ambient light levels (i.e. Lux) and color temperature (i.e. Kelvin)."
                // NOTE: looks correct, actually. reading ~600 lux in my office and went to 4k LUX when i moved the sensor to the window
                var ambient = ReadAmbientLight();
                conditions.AmbientLight = new Illuminance(ambient, Illuminance.UnitType.Lux);

                //---- color
                // TODO: someone needs to verify this.
                var rgbDivisor = 65536 / 256; // come back as 16-bit values (ushorts). need to be byte.
                var r = (int)(ReadRedLight() / rgbDivisor);
                var g = (int)(ReadGreenLight() / rgbDivisor);
                var b = (int)(ReadBlueLight() / rgbDivisor);
                var a = (int)(ambient / rgbDivisor);

                conditions.Color = Foundation.Color.FromRgba(r, g, b, a);

                return conditions;
            });
        }

        protected override void RaiseEventsAndNotify(IChangeResult<(Color? Color, Illuminance? AmbientLight)> changeResult)
        {
            if (changeResult.New.AmbientLight is { } ambient) {
                AmbientLightUpdated?.Invoke(this, new ChangeResult<Illuminance>(ambient, changeResult.Old?.AmbientLight));
            }
            if (changeResult.New.Color is { } color) {
                ColorUpdated?.Invoke(this, new ChangeResult<Color>(color, changeResult.Old?.Color));
            }
            base.RaiseEventsAndNotify(changeResult);
        }

        //==== methods

        private void InterruptPort_Changed(object sender, DigitalPortResult e)
        {
        //    throw new NotImplementedException();
        }

        void Initialize()
        {
            var id = Peripheral.ReadRegister(Registers.APDS9960_ID);

         //  if ((id != APDS9960_ID) && (id != APDS9960_ID_1) && (id != APDS9960_ID_2))
       //     if(id == 0)
        //    {
                Console.WriteLine($"Device found {id}");
       //     }

            Console.WriteLine("SetMode");
            SetMode(OperatingModes.ALL, BooleanValues.OFF);

            /* Set default values for ambient light and proximity registers */
            Peripheral.WriteRegister(Registers.APDS9960_ATIME, DefaultValues.DEFAULT_ATIME);
            Peripheral.WriteRegister(Registers.APDS9960_WTIME, DefaultValues.DEFAULT_WTIME);
            Peripheral.WriteRegister(Registers.APDS9960_PPULSE, DefaultValues.DEFAULT_PROX_PPULSE);
            Peripheral.WriteRegister(Registers.APDS9960_POFFSET_UR, DefaultValues.DEFAULT_POFFSET_UR);
            Peripheral.WriteRegister(Registers.APDS9960_POFFSET_DL, DefaultValues.DEFAULT_POFFSET_DL);
            Peripheral.WriteRegister(Registers.APDS9960_CONFIG1, DefaultValues.DEFAULT_CONFIG1);
            SetLEDDrive(DefaultValues.DEFAULT_LDRIVE);

            Console.WriteLine("SetProximityGain");
            SetProximityGain(DefaultValues.DEFAULT_PGAIN);
            SetAmbientLightGain(DefaultValues.DEFAULT_AGAIN);
            SetProxIntLowThresh(DefaultValues.DEFAULT_PILT);
            SetProxIntHighThresh(DefaultValues.DEFAULT_PIHT);

            SetLightIntLowThreshold(DefaultValues.DEFAULT_AILT);

            SetLightIntHighThreshold(DefaultValues.DEFAULT_AIHT);

            Peripheral.WriteRegister(Registers.APDS9960_PERS, DefaultValues.DEFAULT_PERS);

            Peripheral.WriteRegister(Registers.APDS9960_CONFIG2, DefaultValues.DEFAULT_CONFIG2);

            Peripheral.WriteRegister(Registers.APDS9960_CONFIG3, DefaultValues.DEFAULT_CONFIG3);

            Console.WriteLine("SetGestureEnterThresh");
            SetGestureEnterThresh(DefaultValues.DEFAULT_GPENTH);
            
            SetGestureExitThresh(DefaultValues.DEFAULT_GEXTH);
            
            Peripheral.WriteRegister(Registers.APDS9960_GCONF1, DefaultValues.DEFAULT_GCONF1);
            
            SetGestureGain(DefaultValues.DEFAULT_GGAIN);
            
            SetGestureLEDDrive(DefaultValues.DEFAULT_GLDRIVE);
            
            SetGestureWaitTime(DefaultValues.DEFAULT_GWTIME);
            
            Peripheral.WriteRegister(Registers.APDS9960_GOFFSET_U, DefaultValues.DEFAULT_GOFFSET);
            Peripheral.WriteRegister(Registers.APDS9960_GOFFSET_D, DefaultValues.DEFAULT_GOFFSET);
            Peripheral.WriteRegister(Registers.APDS9960_GOFFSET_L, DefaultValues.DEFAULT_GOFFSET);
            Peripheral.WriteRegister(Registers.APDS9960_GOFFSET_R, DefaultValues.DEFAULT_GOFFSET);
            Peripheral.WriteRegister(Registers.APDS9960_GPULSE, DefaultValues.DEFAULT_GPULSE);
            Peripheral.WriteRegister(Registers.APDS9960_GCONF3, DefaultValues.DEFAULT_GCONF3);
            SetGestureIntEnable(DefaultValues.DEFAULT_GIEN);
        }

        /**
         * @brief Reads and returns the contents of the ENABLE register
         *
         * @return Contents of the ENABLE register. 0xFF if error.
         */
        byte GetMode()
        {
             return Peripheral.ReadRegister(Registers.APDS9960_ENABLE);
        }

        /**
         * @brief Enables or disables a feature in the APDS-9960
         *
         * @param[in] mode which feature to enable
         * @param[in] enable ON (1) or OFF (0)
         * @return True if operation success. False otherwise.
         */
        void SetMode(byte mode, byte enable)
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
                    reg_val &= (byte)(~(1 << mode));
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
            Peripheral.WriteRegister(Registers.APDS9960_ENABLE, reg_val);
        }

        public void EnableLightSensor(bool interrupts)
        {
            /* Set default gain, interrupts, enable power, and enable sensor */
            SetAmbientLightGain(DefaultValues.DEFAULT_AGAIN);
    
            SetAmbientLightIntEnable(interrupts);
    
            EnablePower(true);
            SetMode(OperatingModes.AMBIENT_LIGHT, 1);
        }

        public void DisableLightSensor()
        {
            SetAmbientLightIntEnable(false);
            SetMode(OperatingModes.AMBIENT_LIGHT, 0);
        }

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

        public void DisableProximitySensor()
        {
            SetProximityIntEnable(0);
            SetMode(OperatingModes.PROXIMITY, 0);
        }

        /**
         * @brief Starts the gesture recognition engine on the APDS-9960
         *
         * @param[in] interrupts true to enable hardware external interrupt on gesture
         * @return True if engine enabled correctly. False on error.
         */
        public bool EnableGestureSensor(bool interrupts)
        {

            /* Enable gesture mode
               Set ENABLE to 0 (power off)
               Set WTIME to 0xFF
               Set AUX to LED_BOOST_100
               Enable PON, WEN, PEN, GEN in ENABLE 
            */
            Console.WriteLine("ResetGestureParameters");
            ResetGestureParameters();
            Peripheral.WriteRegister(Registers.APDS9960_WTIME, 0xFF);
            Peripheral.WriteRegister(Registers.APDS9960_PPULSE, DefaultValues.DEFAULT_GESTURE_PPULSE);

            Console.WriteLine("SetLEDBoost");
            SetLEDBoost(GainValues.LED_BOOST_100);

            Console.WriteLine("SetGestureIntEnable");
            SetGestureIntEnable((byte)(interrupts?1:0));

            Console.WriteLine("SetGestureMode");
            SetGestureMode(1);
            EnablePower(true);
            SetMode(OperatingModes.WAIT, 1);
            SetMode(OperatingModes.PROXIMITY, 1);
            SetMode(OperatingModes.GESTURE, 1);

            return true;
        }

        public void DisableGestureSensor()
        {
            ResetGestureParameters();
            SetGestureIntEnable(0);
            SetGestureMode(0);
            SetMode(OperatingModes.GESTURE, 0);
        }

        public bool IsGestureAvailable()
        {
            byte val = Peripheral.ReadRegister(Registers.APDS9960_GSTATUS);

            /* Shift and mask out GVALID bit */
            val &= BitFields.APDS9960_GVALID;

            /* Return true/false based on GVALID bit */
            return val == 1;
        }

        public Direction ReadGesture()
        {
            /* Make sure that power and gesture is on and data is valid */
            if (!IsGestureAvailable() || (GetMode() & 0b01000001) == 0x0)
            {
                Console.WriteLine("Read Gesture failed");
                return (int)Direction.NONE;
            }

            Console.WriteLine("ReadGesture");

            /* Keep looping as long as gesture data is valid */
            while (true)
            {
                byte fifo_level;
                byte bytes_read;

                /* Wait some time to collect next batch of FIFO data */
                Thread.Sleep(FIFO_PAUSE_TIME);

                Console.WriteLine("Read: APDS9960_GSTATUS");
                var gstatus = Peripheral.ReadRegister(Registers.APDS9960_GSTATUS);

                /* If we have valid data, read in FIFO */
                Console.WriteLine("Read: APDS9960_GVALID");
                if ((gstatus & BitFields.APDS9960_GVALID) == BitFields.APDS9960_GVALID)
                {
                    Console.WriteLine("Read: APDS9960_GFLVL");
                    fifo_level = Peripheral.ReadRegister(Registers.APDS9960_GFLVL);

                    /* If there's stuff in the FIFO, read it into our data block */
                    if (fifo_level > 0)
                    {
                        Console.WriteLine($"fifo level {fifo_level}");

                        byte[] fifo_data = Peripheral.ReadRegisters(Registers.APDS9960_GFIFO_U, (ushort)(fifo_level * 4));

                        Console.WriteLine(BitConverter.ToString(fifo_data));

                        bytes_read = (byte)fifo_data.Length;

                        Console.WriteLine($"Fifo bytes read {bytes_read}");

                        if (bytes_read < 1)
                        {
                            throw new Exception();
                        }

                        /* If at least 1 set of data, sort the data into U/D/L/R */
                        if (bytes_read >= 4)
                        {
                            for (int i = 0; i < bytes_read; i += 4)
                            {
                                gestureData.UData[gestureData.Index] = fifo_data[i + 0];
                                gestureData.DData[gestureData.Index] = fifo_data[i + 1];
                                gestureData.LData[gestureData.Index] = fifo_data[i + 2];
                                gestureData.RData[gestureData.Index] = fifo_data[i + 3];
                                gestureData.Index++;
                                gestureData.TotalGestures++;
                            }

                            /* Filter and process gesture data. Decode near/far state */
                            Console.WriteLine("ProcessGestureData");
                            if (ProcessGestureData())
                            {
                                Console.WriteLine("DecodeGesture");
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
                    Console.WriteLine("DecodeGesture");
                    DecodeGesture();

                    Console.WriteLine("ResetGestureParameters");
                    ResetGestureParameters();
                    return gestureDirection;
                }
            }
        }

        public void EnablePower(bool enable)
        {
            SetMode(OperatingModes.POWER, (byte)(enable ? 1 : 0));
        }

        /*******************************************************************************
         * Ambient light and color sensor controls
         ******************************************************************************/
        protected ushort ReadAmbientLight()
        {
            byte val = Peripheral.ReadRegister(Registers.APDS9960_CDATAL);

            byte val_byte = Peripheral.ReadRegister(Registers.APDS9960_CDATAH);

            return (ushort)(val + (val_byte << 8));
        }

        protected ushort ReadRedLight()
        {
            byte val = Peripheral.ReadRegister(Registers.APDS9960_RDATAL);

            byte val_byte = Peripheral.ReadRegister(Registers.APDS9960_RDATAH);

            return (ushort)(val + (val_byte << 8));
        }

        protected ushort ReadGreenLight()
        {
            byte val = Peripheral.ReadRegister(Registers.APDS9960_GDATAL);

            byte val_byte = Peripheral.ReadRegister(Registers.APDS9960_GDATAH);

            return (ushort)(val + (val_byte << 8));
        }

        protected ushort ReadBlueLight()
        {
            byte val = Peripheral.ReadRegister(Registers.APDS9960_BDATAL);

            byte val_byte = Peripheral.ReadRegister(Registers.APDS9960_BDATAH);

            return (ushort)(val + (val_byte << 8));
        }

        /*******************************************************************************
         * Proximity sensor controls
         ******************************************************************************/
        public byte ReadProximity()
        {
            return Peripheral.ReadRegister(Registers.APDS9960_PDATA);
        }

        /*******************************************************************************
         * High-level gesture controls
         ******************************************************************************/

        /**
         * @brief Resets all the parameters in the gesture data member
         */
        void ResetGestureParameters()
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

        /**
         * @brief Processes the raw gesture data to determine swipe direction
         *
         * @return True if near or far state seen. False otherwise.
         */
        bool ProcessGestureData()
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
                (gestureData.TotalGestures > 0) )
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
                    (l_first == 0) || (r_first == 0) )
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
            ud_ratio_first = ((u_first - d_first) * 100) / (u_first + d_first);
            lr_ratio_first = ((l_first - r_first) * 100) / (l_first + r_first);
            ud_ratio_last = ((u_last - d_last) * 100) / (u_last + d_last);
            lr_ratio_last = ((l_last - r_last) * 100) / (l_last + r_last);

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
                    (Math.Abs(lr_delta) < GestureParameters.GESTURE_SENSITIVITY_2) )
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
                            gestureState =States.NEAR_STATE;
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
                if ((Math.Abs(ud_delta) < GestureParameters.GESTURE_SENSITIVITY_2) && (Math.Abs(lr_delta) < GestureParameters.GESTURE_SENSITIVITY_2) ) {

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
        bool DecodeGesture()
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
            return Peripheral.ReadRegister(Registers.APDS9960_PILT);
        }

        /**
         * @brief Sets the lower threshold for proximity detection
         *
         * @param[in] threshold the lower proximity threshold
         * @return True if operation successful. False otherwise.
         */
        public void SetProxIntLowThresh(byte threshold)
        {
            Peripheral.WriteRegister(Registers.APDS9960_PILT, threshold);
        }

        /**
         * @brief Returns the high threshold for proximity detection
         *
         * @return high threshold
         */
        public byte GetProxIntHighThresh()
        {
            return Peripheral.ReadRegister(Registers.APDS9960_PIHT);
        }

        /**
         * @brief Sets the high threshold for proximity detection
         *
         * @param[in] threshold the high proximity threshold
         * @return True if operation successful. False otherwise.
         */
        public void SetProxIntHighThresh(byte threshold)
        {
            Peripheral.WriteRegister(Registers.APDS9960_PIHT, threshold);
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
            byte val = Peripheral.ReadRegister(Registers.APDS9960_CONTROL);

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
            byte val = Peripheral.ReadRegister(Registers.APDS9960_CONTROL);

            /* Set bits in register to given value */
            drive &= 0b00000011;
            drive = (byte)(drive << 6);
            val &= 0b00111111;
            val |= drive;

            /* Write register value back into CONTROL register */
            Peripheral.WriteRegister(Registers.APDS9960_CONTROL, val);

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
            byte val = Peripheral.ReadRegister(Registers.APDS9960_CONTROL);

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
            byte val = Peripheral.ReadRegister(Registers.APDS9960_CONTROL);

            /* Set bits in register to given value */
            drive &= 0b00000011;
            drive = (byte)(drive << 2);
            val &= 0b11110011;
            val |= drive;

            /* Write register value back into CONTROL register */
            Peripheral.WriteRegister(Registers.APDS9960_CONTROL, val);
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
        byte GetAmbientLightGain()
        {
            byte val = Peripheral.ReadRegister(Registers.APDS9960_CONTROL);

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
        void SetAmbientLightGain(byte drive)
        {
            byte val = Peripheral.ReadRegister(Registers.APDS9960_CONTROL);

            /* Set bits in register to given value */
            drive &= 0b00000011;
            val &= 0b11111100;
            val |= drive;

            /* Write register value back into CONTROL register */
            Peripheral.WriteRegister(Registers.APDS9960_CONTROL, val);
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
        byte GetLEDBoost()
        {
            byte val = Peripheral.ReadRegister(Registers.APDS9960_CONTROL);

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
        void SetLEDBoost(byte boost)
        {
            byte val = Peripheral.ReadRegister(Registers.APDS9960_CONFIG2);

            /* Set bits in register to given value */
            boost &= 0b00000011;
            boost = (byte)(boost << 4);
            val &= 0b11001111;
            val |= boost;

            Peripheral.WriteRegister(Registers.APDS9960_CONFIG2, val);
        }

        /**
         * @brief Gets proximity gain compensation enable
         *
         * @return 1 if compensation is enabled. 0 if not. 0xFF on error.
         */
        byte GetProxGainCompEnable()
        {
            byte val = Peripheral.ReadRegister(Registers.APDS9960_CONFIG3);

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
        void SetProxGainCompEnable(byte enable)
        {
            byte val = Peripheral.ReadRegister(Registers.APDS9960_CONFIG3);

            /* Set bits in register to given value */
            enable &= 0b00000001;
            enable = (byte)(enable << 5);
            val &= 0b11011111;
            val |= enable;

            /* Write register value back into CONFIG3 register */
            Peripheral.WriteRegister(Registers.APDS9960_CONFIG3, val);
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
        byte GetProxPhotoMask()
        {
            byte val = Peripheral.ReadRegister(Registers.APDS9960_CONFIG3);

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
        void SetProxPhotoMask(byte mask)
        {
            byte val = Peripheral.ReadRegister(Registers.APDS9960_CONFIG3);

            /* Set bits in register to given value */
            mask &= 0b00001111;
            val &= 0b11110000;
            val |= mask;

            Peripheral.WriteRegister(Registers.APDS9960_CONFIG3, val);
        }

        /**
         * @brief Gets the entry proximity threshold for gesture sensing
         *
         * @return Current entry proximity threshold.
         */
        byte GetGestureEnterThresh()
        {
            byte val = Peripheral.ReadRegister(Registers.APDS9960_GPENTH);

            return val;
        }

        /**
         * @brief Sets the entry proximity threshold for gesture sensing
         *
         * @param[in] threshold proximity value needed to start gesture mode
         * @return True if operation successful. False otherwise.
         */
        void SetGestureEnterThresh(byte threshold)
        {
            Peripheral.WriteRegister(Registers.APDS9960_GPENTH, threshold);
        }

        /**
         * @brief Gets the exit proximity threshold for gesture sensing
         *
         * @return Current exit proximity threshold.
         */
        byte GetGestureExitThresh()
        {
            byte val = Peripheral.ReadRegister(Registers.APDS9960_GEXTH);

            return val;
        }

        /**
         * @brief Sets the exit proximity threshold for gesture sensing
         *
         * @param[in] threshold proximity value needed to end gesture mode
         * @return True if operation successful. False otherwise.
         */
        void SetGestureExitThresh(byte threshold)
        {
            Peripheral.WriteRegister(Registers.APDS9960_GEXTH, threshold);
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
        byte GetGestureGain()
        {
            byte val = Peripheral.ReadRegister(Registers.APDS9960_GCONF2);

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
        void SetGestureGain(byte gain)
        {
            byte val = Peripheral.ReadRegister(Registers.APDS9960_GCONF2);

            /* Set bits in register to given value */
            gain &= 0b00000011;
            gain = (byte)(gain << 5);
            val &= 0b10011111;
            val |= gain;

            /* Write register value back into GCONF2 register */
            Peripheral.WriteRegister(Registers.APDS9960_GCONF2, val);
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
        byte GetGestureLEDDrive()
        {
            /* Read value from GCONF2 register */
            byte val = Peripheral.ReadRegister(Registers.APDS9960_GCONF2);

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
        void SetGestureLEDDrive(byte drive)
        {
            byte val = Peripheral.ReadRegister(Registers.APDS9960_GCONF2);

            /* Set bits in register to given value */
            drive &= 0b00000011;
            drive = (byte)(drive << 3);
            val &= 0b11100111;
            val |= drive;

            Peripheral.WriteRegister(Registers.APDS9960_GCONF2, val);
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
        byte GetGestureWaitTime()
        {
            byte val = Peripheral.ReadRegister(Registers.APDS9960_GCONF2);

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
        void SetGestureWaitTime(byte time)
        {
            byte val = Peripheral.ReadRegister(Registers.APDS9960_GCONF2);

            /* Set bits in register to given value */
            time &= 0b00000111;
            val &= 0b11111000;
            val |= time;

            Peripheral.WriteRegister(Registers.APDS9960_GCONF2, val);
        }

        /**
         * @brief Gets the low threshold for ambient light interrupts
         *
         * @param[out] threshold current low threshold stored on the APDS-9960
         * @return True if operation successful. False otherwise.
         */
        ushort GetLightIntLowThreshold()
        {
            var threshold = Peripheral.ReadRegister(Registers.APDS9960_AILTL);

            var val_byte = Peripheral.ReadRegister(Registers.APDS9960_AILTH);
      
            return (byte)(threshold + ((ushort)val_byte << 8));
        }

        /**
         * @brief Sets the low threshold for ambient light interrupts
         *
         * @param[in] threshold low threshold value for interrupt to trigger
         * @return True if operation successful. False otherwise.
         */
        void SetLightIntLowThreshold(ushort threshold)
        {
            byte val_low;
            byte val_high;

            /* Break 16-bit threshold into 2 8-bit values */
            val_low = (byte)(threshold & 0x00FF);
            val_high = (byte)((threshold & 0xFF00) >> 8);

            Peripheral.WriteRegister(Registers.APDS9960_AILTL, val_low);
            Peripheral.WriteRegister(Registers.APDS9960_AILTL, val_high);
        }

        /**
         * @brief Gets the high threshold for ambient light interrupts
         *
         * @param[out] threshold current low threshold stored on the APDS-9960
         * @return True if operation successful. False otherwise.
         */
        ushort GetLightIntHighThreshold()
        {
            var threshold = Peripheral.ReadRegister(Registers.APDS9960_AIHTL);

            var val_byte = Peripheral.ReadRegister(Registers.APDS9960_AIHTH);

            return (byte)(threshold + ((ushort)val_byte << 8));
        }

        /**
         * @brief Sets the high threshold for ambient light interrupts
         *
         * @param[in] threshold high threshold value for interrupt to trigger
         * @return True if operation successful. False otherwise.
         */
        void SetLightIntHighThreshold(ushort threshold)
        {
            /* Break 16-bit threshold into 2 8-bit values */
            byte val_low = (byte)(threshold & 0x00FF);
            byte val_high = (byte)((threshold & 0xFF00) >> 8);

            Peripheral.WriteRegister(Registers.APDS9960_AIHTL, val_low);
            Peripheral.WriteRegister(Registers.APDS9960_AIHTH, val_high);
        }

        /**
         * @brief Gets the low threshold for proximity interrupts
         *
         * @param[out] threshold current low threshold stored on the APDS-9960
         * @return True if operation successful. False otherwise.
         */
        byte GetProximityIntLowThreshold()
        {
            return Peripheral.ReadRegister(Registers.APDS9960_PILT);
        }

        /**
         * @brief Sets the low threshold for proximity interrupts
         *
         * @param[in] threshold low threshold value for interrupt to trigger
         * @return True if operation successful. False otherwise.
         */
        void SetProximityIntLowThreshold(byte threshold)
        {
            Peripheral.WriteRegister(Registers.APDS9960_PILT, threshold);
        }

        /**
         * @brief Gets the high threshold for proximity interrupts
         *
         * @param[out] threshold current low threshold stored on the APDS-9960
         * @return True if operation successful. False otherwise.
         */
        byte GetProximityIntHighThreshold()
        {
            return Peripheral.ReadRegister(Registers.APDS9960_PIHT);
        }

        /**
         * @brief Sets the high threshold for proximity interrupts
         *
         * @param[in] threshold high threshold value for interrupt to trigger
         * @return True if operation successful. False otherwise.
         */
        void SetProximityIntHighThreshold(byte threshold)
        {
            Peripheral.WriteRegister(Registers.APDS9960_PIHT, threshold);
        }

        /**
         * @brief Gets if ambient light interrupts are enabled or not
         *
         * @return 1 if interrupts are enabled, 0 if not. 0xFF on error.
         */
        byte GetAmbientLightIntEnable()
        {
            byte val = Peripheral.ReadRegister(Registers.APDS9960_ENABLE);

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
        void SetAmbientLightIntEnable(bool enable)
        {
            byte val = Peripheral.ReadRegister(Registers.APDS9960_ENABLE);

            /* Set bits in register to given value */
            byte data = (byte)(enable ? 0x1 : 0x0);
            data &= 0b00000001;
            data = (byte)(data << 4);
            val &= 0b11101111;
            val |= data;

            Peripheral.WriteRegister(Registers.APDS9960_ENABLE, val);
        }

        /**
         * @brief Gets if proximity interrupts are enabled or not
         *
         * @return 1 if interrupts are enabled, 0 if not. 0xFF on error.
         */
        byte GetProximityIntEnable()
        {
            byte val = Peripheral.ReadRegister(Registers.APDS9960_ENABLE);

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
        void SetProximityIntEnable(byte enable)
        {
            byte val = Peripheral.ReadRegister(Registers.APDS9960_ENABLE);

            /* Set bits in register to given value */
            enable &= 0b00000001;
            enable = (byte)(enable << 5);
            val &= 0b11011111;
            val |= enable;

            Peripheral.WriteRegister(Registers.APDS9960_ENABLE, val);
        }

        /**
         * @brief Gets if gesture interrupts are enabled or not
         *
         * @return 1 if interrupts are enabled, 0 if not. 0xFF on error.
         */
        byte GetGestureIntEnable()
        {
            byte val = Peripheral.ReadRegister(Registers.APDS9960_GCONF4);

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
        void SetGestureIntEnable(byte enable)
        {
            byte val = Peripheral.ReadRegister(Registers.APDS9960_GCONF4);

            /* Set bits in register to given value */
            enable &= 0b00000001;
            enable = (byte)(enable << 1);
            val &= 0b11111101;
            val |= enable;

            Peripheral.WriteRegister(Registers.APDS9960_GCONF4, val);
        }

        /**
         * @brief Clears the ambient light interrupt
         *
         * @return True if operation completed successfully. False otherwise.
         */
        void ClearAmbientLightInt()
        {
            Peripheral.WriteRegister(Registers.APDS9960_AICLEAR, 0);
        }

        /**
         * @brief Clears the proximity interrupt
         *
         * @return True if operation completed successfully. False otherwise.
         */
        void ClearProximityInt()
        {
            Peripheral.WriteRegister(Registers.APDS9960_PICLEAR, 0);
        }

        /**
         * @brief Tells if the gesture state machine is currently running
         *
         * @return 1 if gesture state machine is running, 0 if not. 0xFF on error.
         */
        byte GetGestureMode()
        {
            byte val = Peripheral.ReadRegister(Registers.APDS9960_GCONF4);

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
        void SetGestureMode(byte mode)
        {
            byte val = Peripheral.ReadRegister(Registers.APDS9960_GCONF4);

            /* Set bits in register to given value */
            mode &= 0b00000001;
            val &= 0b11111110;
            val |= mode;

            Peripheral.WriteRegister(Registers.APDS9960_GCONF4, val);
        }

        /* Container for gesture data */
        public class GestureData
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