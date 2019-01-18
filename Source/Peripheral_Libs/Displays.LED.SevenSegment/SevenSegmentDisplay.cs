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

        public void SetDisplayOld(Char character, bool showDecimal = false)
        {
            if(!(character >= '0' && character <= '9') &&
               !(character >= 'a' && character <= 'f') &&
               !(character == ' '))
            {
                throw new ArgumentOutOfRangeException();
            }

            portDecimal.State = !showDecimal;

            switch(character)
            {
                case ' ':
                    portA.State = portB.State = portC.State = portD.State = portE.State = portF.State = portG.State = true;
                    break;
                case '0':
                    portA.State = portB.State = portC.State = portD.State = portE.State = portF.State = false;
                    portG.State = true;
                    break;
                case '1':
                    portA.State = portC.State = portD.State = portF.State = portG.State = true;
                    portB.State = portE.State = false;
                    break;
                case '2':
                    portA.State = portB.State = portD.State = portF.State = portG.State = false;
                    portE.State = portC.State = true;
                    break;
                case '3':
                    portA.State = portB.State = portC.State = portD.State = portG.State = false;
                    portE.State = portF.State = true;
                    break;
                case '4':
                    portA.State = portD.State = portE.State = true;
                    portB.State = portG.State = portC.State = portF.State = false;
                    break;
                case '5':
                    portA.State = portC.State = portD.State = portF.State = portG.State = false;
                    portE.State = portB.State = true;
                    break;
                case '6':
                    portA.State = portC.State = portD.State = portE.State = portF.State = portG.State = false;
                    portB.State = true;
                    break;
                case '7':
                    portD.State = portE.State = portF.State = portG.State = true;
                    portB.State = portC.State = portA.State = false;
                    break;
                case '8':
                    portA.State = portB.State = portC.State = portD.State = portE.State = portF.State = portG.State = false;
                    break;
                case '9':
                    portA.State = portB.State = portC.State = portD.State = portF.State = portG.State = false;
                    portE.State = true;
                    break;
                case 'a':
                    portA.State = portB.State = portC.State = portE.State = portF.State = portG.State = false;
                    portD.State = true;
                    break;
                case 'b':
                    portA.State = portB.State = portC.State = portD.State = portE.State = portF.State = portG.State = false;
                    break;
                case 'c':
                    portA.State = portD.State = portE.State = portF.State = false;
                    portB.State = portC.State = portG.State = true;
                    break;
                case 'd':
                    portA.State = portB.State = portC.State = portD.State = portE.State = portF.State = portG.State = false;
                    portG.State = true;
                    break;
                case 'e':
                    portA.State = portD.State = portE.State = portF.State = portG.State = false;
                    portB.State = portC.State = true;
                    break;
                case 'f':
                    portA.State = portE.State = portF.State = portG.State = false;
                    portB.State = portC.State = portD.State = true;
                    break;
            }
        }
    }
}