﻿using Meadow.Hardware;
using System;
using System.Threading;

namespace Meadow.Foundation.Sensors.Hid
{
    /// <summary>
    /// Represents a BBQ10Keyboard Featherwing
    /// </summary>
    public partial class BBQ10Keyboard : II2cPeripheral, IDisposable
    {
        /// <summary>
        /// The default I2C address for the peripheral
        /// </summary>
        public byte DefaultI2cAddress => (byte)Addresses.Default;

        /// <summary>
        /// Is the object disposed
        /// </summary>
        public bool IsDisposed { get; private set; }

        /// <summary>
        /// Did we create the port(s) used by the peripheral
        /// </summary>
        readonly bool createdPort = false;

        /// <summary>
        /// I2C Communication bus used to communicate with the peripheral
        /// </summary>
        protected readonly II2cCommunications i2cComms;
        private readonly IDigitalInterruptPort? interruptPort;

        /// <summary>
        /// Raised when a key press is detected
        /// </summary>
        public event EventHandler<KeyEvent> OnKeyEvent = default!;

        private byte Status => i2cComms.ReadRegister((byte)Registers.KEY);

        private byte KeyCount => (byte)(i2cComms.ReadRegister(KEY_COUNT_MASK) & Status);

        /// <summary>
        /// Get or set the backlight
        /// </summary>
        public byte BackLight
        {
            get => i2cComms.ReadRegister((byte)Registers.BKL);
            set => i2cComms.WriteRegister((byte)Registers.BKL, value);
        }

        /// <summary>
        /// Get or set the 2nd backlight 
        /// </summary>
        public byte BackLight2
        {
            get => i2cComms.ReadRegister((byte)Registers.BK2);
            set => i2cComms.WriteRegister((byte)Registers.BK2, value);
        }

        /// <summary>
        /// Create a new BBQ10Keyboard object
        /// </summary>
        /// <param name="i2cBus">The I2C bus</param>
        /// <param name="interruptPin">The interrupt pin</param>
        /// <param name="address">The I2C address</param>
        public BBQ10Keyboard(II2cBus i2cBus, IPin? interruptPin = null, byte address = (byte)Addresses.Default)
        {
            i2cComms = new I2cCommunications(i2cBus, address);

            if (interruptPin != null)
            {
                createdPort = true;

                interruptPort = interruptPin.CreateDigitalInterruptPort(InterruptMode.EdgeBoth, ResistorMode.InternalPullUp);
                interruptPort.Changed += InterruptPort_Changed;
            }

            Reset();
        }

        /// <summary>
        /// Get the last key event
        /// </summary>
        /// <returns>The event</returns>
        public KeyEvent GetLastKeyEvent()
        {
            if (KeyCount == 0)
            {
                return new KeyEvent('\0', KeyState.StateIdle);
            }

            var keyData = i2cComms.ReadRegisterAsUShort((byte)Registers.FIF);

            return new KeyEvent((char)(keyData >> 8), (KeyState)(keyData & 0xFF));
        }

        /// <summary>
        /// Reset the keyboard
        /// </summary>
        public void Reset()
        {
            i2cComms?.Write((byte)Registers.RST);
            Thread.Sleep(100);
        }

        /// <summary>
        /// Clear the interrupt status
        /// </summary>
        protected void ClearInerruptStatus()
        {
            i2cComms.WriteRegister((byte)Registers.INT, 0x00);
        }

        private void InterruptPort_Changed(object sender, DigitalPortResult e)
        {
            OnKeyEvent?.Invoke(this, GetLastKeyEvent());
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
        protected virtual void Dispose(bool disposing)
        {
            if (!IsDisposed)
            {
                if (disposing && createdPort)
                {
                    interruptPort?.Dispose();
                }

                IsDisposed = true;
            }
        }
    }
}