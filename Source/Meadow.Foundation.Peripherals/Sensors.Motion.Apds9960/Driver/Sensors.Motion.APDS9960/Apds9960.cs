

using System;
using System.Threading;
using Meadow.Hardware;

namespace Meadow.Foundation.Sensors.Motion
{
    public class Apds9960
    {
        #region Member variables / fields

        /// <summary>
        ///     Communication bus used to communicate with the sensor.
        ///     This driver is a work-in-progress, contributions are always welcome
        /// </summary>
        private readonly II2cPeripheral apds9960;

        IDigitalInputPort interruptPort;

        GestureData gestureData;
        int gestureUdDelta;
        int gestureLrDelta;
        int gestureUdCount;
        int gestureLrCount;
        int gestureNearCount;
        int gestureFarCount;
        State gestureState;
        Direction gestureDirection;

        /* APDS-9960 I2C address */
        static readonly byte APDS9960_I2C_ADDR = 0x39;

        /* Gesture parameters */
        static readonly byte GESTURE_THRESHOLD_OUT = 10;
        static readonly byte GESTURE_SENSITIVITY_1 = 50;
        static readonly byte GESTURE_SENSITIVITY_2 = 20;

        /* Error code for returned values */
        static readonly byte ERROR = 0xFF;

        /* Acceptable device IDs */
        static readonly byte APDS9960_ID_1 = 0xAB;
        static readonly byte APDS9960_ID_2 = 0x9C;

        /* Misc parameters */
        static readonly byte FIFO_PAUSE_TIME = 30;      // Wait period (ms) between FIFO reads

        /* APDS-9960 register addresses */
        static readonly byte APDS9960_ENABLE = 0x80;
        static readonly byte APDS9960_ATIME = 0x81;
        static readonly byte APDS9960_WTIME = 0x83;
        static readonly byte APDS9960_AILTL = 0x84;
        static readonly byte APDS9960_AILTH = 0x85;
        static readonly byte APDS9960_AIHTL = 0x86;
        static readonly byte APDS9960_AIHTH = 0x87;
        static readonly byte APDS9960_PILT = 0x89;
        static readonly byte APDS9960_PIHT = 0x8B;
        static readonly byte APDS9960_PERS = 0x8C;
        static readonly byte APDS9960_CONFIG1 = 0x8D;
        static readonly byte APDS9960_PPULSE = 0x8E;
        static readonly byte APDS9960_CONTROL = 0x8F;
        static readonly byte APDS9960_CONFIG2 = 0x90;
        static readonly byte APDS9960_ID = 0x92;
        static readonly byte APDS9960_STATUS = 0x93;
        static readonly byte APDS9960_CDATAL = 0x94;
        static readonly byte APDS9960_CDATAH = 0x95;
        static readonly byte APDS9960_RDATAL = 0x96;
        static readonly byte APDS9960_RDATAH = 0x97;
        static readonly byte APDS9960_GDATAL = 0x98;
        static readonly byte APDS9960_GDATAH = 0x99;
        static readonly byte APDS9960_BDATAL = 0x9A;
        static readonly byte APDS9960_BDATAH = 0x9B;
        static readonly byte APDS9960_PDATA = 0x9C;
        static readonly byte APDS9960_POFFSET_UR = 0x9D;
        static readonly byte APDS9960_POFFSET_DL = 0x9E;
        static readonly byte APDS9960_CONFIG3 = 0x9F;
        static readonly byte APDS9960_GPENTH = 0xA0;
        static readonly byte APDS9960_GEXTH = 0xA1;
        static readonly byte APDS9960_GCONF1 = 0xA2;
        static readonly byte APDS9960_GCONF2 = 0xA3;
        static readonly byte APDS9960_GOFFSET_U = 0xA4;
        static readonly byte APDS9960_GOFFSET_D = 0xA5;
        static readonly byte APDS9960_GOFFSET_L = 0xA7;
        static readonly byte APDS9960_GOFFSET_R = 0xA9;
        static readonly byte APDS9960_GPULSE = 0xA6;
        static readonly byte APDS9960_GCONF3 = 0xAA;
        static readonly byte APDS9960_GCONF4 = 0xAB;
        static readonly byte APDS9960_GFLVL = 0xAE;
        static readonly byte APDS9960_GSTATUS = 0xAF;
        static readonly byte APDS9960_IFORCE = 0xE4;
        static readonly byte APDS9960_PICLEAR = 0xE5;
        static readonly byte APDS9960_CICLEAR = 0xE6;
        static readonly byte APDS9960_AICLEAR = 0xE7;
        static readonly byte APDS9960_GFIFO_U = 0xFC;
        static readonly byte APDS9960_GFIFO_D = 0xFD;
        static readonly byte APDS9960_GFIFO_L = 0xFE;
        static readonly byte APDS9960_GFIFO_R = 0xFF;

        /* Bit fields */
        static readonly byte APDS9960_PON = 0b00000001;
        static readonly byte APDS9960_AEN = 0b00000010;
        static readonly byte APDS9960_PEN = 0b00000100;
        static readonly byte APDS9960_WEN = 0b00001000;
        static readonly byte APSD9960_AIEN = 0b00010000;
        static readonly byte APDS9960_PIEN = 0b00100000;
        static readonly byte APDS9960_GEN = 0b01000000;
        static readonly byte APDS9960_GVALID = 0b00000001;

        /* On/Off definitions */
        static readonly byte OFF = 0;
        static readonly byte ON = 1;

        /* Acceptable parameters for setMode */
        static readonly byte POWER = 0;
        static readonly byte AMBIENT_LIGHT = 1;
        static readonly byte PROXIMITY = 2;
        static readonly byte WAIT = 3;
        static readonly byte AMBIENT_LIGHT_INT = 4;
        static readonly byte PROXIMITY_INT = 5;
        static readonly byte GESTURE = 6;
        static readonly byte ALL = 7;

        /* LED Drive values */
        static readonly byte LED_DRIVE_100MA = 0;
        static readonly byte LED_DRIVE_50MA = 1;
        static readonly byte LED_DRIVE_25MA = 2;
        static readonly byte LED_DRIVE_12_5MA = 3;

        /* Proximity Gain (PGAIN) values */
        static readonly byte PGAIN_1X = 0;
        static readonly byte PGAIN_2X = 1;
        static readonly byte PGAIN_4X = 2;
        static readonly byte PGAIN_8X = 3;

        /* ALS Gain (AGAIN) values */
        static readonly byte AGAIN_1X = 0;
        static readonly byte AGAIN_4X = 1;
        static readonly byte AGAIN_16X = 2;
        static readonly byte AGAIN_64X = 3;

        /* Gesture Gain (GGAIN) values */
        static readonly byte GGAIN_1X = 0;
        static readonly byte GGAIN_2X = 1;
        static readonly byte GGAIN_4X = 2;
        static readonly byte GGAIN_8X = 3;

        /* LED Boost values */
        static readonly byte LED_BOOST_100 = 0;
        static readonly byte LED_BOOST_150 = 1;
        static readonly byte LED_BOOST_200 = 2;
        static readonly byte LED_BOOST_300 = 3;

        /* Gesture wait time values */
        static readonly byte GWTIME_0MS = 0;
        static readonly byte GWTIME_2_8MS = 1;
        static readonly byte GWTIME_5_6MS = 2;
        static readonly byte GWTIME_8_4MS = 3;
        static readonly byte GWTIME_14_0MS = 4;
        static readonly byte GWTIME_22_4MS = 5;
        static readonly byte GWTIME_30_8MS = 6;
        static readonly byte GWTIME_39_2MS = 7;

        /* Default values */
        static readonly byte DEFAULT_ATIME = 219;     // 103ms
        static readonly byte DEFAULT_WTIME = 246;     // 27ms
        static readonly byte DEFAULT_PROX_PPULSE = 0x87;    // 16us, 8 pulses
        static readonly byte DEFAULT_GESTURE_PPULSE = 0x89;    // 16us, 10 pulses
        static readonly byte DEFAULT_POFFSET_UR = 0;       // 0 offset
        static readonly byte DEFAULT_POFFSET_DL = 0;       // 0 offset      
        static readonly byte DEFAULT_CONFIG1 = 0x60;    // No 12x wait (WTIME) factor
        static readonly byte DEFAULT_LDRIVE = LED_DRIVE_100MA;
        static readonly byte DEFAULT_PGAIN = PGAIN_4X;
        static readonly byte DEFAULT_AGAIN = AGAIN_4X;
        static readonly byte DEFAULT_PILT = 0;       // Low proximity threshold
        static readonly byte DEFAULT_PIHT = 50;      // High proximity threshold
        static readonly ushort DEFAULT_AILT = 0xFFFF;  // Force interrupt for calibration
        static readonly byte DEFAULT_AIHT = 0;
        static readonly byte DEFAULT_PERS = 0x11;    // 2 consecutive prox or ALS for int.
        static readonly byte DEFAULT_CONFIG2 = 0x01;    // No saturation interrupts or LED boost  
        static readonly byte DEFAULT_CONFIG3 = 0;       // Enable all photodiodes, no SAI
        static readonly byte DEFAULT_GPENTH = 40;      // Threshold for entering gesture mode
        static readonly byte DEFAULT_GEXTH = 30;      // Threshold for exiting gesture mode    
        static readonly byte DEFAULT_GCONF1 = 0x40;    // 4 gesture events for int., 1 for exit
        static readonly byte DEFAULT_GGAIN = GGAIN_4X;
        static readonly byte DEFAULT_GLDRIVE = LED_DRIVE_100MA;
        static readonly byte DEFAULT_GWTIME = GWTIME_2_8MS;
        static readonly byte DEFAULT_GOFFSET = 0;       // No offset scaling for gesture mode
        static readonly byte DEFAULT_GPULSE = 0xC9;    // 32us, 10 pulses
        static readonly byte DEFAULT_GCONF3 = 0;       // All photodiodes active during gesture
        static readonly byte DEFAULT_GIEN = 0;       // Disable gesture interrupts

        #endregion Member variables / fields

        #region Enums

        /* Direction definitions */
        public enum Direction
        {
            NONE,
            LEFT,
            RIGHT,
            UP,
            DOWN,
            NEAR,
            FAR,
            ALL //huh?
        };

        /* State definitions */
        public enum State
        {
            NA_STATE,
            NEAR_STATE,
            FAR_STATE,
            ALL_STATE
        };

        #endregion Enums

        #region Constructors

        /// <summary>
        ///     Create a new instance of the APDS9960 communicating over the I2C interface.
        /// </summary>
        /// <param name="address">Address of the I2C sensor</param>
        /// <param name="i2cBus">SI2C bus object</param>
        public Apds9960(IIODevice device, II2cBus i2cBus, IPin interruptPin, byte address = 0x39)
        {
            apds9960 = new I2cPeripheral(i2cBus, address);

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

        private void InterruptPort_Changed(object sender, DigitalInputPortEventArgs e)
        {
        //    throw new NotImplementedException();
        }

        #endregion Constructors

        #region Methods

        void Initialize()
        {
            var id = apds9960.ReadRegister(APDS9960_ID);

         //  if ((id != APDS9960_ID) && (id != APDS9960_ID_1) && (id != APDS9960_ID_2))
       //     if(id == 0)
        //    {
                Console.WriteLine($"Device found {id}");
       //     }

            Console.WriteLine("SetMode");
            SetMode(ALL, OFF);

            /* Set default values for ambient light and proximity registers */
            apds9960.WriteRegister(APDS9960_ATIME, DEFAULT_ATIME);
            apds9960.WriteRegister(APDS9960_WTIME, DEFAULT_WTIME);
            apds9960.WriteRegister(APDS9960_PPULSE, DEFAULT_PROX_PPULSE);
            apds9960.WriteRegister(APDS9960_POFFSET_UR, DEFAULT_POFFSET_UR);
            apds9960.WriteRegister(APDS9960_POFFSET_DL, DEFAULT_POFFSET_DL);
            apds9960.WriteRegister(APDS9960_CONFIG1, DEFAULT_CONFIG1);
            SetLEDDrive(DEFAULT_LDRIVE);

            Console.WriteLine("SetProximityGain");
            SetProximityGain(DEFAULT_PGAIN);
            SetAmbientLightGain(DEFAULT_AGAIN);
            SetProxIntLowThresh(DEFAULT_PILT);
            SetProxIntHighThresh(DEFAULT_PIHT);

            SetLightIntLowThreshold(DEFAULT_AILT);

            SetLightIntHighThreshold(DEFAULT_AIHT);

            apds9960.WriteRegister(APDS9960_PERS, DEFAULT_PERS);

            apds9960.WriteRegister(APDS9960_CONFIG2, DEFAULT_CONFIG2);

            apds9960.WriteRegister(APDS9960_CONFIG3, DEFAULT_CONFIG3);

            Console.WriteLine("SetGestureEnterThresh");
            SetGestureEnterThresh(DEFAULT_GPENTH);
            
            SetGestureExitThresh(DEFAULT_GEXTH);
            
            apds9960.WriteRegister(APDS9960_GCONF1, DEFAULT_GCONF1);
            
            SetGestureGain(DEFAULT_GGAIN);
            
            SetGestureLEDDrive(DEFAULT_GLDRIVE);
            
            SetGestureWaitTime(DEFAULT_GWTIME);
            
            apds9960.WriteRegister(APDS9960_GOFFSET_U, DEFAULT_GOFFSET);
            apds9960.WriteRegister(APDS9960_GOFFSET_D, DEFAULT_GOFFSET);
            apds9960.WriteRegister(APDS9960_GOFFSET_L, DEFAULT_GOFFSET);
            apds9960.WriteRegister(APDS9960_GOFFSET_R, DEFAULT_GOFFSET);
            apds9960.WriteRegister(APDS9960_GPULSE, DEFAULT_GPULSE);
            apds9960.WriteRegister(APDS9960_GCONF3, DEFAULT_GCONF3);
            SetGestureIntEnable(DEFAULT_GIEN);
        }

        /**
         * @brief Reads and returns the contents of the ENABLE register
         *
         * @return Contents of the ENABLE register. 0xFF if error.
         */
        byte GetMode()
        {
             return apds9960.ReadRegister(APDS9960_ENABLE);
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
            else if (mode == ALL)
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
            apds9960.WriteRegister(APDS9960_ENABLE, reg_val);
        }

        public void EnableLightSensor(bool interrupts)
        {
            /* Set default gain, interrupts, enable power, and enable sensor */
            SetAmbientLightGain(DEFAULT_AGAIN);
    
            SetAmbientLightIntEnable(interrupts);
    
            EnablePower(true);
            SetMode(AMBIENT_LIGHT, 1);
        }

        public void DisableLightSensor()
        {
            SetAmbientLightIntEnable(false);
            SetMode(AMBIENT_LIGHT, 0);
        }

        public void EnableProximitySensor(bool interrupts)
        {
            /* Set default gain, LED, interrupts, enable power, and enable sensor */
            SetProximityGain(DEFAULT_PGAIN);
            SetLEDDrive(DEFAULT_LDRIVE);

            if (interrupts)
            {
                SetProximityIntEnable(1);
            }
            else
            {
                SetProximityIntEnable(0);
            }
            EnablePower(true);
            SetMode(PROXIMITY, 1);
        }

        public void DisableProximitySensor()
        {
            SetProximityIntEnable(0);
            SetMode(PROXIMITY, 0);
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
            apds9960.WriteRegister(APDS9960_WTIME, 0xFF);
            apds9960.WriteRegister(APDS9960_PPULSE, DEFAULT_GESTURE_PPULSE);

            Console.WriteLine("SetLEDBoost");
            SetLEDBoost(LED_BOOST_100);

            Console.WriteLine("SetGestureIntEnable");
            SetGestureIntEnable((byte)(interrupts?1:0));

            Console.WriteLine("SetGestureMode");
            SetGestureMode(1);
            EnablePower(true);
            SetMode(WAIT, 1);
            SetMode(PROXIMITY, 1);
            SetMode(GESTURE, 1);

            return true;
        }

        public void DisableGestureSensor()
        {
            ResetGestureParameters();
            SetGestureIntEnable(0);
            SetGestureMode(0);
            SetMode(GESTURE, 0);
        }

        public bool IsGestureAvailable()
        {
            byte val = apds9960.ReadRegister(APDS9960_GSTATUS);

            /* Shift and mask out GVALID bit */
            val &= APDS9960_GVALID;

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
                var gstatus = apds9960.ReadRegister(APDS9960_GSTATUS);

                /* If we have valid data, read in FIFO */
                Console.WriteLine("Read: APDS9960_GVALID");
                if ((gstatus & APDS9960_GVALID) == APDS9960_GVALID)
                {
                    Console.WriteLine("Read: APDS9960_GFLVL");
                    fifo_level = apds9960.ReadRegister(APDS9960_GFLVL);

                    /* If there's stuff in the FIFO, read it into our data block */
                    if (fifo_level > 0)
                    {
                        Console.WriteLine($"fifo level {fifo_level}");

                        byte[] fifo_data = apds9960.ReadRegisters(APDS9960_GFIFO_U, (ushort)(fifo_level * 4));

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
            SetMode(POWER, (byte)(enable ? 1 : 0));
        }

        /*******************************************************************************
         * Ambient light and color sensor controls
         ******************************************************************************/
        public ushort ReadAmbientLight()
        {
            byte val = apds9960.ReadRegister(APDS9960_CDATAL);

            byte val_byte = apds9960.ReadRegister(APDS9960_CDATAH);

            return (ushort)(val + (val_byte << 8));
        }

        public ushort ReadRedLight()
        {
            byte val = apds9960.ReadRegister(APDS9960_RDATAL);

            byte val_byte = apds9960.ReadRegister(APDS9960_RDATAH);

            return (ushort)(val + (val_byte << 8));
        }

        public ushort ReadGreenLight()
        {
            byte val = apds9960.ReadRegister(APDS9960_GDATAL);

            byte val_byte = apds9960.ReadRegister(APDS9960_GDATAH);

            return (ushort)(val + (val_byte << 8));
        }

        public ushort ReadBlueLight()
        {
            byte val = apds9960.ReadRegister(APDS9960_BDATAL);

            byte val_byte = apds9960.ReadRegister(APDS9960_BDATAH);

            return (ushort)(val + (val_byte << 8));
        }

        /*******************************************************************************
         * Proximity sensor controls
         ******************************************************************************/
        public byte ReadProximity()
        {
            return apds9960.ReadRegister(APDS9960_PDATA);
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
                    if ((gestureData.UData[i] > GESTURE_THRESHOLD_OUT) &&
                        (gestureData.DData[i] > GESTURE_THRESHOLD_OUT) &&
                        (gestureData.LData[i] > GESTURE_THRESHOLD_OUT) &&
                        (gestureData.RData[i] > GESTURE_THRESHOLD_OUT))
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
                    if ((gestureData.UData[i] > GESTURE_THRESHOLD_OUT) &&
                        (gestureData.DData[i] > GESTURE_THRESHOLD_OUT) &&
                        (gestureData.LData[i] > GESTURE_THRESHOLD_OUT) &&
                        (gestureData.RData[i] > GESTURE_THRESHOLD_OUT))
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
            if (gestureUdDelta >= GESTURE_SENSITIVITY_1)
            {
                gestureUdCount = 1;
            }
            else if (gestureUdDelta <= -GESTURE_SENSITIVITY_1)
            {
                gestureUdCount = -1;
            }
            else
            {
                gestureUdCount = 0;
            }

            /* Determine L/R gesture */
            if (gestureLrDelta >= GESTURE_SENSITIVITY_1)
            {
                gestureLrCount = 1;
            }
            else if (gestureLrDelta <= -GESTURE_SENSITIVITY_1)
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
                if ((Math.Abs(ud_delta) < GESTURE_SENSITIVITY_2) && 
                    (Math.Abs(lr_delta) < GESTURE_SENSITIVITY_2) )
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
                            gestureState =State.NEAR_STATE;
                        }
                        else if ((ud_delta != 0) && (lr_delta != 0))
                        {
                            gestureState = State.FAR_STATE;
                        }
                        return true;
                    }
                }
            }
            else
            {
                if ((Math.Abs(ud_delta) < GESTURE_SENSITIVITY_2) && (Math.Abs(lr_delta) < GESTURE_SENSITIVITY_2) ) {

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
            if (gestureState == State.NEAR_STATE)
            {
                gestureDirection = Direction.NEAR;
                return true;
            }
            if (gestureState == State.FAR_STATE)
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
            return apds9960.ReadRegister(APDS9960_PILT);
        }

        /**
         * @brief Sets the lower threshold for proximity detection
         *
         * @param[in] threshold the lower proximity threshold
         * @return True if operation successful. False otherwise.
         */
        public void SetProxIntLowThresh(byte threshold)
        {
            apds9960.WriteRegister(APDS9960_PILT, threshold);
        }

        /**
         * @brief Returns the high threshold for proximity detection
         *
         * @return high threshold
         */
        public byte GetProxIntHighThresh()
        {
            return apds9960.ReadRegister(APDS9960_PIHT);
        }

        /**
         * @brief Sets the high threshold for proximity detection
         *
         * @param[in] threshold the high proximity threshold
         * @return True if operation successful. False otherwise.
         */
        public void SetProxIntHighThresh(byte threshold)
        {
            apds9960.WriteRegister(APDS9960_PIHT, threshold);
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
            byte val = apds9960.ReadRegister(APDS9960_CONTROL);

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
            byte val = apds9960.ReadRegister(APDS9960_CONTROL);

            /* Set bits in register to given value */
            drive &= 0b00000011;
            drive = (byte)(drive << 6);
            val &= 0b00111111;
            val |= drive;

            /* Write register value back into CONTROL register */
            apds9960.WriteRegister(APDS9960_CONTROL, val);

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
            byte val = apds9960.ReadRegister(APDS9960_CONTROL);

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
            byte val = apds9960.ReadRegister(APDS9960_CONTROL);

            /* Set bits in register to given value */
            drive &= 0b00000011;
            drive = (byte)(drive << 2);
            val &= 0b11110011;
            val |= drive;

            /* Write register value back into CONTROL register */
            apds9960.WriteRegister(APDS9960_CONTROL, val);
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
            byte val = apds9960.ReadRegister(APDS9960_CONTROL);

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
            byte val = apds9960.ReadRegister(APDS9960_CONTROL);

            /* Set bits in register to given value */
            drive &= 0b00000011;
            val &= 0b11111100;
            val |= drive;

            /* Write register value back into CONTROL register */
            apds9960.WriteRegister(APDS9960_CONTROL, val);
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
            byte val = apds9960.ReadRegister(APDS9960_CONTROL);

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
            byte val = apds9960.ReadRegister(APDS9960_CONFIG2);

            /* Set bits in register to given value */
            boost &= 0b00000011;
            boost = (byte)(boost << 4);
            val &= 0b11001111;
            val |= boost;

            apds9960.WriteRegister(APDS9960_CONFIG2, val);
        }

        /**
         * @brief Gets proximity gain compensation enable
         *
         * @return 1 if compensation is enabled. 0 if not. 0xFF on error.
         */
        byte GetProxGainCompEnable()
        {
            byte val = apds9960.ReadRegister(APDS9960_CONFIG3);

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
            byte val = apds9960.ReadRegister(APDS9960_CONFIG3);

            /* Set bits in register to given value */
            enable &= 0b00000001;
            enable = (byte)(enable << 5);
            val &= 0b11011111;
            val |= enable;

            /* Write register value back into CONFIG3 register */
            apds9960.WriteRegister(APDS9960_CONFIG3, val);
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
            byte val = apds9960.ReadRegister(APDS9960_CONFIG3);

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
            byte val = apds9960.ReadRegister(APDS9960_CONFIG3);

            /* Set bits in register to given value */
            mask &= 0b00001111;
            val &= 0b11110000;
            val |= mask;

            apds9960.WriteRegister(APDS9960_CONFIG3, val);
        }

        /**
         * @brief Gets the entry proximity threshold for gesture sensing
         *
         * @return Current entry proximity threshold.
         */
        byte GetGestureEnterThresh()
        {
            byte val = apds9960.ReadRegister(APDS9960_GPENTH);

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
            apds9960.WriteRegister(APDS9960_GPENTH, threshold);
        }

        /**
         * @brief Gets the exit proximity threshold for gesture sensing
         *
         * @return Current exit proximity threshold.
         */
        byte GetGestureExitThresh()
        {
            byte val = apds9960.ReadRegister(APDS9960_GEXTH);

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
            apds9960.WriteRegister(APDS9960_GEXTH, threshold);
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
            byte val = apds9960.ReadRegister(APDS9960_GCONF2);

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
            byte val = apds9960.ReadRegister(APDS9960_GCONF2);

            /* Set bits in register to given value */
            gain &= 0b00000011;
            gain = (byte)(gain << 5);
            val &= 0b10011111;
            val |= gain;

            /* Write register value back into GCONF2 register */
            apds9960.WriteRegister(APDS9960_GCONF2, val);
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
            byte val = apds9960.ReadRegister(APDS9960_GCONF2);

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
            byte val = apds9960.ReadRegister(APDS9960_GCONF2);

            /* Set bits in register to given value */
            drive &= 0b00000011;
            drive = (byte)(drive << 3);
            val &= 0b11100111;
            val |= drive;

            apds9960.WriteRegister(APDS9960_GCONF2, val);
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
            byte val = apds9960.ReadRegister(APDS9960_GCONF2);

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
            byte val = apds9960.ReadRegister(APDS9960_GCONF2);

            /* Set bits in register to given value */
            time &= 0b00000111;
            val &= 0b11111000;
            val |= time;

            apds9960.WriteRegister(APDS9960_GCONF2, val);
        }

        /**
         * @brief Gets the low threshold for ambient light interrupts
         *
         * @param[out] threshold current low threshold stored on the APDS-9960
         * @return True if operation successful. False otherwise.
         */
        ushort GetLightIntLowThreshold()
        {
            var threshold = apds9960.ReadRegister(APDS9960_AILTL);

            var val_byte = apds9960.ReadRegister(APDS9960_AILTH);
      
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

            apds9960.WriteRegister(APDS9960_AILTL, val_low);
            apds9960.WriteRegister(APDS9960_AILTL, val_high);
        }

        /**
         * @brief Gets the high threshold for ambient light interrupts
         *
         * @param[out] threshold current low threshold stored on the APDS-9960
         * @return True if operation successful. False otherwise.
         */
        ushort GetLightIntHighThreshold()
        {
            var threshold = apds9960.ReadRegister(APDS9960_AIHTL);

            var val_byte = apds9960.ReadRegister(APDS9960_AIHTH);

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

            apds9960.WriteRegister(APDS9960_AIHTL, val_low);
            apds9960.WriteRegister(APDS9960_AIHTH, val_high);
        }

        /**
         * @brief Gets the low threshold for proximity interrupts
         *
         * @param[out] threshold current low threshold stored on the APDS-9960
         * @return True if operation successful. False otherwise.
         */
        byte GetProximityIntLowThreshold()
        {
            return apds9960.ReadRegister(APDS9960_PILT);
        }

        /**
         * @brief Sets the low threshold for proximity interrupts
         *
         * @param[in] threshold low threshold value for interrupt to trigger
         * @return True if operation successful. False otherwise.
         */
        void SetProximityIntLowThreshold(byte threshold)
        {
            apds9960.WriteRegister(APDS9960_PILT, threshold);
        }

        /**
         * @brief Gets the high threshold for proximity interrupts
         *
         * @param[out] threshold current low threshold stored on the APDS-9960
         * @return True if operation successful. False otherwise.
         */
        byte GetProximityIntHighThreshold()
        {
            return apds9960.ReadRegister(APDS9960_PIHT);
        }

        /**
         * @brief Sets the high threshold for proximity interrupts
         *
         * @param[in] threshold high threshold value for interrupt to trigger
         * @return True if operation successful. False otherwise.
         */
        void SetProximityIntHighThreshold(byte threshold)
        {
            apds9960.WriteRegister(APDS9960_PIHT, threshold);
        }

        /**
         * @brief Gets if ambient light interrupts are enabled or not
         *
         * @return 1 if interrupts are enabled, 0 if not. 0xFF on error.
         */
        byte GetAmbientLightIntEnable()
        {
            byte val = apds9960.ReadRegister(APDS9960_ENABLE);

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
            byte val = apds9960.ReadRegister(APDS9960_ENABLE);

            /* Set bits in register to given value */
            byte data = (byte)(enable ? 0x1 : 0x0);
            data &= 0b00000001;
            data = (byte)(data << 4);
            val &= 0b11101111;
            val |= data;

            apds9960.WriteRegister(APDS9960_ENABLE, val);
        }

        /**
         * @brief Gets if proximity interrupts are enabled or not
         *
         * @return 1 if interrupts are enabled, 0 if not. 0xFF on error.
         */
        byte GetProximityIntEnable()
        {
            byte val = apds9960.ReadRegister(APDS9960_ENABLE);

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
            byte val = apds9960.ReadRegister(APDS9960_ENABLE);

            /* Set bits in register to given value */
            enable &= 0b00000001;
            enable = (byte)(enable << 5);
            val &= 0b11011111;
            val |= enable;

            apds9960.WriteRegister(APDS9960_ENABLE, val);
        }

        /**
         * @brief Gets if gesture interrupts are enabled or not
         *
         * @return 1 if interrupts are enabled, 0 if not. 0xFF on error.
         */
        byte GetGestureIntEnable()
        {
            byte val = apds9960.ReadRegister(APDS9960_GCONF4);

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
            byte val = apds9960.ReadRegister(APDS9960_GCONF4);

            /* Set bits in register to given value */
            enable &= 0b00000001;
            enable = (byte)(enable << 1);
            val &= 0b11111101;
            val |= enable;

            apds9960.WriteRegister(APDS9960_GCONF4, val);
        }

        /**
         * @brief Clears the ambient light interrupt
         *
         * @return True if operation completed successfully. False otherwise.
         */
        void ClearAmbientLightInt()
        {
            apds9960.WriteRegister(APDS9960_AICLEAR, 0);
        }

        /**
         * @brief Clears the proximity interrupt
         *
         * @return True if operation completed successfully. False otherwise.
         */
        void ClearProximityInt()
        {
            apds9960.WriteRegister(APDS9960_PICLEAR, 0);
        }

        /**
         * @brief Tells if the gesture state machine is currently running
         *
         * @return 1 if gesture state machine is running, 0 if not. 0xFF on error.
         */
        byte GetGestureMode()
        {
            byte val = apds9960.ReadRegister(APDS9960_GCONF4);

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
            byte val = apds9960.ReadRegister(APDS9960_GCONF4);

            /* Set bits in register to given value */
            mode &= 0b00000001;
            val &= 0b11111110;
            val |= mode;

            apds9960.WriteRegister(APDS9960_GCONF4, val);
        }

        #endregion Methods

        #region Classes

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

        #endregion Classes
    }

}