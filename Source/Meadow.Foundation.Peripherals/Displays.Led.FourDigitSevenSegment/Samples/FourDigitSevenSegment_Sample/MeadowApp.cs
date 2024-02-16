﻿using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Displays.Led;
using System.Threading;
using System.Threading.Tasks;

namespace Displays.Led.SevenSegment_Sample
{
    public class MeadowApp : App<F7FeatherV2>
    {
        //<!=SNIP=>

        FourDigitSevenSegment sevenSegment;

        public override Task Initialize()
        {
            Resolver.Log.Info("Initializing...");

            sevenSegment = new FourDigitSevenSegment
            (
                portDigit1: Device.CreateDigitalOutputPort(Device.Pins.D00),
                portDigit2: Device.CreateDigitalOutputPort(Device.Pins.D03),
                portDigit3: Device.CreateDigitalOutputPort(Device.Pins.D04),
                portDigit4: Device.CreateDigitalOutputPort(Device.Pins.D06),
                portA: Device.CreateDigitalOutputPort(Device.Pins.D01),
                portB: Device.CreateDigitalOutputPort(Device.Pins.D05),
                portC: Device.CreateDigitalOutputPort(Device.Pins.D08),
                portD: Device.CreateDigitalOutputPort(Device.Pins.D10),
                portE: Device.CreateDigitalOutputPort(Device.Pins.D11),
                portF: Device.CreateDigitalOutputPort(Device.Pins.D02),
                portG: Device.CreateDigitalOutputPort(Device.Pins.D07),
                portDecimal: Device.CreateDigitalOutputPort(Device.Pins.D09),
                isCommonCathode: true
            );

            return base.Initialize();
        }

        public override Task Run()
        {
            sevenSegment.SetDisplay("1234");

            return base.Run();
        }

        //<!=SNOP=>

        protected void Test()
        {
            Resolver.Log.Info("Test...");

            int number = 0;

            //sevenSegment.SetDisplay("1337".ToCharArray());

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