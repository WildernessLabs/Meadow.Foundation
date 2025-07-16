using Meadow.Foundation.ICs.IOExpanders;
using Meadow.Hardware;
using Meadow.Peripherals.Sensors.Buttons;

namespace Meadow.Foundation.mikroBUS.Displays
{
    /// <summary>
    /// Represents a mikroBUS Altair 8800 Retro click board
    /// </summary>
    public partial class C8800Retro : As1115
    {
        /// <summary>
        /// Creates an Altair 8800 retro click board object
        /// </summary>
        /// <param name="i2cBus">The I2C bus</param>
        /// <param name="buttonInterruptPin">The interrupt pin</param>
        /// <param name="address">The I2C address</param>
        public C8800Retro(II2cBus i2cBus, IPin buttonInterruptPin, byte address = 0)
            : base(i2cBus, buttonInterruptPin, address)
        { }

        /// <summary>
        /// Get the button for a given row and column
        /// </summary>
        /// <param name="column">The column of the button (1-4)</param>
        /// <param name="row">The row of the button (A-D)</param>
        /// <returns>The IButton object</returns>
        public IButton GetButton(ButtonColumn column, ButtonRow row)
        {
            KeyScanButtonType buttonType = KeyScanButtonType.None;

            if (row == ButtonRow.A)
            {
                buttonType = column switch
                {
                    ButtonColumn._1 => KeyScanButtonType.Button1,
                    ButtonColumn._2 => KeyScanButtonType.Button2,
                    ButtonColumn._3 => KeyScanButtonType.Button3,
                    ButtonColumn._4 => KeyScanButtonType.Button4,
                    _ => KeyScanButtonType.None
                };
            }
            if (row == ButtonRow.B)
            {
                buttonType = column switch
                {
                    ButtonColumn._1 => KeyScanButtonType.Button5,
                    ButtonColumn._2 => KeyScanButtonType.Button6,
                    ButtonColumn._3 => KeyScanButtonType.Button7,
                    ButtonColumn._4 => KeyScanButtonType.Button8,
                    _ => KeyScanButtonType.None
                };
            }
            if (row == ButtonRow.C)
            {
                buttonType = column switch
                {
                    ButtonColumn._1 => KeyScanButtonType.Button9,
                    ButtonColumn._2 => KeyScanButtonType.Button10,
                    ButtonColumn._3 => KeyScanButtonType.Button11,
                    ButtonColumn._4 => KeyScanButtonType.Button12,
                    _ => KeyScanButtonType.None
                };
            }
            if (row == ButtonRow.D)
            {
                buttonType = column switch
                {
                    ButtonColumn._1 => KeyScanButtonType.Button13,
                    ButtonColumn._2 => KeyScanButtonType.Button14,
                    ButtonColumn._3 => KeyScanButtonType.Button15,
                    ButtonColumn._4 => KeyScanButtonType.Button16,
                    _ => KeyScanButtonType.None
                };
            }

            return KeyScanButtons![buttonType];
        }
    }
}