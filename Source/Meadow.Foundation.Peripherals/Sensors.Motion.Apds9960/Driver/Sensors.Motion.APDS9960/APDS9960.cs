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

        private readonly APDS9960Control control = new APDS9960Control();
        private readonly APDS9960Enable enable = new APDS9960Enable();
        private readonly APDS9960Persistance persistance = new APDS9960Persistance();
        private readonly APDS9960Pulse pulse = new APDS9960Pulse();
        private readonly APDS9960GestureStatus gestureStatus = new APDS9960GestureStatus();
        private readonly APDS9960Status status = new APDS9960Status();

        private readonly APDS9960Config2 config2 = new APDS9960Config2();

        private readonly APDS9960GConfig1 gconfig1 = new APDS9960GConfig1();
        private readonly APDS9960GConfig2 gconfig2 = new APDS9960GConfig2();
        private readonly APDS9960GConfig3 gconfig3 = new APDS9960GConfig3();
        private readonly APDS9960GConfig4 gconfig4 = new APDS9960GConfig4();

        private byte upCount, downCount, leftCount, rightCount;

        /** I2C Registers */
        static readonly byte APDS9960_RAM = 0x00;
        static readonly byte APDS9960_ENABLE = 0x80;
        static readonly byte APDS9960_ATIME = 0x81;
        static readonly byte APDS9960_WTIME = 0x83;
        static readonly byte APDS9960_AILTIL = 0x84;
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

        static readonly byte APDS9960_UP = 0x01;    /**< Gesture Up */
        static readonly byte APDS9960_DOWN = 0x02;  /**< Gesture Down */
        static readonly byte APDS9960_LEFT = 0x03;  /**< Gesture Left */
        static readonly byte APDS9960_RIGHT = 0x04; /**< Gesture Right */

        IDigitalInputPort interruptPort;

        #endregion

        #region enums

        /** ADC gain settings */
        enum ADCGain : byte
        {
            GAIN_1X = 0x00,  /**< No gain */
            GAIN_4X = 0x01,  /**< 2x gain */
            GAIN_16X = 0x02, /**< 16x gain */
            GAIN_64X = 0x03  /**< 64x gain */
        }

        /** Proximity gain settings */
        enum ProximityGain : byte
        {
            PGAIN_1X = 0x00, /**< 1x gain */
            PGAIN_2X = 0x04, /**< 2x gain */
            PGAIN_4X = 0x08, /**< 4x gain */
            PGAIN_8X = 0x0C  /**< 8x gain */
        }

        /** Pulse length settings */
        enum PulseLength : byte
        {
            PL_4US = 0x00,  /**< 4uS */
            PL_8US = 0x40,  /**< 8uS */
            PL_16US = 0x80, /**< 16uS */
            PL_32US = 0xC0  /**< 32uS */
        }

        /** LED drive settings */
        enum LedDrive : byte
        {
            DRIVE_100MA = 0x00, /**< 100mA */
            DRIVE_50MA = 0x40,  /**< 50mA */
            DRIVE_25MA = 0x80,  /**< 25mA */
            DRIVE_12MA = 0xC0   /**< 12.5mA */
        }

        /** LED boost settings */
        enum LedBoost : byte
        {
            BOOST_100PCNT = 0x00, /**< 100% */
            BOOST_150PCNT = 0x10, /**< 150% */
            BOOST_200PCNT = 0x20, /**< 200% */
            BOOST_300PCNT = 0x30  /**< 300% */
        }

        /** Dimensions */
        enum Dimensions : byte
        {
            ALL = 0x00,        // All dimensions
            UP_DOWN = 0x01,    // Up/Down dimensions
            LEFT_RIGHT = 0x02, // Left/Right dimensions
        };

        /** FIFO Interrupts */
        enum FifoInterurrupt
        {
            FIFO_1 = 0x00,  // Generate interrupt after 1 dataset in FIFO
            FIFO_4 = 0x01,  // Generate interrupt after 2 datasets in FIFO
            FIFO_8 = 0x02,  // Generate interrupt after 3 datasets in FIFO
            FIFO_16 = 0x03, // Generate interrupt after 4 datasets in FIFO
        };

        /** Gesture Gain */
        enum GestureGain
        {
            GGAIN_1 = 0x00, // Gain 1x
            GGAIN_2 = 0x01, // Gain 2x
            GGAIN_4 = 0x02, // Gain 4x
            GGAIN_8 = 0x03, // Gain 8x
        };

        /** Pulse Lenghts */
        enum GesturePulseLength
        {
            GPL_4US = 0x00,  // Pulse 4us
            GPL_8US = 0x01,  // Pulse 8us
            GPL_16US = 0x02, // Pulse 16us
            GPL_32US = 0x03, // Pulse 32us
        };

        #endregion

        #region Constructors

        /// <summary>
        ///     Make the default constructor private so that it cannot be used.
        /// </summary>
        private Apds9960()
        {
        }

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

            Apds9960Init();
        }

        private void InterruptPort_Changed(object sender, DigitalInputPortEventArgs e)
        {
            Console.WriteLine($"Motion detected: {e.Value}");
        }

        #endregion Constructors

        #region methods
        void Apds9960Init()
        {
            var result = apds9960.ReadRegister(0);// APDS9960_ID);

            if (result != 0xAB)
                throw new Exception("APDS9960 isn't connected");
        }

        //Sets the integration time for the ADC of the APDS9960, in millis
        void SetADCIntegrationTime(int timeMS)
        {
            // convert ms into 2.78ms increments
            float time = timeMS;

            time = 256 - time / 2.78f;

            if (time > 255) time = 255;
            if (time < 0) time = 0;

            /* Update the timing register */
            apds9960.WriteRegister(APDS9960_ATIME, (byte)time);
        }

        //Returns the integration time for the ADC of the APDS9960, in millis
        float GetADCIntegrationTime()
        {
            float time = apds9960.ReadRegister(APDS9960_ATIME);
            //this doesn't look right ....
            return (256 - time) * 2.78f;
        }

        //Adjusts the color/ALS gain on the APDS9960 (adjusts the sensitivity to light)
        void SetADCGain(ADCGain gain)
        {
            control.AGAIN = (byte)gain;

            /* Update the timing register */

            apds9960.WriteRegister(APDS9960_CONTROL, control.Get());
        }

        //Returns the ADC gain
        ADCGain GetADCGain()
        {
            return (ADCGain)(apds9960.ReadRegister(APDS9960_CONTROL) & 0x03);
        }

        void SetProximityGain(ProximityGain gain)
        {
            control.PGAIN = (byte)gain;

            apds9960.WriteRegister(APDS9960_CONTROL, control.Get());
        }

        ProximityGain GetProximityGain()
        {
            return (ProximityGain)(apds9960.ReadRegister(APDS9960_CONTROL) & 0x0C);
        }

        public void EnableProximity(bool enableProximity)
        {
            enable.PEN = enableProximity ? (byte)1 : (byte)0;

            apds9960.WriteRegister(APDS9960_ENABLE, enable.Get());
        }

        public void EnableProximityInterrupt(bool enableInterrupt)
        {
            enable.PIEN = enableInterrupt ? (byte)1 : (byte)0;

            apds9960.WriteRegister(APDS9960_ENABLE, enable.Get());

            if (enableInterrupt)
            {
                //ToDo ClearInterupt <- might check on this ....
            }
        }

        public void SetProximityInterruptThreshhold(byte low, byte high, byte persistance = 4)
        {
            apds9960.WriteRegister(APDS9960_PILT, high);
            apds9960.WriteRegister(APDS9960_PIHT, low);

            if (persistance > 7) persistance = 7;

            this.persistance.PPERS = persistance;

            apds9960.WriteRegister(APDS9960_PERS, this.persistance.Get());
        }

        //Sets number of proxmity pulses
        void SetProximityPulse(PulseLength length, byte count)
        {
            if (count < 1 || count > 64)
                throw new ArgumentOutOfRangeException();

            pulse.PPULSE = count;
            pulse.PPLEN = (byte)length;

            apds9960.WriteRegister(APDS9960_PPULSE, pulse.Get());
        }

        byte ReadProximity()
        {
            return apds9960.ReadRegister(APDS9960_PDATA);
        }

        bool IsGestureValid()
        {
            gestureStatus.Set(apds9960.ReadRegister(APDS9960_GSTATUS));
            return gestureStatus.GVALID == 1;
        }


        /*!
         *  @brief  Sets gesture dimensions
         *  @param  dims
         *          Dimensions (APDS9960_DIMENSIONS_ALL, APDS9960_DIMENSIONS_UP_DOWM,
         *          APDS9960_DIMENSIONS_UP_DOWN, APGS9960_DIMENSIONS_LEFT_RIGHT)
         */
        void SetGestureDimentions(byte dimensions)
        {
            gconfig3.GDIMS = dimensions;
            apds9960.WriteRegister(APDS9960_GCONF3, gconfig3.Get());
        }

        //Sets gesture FIFO Threshold
        void SetGestureFIFOThreshold(byte threshhold)
        {
            gconfig1.GFIFOTH = threshhold;
            apds9960.WriteRegister(APDS9960_GCONF1, gconfig1.Get());
        }

        void SetGestureGain(byte gain)
        {
            gconfig2.GGAIN = gain;
            apds9960.WriteRegister(APDS9960_GCONF2, gconfig2.Get());
        }

        void SetGestureProximityThreshold(byte threshhold)
        {
            apds9960.WriteRegister(APDS9960_GPENTH, threshhold);
        }

        void SetGestureOffset(byte offsetUp, byte offsetDown, byte offsetLeft, byte offsetRight)
        {
            apds9960.WriteRegister(APDS9960_GOFFSET_U, offsetUp);
            apds9960.WriteRegister(APDS9960_GOFFSET_D, offsetDown);
            apds9960.WriteRegister(APDS9960_GOFFSET_L, offsetLeft);
            apds9960.WriteRegister(APDS9960_GOFFSET_R, offsetRight);
        }

        public void EnableGestures(bool enable)
        {
            if (enable == false)
            {
                gconfig4.GMODE = 0;
                apds9960.WriteRegister(APDS9960_GCONF4, gconfig4.Get());
            }

            this.enable.GEN = enable ? (byte)1 : (byte)0;
            apds9960.WriteRegister(APDS9960_ENABLE, this.enable.Get());

            ResetCounts();
        }

        void ResetCounts()
        {
            upCount = 0;
            downCount = 0;
            leftCount = 0;
            rightCount = 0;
        }

        byte ReadGesture()
        {
            byte toRead;
            byte[] buf = new byte[256];

            byte gestureReceived;

            DateTime gestureDetected = DateTime.Now;

            while (true)
            {
                int up_down_diff = 0;
                int left_right_diff = 0;

                gestureReceived = 0;

                if (!IsGestureValid())
                    return 0;

                Thread.Sleep(30);

                toRead = apds9960.ReadRegister(APDS9960_GFLVL);

                // bytesRead is unused but produces sideffects needed for readGesture to work
                // ToDo - check on this bytesRead = this->read(APDS9960_GFIFO_U, buf, toRead);

                if (Math.Abs((int)buf[0] - (int)buf[1]) > 13)
                {
                    up_down_diff += (int)buf[0] - (int)buf[1];
                }

                if (Math.Abs((int)buf[2] - (int)buf[3]) > 13)
                {
                    left_right_diff += (int)buf[2] - (int)buf[3];
                }

                if (up_down_diff != 0)
                {
                    if (up_down_diff < 0)
                    {
                        if (downCount > 0)
                            gestureReceived = APDS9960_UP;
                        else
                            upCount++;
                    }
                    else if (up_down_diff > 0)
                    {
                        if (upCount > 0)
                            gestureReceived = APDS9960_DOWN;
                        else
                            downCount++;
                    }
                }

                if (left_right_diff != 0)
                {
                    if (left_right_diff < 0)
                    {
                        if (rightCount > 0)
                            gestureReceived = APDS9960_LEFT;
                        else
                            leftCount++;
                    }
                    else if (left_right_diff > 0)
                    {
                        if (leftCount > 0)
                            gestureReceived = APDS9960_RIGHT;
                        else
                            rightCount++;
                    }
                }

                if (up_down_diff != 0 || left_right_diff != 0)
                {
                    gestureDetected = DateTime.Now;
                }

                if (gestureReceived > 0 ||
                    (DateTime.Now - gestureDetected) > new TimeSpan(0, 0, 0, 0, 3000))
                {
                    ResetCounts();

                    return gestureReceived;
                }
            }
        }

        void SetLed(LedDrive drive, LedBoost boost)
        {
            config2.LED_BOOST = (byte)boost;

            apds9960.WriteRegister(APDS9960_CONFIG2, config2.Get());

            control.LDRIVE = (byte)drive;

            apds9960.WriteRegister(APDS9960_CONTROL, control.Get());
        }

        public void EnableColorSensor(bool colorEnabled)
        {
            enable.AEN = colorEnabled ? (byte)1 : (byte)0;
            apds9960.WriteRegister(APDS9960_ENABLE, enable.Get());
        }

        bool IsColorDataReady()
        {
            status.Set(apds9960.ReadRegister(APDS9960_STATUS));
            return status.AVALID == 1;
        }

        //Red, green, blue, clear
        //ToDo - check byte order ... Arduino driver doesn't specify 
        void GetColorData(out int R, out int G, out int B, out int C)
        {
            R = apds9960.ReadUShort(APDS9960_RDATAL, ByteOrder.BigEndian);
            G = apds9960.ReadUShort(APDS9960_GDATAL, ByteOrder.BigEndian);
            B = apds9960.ReadUShort(APDS9960_BDATAL, ByteOrder.BigEndian);
            C = apds9960.ReadUShort(APDS9960_CDATAL, ByteOrder.BigEndian);
        }

        void EnableColorInterrupt(bool enableInterrupt)
        {
            enable.AIEN = enableInterrupt ? (byte)1 : (byte)0;
            apds9960.WriteRegister(APDS9960_ENABLE, enable.Get());
        }

        void ClearInterrupt()
        {
            //ToDo - Arduino driver call here includes a null in the param list 
            apds9960.WriteRegister(APDS9960_AICLEAR, 0);
        }

        public void SetInteruptLimits(int low, int high)
        {
            apds9960.WriteRegister(APDS9960_AILTIL, (byte)(low & 0xFF));
            apds9960.WriteRegister(APDS9960_AILTH, (byte)(low >> 8));
            apds9960.WriteRegister(APDS9960_AIHTL, (byte)(high & 0xFF));
            apds9960.WriteRegister(APDS9960_AIHTH, (byte)(high >> 8));
        }

        //enable sensor
        public void Enable(bool enable)
        {
            apds9960.WriteRegister(APDS9960_ENABLE, (byte)(enable ? 1 : 0));
        }

        #endregion

        #region classes
        class APDS9960Enable
        {
            // power on
            public byte PON { get; set; }

            // ALS enable
            public byte AEN { get; set; }

            // Proximity detect enable
            public byte PEN { get; set; }

            // wait timer enable
            public byte WEN { get; set; }

            // ALS interrupt enable
            public byte AIEN { get; set; }

            // proximity interrupt enable
            public byte PIEN { get; set; }

            // gesture enable
            public byte GEN { get; set; }

            public byte Get()
            {
                return (byte)((GEN << 6) | (PIEN << 5) | (AIEN << 4) | (WEN << 3) | (PEN << 2) |
                       (AEN << 1) | PON);
            }
        }

        // ALS Interrupt Persistence. Controls rate of Clear channel interrupt to
        // the host processor
        class APDS9960Persistance
        {
            public byte APERS { get; set; }
            public byte PPERS { get; set; }

            public byte Get()
            {
                return (byte)((PPERS << 4) | APERS);
            }
        }

        class APDS9960Control
        {
            public byte AGAIN { get; set; }
            public byte PGAIN { get; set; }
            public byte LDRIVE { get; set; }

            public byte Get()
            {
                return (byte)((LDRIVE << 6) | (PGAIN << 2) | AGAIN);
            }
        }

        class APDS9960Pulse
        {
            public byte PPULSE { get; set; }
            public byte PPLEN { get; set; }

            public byte Get()
            {
                return (byte)((PPLEN << 6) | PPULSE);
            }
        }

        class APDS9960Config2
        {
            public byte LED_BOOST { get; set; }
            public byte CPSIEN { get; set; }
            public byte PSIEN { get; set; }

            public byte Get()
            {
                return (byte)((PSIEN << 7) | (CPSIEN << 6) | (LED_BOOST << 4) | 1);
            }
        }

        class APDS9960GConfig1
        {
            public byte GEXPERS { get; set; }
            public byte GEXMSK { get; set; }
            public byte GFIFOTH { get; set; }

            public byte Get()
            {
                return (byte)((GFIFOTH << 6) | (GEXMSK << 2) | GEXPERS);
            }
        }

        class APDS9960GConfig2
        {
            public byte GWTIME { get; set; }
            public byte GLDRIVE { get; set; }
            public byte GGAIN { get; set; }

            public byte Get()
            {
                return (byte)((GGAIN << 5) | (GLDRIVE << 3) | GWTIME);
            }
        }

        class APDS9960GConfig3
        {
            public byte GDIMS { get; set; }

            public byte Get()
            {
                return GDIMS;
            }
        }

        class APDS9960GConfig4
        {
            public byte GMODE { get; set; }
            public byte GIEN { get; set; }

            public byte Get()
            {
                return (byte)((GIEN << 1) | GMODE);
            }

            public void Set(byte data)
            {
                GIEN = (byte)((data >> 1) & 0x01);
                GMODE = (byte)(data & 0x01);
            }
        }

        class APDS9960GestureStatus
        {
            public byte GVALID { get; set; }
            public byte GFOV { get; set; }

            public void Set(byte value)
            {
                GFOV = (byte)((value >> 1) & 0x01);

                GVALID = (byte)(value & 0x01);
            }
        }

        class APDS9960Status
        {
            public byte AVALID { get; set; }
            public byte PVALID { get; set; }
            public byte GINT { get; set; }
            public byte AINT { get; set; }
            public byte PINT { get; set; }
            public byte PGSAT { get; set; }
            public byte CPSAT { get; set; }

            public void Set(byte data)
            {
                AVALID = (byte)(data & 0x01);
                PVALID = (byte)((data >> 1) & 0x01);
                GINT = (byte)((data >> 2) & 0x01);
                AINT = (byte)((data >> 4) & 0x01);
                PINT = (byte)((data >> 5) & 0x01);
                PGSAT = (byte)((data >> 6) & 0x01);
                CPSAT = (byte)((data >> 7) & 0x01);
            }
        }

        #endregion
    }
}