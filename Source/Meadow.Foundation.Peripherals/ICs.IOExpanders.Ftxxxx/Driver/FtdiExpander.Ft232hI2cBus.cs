using Meadow.Hardware;
using System;
using System.Threading;

namespace Meadow.Foundation.ICs.IOExpanders;

public abstract partial class FtdiExpander
{
    public class Ft232hI2cBus : I2CBus
    {
        private FtdiExpander _expander;

        internal Ft232hI2cBus(FtdiExpander expander, I2cBusSpeed busSpeed)
            : base(expander, busSpeed)
        {
        }

        internal override void Configure()
        {
            // Setup the clock and other elements
            Span<byte> toSend = stackalloc byte[10];
            int idx = 0;
            // Disable clock divide by 5 for 60Mhz master clock
            toSend[idx++] = (byte)Native.FT_OPCODE.DisableClockDivideBy5;
            // Turn off adaptive clocking
            toSend[idx++] = (byte)Native.FT_OPCODE.TurnOffAdaptiveClocking;
            // Enable 3 phase data clock, used by I2C to allow data on both clock edges
            toSend[idx++] = (byte)Native.FT_OPCODE.Enable3PhaseDataClocking;

            // The SK clock frequency can be worked out by below algorithm with divide by 5 set as off
            // TCK period = 60MHz / (( 1 + [ (0xValueH * 256) OR 0xValueL] ) * 2)
            // Command to set clock divisor
            toSend[idx++] = (byte)Native.FT_OPCODE.SetClockDivisor;
            uint clockDivisor = (60000 / (((uint)BusSpeed / 1000) * 2)) - 1;
            toSend[idx++] = (byte)(clockDivisor & 0x00FF);
            toSend[idx++] = (byte)((clockDivisor >> 8) & 0x00FF);

            // loopback off
            toSend[idx++] = (byte)Native.FT_OPCODE.DisconnectTDItoTDOforLoopback;

            // DEV NOTE (21 Fed 20204):
            // the following is in FTDI's FT232H samples, but makes the clock signal an ugly sawtooth instead of squares

            // Enable the FT232H's drive-zero mode with the following enable mask
            //toSend[idx++] = (byte)Native.FT_OPCODE.SetIOOnlyDriveOn0AndTristateOn1;
            // Low byte (ADx) enables - bits 0, 1 and 2
            //toSend[idx++] = 0x07;
            // High byte (ACx) enables - all off
            //toSend[idx++] = 0x00;


            // modify the GPIO state and direction without breaking other stuff
            // SDA and SCL both output high(open drain)
            toSend[idx++] = (byte)Native.FT_OPCODE.SetDataBitsLowByte;
            _expander.GpioStateLow = (byte)(PinData.SDAhiSCLhi | (_expander.GpioStateLow & MaskGpio));
            _expander.GpioDirectionLow = (byte)(PinDirection.SDAoutSCLout | (_expander.GpioDirectionLow & MaskGpio));
            toSend[idx++] = _expander.GpioStateLow;
            toSend[idx++] = _expander.GpioDirectionLow;

            _expander.Write(toSend);

            Thread.Sleep(20);
            Idle();
            Thread.Sleep(30);
        }

        internal override void Start()
        {
            // SDA high, SCL high
            var direction = (byte)(PinDirection.SDAoutSCLout | (_expander.GpioDirectionLow & MaskGpio));
            Idle();
            //            Wait(2);

            // SDA lo, SCL high
            var state = (byte)(PinData.SDAloSCLhi | (_expander.GpioStateLow & MaskGpio));
            _expander.SetGpioDirectionAndState(true, direction, state);
            //            Wait(2);

            // SDA lo, SCL lo
            state = (byte)(PinData.SDAloSCLlo | (_expander.GpioStateLow & MaskGpio));
            _expander.SetGpioDirectionAndState(true, direction, state);
            //            Wait(2);

            // Release SDA
            //            state = (byte)(PinData.SDAhiSCLlo | (_expander.GpioStateLow & MaskGpio));
            //            _expander.SetGpioDirectionAndState(true, direction, state);
            //            Wait(6);
        }

        internal override void Stop()
        {
            // SDA low, SCL low
            var state = (byte)(PinData.SDAloSCLlo | (_expander.GpioStateLow & MaskGpio));
            var direction = (byte)(PinDirection.SDAoutSCLout | (_expander.GpioDirectionLow & MaskGpio));

            // SDA low, SCL high
            state = (byte)(PinData.SDAloSCLhi | (_expander.GpioStateLow & MaskGpio));
            _expander.SetGpioDirectionAndState(true, direction, state);
            Wait(6);

            // SDA high, SCL high
            state = (byte)(PinData.SDAhiSCLhi | (_expander.GpioStateLow & MaskGpio));
            _expander.SetGpioDirectionAndState(true, direction, state);
            Wait(6);
        }

        internal override void Idle()
        {
            // SDA high, SCL high
            // FT232H always output due to open drain capability
            var state = (byte)(PinData.SDAhiSCLhi | (_expander.GpioStateLow & MaskGpio));
            var direction = (byte)(PinDirection.SDAoutSCLout | (_expander.GpioDirectionLow & MaskGpio));
            _expander.SetGpioDirectionAndState(true, direction, state);
        }

        internal override TransferStatus SendDataByte(byte data)
        {
            Span<byte> txBuffer = stackalloc byte[10];
            Span<byte> rxBuffer = stackalloc byte[1];
            var idx = 0;

            // Just clock with one byte (0 = 1 byte)
            txBuffer[idx++] = (byte)Native.FT_OPCODE.ClockDataBytesOutOnMinusVeClockMSBFirst;
            txBuffer[idx++] = 0;
            txBuffer[idx++] = 0;
            txBuffer[idx++] = data;
            // Put line back to idle (data released, clock pulled low)
            _expander.GpioStateLow = (byte)(PinData.SDAloSCLlo | (_expander.GpioStateLow & MaskGpio));
            _expander.GpioDirectionLow = (byte)(PinDirection.SDAoutSCLout | (_expander.GpioDirectionLow & MaskGpio));
            txBuffer[idx++] = (byte)Native.FT_OPCODE.SetDataBitsLowByte;
            txBuffer[idx++] = _expander.GpioStateLow;
            txBuffer[idx++] = _expander.GpioDirectionLow;
            // Clock in (0 = 1 bit)
            txBuffer[idx++] = (byte)Native.FT_OPCODE.ClockDataBitsInOnPlusVeClockMSBFirst;
            txBuffer[idx++] = 0;
            txBuffer[idx++] = (byte)Native.FT_OPCODE.SendImmediate;
            _expander.Write(txBuffer);
            _expander.ReadInto(rxBuffer);

            return (rxBuffer[0] & 0x01) == 0 ? TransferStatus.Ack : TransferStatus.Nack;
        }

        internal override byte ReadDataByte(bool ackAfterRead)
        {
            int idx = 0;
            Span<byte> toSend = stackalloc byte[10];
            Span<byte> toRead = stackalloc byte[1];

            // The behavior is a bit different for FT232H and FT2232H/FT4232H
            // Read one byte
            toSend[idx++] = (byte)Native.FT_OPCODE.ClockDataBytesInOnPlusVeClockMSBFirst;
            toSend[idx++] = 0;
            toSend[idx++] = 0;
            // Send out either ack either nak
            toSend[idx++] = (byte)Native.FT_OPCODE.ClockDataBitsOutOnMinusVeClockMSBFirst;
            toSend[idx++] = 0;
            toSend[idx++] = (byte)(ackAfterRead ? 0x00 : 0xFF);
            // I2C lines back to idle state
            toSend[idx++] = (byte)Native.FT_OPCODE.SetDataBitsLowByte;
            _expander.GpioStateLow = (byte)(PinData.SDAhiSCLlo | (_expander.GpioStateLow & MaskGpio));
            _expander.GpioDirectionLow = (byte)(PinDirection.SDAoutSCLout | (_expander.GpioDirectionLow & MaskGpio));
            toSend[idx++] = _expander.GpioStateLow;
            toSend[idx++] = _expander.GpioDirectionLow;
            toSend[idx++] = (byte)Native.FT_OPCODE.SendImmediate;
            _expander.Write(toSend);
            _expander.ReadInto(toRead);
            return toRead[0];
        }
    }
}