using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Relays;
using Meadow.Hardware;
using System;
using System.Threading;

namespace RelayToggle
{
    public class App : AppBase<F7Micro, App>
    {
        DigitalOutputPort _redLED;
        DigitalOutputPort _blueLED;
        DigitalOutputPort _greenLED;
        Relay _relay;

        public App()
        {
            InitializePeripherals();

            ShowLights();
        }

        public void InitializePeripherals()
        {
            _redLED = new DigitalOutputPort(Device.Pins.OnboardLEDRed, false);
            _blueLED = new DigitalOutputPort(Device.Pins.OnboardLEDBlue, false);
            _greenLED = new DigitalOutputPort(Device.Pins.OnboardLEDGreen, false);
            _relay = new Relay(Device.Pins.D00);
        }

        public void ShowLights()
        {
            var state = false;

            while (true)
            {
                state = !state;

                Console.WriteLine($"- State: {state}");

                _redLED.State = state;
                Thread.Sleep(200);
                _greenLED.State = state;
                Thread.Sleep(200);
                _blueLED.State = state;
                Thread.Sleep(200);

                _relay.IsOn = state;
                Thread.Sleep(200);
            }
        }
    }
}
