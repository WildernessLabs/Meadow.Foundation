using Meadow.Foundation.Displays;
using Meadow.Foundation.Sensors.Buttons;
using Meadow.Hardware;
using Meadow.Peripherals.Sensors.Buttons;

namespace Meadow.Foundation.FeatherWings
{
    /// <summary>
    /// Represents Adafruits OLED Feather Wing
    /// </summary>
    public class OLED128x32Wing
    {
        readonly II2cBus i2cBus;

        public Ssd1306 Display { get; protected set; }

        public IButton ButtonA { get; protected set; }

        public IButton ButtonB { get; protected set; }

        public IButton ButtonC { get; protected set; }

        public OLED128x32Wing(II2cBus i2cBus, IIODevice device, IPin pinA, IPin pinB, IPin pinC) : 
            this(i2cBus, 
                device.CreateDigitalInputPort(pinA, InterruptMode.EdgeBoth, ResistorMode.PullUp),
                device.CreateDigitalInputPort(pinB, InterruptMode.EdgeBoth, ResistorMode.PullUp),
                device.CreateDigitalInputPort(pinC, InterruptMode.EdgeBoth, ResistorMode.PullUp)){ }

        public OLED128x32Wing(II2cBus i2cBus, IDigitalInputPort portA, IDigitalInputPort portB, IDigitalInputPort portC)
        {
            this.i2cBus = i2cBus;
            Display = new Ssd1306(this.i2cBus, 0x3C, Ssd1306.DisplayType.OLED128x32);

            //Bug? Resistor Mode is being set properly from the above constructor but unless it is set again it doesn't work.
            portA.Resistor = portA.Resistor;
            portC.Resistor = portC.Resistor;

            ButtonA = new PushButton(portA);
            ButtonB = new PushButton(portB);
            ButtonC = new PushButton(portC);
        }
    }
}