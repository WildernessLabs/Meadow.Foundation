using System;
using System.Threading;
using System.Threading.Tasks;
using Meadow.Devices;
using Meadow.Hardware;

namespace Meadow.Foundation.Displays.Led
{
    /// <summary>
    /// Four Digit Seven Segment Display
    /// </summary>
    public class FourDigitSevenSegment
    {
        protected Task animationThread = null;
        protected CancellationTokenSource cts = null;

        protected readonly IDigitalOutputPort[] digits;

        protected SevenSegment[] sevenSegments;

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
            IMeadowDevice device, 
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
            digits = new IDigitalOutputPort[4];
            digits[0] = portDigit1;
            digits[1] = portDigit2;
            digits[2] = portDigit3;
            digits[3] = portDigit4;

            sevenSegments = new SevenSegment[4];
            for(int i=0; i < 4; i++) 
            {
                sevenSegments[i] = new SevenSegment(portA, portB, portC, portD, portE, portF, portG, portDecimal, isCommonCathode);
            }

            cts = new CancellationTokenSource();
        }

        

        

        /// <summary>
        /// Displays the especified valid character
        /// </summary>
        /// <param name="character"></param>
        /// <param name="showDecimal"></param>
        public void SetDisplay(char[] character, int decimalLocation = -1)
        {
            if (!cts.Token.IsCancellationRequested)
            {
                cts.Cancel();
            }

            cts = new CancellationTokenSource();

            Task.Run(async ()=> await StartDisplayLoop(character, decimalLocation, cts.Token));
        }

        protected async Task StartDisplayLoop(char[] character, int decimalLocation, CancellationToken cancellationToken) 
        {
            while (true)
            {
                if (cancellationToken.IsCancellationRequested)
                { 
                    break;
                }

                for (int i = 0; i < 4; i++)
                {
                    sevenSegments[i].SetDisplay(character[i], decimalLocation == i);

                    digits[i].State = false;
                    digits[i].State = true;
                }

                await Task.Delay(7);
            }
        }

        public void Stop()
        {
            cts.Cancel();
        }

        
    }
}