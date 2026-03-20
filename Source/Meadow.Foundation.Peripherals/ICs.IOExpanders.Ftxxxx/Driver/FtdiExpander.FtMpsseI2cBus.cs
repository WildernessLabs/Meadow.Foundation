using Meadow.Hardware;
using System;
using System.Threading;

namespace Meadow.Foundation.ICs.IOExpanders;

public abstract partial class FtdiExpander
{
    /// <summary>
    /// Represents an MPSSE-based I2C bus for FTDI chips with MPSSE support (FT232H, FT2232H, FT4232H).
    /// </summary>
    public class FtMpsseI2cBus : I2CBus
    {
        internal FtMpsseI2cBus(FtdiExpander expander, I2cBusSpeed busSpeed)
            : base(expander, busSpeed)
        {
        }

        internal override void Configure()
        {
            // Setup the clock and other elements
            Span<byte> toSend = stackalloc byte[12];
            int idx = 0;

            // Disable clock divide by 5 for 60Mhz master clock
            toSend[idx++] = (byte)Native.FT_OPCODE.DisableClockDivideBy5;

            // Turn off adaptive clocking
            toSend[idx++] = (byte)Native.FT_OPCODE.TurnOffAdaptiveClocking;

            // Enable 3 phase data clock, used by I2C to allow data on both clock edges
            toSend[idx++] = (byte)Native.FT_OPCODE.Enable3PhaseDataClocking;

            // Calculate clock divisor for I2C speed
            toSend[idx++] = (byte)Native.FT_OPCODE.SetClockDivisor;
            uint targetFreqKHz = (uint)BusSpeed / 1000;
            uint clockDivisor;

            // More conservative clock divisor calculation
            switch (BusSpeed)
            {
                case I2cBusSpeed.Standard: // 100kHz
                    clockDivisor = (60000 / (100 * 2)) - 1;
                    break;
                case I2cBusSpeed.Fast: // 400kHz
                    clockDivisor = (60000 / (400 * 2)) - 1;
                    break;
                case I2cBusSpeed.FastPlus: // 1MHz
                    clockDivisor = (60000 / (1000 * 2)) - 1;
                    break;
                default:
                    clockDivisor = (60000 / (100 * 2)) - 1; // Default to 100kHz
                    break;
            }

            toSend[idx++] = (byte)(clockDivisor & 0x00FF);
            toSend[idx++] = (byte)((clockDivisor >> 8) & 0x00FF);

            // loopback off
            toSend[idx++] = (byte)Native.FT_OPCODE.DisconnectTDItoTDOforLoopback;

            // Command to set directions of lower 8 pins and force value on bits set as output
            toSend[idx++] = (byte)Native.FT_OPCODE.SetDataBitsLowByte;

            // For FT232H, we need to simulate open drain by using inputs to go high
            // SDA and SCL both start as inputs (high due to pull-ups) to simulate idle state
            _expander.GpioStateLow = (byte)(PinData.SDAloSCLlo | (_expander.GpioStateLow & MaskGpio));
            _expander.GpioDirectionLow = (byte)(PinDirection.SDAinSCLin | (_expander.GpioDirectionLow & MaskGpio));

            toSend[idx++] = _expander.GpioStateLow;
            toSend[idx++] = _expander.GpioDirectionLow;

            _expander.Write(toSend.Slice(0, idx));

            // Allow time for configuration to settle
            Thread.Sleep(50);
            Idle();
        }

        internal override void Start()
        {
            // I2C Start condition: SDA high->low while SCL is high

            // Both lines high (idle state) - use inputs to simulate pull-ups
            Idle();
            PreciseDelay(5);

            // SDA goes low while SCL stays high (start condition)
            var state = (byte)(PinData.SDAloSCLlo | (_expander.GpioStateLow & MaskGpio));
            var direction = (byte)(PinDirection.SDAoutSCLin | (_expander.GpioDirectionLow & MaskGpio));
            _expander.SetGpioDirectionAndState(true, direction, state);
            PreciseDelay(5);

            // SCL goes low
            direction = (byte)(PinDirection.SDAoutSCLout | (_expander.GpioDirectionLow & MaskGpio));
            _expander.SetGpioDirectionAndState(true, direction, state);
            PreciseDelay(3);
        }

        internal override void Stop()
        {
            // I2C Stop condition: SDA low->high while SCL is high

            // Ensure both lines are in output mode and low
            var state = (byte)(PinData.SDAloSCLlo | (_expander.GpioStateLow & MaskGpio));
            var direction = (byte)(PinDirection.SDAoutSCLout | (_expander.GpioDirectionLow & MaskGpio));
            _expander.SetGpioDirectionAndState(true, direction, state);
            PreciseDelay(3);

            // SCL goes high (release to input for pull-up)
            direction = (byte)(PinDirection.SDAoutSCLin | (_expander.GpioDirectionLow & MaskGpio));
            _expander.SetGpioDirectionAndState(true, direction, state);
            PreciseDelay(5);

            // SDA goes high (release to input for pull-up) - this creates the stop condition
            direction = (byte)(PinDirection.SDAinSCLin | (_expander.GpioDirectionLow & MaskGpio));
            _expander.SetGpioDirectionAndState(true, direction, state);
            PreciseDelay(5);
        }

        private void PreciseDelay(int microseconds)
        {
            // we are using netstandard 2.1, so high-precision is not available
            var sw = System.Diagnostics.Stopwatch.StartNew();
            while (sw.ElapsedMilliseconds < 1)
            {
                Thread.Yield();
            }

            // TODO: Use high-resolution timer for better precision
            //var sw = System.Diagnostics.Stopwatch.StartNew();
            //double targetMs = microseconds / 1000.0;

            //while (sw.Elapsed.TotalMilliseconds < targetMs)
            //{
            //    // Busy wait for precision with occasional yield
            //    if (sw.Elapsed.TotalMicroseconds > microseconds / 2)
            //    {
            //        Thread.Yield();
            //    }
            //}
        }

        internal override void Idle()
        {
            // I2C idle state: both SDA and SCL high (using inputs to simulate open drain with pull-ups)
            var state = (byte)(PinData.SDAloSCLlo | (_expander.GpioStateLow & MaskGpio));
            var direction = (byte)(PinDirection.SDAinSCLin | (_expander.GpioDirectionLow & MaskGpio));
            _expander.SetGpioDirectionAndState(true, direction, state);
        }

        internal override TransferStatus SendDataByte(byte data)
        {
            Span<byte> txBuffer = stackalloc byte[13];
            Span<byte> rxBuffer = stackalloc byte[1];
            var idx = 0;

            // Clear any stale data first
            ClearInputBuffer();

            // Ensure we're in output mode for both pins initially
            _expander.GpioStateLow = (byte)(PinData.SDAloSCLlo | (_expander.GpioStateLow & MaskGpio));
            _expander.GpioDirectionLow = (byte)(PinDirection.SDAoutSCLout | (_expander.GpioDirectionLow & MaskGpio));
            txBuffer[idx++] = (byte)Native.FT_OPCODE.SetDataBitsLowByte;
            txBuffer[idx++] = _expander.GpioStateLow;
            txBuffer[idx++] = _expander.GpioDirectionLow;

            // Clock out one byte (MSB first, data changes on falling edge, sampled on rising edge)
            txBuffer[idx++] = (byte)Native.FT_OPCODE.ClockDataBytesOutOnMinusVeClockMSBFirst;
            txBuffer[idx++] = 0x00; // Length low byte (0 = 1 byte)
            txBuffer[idx++] = 0x00; // Length high byte
            txBuffer[idx++] = data; // The actual data byte

            // Release SDA for ACK bit (set to input, SCL stays output)
            _expander.GpioStateLow = (byte)(PinData.SDAloSCLlo | (_expander.GpioStateLow & MaskGpio));
            _expander.GpioDirectionLow = (byte)(PinDirection.SDAinSCLout | (_expander.GpioDirectionLow & MaskGpio));
            txBuffer[idx++] = (byte)Native.FT_OPCODE.SetDataBitsLowByte;
            txBuffer[idx++] = _expander.GpioStateLow;
            txBuffer[idx++] = _expander.GpioDirectionLow;

            // Clock in ACK bit (1 bit)
            txBuffer[idx++] = (byte)Native.FT_OPCODE.ClockDataBitsInOnPlusVeClockMSBFirst;
            txBuffer[idx++] = 0x00; // Length (0 = 1 bit)

            // Send immediate command to get response
            txBuffer[idx++] = (byte)Native.FT_OPCODE.SendImmediate;

            _expander.Write(txBuffer.Slice(0, idx));

            // Read the ACK/NACK response with timeout
            if (!ReadWithTimeout(rxBuffer, 1000))
            {
                throw new TimeoutException("Timeout waiting for ACK/NACK response");
            }

            return (rxBuffer[0] & 0x01) == 0 ? TransferStatus.Ack : TransferStatus.Nack;
        }

        internal override byte ReadDataByte(bool ackAfterRead)
        {
            int idx = 0;
            Span<byte> toSend = stackalloc byte[16];
            Span<byte> toRead = stackalloc byte[1];

            // Clear any stale data
            ClearInputBuffer();

            // Ensure SDA is input for reading, SCL is output
            _expander.GpioStateLow = (byte)(PinData.SDAloSCLlo | (_expander.GpioStateLow & MaskGpio));
            _expander.GpioDirectionLow = (byte)(PinDirection.SDAinSCLout | (_expander.GpioDirectionLow & MaskGpio));
            toSend[idx++] = (byte)Native.FT_OPCODE.SetDataBitsLowByte;
            toSend[idx++] = _expander.GpioStateLow;
            toSend[idx++] = _expander.GpioDirectionLow;

            // Clock in one byte
            toSend[idx++] = (byte)Native.FT_OPCODE.ClockDataBytesInOnPlusVeClockMSBFirst;
            toSend[idx++] = 0x00; // Length low byte (0 = 1 byte)
            toSend[idx++] = 0x00; // Length high byte

            // Switch SDA back to output for ACK/NACK
            _expander.GpioStateLow = (byte)(PinData.SDAloSCLlo | (_expander.GpioStateLow & MaskGpio));
            _expander.GpioDirectionLow = (byte)(PinDirection.SDAoutSCLout | (_expander.GpioDirectionLow & MaskGpio));
            toSend[idx++] = (byte)Native.FT_OPCODE.SetDataBitsLowByte;
            toSend[idx++] = _expander.GpioStateLow;
            toSend[idx++] = _expander.GpioDirectionLow;

            // Send ACK (0) or NACK (1)
            toSend[idx++] = (byte)Native.FT_OPCODE.ClockDataBitsOutOnMinusVeClockMSBFirst;
            toSend[idx++] = 0x00; // Length (0 = 1 bit)
            toSend[idx++] = (byte)(ackAfterRead ? 0x00 : 0xFF); // ACK=0, NACK=1

            // Release SDA back to input (idle state preparation)
            _expander.GpioStateLow = (byte)(PinData.SDAloSCLlo | (_expander.GpioStateLow & MaskGpio));
            _expander.GpioDirectionLow = (byte)(PinDirection.SDAinSCLout | (_expander.GpioDirectionLow & MaskGpio));
            toSend[idx++] = (byte)Native.FT_OPCODE.SetDataBitsLowByte;
            toSend[idx++] = _expander.GpioStateLow;
            toSend[idx++] = _expander.GpioDirectionLow;

            // Send immediate command
            toSend[idx++] = (byte)Native.FT_OPCODE.SendImmediate;

            _expander.Write(toSend.Slice(0, idx));

            if (!ReadWithTimeout(toRead, 1000))
            {
                throw new TimeoutException("Timeout reading data byte");
            }

            return toRead[0];
        }

        private bool ReadWithTimeout(Span<byte> buffer, int timeoutMs)
        {
            var start = Environment.TickCount;
            int totalRead = 0;

            while (totalRead < buffer.Length)
            {
                if ((Environment.TickCount - start) > timeoutMs)
                {
                    return false;
                }

                uint available = 0;
                Native.CheckStatus(Native.Ftd2xx.FT_GetQueueStatus(_expander.Handle, ref available));

                if (available > 0)
                {
                    int toRead = Math.Min((int)available, buffer.Length - totalRead);
                    uint read = 0;
                    Native.CheckStatus(Native.Ftd2xx.FT_Read(
                        _expander.Handle,
                        in buffer[totalRead],
                        (uint)toRead,
                        ref read));
                    totalRead += (int)read;
                }
                else
                {
                    Thread.Sleep(1);
                }
            }

            return true;
        }

        private void ClearInputBuffer()
        {
            uint available = 0;
            Native.CheckStatus(Native.Ftd2xx.FT_GetQueueStatus(_expander.Handle, ref available));

            if (available > 0)
            {
                Span<byte> clearBuffer = stackalloc byte[(int)available];
                uint read = 0;
                Native.CheckStatus(Native.Ftd2xx.FT_Read(_expander.Handle, in clearBuffer[0], available, ref read));
            }
        }
    }
}