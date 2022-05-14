using Meadow.Foundation.Sensors.Hid;
using Meadow.Hardware;
using Meadow.Peripherals.Sensors.Buttons;
using Meadow.Peripherals.Sensors.Hid;

namespace Meadow.Foundation.Sensors.Hid
{
    /// <summary>
    /// Represents a Nintendo NES Classic Mini controller
    /// </summary>
    public class NesClassicController : WiiClassicControllerBase
    {
        /// <summary>
        /// D-pad
        /// </summary>
        public IDigitalJoystick DPad { get; } = new WiiExtensionDPad();

        /// <summary>
        /// A Button
        /// </summary>
        public IButton AButton { get; } = new WiiExtensionButton();
        /// <summary>
        /// B Button
        /// </summary>
        public IButton BButton { get; } = new WiiExtensionButton();

        /// <summary>
        /// + Button
        /// </summary>
        public IButton StartButton { get; } = new WiiExtensionButton();
        /// <summary>
        /// - Button
        /// </summary>
        public IButton SelectButton { get; } = new WiiExtensionButton();

        /// <summary>
        /// Creates a NES Classic Mini Controller object
        /// </summary>
        /// <param name="i2cBus">the I2C bus connected to controller</param>
        public NesClassicController(II2cBus i2cBus) : base(i2cBus, (byte)Addresses.Default)
        {
        }

        /// <summary>
        /// Get the latest sensor data from the device
        /// </summary>
        public override void Update()
        {
            base.Update();

            //DPad
            (DPad as WiiExtensionDPad).Update(DPadLeftPressed, DPadRightPressed, DPadUpPressed, DPadDownPressed);

            //A, B
            (AButton as WiiExtensionButton).Update(AButtonPressed);
            (BButton as WiiExtensionButton).Update(BButtonPressed);

            //Start, Select
            (StartButton as WiiExtensionButton).Update(PlusButtonPressed);
            (SelectButton as WiiExtensionButton).Update(MinusButtonPressed);
        }
    }
}