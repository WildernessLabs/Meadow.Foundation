using Meadow.Hardware;
using System;

namespace Meadow.Foundation.Displays.Led
{
    /// <summary>
    /// Sixteen Segment Display
    /// </summary>
    public partial class SixteenSegment : IDisposable
    {
        /// <summary>
        /// Is the object disposed
        /// </summary>
        public bool IsDisposed { get; private set; }

        /// <summary>
        /// Did we create the IO ports or where they passed in
        /// </summary>
        readonly bool createdPorts = false;

        private readonly IDigitalOutputPort portA;
        private readonly IDigitalOutputPort portB;
        private readonly IDigitalOutputPort portC;
        private readonly IDigitalOutputPort portD;
        private readonly IDigitalOutputPort portE;
        private readonly IDigitalOutputPort portF;
        private readonly IDigitalOutputPort portG;
        private readonly IDigitalOutputPort portH;
        private readonly IDigitalOutputPort portK;
        private readonly IDigitalOutputPort portM;
        private readonly IDigitalOutputPort portN;
        private readonly IDigitalOutputPort portP;
        private readonly IDigitalOutputPort portR;
        private readonly IDigitalOutputPort portS;
        private readonly IDigitalOutputPort portT;
        private readonly IDigitalOutputPort portU;
        private readonly IDigitalOutputPort portDecimal;

        private readonly bool isCommonCathode;

        /// <summary>
        /// Creates a SixteenSegment connected to the specified IPins to a IODevice
        /// </summary>
        /// <param name="pinA">Pin A</param>
        /// <param name="pinB">Pin B</param>
        /// <param name="pinC">Pin C</param>
        /// <param name="pinD">Pin D</param>
        /// <param name="pinE">Pin E</param>
        /// <param name="pinF">Pin F</param>
        /// <param name="pinG">Pin G</param>
        /// <param name="pinH">Pin H</param>
        /// <param name="pinK">Pin K</param>
        /// <param name="pinM">Pin M</param>
        /// <param name="pinN">Pin N</param>
        /// <param name="pinP">Pin P</param>
        /// <param name="pinR">Pin R</param>
        /// <param name="pinS">Pin S</param>
        /// <param name="pinT">Pin T</param>
        /// <param name="pinU">Pin U</param>
        /// <param name="pinDecimal">Pin decimal</param>
        /// <param name="isCommonCathode">Is the display using common cathode (true) or common anode (false)</param>
        public SixteenSegment(
            IPin pinA, IPin pinB, IPin pinC, IPin pinD,
            IPin pinE, IPin pinF, IPin pinG, IPin pinH,
            IPin pinK, IPin pinM, IPin pinN, IPin pinP,
            IPin pinR, IPin pinS, IPin pinT, IPin pinU,
            IPin pinDecimal,
            bool isCommonCathode) :
            this(pinA.CreateDigitalOutputPort(),
                 pinB.CreateDigitalOutputPort(),
                 pinC.CreateDigitalOutputPort(),
                 pinD.CreateDigitalOutputPort(),
                 pinE.CreateDigitalOutputPort(),
                 pinF.CreateDigitalOutputPort(),
                 pinG.CreateDigitalOutputPort(),
                 pinH.CreateDigitalOutputPort(),
                 pinK.CreateDigitalOutputPort(),
                 pinM.CreateDigitalOutputPort(),
                 pinN.CreateDigitalOutputPort(),
                 pinP.CreateDigitalOutputPort(),
                 pinR.CreateDigitalOutputPort(),
                 pinS.CreateDigitalOutputPort(),
                 pinT.CreateDigitalOutputPort(),
                 pinU.CreateDigitalOutputPort(),
                 pinDecimal.CreateDigitalOutputPort(),
                 isCommonCathode)
        { }

        /// <summary>
        /// Creates a SixteenSegment connected to the specified IDigitalOutputPorts
        /// </summary>
        /// <param name="portA">Digital input port for pin A</param>
        /// <param name="portB">Digital input port for pin B</param>
        /// <param name="portC">Digital input port for pin C</param>
        /// <param name="portD">Digital input port for pin D</param>
        /// <param name="portE">Digital input port for pin E</param>
        /// <param name="portF">Digital input port for pin F</param>
        /// <param name="portG">Digital input port for pin G</param>
        /// <param name="portH">Digital input port for pin H</param>
        /// <param name="portK">Digital input port for pin K</param>
        /// <param name="portM">Digital input port for pin M</param>
        /// <param name="portN">Digital input port for pin N</param>
        /// <param name="portP">Digital input port for pin P</param>
        /// <param name="portR">Digital input port for pin R</param>
        /// <param name="portS">Digital input port for pin S</param>
        /// <param name="portT">Digital input port for pin T</param>
        /// <param name="portU">Digital input port for pin U</param>
        /// <param name="portDecimal">Digital input port for decimal pin</param>
        /// <param name="isCommonCathode">Is the display using common cathode (true) or common anode (false)</param>
        public SixteenSegment(
            IDigitalOutputPort portA, IDigitalOutputPort portB, IDigitalOutputPort portC, IDigitalOutputPort portD,
            IDigitalOutputPort portE, IDigitalOutputPort portF, IDigitalOutputPort portG, IDigitalOutputPort portH,
            IDigitalOutputPort portK, IDigitalOutputPort portM, IDigitalOutputPort portN, IDigitalOutputPort portP,
            IDigitalOutputPort portR, IDigitalOutputPort portS, IDigitalOutputPort portT, IDigitalOutputPort portU,
            IDigitalOutputPort portDecimal,
            bool isCommonCathode)
        {
            this.portA = portA;
            this.portB = portB;
            this.portC = portC;
            this.portD = portD;
            this.portE = portE;
            this.portF = portF;
            this.portG = portG;
            this.portH = portH;
            this.portK = portK;
            this.portM = portM;
            this.portN = portN;
            this.portP = portP;
            this.portR = portR;
            this.portS = portS;
            this.portT = portT;
            this.portU = portU;
            this.portDecimal = portDecimal;

            this.isCommonCathode = isCommonCathode;
        }

        /// <summary>
        /// Displays the specified ASCII character (from 32 to 126)
        /// Unsupported ASCII values will be blank
        /// </summary>
        public void SetDisplay(char asciiCharacter, bool? showDecimal = null)
        {
            if (asciiCharacter < 32 || asciiCharacter > 126)
            {
                asciiCharacter = ' ';
            }

            var data = sixteenSegmentASCII[asciiCharacter - 32];

            portA.State = (data & 1 << 0) != 0;
            portB.State = (data & 1 << 1) != 0;
            portC.State = (data & 1 << 2) != 0;
            portD.State = (data & 1 << 3) != 0;
            portE.State = (data & 1 << 4) != 0;
            portF.State = (data & 1 << 5) != 0;
            portG.State = (data & 1 << 6) != 0;
            portH.State = (data & 1 << 7) != 0;
            portK.State = (data & 1 << 8) != 0;
            portM.State = (data & 1 << 9) != 0;
            portN.State = (data & 1 << 10) != 0;
            portP.State = (data & 1 << 11) != 0;
            portR.State = (data & 1 << 12) != 0;
            portS.State = (data & 1 << 13) != 0;
            portT.State = (data & 1 << 14) != 0;
            portU.State = (data & 1 << 15) != 0;
            portDecimal.State = (data & 1 << 16) != 0;

            if (showDecimal != null)
            {
                portDecimal.State = isCommonCathode ? showDecimal.Value : !showDecimal.Value;
            }
        }

        /// <summary>
        /// Is a specific led segment enabled for an ASCII character
        /// </summary>
        /// <param name="segment">The led segment</param>
        /// <param name="asciiCharacter">The ASCII character</param>
        /// <returns></returns>
        public static bool IsEnabled(Segment segment, char asciiCharacter)
        {
            if (asciiCharacter < 32 || asciiCharacter > 126)
            {
                return false;
            }

            var data = sixteenSegmentASCII[asciiCharacter - 32];

            return (data & 1 << (int)segment) != 0;
        }

        /// <summary>
        /// Dispose of the object
        /// </summary>
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
                    portH.Dispose();
                    portK.Dispose();
                    portM.Dispose();
                    portN.Dispose();
                    portP.Dispose();
                    portR.Dispose();
                    portS.Dispose();
                    portT.Dispose();
                    portU.Dispose();
                    portDecimal.Dispose();
                }

                IsDisposed = true;
            }
        }
    }
}