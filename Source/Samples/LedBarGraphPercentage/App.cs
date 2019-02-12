using Meadow;
using Meadow.Devices;
using Meadow.Foundation.LEDs;
using Meadow.Hardware;
using System.Threading;

namespace LedBarGraphPercentage
{
    public class App : AppBase<F7Micro, App>
    {
        DigitalOutputPort _blueLED;
        LedBarGraph _segmentedLedBar;

        public App()
        {
            _blueLED = new DigitalOutputPort(Device.Pins.OnboardLEDBlue, true);

            var pins = new IDigitalPin[10];
            pins[0] = Device.Pins.D06;
            pins[1] = Device.Pins.D07;
            pins[2] = Device.Pins.D08;
            pins[3] = Device.Pins.D09;
            pins[4] = Device.Pins.D10;
            pins[5] = Device.Pins.D11;
            pins[6] = Device.Pins.D12;
            pins[7] = Device.Pins.D13;
            pins[8] = Device.Pins.D14;
            pins[9] = Device.Pins.D15;

            _segmentedLedBar = new LedBarGraph(pins);

            Run();
        }

        void Run()
        {
            while (true)
            {
                float percentage = 0;

                while (percentage < 1)
                {
                    _segmentedLedBar.Percentage = percentage;
                    percentage += 0.1f;
                    Thread.Sleep(200);                    
                }

                percentage = 1.0f;

                while (percentage > 0)
                {
                    _segmentedLedBar.Percentage = percentage;
                    percentage -= 0.1f;
                    Thread.Sleep(200);                    
                }
            }
        }
    }
}
