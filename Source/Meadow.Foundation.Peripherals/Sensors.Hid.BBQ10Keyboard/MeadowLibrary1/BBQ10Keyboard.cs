using Meadow.Devices;
using Meadow.Hardware;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace Meadow.Foundation.Sensors.Hid
{
    public partial class BBQ10Keyboard
    {
        I2cPeripheral i2CPeripheral;

        IDigitalInterruptPort interruptPort;

        public event EventHandler<KeyEvent> OnKeyEvent = delegate { };

        byte Status => i2CPeripheral.ReadRegister((byte)Registers.KEY);

        byte KeyCount => (byte)(i2CPeripheral.ReadRegister(KEY_COUNT_MASK) & Status);

        public byte BackLight
        {
            get => i2CPeripheral.ReadRegister((byte)Registers.BKL);
            set => i2CPeripheral.WriteRegister((byte)Registers.BKL, value);
        }

        public byte BackLight2
        {
            get => i2CPeripheral.ReadRegister((byte)Registers.BK2);
            set => i2CPeripheral.WriteRegister((byte)Registers.BK2, value);
        }

        public BBQ10Keyboard(IMeadowDevice device, II2cBus i2cBus, IPin interruptPin = null, byte address = (byte)Addresses.Default)
        {
            i2CPeripheral = new I2cPeripheral(i2cBus, address);

            if (interruptPin != null)
            {
                interruptPort = device.CreateDigitalInputPort(interruptPin, InterruptMode.EdgeBoth, ResistorMode.InternalPullUp);
                interruptPort.Changed += InterruptPort_Changed;
            }

            Reset();
        }

        public KeyEvent GetLastKeyEvent()
        {
            if (KeyCount == 0)
            {
                return new KeyEvent('\0', KeyState.StateIdle);
            }

            var keyData = i2CPeripheral.ReadRegisterAsUShort((byte)Registers.FIF);

            return new KeyEvent((char)(keyData >> 8), (KeyState)(keyData & 0xFF));
        }

        public void Reset()
        {
            i2CPeripheral.Write((byte)Registers.RST);
            Thread.Sleep(100);
        }

        void ClearInerruptStatus()
        {
            i2CPeripheral.WriteRegister((byte)Registers.INT, 0x00);
        }

        private void InterruptPort_Changed(object sender, DigitalPortResult e)
        {
            OnKeyEvent?.Invoke(this, GetLastKeyEvent());
        }
    }
}