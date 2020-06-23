using System;
using System.Threading;
using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Displays.Led;

namespace Displays.Led.SevenSegment_Sample
{
    public class MeadowApp : App<F7Micro, MeadowApp>
    {
        FourDigitSevenSegment sevenSegment;

        public MeadowApp()
        {
            Console.Write("Initializing...");

            sevenSegment = new FourDigitSevenSegment
            (
                portDigit1: Device.CreateDigitalOutputPort(Device.Pins.D01),
                portDigit2: Device.CreateDigitalOutputPort(Device.Pins.D02),
                portDigit3: Device.CreateDigitalOutputPort(Device.Pins.D03),
                portDigit4: Device.CreateDigitalOutputPort(Device.Pins.D04),
                portA: Device.CreateDigitalOutputPort(Device.Pins.D09),
                portB: Device.CreateDigitalOutputPort(Device.Pins.D10),
                portC: Device.CreateDigitalOutputPort(Device.Pins.D11),
                portD: Device.CreateDigitalOutputPort(Device.Pins.D12),
                portE: Device.CreateDigitalOutputPort(Device.Pins.D13),
                portF: Device.CreateDigitalOutputPort(Device.Pins.D14),
                portG: Device.CreateDigitalOutputPort(Device.Pins.D15),
                portDecimal: Device.CreateDigitalOutputPort(Device.Pins.D08),
                isCommonCathode: true
            );

            Test();
        }

        protected void Test() 
        {
            Console.WriteLine("Test...");

            int number = 0;

            while (true)
            {
                string stringNumber = number.ToString("D4");                
                sevenSegment.SetDisplay(stringNumber.ToCharArray());
                Thread.Sleep(1000);
                number++;
            }
        }
    }
}