using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Leds;
using Meadow.Foundation.Sensors.Buttons;
using Meadow.Hardware;
using Meadow.Peripherals.Leds;
using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Sc16is7x2_SerialTestGen
{
    public class MeadowApp : App<F7FeatherV2>
    {
        RgbPwmLed onboardLed;

        PushButton _pushButton;
        ISerialPort? _com1Port = null;
        const int Rate = 9600;      // OK
        //const int Rate = 19200;     // OK
        //const int Rate = 38400;     // Too high for the Meadow/SC16IS752
        //const int Rate = 57600;     // Too high for the Meadow/SC16IS752

        public override Task Initialize()
        {
            Resolver.Log.Info("Initialize...");

            _com1Port = Device.PlatformOS.GetSerialPortName("COM1")?.CreateSerialPort(Rate);
            _com1Port.Open();

            // Initialize by sending Device and Pins
            _pushButton = new PushButton(Device.Pins.D00, ResistorMode.InternalPullDown);
            _pushButton.LongClicked += (s, e) =>
            {
                Resolver.Log.Info("LongClicked");
                string sendText = "Meadow calling! Wakeup!\n";
                var sendBytes = Encoding.ASCII.GetBytes(sendText);
                Resolver.Log.Info($"Writing {sendBytes.Length} bytes on port COM1: {NiceMsg(sendText)}");
                _com1Port.Write(sendBytes);
            };
            _pushButton.Clicked += (s, e) =>
            {
                Resolver.Log.Info("Clicked");
                // Send long message. (300 characters) Much larger than the FIFO buffer in SC16IS7x2.
                string sendText =
                    "Meadow calling! aaa bbbbbbbbb ccccccccc ddddddddd eeeeeeeee fffffffff ggggggggg hhhhhhhhh iiiiiiiii " +
                    "000000000 111111111 222222222 333333333 444444444 555555555 666666666 777777777 888888888 999999999 " +
                    "qqqqqqqqq rrrrrrrrr sssssssss ttttttttt uuuuuuuuu vvvvvvvvv wwwwwwwww xxxxxxxxx yyyyyyyyy zzzzzzzzz\n";
                var sendBytes = Encoding.ASCII.GetBytes(sendText);
                Resolver.Log.Info($"Writing {sendBytes.Length} bytes on port COM1: {NiceMsg(sendText)}");
                _com1Port.Write(sendBytes);
            };

            onboardLed = new RgbPwmLed(
                redPwmPin: Device.Pins.OnboardLedRed,
                greenPwmPin: Device.Pins.OnboardLedGreen,
                bluePwmPin: Device.Pins.OnboardLedBlue,
                CommonType.CommonAnode);

            return base.Initialize();
        }

        public override Task Run()
        {
            Resolver.Log.Info("Run...");

            return CycleColors(TimeSpan.FromMilliseconds(1000));
        }

        private string NiceMsg(string message)
        {
            return $"\"{message.Replace("\n", "\\n")}\"";
        }

        async Task CycleColors(TimeSpan duration)
        {
            Resolver.Log.Info("Cycle colors...");

            while (true)
            {
                await ShowColorPulse(Color.Blue, duration);
                await ShowColorPulse(Color.Cyan, duration);
                await ShowColorPulse(Color.Green, duration);
                await ShowColorPulse(Color.GreenYellow, duration);
                await ShowColorPulse(Color.Yellow, duration);
                await ShowColorPulse(Color.Orange, duration);
                await ShowColorPulse(Color.OrangeRed, duration);
                await ShowColorPulse(Color.Red, duration);
                await ShowColorPulse(Color.MediumVioletRed, duration);
                await ShowColorPulse(Color.Purple, duration);
                await ShowColorPulse(Color.Magenta, duration);
                await ShowColorPulse(Color.Pink, duration);
            }
        }

        async Task ShowColorPulse(Color color, TimeSpan duration)
        {
            await onboardLed.StartPulse(color, duration / 2);
            await Task.Delay(duration);
        }
    }
}