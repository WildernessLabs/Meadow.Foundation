using Meadow.Devices;
using Meadow.Foundation.Displays;
using Meadow.Foundation.Graphics;
using Meadow.Foundation.Sensors.Buttons;
using Meadow.Hardware;
using Meadow.Peripherals.Sensors.Buttons;
using System;

namespace Meadow.Foundation.FeatherWings
{
    /// <summary>
    /// Represents Adafruits OLED Feather Wing
    /// </summary>
    public class OLED128x32Wing
    {
        public event EventHandler OnA;
        public event EventHandler OnB;
        public event EventHandler OnC;

        readonly II2cBus _i2cBus;
        readonly Ssd1306 _display;
        readonly GraphicsLibrary _graphics;

        readonly IButton _buttonA;
        readonly IButton _buttonB;
        readonly IButton _buttonC;

        public Ssd1306 Display => _display;

        public GraphicsLibrary Graphics => _graphics;

        public IButton ButtonA => _buttonA;

        public IButton ButtonB => _buttonB;

        public IButton ButtonC => _buttonC;

        public OLED128x32Wing(II2cBus i2cBus, IIODevice device, IPin pinA, IPin pinB, IPin pinC) : 
            this(i2cBus, 
                device.CreateDigitalInputPort(pinA, InterruptMode.EdgeBoth, ResistorMode.PullUp),
                device.CreateDigitalInputPort(pinB, InterruptMode.EdgeBoth),
                device.CreateDigitalInputPort(pinC, InterruptMode.EdgeBoth, ResistorMode.PullUp)){ }

        public OLED128x32Wing(II2cBus i2cBus, IDigitalInputPort portA, IDigitalInputPort portB, IDigitalInputPort portC)
        {
            _i2cBus = i2cBus;
            _display = new Ssd1306(_i2cBus, 0x3C, Ssd1306.DisplayType.OLED128x32);
            _graphics = new GraphicsLibrary(_display);
            _graphics.CurrentFont = new Font8x8();

            //Bug? Resistor Mode is being set properly from the above constructor but unless it is set again it doesn't work.
            portA.Resistor = portA.Resistor;
            portC.Resistor = portC.Resistor;

            _buttonA = new PushButton(portA);
            _buttonB = new PushButton(portB);
            _buttonC = new PushButton(portC);

            ButtonA.PressEnded += (s, e) => OnA?.Invoke(s, e);
            ButtonB.PressEnded += (s, e) => OnB?.Invoke(s, e);
            ButtonC.PressEnded += (s, e) => OnC?.Invoke(s, e);

        }

        /// <summary>
        /// Writes the lines to the screen
        /// </summary>
        /// <param name="lines"></param>
        public virtual void WriteLines(params string[] lines)
        {
            Display.Clear();
            int yPosition = 0;
            foreach (var line in lines)
            {
                if(yPosition + Graphics.CurrentFont.Height > 32)
                {
                    break;
                }

                Graphics.DrawText(0, yPosition, line);
                yPosition += Graphics.CurrentFont.Height;
            }

            Display.Show();
        }

        /// <summary>
        /// Clears the screen
        /// </summary>
        public virtual void Clear()
        {
            Display.Clear();
        }

    }
}
