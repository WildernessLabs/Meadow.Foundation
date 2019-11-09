using System;
using Meadow.Hardware;

namespace Meadow.Foundation.Displays.LED
{
    /// <summary>
    /// Seven Segment Display
    /// </summary>
    public class SevenSegment
    {
        #region Enums
        /// <summary>
        /// Valid Characters to display
        /// </summary>
        public enum CharacterType
        {
            Zero,
            One,
            Two,
            Three,
            Four,
            Five,
            Six,
            Seven,
            Eight,
            Nine,
            A,
            B,
            C,
            D,
            E,
            F,
            Blank,
            count
        }

        #endregion

        #region Member variables / fields

        private readonly IDigitalOutputPort _portA;
        private readonly IDigitalOutputPort _portB;
        private readonly IDigitalOutputPort _portC;
        private readonly IDigitalOutputPort _portD;
        private readonly IDigitalOutputPort _portE;
        private readonly IDigitalOutputPort _portF;
        private readonly IDigitalOutputPort _portG;
        private readonly IDigitalOutputPort _portDecimal;

        private readonly bool _isCommonCathode;

        private byte[,] _segments =
        {
             {1, 1, 1, 1, 1, 1, 0}, //0
             {0, 1, 1, 0, 0, 0, 0}, //1
             {1, 1, 0, 1, 1, 0, 1}, //2
             {1, 1, 1, 1, 0, 0, 1}, //3
             {0, 1, 1, 0, 0, 1, 1}, //4
             {1, 0, 1, 1, 0, 1, 1}, //5
             {1, 0, 1, 1, 1, 1, 1}, //6
             {1, 1, 1, 0, 0, 0, 0}, //7
             {1, 1, 1, 1, 1, 1, 1}, //8
             {1, 1, 1, 1, 0, 1, 1}, //9
             {1, 1, 1, 0, 1, 1, 1}, //A
             {0, 0, 1, 1, 1, 1, 1}, //b
             {1, 0, 0, 1, 1, 1, 0}, //C
             {0, 1, 1, 1, 1, 0, 1}, //d
             {1, 0, 0, 1, 1, 1, 1}, //E
             {1, 0, 0, 0, 1, 1, 1}, //F
             {0, 0, 0, 0, 0, 0, 0}, //blank
        };

        #endregion

        #region Constructor

        /// <summary>
        /// Default constructor is private to prevent it being called.
        /// </summary>
        private SevenSegment() { }

        /// <summary>
        /// Creates a SevenSegment connected to the especified IPins to a IODevice
        /// </summary>
        /// <param name="device"></param>
        /// <param name="pinA"></param>
        /// <param name="pinB"></param>
        /// <param name="pinC"></param>
        /// <param name="pinD"></param>
        /// <param name="pinE"></param>
        /// <param name="pinF"></param>
        /// <param name="pinG"></param>
        /// <param name="pinDecimal"></param>
        /// <param name="isCommonCathode"></param>
        public SevenSegment(
            IIODevice device, 
            IPin pinA, IPin pinB,
            IPin pinC, IPin pinD,
            IPin pinE, IPin pinF,
            IPin pinG, IPin pinDecimal,
            bool isCommonCathode) :
            this(device.CreateDigitalOutputPort(pinA),
                 device.CreateDigitalOutputPort(pinB),
                 device.CreateDigitalOutputPort(pinC),
                 device.CreateDigitalOutputPort(pinD),
                 device.CreateDigitalOutputPort(pinE),
                 device.CreateDigitalOutputPort(pinF),
                 device.CreateDigitalOutputPort(pinG),
                 device.CreateDigitalOutputPort(pinDecimal),
                 isCommonCathode) { }

        /// <summary>
        /// Creates a SevenSegment connected to the especified IDigitalOutputPorts
        /// </summary>
        /// <param name="portA"></param>
        /// <param name="portB"></param>
        /// <param name="portC"></param>
        /// <param name="portD"></param>
        /// <param name="portE"></param>
        /// <param name="portF"></param>
        /// <param name="portG"></param>
        /// <param name="portDecimal"></param>
        /// <param name="isCommonCathode"></param>
        public SevenSegment(
            IDigitalOutputPort portA, IDigitalOutputPort portB,
            IDigitalOutputPort portC, IDigitalOutputPort portD,
            IDigitalOutputPort portE, IDigitalOutputPort portF,
            IDigitalOutputPort portG, IDigitalOutputPort portDecimal, 
            bool isCommonCathode)
        {
            _portA = portA;
            _portB = portB;
            _portC = portC;
            _portD = portD;
            _portE = portE;
            _portF = portF;
            _portG = portG;
            _portDecimal = portDecimal;

            _isCommonCathode = isCommonCathode;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Displays the especified character
        /// </summary>
        /// <param name="character"></param>
        /// <param name="showDecimal"></param>
        public void SetDisplay(CharacterType character, bool showDecimal = false)
        {
            _portDecimal.State = _isCommonCathode ? showDecimal : !showDecimal;

            int index = (int)character;

            _portA.State = IsEnabled(_segments[index, 0]);
            _portB.State = IsEnabled(_segments[index, 1]);
            _portC.State = IsEnabled(_segments[index, 2]);
            _portD.State = IsEnabled(_segments[index, 3]);
            _portE.State = IsEnabled(_segments[index, 4]);
            _portF.State = IsEnabled(_segments[index, 5]);
            _portG.State = IsEnabled(_segments[index, 6]);
        }

        /// <summary>
        /// Returns 
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        bool IsEnabled (byte value)
        {
            return _isCommonCathode ? value == 1 : value == 0;
        }

        /// <summary>
        /// Displays the especified valid character
        /// </summary>
        /// <param name="character"></param>
        /// <param name="showDecimal"></param>
        public void SetDisplay(char character, bool showDecimal = false)
        {
            CharacterType charType;

            if (character == ' ')
                charType = CharacterType.Blank;
            else if (character >= '0' && character <= '9')
                charType = (CharacterType)(character - '0');
            else if (character >= 'a' && character <= 'f')
                charType = (CharacterType)(character - 'a' + 10);
            else
                throw new ArgumentOutOfRangeException();

            SetDisplay(charType, showDecimal);
        }

        #endregion
    }
}