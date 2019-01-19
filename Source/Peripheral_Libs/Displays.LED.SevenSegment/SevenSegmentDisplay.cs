using System;
using Meadow.Hardware;

namespace Meadow.Foundation
{
    public class SevenSegmentDisplay
    {
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

        private readonly DigitalOutputPort portA;
        private readonly DigitalOutputPort portB;
        private readonly DigitalOutputPort portC;
        private readonly DigitalOutputPort portD;
        private readonly DigitalOutputPort portE;
        private readonly DigitalOutputPort portF;
        private readonly DigitalOutputPort portG;
        private readonly DigitalOutputPort portDecimal;

        private readonly bool isCommonCathode;

        private byte[,] segments =
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


        public SevenSegmentDisplay(IDigitalPin pinA, IDigitalPin pinB,
                                   IDigitalPin pinC, IDigitalPin pinD,
                                   IDigitalPin pinE, IDigitalPin pinF,
                                   IDigitalPin pinG, IDigitalPin pinDecimal, 
                                   bool isCommonCathode)
        {
            portA = new DigitalOutputPort(pinA, false);
            portB = new DigitalOutputPort(pinB, false);
            portC = new DigitalOutputPort(pinC, false);
            portD = new DigitalOutputPort(pinD, false);
            portE = new DigitalOutputPort(pinE, false);
            portF = new DigitalOutputPort(pinF, false);
            portG = new DigitalOutputPort(pinG, false);
            portDecimal = new DigitalOutputPort(pinDecimal, false);

            this.isCommonCathode = isCommonCathode;
        }

        public void SetDisplay(CharacterType character, bool showDecimal = false)
        {
            portDecimal.State = isCommonCathode ? showDecimal : !showDecimal;

            int index = (int)character;

            portA.State = IsEnabled(segments[index, 0]);
            portB.State = IsEnabled(segments[index, 1]);
            portC.State = IsEnabled(segments[index, 2]);
            portD.State = IsEnabled(segments[index, 3]);
            portE.State = IsEnabled(segments[index, 4]);
            portF.State = IsEnabled(segments[index, 5]);
            portG.State = IsEnabled(segments[index, 6]);
        }

        bool IsEnabled (byte value)
        {
            return isCommonCathode ? value == 1 : value == 0;
        }

        public void SetDisplay(Char character, bool showDecimal = false)
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
    }
}