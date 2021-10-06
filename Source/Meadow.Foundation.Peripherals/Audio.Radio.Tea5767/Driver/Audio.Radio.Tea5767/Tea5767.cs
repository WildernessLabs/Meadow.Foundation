
using System;
using System.Threading;
using Meadow.Hardware;

namespace Meadow.Foundation.Audio.Radio
{
    public partial class Tea5767
    {
        /// <summary>
        ///     TEA5767 radio.
        /// </summary>
        private readonly II2cPeripheral i2cPeripheral;

        byte hiInjection;
        Memory<byte> writeBuffer = new byte[5];
        Memory<byte> readBuffer = new byte[5];

        public bool IsMuted { get; set; }

        /// <summary>
        ///     Create a new TEA5767 object using the default parameters
        /// </summary>
        /// <param name="address">Address of the bus on the I2C display.</param>
        public Tea5767(II2cBus i2cBus, byte address = (byte)Addresses.Default)
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

        public void SetFrequency(float frequency)
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
            i2cPeripheral.Read(readBuffer.Span);
            Thread.Sleep(100);
        }

        public double GetFrequency()
        {
            LoadFrequency();

            uint frequencyW = (uint)(((readBuffer.Span[(byte)Command.FIRST_DATA] & 0x3F) * 256) + readBuffer.Span[(byte)Command.SECOND_DATA]);

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

        void setSearchLowStopLevel()
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

        void SetHighSideLOInjection()
        {
            writeBuffer.Span[(byte)Command.THIRD_DATA] |= 16;
        }

        void SetLowSideLOInjection()
        {
            writeBuffer.Span[(byte)Command.THIRD_DATA] &= 239;
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
            writeBuffer.Span[(byte)Command.FIRST_DATA] |= 64;
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
            writeBuffer.Span[(byte)Command.FIRST_DATA] &= 191;
            TransmitData();

            return bandLimitReached;
        }

        byte SearchMutingFromBeginning()
        {
            byte bandLimitReached;

            Mute();
            bandLimitReached = StartSearchFromBeginning();
            SetSoundBackOn();

            return bandLimitReached;
        }

        byte SearchMutingFromEnd()
        {
            byte bandLimitReached;

            Mute();
            bandLimitReached = StartSearchFromEnd();
            SetSoundBackOn();

            return bandLimitReached;
        }

        byte StartSearchFromBeginning()
        {
            SetSearchUp();
            return StartSearchFrom(87.0f);
        }

        byte StartSearchFromEnd()
        {
            SetSearchDown();
            return StartSearchFrom(108.0f);
        }

        byte StartSearchFrom(float frequency)
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
            return (byte)(readBuffer.Span[(byte)Command.FOURTH_DATA] >> 4);
        }

        public bool IsStereo()
        {
            ReadStatus();
            return (readBuffer.Span[(byte)Command.THIRD_DATA] >> 7) > 0;
        }

        public bool IsReady()
        {
            ReadStatus();
            return (readBuffer.Span[(byte)Command.FIRST_DATA] >> 7) > 0;
        }

        public byte IsBandLimitReached()
        {
            ReadStatus();
            return (byte)((readBuffer.Span[(byte)Command.FIRST_DATA] >> 6) & 1);
        }

        bool IsSearchUp()
        {
            return ((writeBuffer.Span[(byte)Command.THIRD_DATA] & 128) != 0);
        }

        bool IsSearchDown()
        {
            return ((writeBuffer.Span[(byte)Command.THIRD_DATA] & 128) == 0);
        }

        public bool IsStandby()
        {
            ReadStatus();
            return (writeBuffer.Span[(byte)Command.FOURTH_DATA] & 64) != 0;
        }

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

        public void MuteRight()
        {
            writeBuffer.Span[(byte)Command.THIRD_DATA] |= 4;
            TransmitData();
        }

        public void UnuteRight()
        {
            writeBuffer.Span[(byte)Command.THIRD_DATA] &= 251;
            TransmitData();
        }

        public void MuteLeft()
        {
            writeBuffer.Span[(byte)Command.THIRD_DATA] |= 2;
            TransmitData();
        }

        public void UnmuteLeft()
        {
            writeBuffer.Span[(byte)Command.THIRD_DATA] &= 253;
            TransmitData();
        }

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

        public void SetHighCutControlOn()
        {
            writeBuffer.Span[(byte)Command.FOURTH_DATA] |= 4;
            TransmitData();
        }

        public void SetHighCutControlOff()
        {
            writeBuffer.Span[(byte)Command.FOURTH_DATA] &= 251;
            TransmitData();
        }

        public void SetStereoNoiseCancellingOn()
        {
            writeBuffer.Span[(byte)Command.FOURTH_DATA] |= 2;
            TransmitData();
        }

        public void SetStereoNoiseCancellingOff()
        {
            writeBuffer.Span[(byte)Command.FOURTH_DATA] &= 253;
            TransmitData();
        }
    }
}