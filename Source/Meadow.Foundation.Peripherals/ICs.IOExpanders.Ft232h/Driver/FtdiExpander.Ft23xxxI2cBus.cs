using Meadow.Hardware;
using System;

namespace Meadow.Foundation.ICs.IOExpanders;

public abstract partial class FtdiExpander
{
    public class Ft23xxxI2cBus : I2CBus, II2cBus
    {
        internal Ft23xxxI2cBus(FtdiExpander expander, I2cBusSpeed busSpeed)
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
            // Enable the FT232H's drive-zero mode with the following enable mask
            //            toSend[idx++] = (byte)Native.FT_OPCODE.SetIOOnlyDriveOn0AndTristateOn1;
            // Low byte (ADx) enables - bits 0, 1 and 2
            //            toSend[idx++] = 0x07;
            // High byte (ACx) enables - all off
            //            toSend[idx++] = 0x00;
            // Command to set directions of lower 8 pins and force value on bits set as output
            toSend[idx++] = (byte)Native.FT_OPCODE.SetDataBitsLowByte;

            // modify the GPIO state and direction without breaking other stuff
            // SDA and SCL set low but as input to mimic open drain
            _expander.GpioStateLow = (byte)(PinData.SDAloSCLlo | (_expander.GpioStateLow & MaskGpio));
            _expander.GpioDirectionLow = (byte)(PinDirection.SDAinSCLin | (_expander.GpioDirectionLow & MaskGpio));

            toSend[idx++] = _expander.GpioStateLow;
            toSend[idx++] = _expander.GpioDirectionLow;
            _expander.Write(toSend);
        }

        internal override void Start()
        {
            // Both SDA and SCL high (setting to input simulates open drain high)
            var state = (byte)(PinData.SDAloSCLlo | (_expander.GpioStateLow & MaskGpio));
            Idle();
            Wait(6);

            // SDA low, SCL high (setting to input simulates open drain high)
            var direction = (byte)(0x00 | PinDirection.SDAoutSCLin | (_expander.GpioDirectionLow & 0xF8));
            _expander.SetGpioDirectionAndState(true, direction, state);
            Wait(6);

            // SDA low, SCL low
            direction = (byte)(0x00 | PinDirection.SDAoutSCLout | (_expander.GpioDirectionLow & 0xF8));
            _expander.SetGpioDirectionAndState(true, direction, state);
            Wait(6);

            // Release SDA (setting to input simulates open drain high)
            direction = (byte)(0x00 | PinDirection.SDAinSCLout | (_expander.GpioDirectionLow & 0xF8));
            _expander.SetGpioDirectionAndState(true, direction, state);
            Wait(6);
        }

        internal override void Stop()
        {
            // SDA low, SCL low
            var state = (byte)(PinData.SDAloSCLlo | (_expander.GpioStateLow & MaskGpio));
            var direction = (byte)(PinDirection.SDAoutSCLout | (_expander.GpioDirectionLow & MaskGpio));
            _expander.SetGpioDirectionAndState(true, direction, state);
            Wait(6);


            // SDA low, SCL high (note: setting to input simulates open drain high)
            direction = (byte)(PinDirection.SDAoutSCLin | (_expander.GpioDirectionLow & MaskGpio));
            _expander.SetGpioDirectionAndState(true, direction, state);
            Wait(6);

            // SDA high, SCL high (note: setting to input simulates open drain high)
            direction = (byte)(PinDirection.SDAinSCLin | (_expander.GpioDirectionLow & MaskGpio));
            _expander.SetGpioDirectionAndState(true, direction, state);
            Wait(6);
        }

        internal override void Idle()
        {
            // SDA high, SCL high
            var state = (byte)(PinData.SDAloSCLlo | (_expander.GpioStateLow & MaskGpio));
            // FT2232H/FT4232H use input to mimic open drain
            var direction = (byte)(PinDirection.SDAinSCLin | (_expander.GpioDirectionLow & MaskGpio));
            _expander.SetGpioDirectionAndState(true, direction, state);
        }

        internal override TransferStatus SendDataByte(byte data)
        {
            Span<byte> txBuffer = stackalloc byte[13];
            Span<byte> rxBuffer = stackalloc byte[1];
            var idx = 0;

            _expander.GpioStateLow = (byte)(0x00 | PinData.SDAloSCLlo | (_expander.GpioStateLow & 0xF8));
            _expander.GpioDirectionLow = (byte)(0x00 | PinDirection.SDAoutSCLout | (_expander.GpioDirectionLow & 0xF8));// back to output
            txBuffer[idx++] = (byte)Native.FT_OPCODE.SetDataBitsLowByte;                                   // Command - set low byte
            txBuffer[idx++] = _expander.GpioStateLow;                               // Set the values
            txBuffer[idx++] = _expander.GpioDirectionLow;                               // Set the directions
            // clock out one byte
            txBuffer[idx++] = (byte)Native.FT_OPCODE.ClockDataBytesOutOnMinusVeClockMSBFirst;        // clock data byte out
            txBuffer[idx++] = 0x00;                                   // 
            txBuffer[idx++] = 0x00;                                   // Data length of 0x0000 means 1 byte data to clock in
            txBuffer[idx++] = data;                         // Byte to send

            // Put line back to idle (data released, clock pulled low) so that sensor can drive data line
            _expander.GpioStateLow = (byte)(0x00 | PinData.SDAloSCLlo | (_expander.GpioStateLow & 0xF8));
            _expander.GpioDirectionLow = (byte)(0x00 | PinDirection.SDAinSCLout | (_expander.GpioDirectionLow & 0xF8)); // make data input
            txBuffer[idx++] = 0x80;                                   // Command - set low byte
            txBuffer[idx++] = _expander.GpioStateLow;                               // Set the values
            txBuffer[idx++] = _expander.GpioDirectionLow;                               // Set the directions

            // CLOCK IN ACK (0 == 1 bit)
            txBuffer[idx++] = (byte)Native.FT_OPCODE.ClockDataBitsInOnPlusVeClockMSBFirst;         // clock data byte in
            txBuffer[idx++] = 0x00;                                   // Length of 0 means 1 bit

            // This command then tells the MPSSE to send any results gathered (in this case the ack bit) back immediately
            txBuffer[idx++] = (byte)Native.FT_OPCODE.SendImmediate;                                //  ' Send answer back immediate command
            _expander.Write(txBuffer);
            _expander.ReadInto(rxBuffer);

            return (rxBuffer[0] & 0x01) == 0 ? TransferStatus.Ack : TransferStatus.Nack;
        }

        internal override byte ReadDataByte(bool ackAfterRead)
        {
            int idx = 0;
            Span<byte> toSend = stackalloc byte[16];
            Span<byte> toRead = stackalloc byte[1];

            // Make sure no open gain
            _expander.GpioStateLow = (byte)(PinData.SDAloSCLlo | (_expander.GpioStateLow & MaskGpio));
            _expander.GpioDirectionLow = (byte)(PinDirection.SDAinSCLout | (_expander.GpioDirectionLow & MaskGpio));
            toSend[idx++] = (byte)Native.FT_OPCODE.SetDataBitsLowByte;
            toSend[idx++] = _expander.GpioStateLow;
            toSend[idx++] = _expander.GpioDirectionLow;
            // Read one byte
            toSend[idx++] = (byte)Native.FT_OPCODE.ClockDataBytesInOnPlusVeClockMSBFirst;
            toSend[idx++] = 0;
            toSend[idx++] = 0;
            // Change direction
            _expander.GpioStateLow = (byte)(PinData.SDAloSCLlo | (_expander.GpioStateLow & MaskGpio));
            _expander.GpioDirectionLow = (byte)(PinDirection.SDAoutSCLout | (_expander.GpioDirectionLow & MaskGpio));
            toSend[idx++] = (byte)Native.FT_OPCODE.SetDataBitsLowByte;
            toSend[idx++] = _expander.GpioStateLow;
            toSend[idx++] = _expander.GpioDirectionLow;
            // Send out either ack either nak
            toSend[idx++] = (byte)Native.FT_OPCODE.ClockDataBitsOutOnMinusVeClockMSBFirst;
            toSend[idx++] = 0;
            toSend[idx++] = (byte)(ackAfterRead ? 0x00 : 0xFF);
            // I2C lines back to idle state
            toSend[idx++] = (byte)Native.FT_OPCODE.SetDataBitsLowByte;
            _expander.GpioStateLow = (byte)(PinData.SDAhiSCLlo | (_expander.GpioStateLow & MaskGpio));
            _expander.GpioDirectionLow = (byte)(PinDirection.SDAinSCLout | (_expander.GpioDirectionLow & MaskGpio));
            toSend[idx++] = _expander.GpioStateLow;
            toSend[idx++] = _expander.GpioDirectionLow;

            // And ask it right away
            toSend[idx++] = (byte)Native.FT_OPCODE.SendImmediate;
            _expander.Write(toSend);
            _expander.ReadInto(toRead);
            return toRead[0];
        }
    }
}