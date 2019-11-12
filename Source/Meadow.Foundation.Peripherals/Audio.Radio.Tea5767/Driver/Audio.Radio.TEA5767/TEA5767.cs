
using System.Threading;
using Meadow.Hardware;

namespace Meadow.Foundation.Audio.Radio
{
    public class TEA5767
    {
        #region Member variables / fields
        /// <summary>
        ///     TEA5767 radio.
        /// </summary>
        private readonly II2cPeripheral _I2cPeripheral;

        #endregion Member variables / fields


        static readonly byte TEA5767_ADDRESS = 0x60;
        static byte FIRST_DATA = 0;
        static byte SECOND_DATA = 1;
        static byte THIRD_DATA = 2;
        static byte FOURTH_DATA = 3;
        static byte FIFTH_DATA = 4;
        static byte LOW_STOP_LEVEL = 1;
        static byte MID_STOP_LEVEL = 2;
        static byte HIGH_STOP_LEVEL = 3;
        /*    static byte HIGH_SIDE_INJECTION           = 1;
            static byte LOW_SIDE_INJECTION            = 0;
            static byte STEREO_ON                     = 0;
            static byte STEREO_OFF                    = 1;
            static byte MUTE_RIGHT_ON                 = 1;
            static byte MUTE_RIGHT_OFF                = 0;
            static byte MUTE_LEFT_ON                  = 1;
            static byte MUTE_LEFT_OFF = 0;
            static byte SWP1_HIGH                     = 1;
            static byte SWP1_LOW                      = 0;
            static byte SWP2_HIGH                     = 1;
            static byte SWP2_LOW                      = 0;
            static byte STBY_ON                       = 1;
            static byte STBY_OFF                      = 0;
            static byte JAPANESE_FM_BAND              = 1;
            static byte US_EUROPE_FM_BAND = 0;
            static byte SOFT_MUTE_ON = 1;
            static byte SOFT_MUTE_OFF = 0;
            static byte HIGH_CUT_CONTROL_ON = 1;
            static byte HIGH_CUT_CONTROL_OFF = 0;
            static byte STEREO_NOISE_CANCELLING_ON = 1;
            static byte STEREO_NOISE_CANCELLING_OFF = 0;
            static byte SEARCH_INDICATOR_ON = 1;
            static byte SEARCH_INDICATOR_OFF = 0;*/

        float frequency;
        byte hiInjection;
        byte[] transmissionData = new byte[5];
        byte[] reception_data = new byte[5];

        public bool IsMuted { get; set; }

        #region Properties

        #endregion Properties

        #region Constructors

        /// <summary>
        ///     Default constructor is private to prevent it being used.
        /// </summary>
        private TEA5767() { }

        /// <summary>
        ///     Create a new TEA5767 object using the default parameters
        /// </summary>
        /// <param name="address">Address of the bus on the I2C display.</param>
        public TEA5767(II2cBus i2cBus, byte address = 0x60)
        {
            _I2cPeripheral = new I2cPeripheral(i2cBus, address);

            InitTEA5767();
        }

        #endregion Constructors

        #region Methods

        void InitTEA5767()
        {
            transmissionData[FIRST_DATA] = 0;            //MUTE: 0 - not muted
            //SEARCH MODE: 0 - not in search mode

            transmissionData[SECOND_DATA] = 0;           //No frequency defined yet
            transmissionData[THIRD_DATA] = 0xB0;         //10110000
            //SUD: 1 - search up
            //SSL[1:0]: 01 - low; level ADC output = 5
            //HLSI: 1 - high side LO injection
            //MS: 0 - stereo ON
            //MR: 0 - right audio channel is not muted
            //ML: 0 - left audio channel is not muted
            //SWP1: 0 - port 1 is LOW

            transmissionData[FOURTH_DATA] = 0x10;        //00010000
            //SWP2: 0 - port 2 is LOW
            //STBY: 0 - not in Standby mode
            //BL: 0 - US/Europe FM band
            //XTAL: 1 - 32.768 kHz
            //SMUTE: 0 - soft mute is OFF
            //HCC: 0 - high cut control is OFF
            //SNC: 0 - stereo noise cancelling is OFF
            //SI: 0 - pin SWPORT1 is software programmable port 1

            transmissionData[FIFTH_DATA] = 0x00;         //PLLREF: 0 - the 6.5 MHz reference frequency for the PLL is disabled
            //DTC: 0 - the de-emphasis time constant is 50 ms
        }

        void CalculateOptimalHiLoInjection(float freq)
        {
            byte signalHigh;
            byte signalLow;

            SetHighSideLOInjection();
            SetFrequency((float)(freq + 0.45));

            signalHigh = GetSignalLevel();

            SetLowSideLOInjection();
            SetFrequency((float)(freq - 0.45));

            signalLow = GetSignalLevel();

            hiInjection = (signalHigh < signalLow) ? (byte)1 : (byte)0;
        }

        public void SetFrequency(float frequency)
        {
            this.frequency = frequency;
            uint frequencyW;

            if (hiInjection > 0)
            {
                SetHighSideLOInjection();
                frequencyW = (uint)(4 * ((frequency * 1000000) + 225000) / 32768);
            }
            else
            {
                SetLowSideLOInjection();
                frequencyW = (uint)(4 * ((frequency * 1000000) - 225000) / 32768);
            }

            transmissionData[FIRST_DATA] = (byte)((transmissionData[FIRST_DATA] & 0xC0) | ((frequencyW >> 8) & 0x3F));
            transmissionData[SECOND_DATA] = (byte)(frequencyW & 0xFF);

            TransmitData();
        }

        void TransmitData()
        {
            _I2cPeripheral.WriteBytes(transmissionData);

            Thread.Sleep(100);
        }

        public void Mute()
        {
            IsMuted = true;
            SetSoundOff();
            TransmitData();
        }

        void SetSoundOff()
        {
            transmissionData[FIRST_DATA] |= 128;
        }

        void SetSoundBackOn()
        {
            IsMuted = false;
            SetSoundOn();
            TransmitData();
        }

        void SetSoundOn()
        {
            transmissionData[FIRST_DATA] &= 127;
        }

        public void SelectFrequency(float frequency)
        {
            CalculateOptimalHiLoInjection(frequency);
            SetFrequency(frequency);
        }

        void SelectFrequencyMuting(float frequency)
        {
            Mute();
            CalculateOptimalHiLoInjection(frequency);
            SetFrequency(frequency);
            SetSoundBackOn();
        }

        void ReadStatus()
        {
            reception_data = _I2cPeripheral.ReadBytes(5);
            Thread.Sleep(100);
        }

        public double GetFrequency()
        {
            LoadFrequency();

            uint frequencyW = (uint)(((reception_data[FIRST_DATA] & 0x3F) * 256) + reception_data[SECOND_DATA]);

            if (hiInjection > 0)
            {
                return (frequencyW / 4.0 * 32768.0 - 225000.0) / 1000000.0;
            }
            else
            {
                return (frequencyW / 4.0 * 32768.0 + 225000.0) / 1000000.0;
            }
        }

        void LoadFrequency()
        {
            ReadStatus();

            //Stores the read frequency that can be the result of a search and it�s not yet in transmission data
            //and is necessary to subsequent calls to search.
            transmissionData[FIRST_DATA] = (byte)((transmissionData[FIRST_DATA] & 0xC0) | (reception_data[FIRST_DATA] & 0x3F));
            transmissionData[SECOND_DATA] = reception_data[SECOND_DATA];
        }

        void SetSearchUp()
        {
            transmissionData[THIRD_DATA] |= 128;
        }

        void SetSearchDown()
        {
            transmissionData[THIRD_DATA] &= 127;
        }

        void setSearchLowStopLevel()
        {
            transmissionData[THIRD_DATA] &= 237;
            transmissionData[THIRD_DATA] |= (byte)(LOW_STOP_LEVEL << 5);
        }

        void SetSearchMidStopLevel()
        {
            transmissionData[THIRD_DATA] &= 237;
            transmissionData[THIRD_DATA] |= (byte)(MID_STOP_LEVEL << 5);
        }

        void SetSearchHighStopLevel()
        {
            transmissionData[THIRD_DATA] &= 237;
            transmissionData[THIRD_DATA] |= (byte)(HIGH_STOP_LEVEL << 5);
        }

        void SetHighSideLOInjection()
        {
            transmissionData[THIRD_DATA] |= 16;
        }

        void SetLowSideLOInjection()
        {
            transmissionData[THIRD_DATA] &= 239;
        }

        public byte SearchNextSilent()
        {
            byte bandLimitReached;

            Mute();
            bandLimitReached = SearchNext();
            SetSoundBackOn();

            return bandLimitReached;
        }

        public byte SearchNext()
        {
            byte bandLimitReached;

            if (IsSearchUp())
            {
                SelectFrequency((float)GetFrequency() + 0.1f);
            }
            else
            {
                SelectFrequency((float)GetFrequency() - 0.1f);
            }

            //Turns the search on
            transmissionData[FIRST_DATA] |= 64;
            TransmitData();

            while (IsReady())
            {
                Thread.Sleep(50);
            }

            //Read Band Limit flag
            bandLimitReached = IsBandLimitReached();
            //Loads the new selected frequency
            LoadFrequency();

            //Turns de search off
            transmissionData[FIRST_DATA] &= 191;
            TransmitData();

            return bandLimitReached;
        }

        byte SearchMutingFromBeginning()
        {
            byte bandLimitReached;

            Mute();
            bandLimitReached = startsSearchFromBeginning();
            SetSoundBackOn();

            return bandLimitReached;
        }

        byte SearchMutingFromEnd()
        {
            byte bandLimitReached;

            Mute();
            bandLimitReached = startsSearchFromEnd();
            SetSoundBackOn();

            return bandLimitReached;
        }

        byte startsSearchFromBeginning()
        {
            SetSearchUp();
            return startsSearchFrom(87.0f);
        }

        byte startsSearchFromEnd()
        {
            SetSearchDown();
            return startsSearchFrom(108.0f);
        }

        byte startsSearchFrom(float frequency)
        {
            SelectFrequency(frequency);
            return SearchNext();
        }

        public byte GetSignalLevel()
        {
            //Necessary before read status
            TransmitData();
            //Read updated status
            ReadStatus();
            return (byte)(reception_data[FOURTH_DATA] >> 4);
        }

        public bool IsStereo()
        {
            ReadStatus();
            return (reception_data[THIRD_DATA] >> 7) > 0;
        }

        public bool IsReady()
        {
            ReadStatus();
            return (reception_data[FIRST_DATA] >> 7) > 0;
        }

        byte IsBandLimitReached()
        {
            ReadStatus();
            return (byte)((reception_data[FIRST_DATA] >> 6) & 1);
        }

        bool IsSearchUp()
        {
            return ((transmissionData[THIRD_DATA] & 128) != 0);
        }

        bool IsSearchDown()
        {
            return ((transmissionData[THIRD_DATA] & 128) == 0);
        }

        bool IsStandby()
        {
            ReadStatus();
            return (transmissionData[FOURTH_DATA] & 64) != 0;
        }

        public void EnableStereo(bool enable)
        {
            if (enable)
            {
                transmissionData[THIRD_DATA] &= 247;
            }
            else
            {
                transmissionData[THIRD_DATA] |= 8;
            }
            TransmitData();
        }

        void SetSoftMuteOn()
        {
            transmissionData[FOURTH_DATA] |= 8;
            TransmitData();
        }

        void SetSoftMuteOff()
        {
            transmissionData[FOURTH_DATA] &= 247;
            TransmitData();
        }

        void MuteRight()
        {
            transmissionData[THIRD_DATA] |= 4;
            TransmitData();
        }

        void TurnTheRightSoundBackOn()
        {
            transmissionData[THIRD_DATA] &= 251;
            TransmitData();
        }

        void MuteLeft()
        {
            transmissionData[THIRD_DATA] |= 2;
            TransmitData();
        }

        void TurnTheLeftSoundBackOn()
        {
            transmissionData[THIRD_DATA] &= 253;
            TransmitData();
        }

        void EnableStandby(bool enable)
        {
            if (enable)
                transmissionData[FOURTH_DATA] |= 64;
            else
                transmissionData[FOURTH_DATA] &= 191;
            TransmitData();
        }

        void SetHighCutControlOn()
        {
            transmissionData[FOURTH_DATA] |= 4;
            TransmitData();
        }

        void SetHighCutControlOff()
        {
            transmissionData[FOURTH_DATA] &= 251;
            TransmitData();
        }

        void SetStereoNoiseCancellingOn()
        {
            transmissionData[FOURTH_DATA] |= 2;
            TransmitData();
        }

        void SetStereoNoiseCancellingOff()
        {
            transmissionData[FOURTH_DATA] &= 253;
            TransmitData();
        }

        #endregion Methods
    }
}