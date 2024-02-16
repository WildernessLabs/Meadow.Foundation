﻿using Meadow.Hardware;
using System;

namespace Meadow.Foundation.Displays.Led
{
    /// <summary>
    /// Seven Segment Display
    /// </summary>
    public class SevenSegment : IDisposable
    {
        /// <summary>
        /// Is the object disposed
        /// </summary>
        public bool IsDisposed { get; private set; }

        /// <summary>
        /// Did we create the port(s) used by the peripheral
        /// </summary>
        readonly bool createdPorts = false;

        /// <summary>
        /// Valid Characters to display
        /// </summary>
        public enum CharacterType
        {
            /// <summary>
            /// Zero (0) character
            /// </summary>
            Zero,
            /// <summary>
            /// One (1) character
            /// </summary>
            One,
            /// <summary>
            /// Two (2) character
            /// </summary>
            Two,
            /// <summary>
            /// Three (3) character
            /// </summary>
            Three,
            /// <summary>
            /// Four (4) character
            /// </summary>
            Four,
            /// <summary>
            /// Five (5) character
            /// </summary>
            Five,
            /// <summary>
            /// Six (6) character
            /// </summary>
            Six,
            /// <summary>
            /// Seven (7) character
            /// </summary>
            Seven,
            /// <summary>
            /// Eight (8) character
            /// </summary>
            Eight,
            /// <summary>
            /// Nine (9) character
            /// </summary>
            Nine,
            /// <summary>
            /// A character
            /// </summary>
            A,
            /// <summary>
            /// B character
            /// </summary>
            B,
            /// <summary>
            /// C character
            /// </summary>
            C,
            /// <summary>
            /// D character
            /// </summary>
            D,
            /// <summary>
            /// E character
            /// </summary>
            E,
            /// <summary>
            /// F character
            /// </summary>
            F,
            /// <summary>
            /// Blank character
            /// </summary>
            Blank,
            /// <summary>
            /// The count of values in CharacterType
            /// </summary>
            count
        }

        private readonly IDigitalOutputPort portA;
        private readonly IDigitalOutputPort portB;
        private readonly IDigitalOutputPort portC;
        private readonly IDigitalOutputPort portD;
        private readonly IDigitalOutputPort portE;
        private readonly IDigitalOutputPort portF;
        private readonly IDigitalOutputPort portG;
        private readonly IDigitalOutputPort portDecimal;

        private readonly bool isCommonCathode;

        private readonly byte[,] segments =
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

        /// <summary>
        /// Creates a SevenSegment connected to the specified IPins to a IODevice
        /// </summary>
        /// <param name="pinA">Pin A</param>
        /// <param name="pinB">Pin B</param>
        /// <param name="pinC">Pin C</param>
        /// <param name="pinD">Pin D</param>
        /// <param name="pinE">Pin E</param>
        /// <param name="pinF">Pin F</param>
        /// <param name="pinG">Pin G</param>
        /// <param name="pinDecimal">Pin decimal</param>
        /// <param name="isCommonCathode">Is the display using common cathode (true) or common anode (false)</param>
        public SevenSegment(
            IPin pinA, IPin pinB,
            IPin pinC, IPin pinD,
            IPin pinE, IPin pinF,
            IPin pinG, IPin pinDecimal,
            bool isCommonCathode) :
            this(pinA.CreateDigitalOutputPort(),
                 pinB.CreateDigitalOutputPort(),
                 pinC.CreateDigitalOutputPort(),
                 pinD.CreateDigitalOutputPort(),
                 pinE.CreateDigitalOutputPort(),
                 pinF.CreateDigitalOutputPort(),
                 pinG.CreateDigitalOutputPort(),
                 pinDecimal.CreateDigitalOutputPort(),
                 isCommonCathode)
        {
            createdPorts = true;
        }

        /// <summary>
        /// Creates a SevenSegment connected to the specified IDigitalOutputPorts
        /// </summary>
        /// <param name="portA">Digital input port for pin A</param>
        /// <param name="portB">Digital input port for pin B</param>
        /// <param name="portC">Digital input port for pin C</param>
        /// <param name="portD">Digital input port for pin D</param>
        /// <param name="portE">Digital input port for pin E</param>
        /// <param name="portF">Digital input port for pin F</param>
        /// <param name="portG">Digital input port for pin G</param>
        /// <param name="portDecimal">Digital input port for decimal pin</param>
        /// <param name="isCommonCathode">Is the display using common cathode (true) or common anode (false)</param>
        public SevenSegment(
            IDigitalOutputPort portA, IDigitalOutputPort portB,
            IDigitalOutputPort portC, IDigitalOutputPort portD,
            IDigitalOutputPort portE, IDigitalOutputPort portF,
            IDigitalOutputPort portG, IDigitalOutputPort portDecimal,
            bool isCommonCathode)
        {
            this.portA = portA;
            this.portB = portB;
            this.portC = portC;
            this.portD = portD;
            this.portE = portE;
            this.portF = portF;
            this.portG = portG;
            this.portDecimal = portDecimal;

            this.isCommonCathode = isCommonCathode;
        }

        /// <summary>
        /// Displays the specified character
        /// </summary>
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

        bool IsEnabled(byte value)
        {
            return isCommonCathode ? value == 1 : value == 0;
        }

        /// <summary>
        /// Displays the specified valid character
        /// </summary>
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

        ///<inheritdoc/>
        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Dispose of the object
        /// </summary>
        /// <param name="disposing">Is disposing</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!IsDisposed)
            {
                if (disposing && createdPorts)
                {
                    portA.Dispose();
                    portB.Dispose();
                    portC.Dispose();
                    portD.Dispose();
                    portE.Dispose();
                    portF.Dispose();
                    portG.Dispose();
                    portDecimal.Dispose();
                }

                IsDisposed = true;
            }
        }
    }
}