using Meadow.Hardware;
using System.Threading;
using System.Threading.Tasks;

namespace Meadow.Foundation.Displays.Led
{
    /// <summary>
    /// Four Digit Seven Segment Display
    /// </summary>
    public class FourDigitSevenSegment
    {
        CancellationTokenSource cts = null;

        readonly IDigitalOutputPort[] digits;

        SevenSegment[] sevenSegments;

        /// <summary>
        /// Creates a SevenSegment connected to the especified IPins to a IODevice
        /// </summary>
        /// <param name="pinDigit1">Digit 1 pin</param>
        /// <param name="pinDigit2">Digit 2 pin</param>
        /// <param name="pinDigit3">Digit 3 pin</param>
        /// <param name="pinDigit4">Digit 4 pin</param>
        /// <param name="pinA">A pin</param>
        /// <param name="pinB">B pin</param>
        /// <param name="pinC">C pin</param>
        /// <param name="pinD">D pin</param>
        /// <param name="pinE">E pin</param>
        /// <param name="pinF">F pin</param>
        /// <param name="pinG">G pin</param>
        /// <param name="pinDecimal">Decimal pin</param>
        /// <param name="isCommonCathode">Is the display common cathode (true)</param>
        public FourDigitSevenSegment(
            IPin pinDigit1, IPin pinDigit2,
            IPin pinDigit3, IPin pinDigit4,
            IPin pinA, IPin pinB,
            IPin pinC, IPin pinD,
            IPin pinE, IPin pinF,
            IPin pinG, IPin pinDecimal,
            bool isCommonCathode) :
            this(
                pinDigit1.CreateDigitalOutputPort(),
                pinDigit2.CreateDigitalOutputPort(),
                pinDigit3.CreateDigitalOutputPort(),
                pinDigit4.CreateDigitalOutputPort(),
                pinA.CreateDigitalOutputPort(),
                pinB.CreateDigitalOutputPort(),
                pinC.CreateDigitalOutputPort(),
                pinD.CreateDigitalOutputPort(),
                pinE.CreateDigitalOutputPort(),
                pinF.CreateDigitalOutputPort(),
                pinG.CreateDigitalOutputPort(),
                pinDecimal.CreateDigitalOutputPort(),
                isCommonCathode)
        { }

        /// <summary>
        /// Creates a SevenSegment connected to the especified IDigitalOutputPorts
        /// </summary>
        /// <param name="portDigit1">Port for digit 1</param>
        /// <param name="portDigit2">Port for digit 2</param>
        /// <param name="portDigit3">Port for digit 3</param>
        /// <param name="portDigit4">Port for digit 4</param>
        /// <param name="portA">Port for pin A</param>
        /// <param name="portB">Port for pin B</param>
        /// <param name="portC">Port for pin C</param>
        /// <param name="portD">Port for pin D</param>
        /// <param name="portE">Port for pin E</param>
        /// <param name="portF">Port for pin F</param>
        /// <param name="portG">Port for pin G</param>
        /// <param name="portDecimal">Port for decimal pin</param>
        /// <param name="isCommonCathode">Is the display common cathode (true)</param>
        public FourDigitSevenSegment(
            IDigitalOutputPort portDigit1, IDigitalOutputPort portDigit2,
            IDigitalOutputPort portDigit3, IDigitalOutputPort portDigit4,
            IDigitalOutputPort portA, IDigitalOutputPort portB,
            IDigitalOutputPort portC, IDigitalOutputPort portD,
            IDigitalOutputPort portE, IDigitalOutputPort portF,
            IDigitalOutputPort portG, IDigitalOutputPort portDecimal,
            bool isCommonCathode)
        {
            digits = new IDigitalOutputPort[4];
            digits[0] = portDigit1;
            digits[1] = portDigit2;
            digits[2] = portDigit3;
            digits[3] = portDigit4;

            sevenSegments = new SevenSegment[4];
            for (int i = 0; i < 4; i++)
            {
                sevenSegments[i] = new SevenSegment(portA, portB, portC, portD, portE, portF, portG, portDecimal, isCommonCathode);
            }

            cts = new CancellationTokenSource();
        }

        /// <summary>
        /// Displays the specified characters
        /// </summary>
        /// <param name="characters">The chracters to display</param>
        /// <param name="decimalLocation">The decimal position (0 indexed)</param>
        public void SetDisplay(string characters, int decimalLocation = -1)
        {
            SetDisplay(characters.ToCharArray(), decimalLocation);
        }

        /// <summary>
        /// Displays the specified characters
        /// </summary>
        /// <param name="characters">The chracters to display</param>
        /// <param name="decimalLocation">The decimal position (0 indexed)</param>
        public void SetDisplay(char[] characters, int decimalLocation = -1)
        {
            if (!cts.Token.IsCancellationRequested)
            {
                cts.Cancel();
            }

            cts = new CancellationTokenSource();

            Task.Run(async () => await StartDisplayLoop(characters, decimalLocation, cts.Token));
        }

        async Task StartDisplayLoop(char[] characters, int decimalLocation, CancellationToken cancellationToken)
        {
            while (true)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    break;
                }

                for (int i = 0; i < 4; i++)
                {
                    sevenSegments[i].SetDisplay(characters[i], decimalLocation == i);

                    digits[i].State = false;
                    digits[i].State = true;
                }

                await Task.Delay(7);
            }
        }
    }
}