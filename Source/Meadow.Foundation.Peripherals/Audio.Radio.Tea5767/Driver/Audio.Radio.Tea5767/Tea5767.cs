
using System;
using System.Threading;
using Meadow.Hardware;
using Meadow.Units;

namespace Meadow.Foundation.Audio.Radio
{
    /// <summary>
    /// Represents a TEA5767 radio
    /// </summary>
    public partial class Tea5767
    {
        /// <summary>
        ///     TEA5767 radio.
        /// </summary>
        private readonly II2cPeripheral i2cPeripheral;

        byte hiInjection;
        readonly Memory<byte> writeBuffer = new byte[5];
        readonly Memory<byte> readBuffer = new byte[5];

        /// <summary>
        /// Is the audio muted
        /// </summary>
        public bool IsMuted { get; set; }

        /// <summary>
        ///     Create a new TEA5767 object using the default parameters
        /// </summary>
        /// <param name="i2cBus">I2Cbus connected to the radio</param>
        /// <param name="address">Address of the bus on the I2C display.</param>
        public Tea5767(II2cBus i2cBus, byte address = (byte)Address.Default)
        {
            i2cPeripheral = new I2cPeripheral(i2cBus, address);

            InitTEA5767();
        }

        void InitTEA5767()
        {
            writeBuffer.Span[(byte)Command.FIRST_DATA] = 0;            //MUTE: 0 - not muted
            //SEARCH MODE: 0 - not in search mode

            writeBuffer.Span[(byte)Command.SECOND_DATA] = 0;           //No frequency defined yet
            writeBuffer.Span[(byte)Command.THIRD_DATA] = 0xB0;         //10110000
            //SUD: 1 - search up
            //SSL[1:0]: 01 - low; level ADC output = 5
            //HLSI: 1 - high side LO injection
            //MS: 0 - stereo ON
            //MR: 0 - right audio channel is not muted
            //ML: 0 - left audio channel is not muted
            //SWP1: 0 - port 1 is LOW

            writeBuffer.Span[(byte)Command.FOURTH_DATA] = 0x10;        //00010000
            //SWP2: 0 - port 2 is LOW
            //STBY: 0 - not in Standby mode
            //BL: 0 - US/Europe FM band
            //XTAL: 1 - 32.768 kHz
            //SMUTE: 0 - soft mute is OFF
            //HCC: 0 - high cut control is OFF
            //SNC: 0 - stereo noise cancelling is OFF
            //SI: 0 - pin SWPORT1 is software programmable port 1

            writeBuffer.Span[(byte)Command.FIFTH_DATA] = 0x00;         //PLLREF: 0 - the 6.5 MHz reference frequency for the PLL is disabled
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

        /// <summary>
        /// Frequency in Mhz
        /// </summary>
        /// <param name="frequency">frequency in Mhz</param>
        void SetFrequency(double frequency)
        {
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

            writeBuffer.Span[(byte)Command.FIRST_DATA] = (byte)((byte)(writeBuffer.Span[(byte)Command.FIRST_DATA] & 0xC0) | ((frequencyW >> 8) & 0x3F));
            writeBuffer.Span[(byte)Command.SECOND_DATA] = (byte)(frequencyW & 0xFF);

            TransmitData();
        }

        void TransmitData()
        {
            i2cPeripheral.Exchange(writeBuffer.Span, readBuffer.Span);

            Thread.Sleep(100);
        }

        /// <summary>
        /// Mute audio if not muted
        /// </summary>
        public void Mute()
        {
            IsMuted = true;
            SetSoundOff();
            TransmitData();
        }

        void SetSoundOff()
        {
            writeBuffer.Span[(byte)Command.FIRST_DATA] |= 128;
        }

        void SetSoundBackOn()
        {
            IsMuted = false;
            SetSoundOn();
            TransmitData();
        }

        void SetSoundOn()
        {
            writeBuffer.Span[(byte)Command.FIRST_DATA] &= 127;
        }

        /// <summary>
        /// Select radio frequency
        /// </summary>
        /// <param name="frequency">the frequency</param>
        public void SelectFrequency(Frequency frequency)
        {
            CalculateOptimalHiLoInjection((float)frequency.Megahertz);
            SetFrequency(frequency.Megahertz);
        }

        void ReadStatus()
        {
            i2cPeripheral.Read(readBuffer.Span);
            Thread.Sleep(100);
        }

        /// <summary>
        /// Get the current radio frequency
        /// </summary>
        /// <returns></returns>
        public Frequency GetFrequency()
        {
            LoadFrequency();

            uint frequencyW = (uint)(((readBuffer.Span[(byte)Command.FIRST_DATA] & 0x3F) * 256) + readBuffer.Span[(byte)Command.SECOND_DATA]);

            if (hiInjection > 0)
            {
                return new Frequency(frequencyW / 4.0 * 32768.0 - 225000.0, Frequency.UnitType.Hertz);
            }
            else
            {
                return new Frequency(frequencyW / 4.0 * 32768.0 + 225000.0, Frequency.UnitType.Hertz);
            }
        }

        void LoadFrequency()
        {
            ReadStatus();

            //Stores the read frequency that can be the result of a search and it�s not yet in transmission data
            //and is necessary to subsequent calls to search.
            writeBuffer.Span[(byte)Command.FIRST_DATA] = (byte)((writeBuffer.Span[(byte)Command.FIRST_DATA] & 0xC0) | (readBuffer.Span[(byte)Command.FIRST_DATA] & 0x3F));
            writeBuffer.Span[(byte)Command.SECOND_DATA] = readBuffer.Span[(byte)Command.SECOND_DATA];
        }

        void SetSearchUp()
        {
            writeBuffer.Span[(byte)Command.THIRD_DATA] |= 128;
        }

        void SetSearchDown()
        {
            writeBuffer.Span[(byte)Command.THIRD_DATA] &= 127;
        }

        /*
        void SetSearchLowStopLevel()
        {
            writeBuffer.Span[(byte)Command.THIRD_DATA] &= 237;
            writeBuffer.Span[(byte)Command.THIRD_DATA] |= ((byte)Command.LOW_STOP_LEVEL << 5);
        }

        void SetSearchMidStopLevel()
        {
            writeBuffer.Span[(byte)Command.THIRD_DATA] &= 237;
            writeBuffer.Span[(byte)Command.THIRD_DATA] |= ((byte)Command.MID_STOP_LEVEL << 5);
        }

        void SetSearchHighStopLevel()
        {
            writeBuffer.Span[(byte)Command.THIRD_DATA] &= 237;
            writeBuffer.Span[(byte)Command.THIRD_DATA] |= ((byte)Command.HIGH_STOP_LEVEL << 5);
        }
        */

        void SetHighSideLOInjection()
        {
            writeBuffer.Span[(byte)Command.THIRD_DATA] |= 16;
        }

        void SetLowSideLOInjection()
        {
            writeBuffer.Span[(byte)Command.THIRD_DATA] &= 239;
        }

        /// <summary>
        /// Search to next station and mute while seeking
        /// </summary>
        /// <returns></returns>
        public bool SearchNextSilent()
        {
            Mute();
            SetSoundBackOn();

            return SearchNext();
        }

        /// <summary>
        /// Search to next station
        /// </summary>
        /// <returns></returns>
        public bool SearchNext()
        {
            if (IsSearchUp())
            {
                SelectFrequency(GetFrequency() + new Frequency(0.1, Frequency.UnitType.Megahertz));
            }
            else
            {
                SelectFrequency(GetFrequency() - new Frequency(0.1, Frequency.UnitType.Megahertz));
            }

            //Turns the search on
            writeBuffer.Span[(byte)Command.FIRST_DATA] |= 64;
            TransmitData();

            while (IsReady())
            {
                Thread.Sleep(50);
            }

            //Loads the new selected frequency
            LoadFrequency();

            //Turns de search off
            writeBuffer.Span[(byte)Command.FIRST_DATA] &= 191;
            TransmitData();

            //Read Band Limit flag
            return IsBandLimitReached();
        }

        /// <summary>
        /// Start searching for station from lowest frequency (87Mhz)
        /// </summary>
        /// <returns>true if station found</returns>
        public bool SearchFromBeginningMuted()
        {
            bool bandLimitReached;

            Mute();
            bandLimitReached = StartSearchFromBeginning();
            SetSoundBackOn();

            return !bandLimitReached;
        }

        /// <summary>
        /// Start searching for station from highest frequency (108Mhz)
        /// </summary>
        /// <returns></returns>
        public bool SearchFromEndMuted()
        {
            bool bandLimitReached;

            Mute();
            bandLimitReached = StartSearchFromEnd();
            SetSoundBackOn();

            return !bandLimitReached;
        }

        /// <summary>
        /// Start searching for station from lowest frequency (87Mhz)
        /// </summary>
        /// <returns>true if station found</returns>
        public bool StartSearchFromBeginning()
        {
            SetSearchUp();
            return StartSearchFrom(87.0f);
        }

        /// <summary>
        /// Start searching for station from highest frequency (108Mhz)
        /// </summary>
        /// <returns></returns>
        public bool StartSearchFromEnd()
        {
            SetSearchDown();
            return StartSearchFrom(108.0f);
        }

        bool StartSearchFrom(float frequency)
        {
            SelectFrequency(new Frequency(frequency, Frequency.UnitType.Megahertz));
            return SearchNext();
        }

        /// <summary>
        /// Get Signal Level
        /// </summary>
        /// <returns>level as a byte (0-255)</returns>
        public byte GetSignalLevel()
        {
            //Necessary before read status
            TransmitData();
            //Read updated status
            ReadStatus();
            return (byte)(readBuffer.Span[(byte)Command.FOURTH_DATA] >> 4);
        }

        /// <summary>
        /// Is the current station stereo
        /// </summary>
        /// <returns></returns>
        public bool IsStereo()
        {
            ReadStatus();
            return (readBuffer.Span[(byte)Command.THIRD_DATA] >> 7) > 0;
        }

        /// <summary>
        /// Is the radio ready for operation
        /// </summary>
        /// <returns></returns>
        public bool IsReady()
        {
            ReadStatus();
            return (readBuffer.Span[(byte)Command.FIRST_DATA] >> 7) > 0;
        }

        bool IsBandLimitReached()
        {
            ReadStatus();
            return ((readBuffer.Span[(byte)Command.FIRST_DATA] >> 6) & 1) == 1;
        }

        /// <summary>
        /// Is search mode up
        /// </summary>
        /// <returns></returns>
        public bool IsSearchUp()
        {
            return ((writeBuffer.Span[(byte)Command.THIRD_DATA] & 128) != 0);
        }

        /// <summary>
        /// Is search mode down
        /// </summary>
        /// <returns></returns>
        public bool IsSearchDown()
        {
            return ((writeBuffer.Span[(byte)Command.THIRD_DATA] & 128) == 0);
        }

        /// <summary>
        /// Is the radio in standby mode
        /// </summary>
        /// <returns></returns>
        public bool IsStandby()
        {
            ReadStatus();
            return (writeBuffer.Span[(byte)Command.FOURTH_DATA] & 64) != 0;
        }

        /// <summary>
        /// Enable stereo if set to mono
        /// </summary>
        /// <param name="enable"></param>
        public void EnableStereo(bool enable)
        {
            if (enable)
            {
                writeBuffer.Span[(byte)Command.THIRD_DATA] &= 247;
            }
            else
            {
                writeBuffer.Span[(byte)Command.THIRD_DATA] |= 8;
            }
            TransmitData();
        }

        /// <summary>
        /// Enable soft mute
        /// </summary>
        public void SetSoftMuteOn()
        {
            writeBuffer.Span[(byte)Command.FOURTH_DATA] |= 8;
            TransmitData();
        }

        public void SetSoftMuteOff()
        {
            writeBuffer.Span[(byte)Command.FOURTH_DATA] &= 247;
            TransmitData();
        }

        /// <summary>
        /// Mute the right channel
        /// </summary>
        public void MuteRight()
        {
            writeBuffer.Span[(byte)Command.THIRD_DATA] |= 4;
            TransmitData();
        }

        /// <summary>
        /// Unmute the right channel
        /// </summary>
        public void UnuteRight()
        {
            writeBuffer.Span[(byte)Command.THIRD_DATA] &= 251;
            TransmitData();
        }

        /// <summary>
        /// Mute the left channel
        /// </summary>
        public void MuteLeft()
        {
            writeBuffer.Span[(byte)Command.THIRD_DATA] |= 2;
            TransmitData();
        }

        /// <summary>
        /// Unmute the left channel
        /// </summary>
        public void UnmuteLeft()
        {
            writeBuffer.Span[(byte)Command.THIRD_DATA] &= 253;
            TransmitData();
        }

        /// <summary>
        /// Enable standby mode
        /// </summary>
        /// <param name="enable"></param>
        public void EnableStandby(bool enable)
        {
            if (enable)
            {
                writeBuffer.Span[(byte)Command.FOURTH_DATA] |= 64;
            }
            else
            {
                writeBuffer.Span[(byte)Command.FOURTH_DATA] &= 191;
            }
            TransmitData();
        }

        /// <summary>
        /// Enable high cut control
        /// </summary>
        public void SetHighCutControlOn()
        {
            writeBuffer.Span[(byte)Command.FOURTH_DATA] |= 4;
            TransmitData();
        }

        /// <summary>
        /// Disable high cut control
        /// </summary>
        public void SetHighCutControlOff()
        {
            writeBuffer.Span[(byte)Command.FOURTH_DATA] &= 251;
            TransmitData();
        }

        /// <summary>
        /// Enable stereo noise cancelling
        /// </summary>
        public void SetStereoNoiseCancellingOn()
        {
            writeBuffer.Span[(byte)Command.FOURTH_DATA] |= 2;
            TransmitData();
        }

        /// <summary>
        /// Disable stereo noise cancelling
        /// </summary>
        public void SetStereoNoiseCancellingOff()
        {
            writeBuffer.Span[(byte)Command.FOURTH_DATA] &= 253;
            TransmitData();
        }
    }
}