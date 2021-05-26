using Meadow.Foundation.Displays.Ssd130x;
using Meadow.Foundation.Sensors.Buttons;
using Meadow.Hardware;

namespace Meadow.Foundation.FeatherWings
{
    /// <summary>
    /// Represents Adafruits OLED Feather Wing
    /// </summary>
    public class OLED128x32Wing
    {
        public Ssd1306 Display { get; protected set; }

        public PushButton ButtonA { get; protected set; }

        public PushButton ButtonB { get; protected set; }

        public PushButton ButtonC { get; protected set; }

        public OLED128x32Wing(II2cBus i2cBus, IDigitalInputController device, IPin pinA, IPin pinB, IPin pinC) : 
            this(i2cBus, 
                device.CreateDigitalInputPort(pinA, InterruptMode.EdgeBoth, ResistorMode.InternalPullUp),
                device.CreateDigitalInputPort(pinB, InterruptMode.EdgeBoth, ResistorMode.InternalPullUp),
                device.CreateDigitalInputPort(pinC, InterruptMode.EdgeBoth, ResistorMode.InternalPullUp)){ }

        public OLED128x32Wing(II2cBus i2cBus, IDigitalInputPort portA, IDigitalInputPort portB, IDigitalInputPort portC)
        {
            Display = new Ssd1306(i2cBus, 0x3C, Ssd1306.DisplayType.OLED128x32);
            Display.IgnoreOutOfBoundsPixels = true;

            ButtonA = new PushButton(portA);
            ButtonB = new PushButton(portB); 
            ButtonC = new PushButton(portC);
        }
    }
}