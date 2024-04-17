using Meadow.Hardware;
using Meadow.Peripherals.Leds;
using System;

namespace Meadow.Foundation.Leds
{
    /// <summary>
    /// Represents an RGB LED
    /// </summary>
    public partial class RgbLed : IRgbLed, IDisposable
    {
        readonly bool createdPorts = false;

        /// <summary>
        /// The current LED color
        /// </summary>
        public RgbLedColors Color { get; protected set; } = RgbLedColors.White;

        /// <summary>
        /// The red LED port
        /// </summary>
        protected IDigitalOutputPort RedPort { get; set; }

        /// <summary>
        /// The green LED port
        /// </summary>
        protected IDigitalOutputPort GreenPort { get; set; }

        /// <summary>
        ///The blue LED port
        /// </summary>
        protected IDigitalOutputPort BluePort { get; set; }

        /// <summary>
        /// The common type (common anode or common cathode)
        /// </summary>
        public CommonType Common { get; protected set; }

        /// <inheritdoc/>
        public bool IsOn
        {
            get => isOn;
            set => UpdateLed(isOn = value);
        }
        bool isOn;

        /// <summary>
        /// Is the object disposed
        /// </summary>
        public bool IsDisposed { get; private set; }

        /// <summary>
        /// Create instance of RgbLed
        /// </summary>
        /// <param name="redPin">Red Pin</param>
        /// <param name="greenPin">Green Pin</param>
        /// <param name="bluePin">Blue Pin</param>
        /// <param name="commonType">Is Common Cathode</param>
        public RgbLed(
            IPin redPin,
            IPin greenPin,
            IPin bluePin,
            CommonType commonType = CommonType.CommonCathode) :
            this(
                redPin.CreateDigitalOutputPort(),
                greenPin.CreateDigitalOutputPort(),
                bluePin.CreateDigitalOutputPort(),
                commonType)
        {
            createdPorts = true;
        }

        /// <summary>
        /// Create instance of RgbLed
        /// </summary>
        /// <param name="redPort">Red Port</param>
        /// <param name="greenPort">Green Port</param>
        /// <param name="bluePort">Blue Port</param>
        /// <param name="commonType">Is Common Cathode</param>
        public RgbLed(
            IDigitalOutputPort redPort,
            IDigitalOutputPort greenPort,
            IDigitalOutputPort bluePort,
            CommonType commonType = CommonType.CommonCathode)
        {
            RedPort = redPort;
            GreenPort = greenPort;
            BluePort = bluePort;
            Common = commonType;
        }

        ///<inheritdoc/>
        public void SetColor(RgbLedColors color)
        {
            Color = color;

            IsOn = true;
        }

        /// <summary>
        /// Turns on LED with current color or LED off
        /// </summary>
        /// <param name="isOn">True for on, False for off</param>
        protected void UpdateLed(bool isOn)
        {
            bool onState = (Common == CommonType.CommonCathode);

            if (isOn)
            {
                switch (Color)
                {
                    case RgbLedColors.Red:
                        RedPort.State = onState;
                        GreenPort.State = !onState;
                        BluePort.State = !onState;
                        break;
                    case RgbLedColors.Green:
                        RedPort.State = !onState;
                        GreenPort.State = onState;
                        BluePort.State = !onState;
                        break;
                    case RgbLedColors.Blue:
                        RedPort.State = !onState;
                        GreenPort.State = !onState;
                        BluePort.State = onState;
                        break;
                    case RgbLedColors.Yellow:
                        RedPort.State = onState;
                        GreenPort.State = onState;
                        BluePort.State = !onState;
                        break;
                    case RgbLedColors.Magenta:
                        RedPort.State = onState;
                        GreenPort.State = !onState;
                        BluePort.State = onState;
                        break;
                    case RgbLedColors.Cyan:
                        RedPort.State = !onState;
                        GreenPort.State = onState;
                        BluePort.State = onState;
                        break;
                    case RgbLedColors.White:
                        RedPort.State = onState;
                        GreenPort.State = onState;
                        BluePort.State = onState;
                        break;
                }
            }
            else
            {
                RedPort.State = !onState;
                GreenPort.State = !onState;
                BluePort.State = !onState;
            }
        }

        ///<inheritdoc/>
        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Dispose of the object
        /// </summary>
        /// <param name="disposing">Is disposing</param>
        public virtual void Dispose(bool disposing)
        {
            if (!IsDisposed)
            {
                if (disposing && createdPorts)
                {
                    RedPort.Dispose();
                    GreenPort.Dispose();
                    BluePort.Dispose();
                }

                IsDisposed = true;
            }
        }
    }
}