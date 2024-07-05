using Meadow.Foundation.Audio;
using Meadow.Foundation.Displays.TftSpi;
using Meadow.Foundation.Graphics;
using Meadow.Foundation.Sensors.Atmospheric;
using Meadow.Foundation.Sensors.Buttons;
using Meadow.Foundation.Sensors.Light;
using Meadow.Hardware;
using Meadow.Units;
using System;

namespace Meadow.Devices
{
    public class ProjLab
    {
        public ISpiBus SpiBus { get; }
        public II2cBus I2CBus { get; }

        private readonly Lazy<St7789> _display;
        private readonly Lazy<Bh1750> _lightSensor;
        private readonly Lazy<PushButton> _upButton;
        private readonly Lazy<PushButton> _downButton;
        private readonly Lazy<PushButton> _leftButton;
        private readonly Lazy<PushButton> _rightButton;
        private readonly Lazy<Bme680> _bme680;
        private readonly Lazy<PiezoSpeaker> _speaker;
        // onboardLed = new RgbPwmLed(device: Device,
        //redPwmPin: Device.Pins.OnboardLedRed,
        //        greenPwmPin: Device.Pins.OnboardLedGreen,
        //        bluePwmPin: Device.Pins.OnboardLedBlue);

        public St7789 Display => _display.Value;
        public Bh1750 LightSensor => _lightSensor.Value;
        public PushButton UpButton => _upButton.Value;
        public PushButton DownButton => _downButton.Value;
        public PushButton LeftButton => _leftButton.Value;
        public PushButton RightButton => _rightButton.Value;
        public Bme680 EnvironmentalSensor => _bme680.Value;
        public PiezoSpeaker Speaker => _speaker.Value;

        public ProjLab()
        {
            // create our busses
            var config = new SpiClockConfiguration(
                           new Frequency(48000, Frequency.UnitType.Kilohertz),
                           SpiClockConfiguration.Mode.Mode3);

            SpiBus = Resolver.Device.CreateSpiBus(
                Resolver.Device.GetPin("SCK"),
                Resolver.Device.GetPin("MOSI"),
                Resolver.Device.GetPin("MISO"),
                config);

            I2CBus = Resolver.Device.CreateI2cBus();

            // lazy load all components
            _display = new Lazy<St7789>(() =>
                new St7789(
                    device: Resolver.Device,
                    spiBus: SpiBus,
                    chipSelectPin: Resolver.Device.GetPin("A03"),
                    dcPin: Resolver.Device.GetPin("A04"),
                    resetPin: Resolver.Device.GetPin("A05"),
                    width: 240, height: 240,
                    displayColorMode: ColorType.Format16bppRgb565));

            _lightSensor = new Lazy<Bh1750>(() =>
                new Bh1750(
                    i2cBus: I2CBus,
                    measuringMode: Bh1750.MeasuringModes.ContinuouslyHighResolutionMode, // the various modes take differing amounts of time.
                    lightTransmittance: 0.5, // lower this to increase sensitivity, for instance, if it's behind a semi opaque window
                    address: (byte)Bh1750.Addresses.Address_0x23));

            _upButton = new Lazy<PushButton>(() =>
                new PushButton(
                    Resolver.Device.CreateDigitalInputPort(
                        Resolver.Device.GetPin("D15"),
                        InterruptMode.EdgeBoth,
                        ResistorMode.InternalPullDown)));

            _downButton = new Lazy<PushButton>(() =>
                new PushButton(
                    Resolver.Device.CreateDigitalInputPort(
                        Resolver.Device.GetPin("D02"),
                        InterruptMode.EdgeBoth,
                        ResistorMode.InternalPullDown)));

            _leftButton = new Lazy<PushButton>(() =>
                new PushButton(
                    Resolver.Device.CreateDigitalInputPort(
                        Resolver.Device.GetPin("D10"),
                        InterruptMode.EdgeBoth,
                        ResistorMode.InternalPullDown)));

            _rightButton = new Lazy<PushButton>(() =>
                new PushButton(
                    Resolver.Device.CreateDigitalInputPort(
                        Resolver.Device.GetPin("D05"),
                        InterruptMode.EdgeBoth,
                        ResistorMode.InternalPullDown)));

            _bme680 = new Lazy<Bme680>(() =>
                new Bme680(I2CBus, (byte)Bme680.Addresses.Address_0x76));

            _speaker = new Lazy<PiezoSpeaker>(() =>
               new PiezoSpeaker(Resolver.Device, Resolver.Device.GetPin("D11")));
        }

        public static (
            IPin MB1_CS,
            IPin MB1_INT,
            IPin MB1_PWM,
            IPin MB1_AN,
            IPin MB1_SO,
            IPin MB1_SI,
            IPin MB1_SCK,
            IPin MB1_SCL,
            IPin MB1_SDA,

            IPin MB2_CS,
            IPin MB2_INT,
            IPin MB2_PWM,
            IPin MB2_AN,
            IPin MB2_SO,
            IPin MB2_SI,
            IPin MB2_SCK,
            IPin MB2_SCL,
            IPin MB2_SDA,

            IPin A0,
            IPin D03,
            IPin D04
            ) Pins = (
            Resolver.Device.GetPin("D14"),
            Resolver.Device.GetPin("D03"),
            Resolver.Device.GetPin("D04"),
            Resolver.Device.GetPin("A00"),
            Resolver.Device.GetPin("CIPO"),
            Resolver.Device.GetPin("COPI"),
            Resolver.Device.GetPin("SCK"),
            Resolver.Device.GetPin("D08"),
            Resolver.Device.GetPin("D07"),

            Resolver.Device.GetPin("A02"),
            Resolver.Device.GetPin("D04"),
            Resolver.Device.GetPin("D03"),
            Resolver.Device.GetPin("A01"),
            Resolver.Device.GetPin("CIPO"),
            Resolver.Device.GetPin("COPI"),
            Resolver.Device.GetPin("SCK"),
            Resolver.Device.GetPin("D08"),
            Resolver.Device.GetPin("D07"),

            Resolver.Device.GetPin("A00"),
            Resolver.Device.GetPin("D03"),
            Resolver.Device.GetPin("D04")
            );
    }
}

