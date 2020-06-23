using System.Threading;
using System.Threading.Tasks;
using Meadow.Hardware;

namespace Meadow.Foundation.Displays.Led
{
    /// <summary>
    /// Seven Segment Display
    /// </summary>
    public class FourDigitSevenSegment
    {
        protected Task animationThread = null;
        protected CancellationTokenSource cancellationTokenSource = null;

        #region Member variables / fields

        private readonly IDigitalOutputPort _digit1;
        private readonly IDigitalOutputPort _digit2;
        private readonly IDigitalOutputPort _digit3;
        private readonly IDigitalOutputPort _digit4;

        SevenSegment[] sevenSegment;

        #endregion

        #region Constructor

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
        public FourDigitSevenSegment(
            IIODevice device, 
            IPin pinDigit1, IPin pinDigit2,
            IPin pinDigit3, IPin pinDigit4,
            IPin pinA, IPin pinB,
            IPin pinC, IPin pinD,
            IPin pinE, IPin pinF,
            IPin pinG, IPin pinDecimal,
            bool isCommonCathode) :
            this (
                device.CreateDigitalOutputPort(pinDigit1),
                device.CreateDigitalOutputPort(pinDigit2),
                device.CreateDigitalOutputPort(pinDigit3),
                device.CreateDigitalOutputPort(pinDigit4),
                device.CreateDigitalOutputPort(pinA),
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
        public FourDigitSevenSegment(
            IDigitalOutputPort portDigit1, IDigitalOutputPort portDigit2,
            IDigitalOutputPort portDigit3, IDigitalOutputPort portDigit4,
            IDigitalOutputPort portA, IDigitalOutputPort portB,
            IDigitalOutputPort portC, IDigitalOutputPort portD,
            IDigitalOutputPort portE, IDigitalOutputPort portF,
            IDigitalOutputPort portG, IDigitalOutputPort portDecimal, 
            bool isCommonCathode)
        {
            _digit1 = portDigit1;
            _digit2 = portDigit2;
            _digit3 = portDigit3;
            _digit4 = portDigit4;

            sevenSegment = new SevenSegment[4];
            for(int i=0; i < 4; i++) 
            {
                sevenSegment[i] = new SevenSegment(portA, portB, portC, portD, portE, portF, portG, portDecimal, isCommonCathode);
            }

            cancellationTokenSource = new CancellationTokenSource();
        }

        #endregion

        #region Methods

        /// <summary>
        /// Displays the especified valid character
        /// </summary>
        /// <param name="character"></param>
        /// <param name="showDecimal"></param>
        public void SetDisplay(char[] character, bool showDecimal = false)
        {
            if (!cancellationTokenSource.Token.IsCancellationRequested)
                cancellationTokenSource.Cancel();

            cancellationTokenSource = new CancellationTokenSource();

            animationThread = new Task(async () =>
            {
                await Display(character, showDecimal, cancellationTokenSource.Token);
            }, cancellationTokenSource.Token);
            animationThread.Start();
        }

        protected async Task Display(char[] character, bool showDecimal, CancellationToken cancellationToken) 
        {
            while (true)
            {
                if (cancellationToken.IsCancellationRequested)
                    break;

                for (int i = 0; i < 4; i++)
                {
                    sevenSegment[i].SetDisplay(character[i], showDecimal);

                    switch (i)
                    {
                        case 0:
                            _digit4.State = false;
                            _digit4.State = true;
                            break;
                        case 1:
                            _digit3.State = false;
                            _digit3.State = true;
                            break;
                        case 2:
                            _digit2.State = false;
                            _digit2.State = true;
                            break;
                        case 3:
                            _digit1.State = false;
                            _digit1.State = true;
                            break;
                    }
                }

                await Task.Delay(7);
            }
        }

        public void Stop()
        {
            cancellationTokenSource.Cancel();
        }

        #endregion
    }
}