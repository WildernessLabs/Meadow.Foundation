using Meadow.Hardware;

namespace Meadow.Foundation.Displays.Led
{
    /// <summary>
    /// Fourteen Segment Display
    /// </summary>
    public partial class FourteenSegment
    {
        private readonly IDigitalOutputPort portA;
        private readonly IDigitalOutputPort portB;
        private readonly IDigitalOutputPort portC;
        private readonly IDigitalOutputPort portD;
        private readonly IDigitalOutputPort portE;
        private readonly IDigitalOutputPort portF;
        private readonly IDigitalOutputPort portG;
        private readonly IDigitalOutputPort portG2;
        private readonly IDigitalOutputPort portH;
        private readonly IDigitalOutputPort portJ;
        private readonly IDigitalOutputPort portK;
        private readonly IDigitalOutputPort portL;
        private readonly IDigitalOutputPort portM;
        private readonly IDigitalOutputPort portN;
        private readonly IDigitalOutputPort portDecimal;

        private readonly bool isCommonCathode;

        /// <summary>
        /// Creates a FourteenSegment connected to the especified IPins to a IODevice
        /// </summary>
        /// <param name="pinA">Pin A</param>
        /// <param name="pinB">Pin B</param>
        /// <param name="pinC">Pin C</param>
        /// <param name="pinD">Pin D</param>
        /// <param name="pinE">Pin E</param>
        /// <param name="pinF">Pin F</param>
        /// <param name="pinG">Pin G</param>
        /// <param name="pinG2">Pin G2</param>
        /// <param name="pinH">Pin H</param>
        /// <param name="pinJ">Pin J</param>
        /// <param name="pinK">Pin K</param>
        /// <param name="pinL">Pin L</param>
        /// <param name="pinM">Pin M</param>
        /// <param name="pinN">Pin N</param>
        /// <param name="pinDecimal">Pin decimal</param>
        /// <param name="isCommonCathode">Is the display using common cathod (true) or common annode (false)</param>
        public FourteenSegment(
            IPin pinA, IPin pinB, IPin pinC, IPin pinD, IPin pinE, IPin pinF, IPin pinG,
            IPin pinG2, IPin pinH, IPin pinJ, IPin pinK, IPin pinL, IPin pinM, IPin pinN,
            IPin pinDecimal,
            bool isCommonCathode) :
            this(pinA.CreateDigitalOutputPort(),
                 pinB.CreateDigitalOutputPort(),
                 pinC.CreateDigitalOutputPort(),
                 pinD.CreateDigitalOutputPort(),
                 pinE.CreateDigitalOutputPort(),
                 pinF.CreateDigitalOutputPort(),
                 pinG.CreateDigitalOutputPort(),
                 pinG2.CreateDigitalOutputPort(),
                 pinH.CreateDigitalOutputPort(),
                 pinJ.CreateDigitalOutputPort(),
                 pinK.CreateDigitalOutputPort(),
                 pinL.CreateDigitalOutputPort(),
                 pinM.CreateDigitalOutputPort(),
                 pinN.CreateDigitalOutputPort(),
                 pinDecimal.CreateDigitalOutputPort(),
                 isCommonCathode)
        { }

        /// <summary>
        /// Creates a FourteenSegment connected to the especified IDigitalOutputPorts
        /// </summary>
        /// <param name="portA">Digital input port for pin A</param>
        /// <param name="portB">Digital input port for pin B</param>
        /// <param name="portC">Digital input port for pin C</param>
        /// <param name="portD">Digital input port for pin D</param>
        /// <param name="portE">Digital input port for pin E</param>
        /// <param name="portF">Digital input port for pin F</param>
        /// <param name="portG">Digital input port for pin G</param>
        /// <param name="portG2">Digital input port for pin G2</param>
        /// <param name="portH">Digital input port for pin H</param>
        /// <param name="portJ">Digital input port for pin J</param>
        /// <param name="portK">Digital input port for pin K</param>
        /// <param name="portL">Digital input port for pin L</param>
        /// <param name="portM">Digital input port for pin M</param>
        /// <param name="portN">Digital input port for pin N</param>
        /// <param name="portDecimal">Digital input port for decimal pin</param>
        /// <param name="isCommonCathode">Is the display using common cathod (true) or common annode (false)</param>
        public FourteenSegment(
            IDigitalOutputPort portA, IDigitalOutputPort portB, IDigitalOutputPort portC, IDigitalOutputPort portD,
            IDigitalOutputPort portE, IDigitalOutputPort portF, IDigitalOutputPort portG, IDigitalOutputPort portG2,
            IDigitalOutputPort portH, IDigitalOutputPort portJ, IDigitalOutputPort portK, IDigitalOutputPort portL,
            IDigitalOutputPort portM, IDigitalOutputPort portN,
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
            this.portG2 = portG2;
            this.portH = portH;
            this.portJ = portJ;
            this.portK = portK;
            this.portL = portL;
            this.portM = portM;
            this.portN = portN;
            this.portDecimal = portDecimal;

            this.isCommonCathode = isCommonCathode;
        }

        /// <summary>
        /// Displays the specified ascii character (from 32 to 126)
        /// Unsupported ascii values will be blank
        /// </summary>
        public void SetDisplay(char asciiCharacter, bool? showDecimal = null)
        {
            if (asciiCharacter < 32 || asciiCharacter > 126)
            {
                asciiCharacter = ' ';
            }

            var data = fourteenSegmentASCII[asciiCharacter - 32];

            portA.State = (data & 1 << 0) != 0;
            portB.State = (data & 1 << 1) != 0;
            portC.State = (data & 1 << 2) != 0;
            portD.State = (data & 1 << 3) != 0;
            portE.State = (data & 1 << 4) != 0;
            portF.State = (data & 1 << 5) != 0;
            portG.State = (data & 1 << 6) != 0;
            portG2.State = (data & 1 << 7) != 0;
            portH.State = (data & 1 << 8) != 0;
            portJ.State = (data & 1 << 9) != 0;
            portK.State = (data & 1 << 10) != 0;
            portL.State = (data & 1 << 11) != 0;
            portM.State = (data & 1 << 12) != 0;
            portN.State = (data & 1 << 13) != 0;
            portDecimal.State = (data & 1 << 14) != 0;

            if (showDecimal != null)
            {
                portDecimal.State = isCommonCathode ? showDecimal.Value : !showDecimal.Value;
            }
        }

        /// <summary>
        /// Is a specific led segment enabled for an ascii character
        /// </summary>
        /// <param name="segment">The led segment</param>
        /// <param name="asciiCharacter">The ascii character</param>
        /// <returns></returns>
        public static bool IsEnabled(Segment segment, char asciiCharacter)
        {
            if (asciiCharacter < 32 || asciiCharacter > 126)
            {
                return false;
            }

            var data = fourteenSegmentASCII[asciiCharacter - 32];

            return (data & 1 << (int)segment) != 0;
        }
    }
}