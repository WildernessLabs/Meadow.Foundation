using System;
using Meadow.Foundation.Sensors.Buttons;
using Meadow.Hardware;
using Meadow.Peripherals.Sensors.Buttons;

namespace Meadow.Foundation.FeatherWings
{
    public class JoyWing
    {
        public event EventHandler OnA;
        public event EventHandler OnB;
        public event EventHandler OnX;
        public event EventHandler OnY;
        public event EventHandler OnSelect;

        public IButton ButtonX { get; private set; }
        public IButton ButtonY { get; private set; }
        public IButton ButtonA { get; private set; }
        public IButton ButtonB { get; private set; }
        public IButton ButtonSelect { get; private set; }

        public JoyWing(IIODevice device, IPin pinX, IPin pinY, IPin pinA, IPin pinB, IPin pinSelect,
            IPin pinJoyHorizontal, IPin pinJoyVertical) :
            this(device.CreateDigitalInputPort(pinX, InterruptMode.LevelHigh),
                device.CreateDigitalInputPort(pinY, InterruptMode.LevelHigh),
                device.CreateDigitalInputPort(pinA, InterruptMode.LevelHigh),
                device.CreateDigitalInputPort(pinB, InterruptMode.LevelHigh),
                device.CreateDigitalInputPort(pinSelect, InterruptMode.LevelHigh),
                device.CreateDigitalInputPort(pinJoyHorizontal, InterruptMode.LevelHigh),
                device.CreateDigitalInputPort(pinJoyVertical, InterruptMode.LevelHigh))
        {

        }

        public JoyWing(IDigitalInputPort portX, IDigitalInputPort portY, IDigitalInputPort portA, IDigitalInputPort portB,
            IDigitalInputPort portSelect, IDigitalInputPort portJoyHorizontal, IDigitalInputPort portJoyVertical)
        {
            ButtonA = new PushButton(portA);
            ButtonB = new PushButton(portB);
            ButtonX = new PushButton(portX);
            ButtonY = new PushButton(portY);
            ButtonSelect = new PushButton(portSelect);

            ButtonA.PressEnded += (s, e) => OnA?.Invoke(s, e);
            ButtonB.PressEnded += (s, e) => OnB?.Invoke(s, e);
            ButtonX.PressEnded += (s, e) => OnX?.Invoke(s, e);
            ButtonY.PressEnded += (s, e) => OnY?.Invoke(s, e);
            ButtonSelect.PressEnded += (s, e) => OnSelect?.Invoke(s, e);
        }
    }
}