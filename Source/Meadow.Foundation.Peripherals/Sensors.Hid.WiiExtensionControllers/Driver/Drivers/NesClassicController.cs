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
            ((WiiExtensionDPad)DPad).Update(DPadLeftPressed, DPadRightPressed, DPadUpPressed, DPadDownPressed);

            //A, B
            ((WiiExtensionButton)AButton).Update(AButtonPressed);
            ((WiiExtensionButton)BButton).Update(BButtonPressed);

            //Start, Select
            ((WiiExtensionButton)StartButton).Update(PlusButtonPressed);
            ((WiiExtensionButton)SelectButton).Update(MinusButtonPressed);
        }
    }
}