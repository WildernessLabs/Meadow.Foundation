using FTD2XX;
using Meadow.Hardware;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Meadow.Foundation.ICs.IOExpanders;

public abstract partial class FtdiExpander
{
    /// <summary>
    /// Represents an Ft232h expander I2C bus.
    /// </summary>
    public class Ft232hI2cBus : II2cBus
    {
        private readonly FTDI _device;

        internal Ft232hI2cBus(FTDI device, int channel, I2cBusSpeed busSpeed)
        {
            _device = device;
            ConfigureMpsse();

            // TODO: figure out how to set bus speed
        }

        public I2cBusSpeed BusSpeed { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        private void ConfigureMpsse()
        {
            _device.SetTimeouts(1000, 1000).ThrowIfNotOK();
            _device.SetLatency(16).ThrowIfNotOK();
            _device.SetFlowControl(FT_FLOW_CONTROL.FT_FLOW_RTS_CTS, 0x00, 0x00).ThrowIfNotOK();
            _device.SetBitMode(0x00, 0x00).ThrowIfNotOK(); // RESET
            _device.SetBitMode(0x00, 0x02).ThrowIfNotOK(); // MPSSE

            _device.FlushBuffer();

            /***** Synchronize the MPSSE interface by sending bad command 0xAA *****/
            _device.Write(new byte[] { 0xAA }).ThrowIfNotOK();
            byte[] rx1 = _device.ReadBytes(2, out FT_STATUS status1);
            status1.ThrowIfNotOK();
            if ((rx1[0] != 0xFA) || (rx1[1] != 0xAA))
                throw new InvalidOperationException($"bad echo bytes: {rx1[0]} {rx1[1]}");

            /***** Synchronize the MPSSE interface by sending bad command 0xAB *****/
            _device.Write(new byte[] { 0xAB }).ThrowIfNotOK();
            byte[] rx2 = _device.ReadBytes(2, out FT_STATUS status2);
            status2.ThrowIfNotOK();
            if ((rx2[0] != 0xFA) || (rx2[1] != 0xAB))
                throw new InvalidOperationException($"bad echo bytes: {rx2[0]} {rx2[1]}");

            const uint ClockDivisor = 199; //49 for 200 KHz, 199 for 100 KHz
            const byte I2C_Data_SDAhi_SCLhi = 0x03;
            const byte I2C_Dir_SDAout_SCLout = 0x03;

            int numBytesToSend = 0;
            byte[] buffer = new byte[100];
            buffer[numBytesToSend++] = 0x8A;   // Disable clock divide by 5 for 60Mhz master clock
            buffer[numBytesToSend++] = 0x97;   // Turn off adaptive clocking
            buffer[numBytesToSend++] = 0x8C;   // Enable 3 phase data clock, used by I2C to allow data on both clock edges
                                               // The SK clock frequency can be worked out by below algorithm with divide by 5 set as off
                                               // SK frequency  = 60MHz /((1 +  [(1 +0xValueH*256) OR 0xValueL])*2)
            buffer[numBytesToSend++] = 0x86;   //Command to set clock divisor
            buffer[numBytesToSend++] = (byte)(ClockDivisor & 0x00FF);  //Set 0xValueL of clock divisor
            buffer[numBytesToSend++] = (byte)((ClockDivisor >> 8) & 0x00FF);   //Set 0xValueH of clock divisor
            buffer[numBytesToSend++] = 0x85;           // loopback off

            buffer[numBytesToSend++] = 0x9E;       //Enable the FT232H's drive-zero mode with the following enable mask...
            buffer[numBytesToSend++] = 0x07;       // ... Low byte (ADx) enables - bits 0, 1 and 2 and ... 
            buffer[numBytesToSend++] = 0x00;       //...High byte (ACx) enables - all off

            buffer[numBytesToSend++] = 0x80;   //Command to set directions of lower 8 pins and force value on bits set as output 
            buffer[numBytesToSend++] = I2C_Data_SDAhi_SCLhi;
            buffer[numBytesToSend++] = I2C_Dir_SDAout_SCLout;

            byte[] msg = buffer.Take(numBytesToSend).ToArray();
            _device.Write(msg).ThrowIfNotOK();
        }

        private const byte I2C_Data_SDAlo_SCLlo = 0x00;
        private const byte I2C_Data_SDAlo_SCLhi = 0x01;
        private const byte I2C_Data_SDAhi_SCLlo = 0x02;
        private const byte I2C_Data_SDAhi_SCLhi = 0x03;
        private const byte I2C_ADbus = 0x80;
        private const byte I2C_Dir_SDAout_SCLout = 0x03;

        private void Start()
        {
            List<byte> bytes = new();

            for (int i = 0; i < 6; i++)
                bytes.AddRange(new byte[] { I2C_ADbus, I2C_Data_SDAhi_SCLhi, I2C_Dir_SDAout_SCLout, });

            for (int i = 0; i < 6; i++)
                bytes.AddRange(new byte[] { I2C_ADbus, I2C_Data_SDAlo_SCLhi, I2C_Dir_SDAout_SCLout, });

            for (int i = 0; i < 6; i++)
                bytes.AddRange(new byte[] { I2C_ADbus, I2C_Data_SDAlo_SCLlo, I2C_Dir_SDAout_SCLout, });

            bytes.AddRange(new byte[] { I2C_ADbus, I2C_Data_SDAhi_SCLlo, I2C_Dir_SDAout_SCLout, });

            _device.Write(bytes.ToArray()).ThrowIfNotOK();
        }

        private void Stop()
        {
            List<byte> bytes = new();

            for (int i = 0; i < 6; i++)
                bytes.AddRange(new byte[] { I2C_ADbus, I2C_Data_SDAlo_SCLlo, I2C_Dir_SDAout_SCLout, });

            for (int i = 0; i < 6; i++)
                bytes.AddRange(new byte[] { I2C_ADbus, I2C_Data_SDAlo_SCLhi, I2C_Dir_SDAout_SCLout, });

            for (int i = 0; i < 6; i++)
                bytes.AddRange(new byte[] { I2C_ADbus, I2C_Data_SDAhi_SCLhi, I2C_Dir_SDAout_SCLout, });

            _device.Write(bytes.ToArray()).ThrowIfNotOK();
        }

        private bool CommandWrite(byte address)
        {
            address <<= 1;
            return SendDataByte(address);
        }

        private bool CommandRead(byte address)
        {
            address <<= 1;
            address |= 0x01;
            return SendDataByte(address);
        }

        private bool SendDataByte(byte byteToSend)
        {
            const byte I2C_Data_SDAhi_SCLlo = 0x02;
            const byte MSB_FALLING_EDGE_CLOCK_BYTE_OUT = 0x11;
            const byte I2C_Dir_SDAout_SCLout = 0x03;
            const byte MSB_RISING_EDGE_CLOCK_BIT_IN = 0x22;

            byte[] buffer = new byte[100];
            int bytesToSend = 0;
            buffer[bytesToSend++] = MSB_FALLING_EDGE_CLOCK_BYTE_OUT;        // clock data byte out
            buffer[bytesToSend++] = 0x00;                                   // 
            buffer[bytesToSend++] = 0x00;                                   //  Data length of 0x0000 means 1 byte data to clock in
            buffer[bytesToSend++] = byteToSend;                             //  Byte to send

            // Put line back to idle (data released, clock pulled low)
            buffer[bytesToSend++] = 0x80;                                   // Command - set low byte
            buffer[bytesToSend++] = I2C_Data_SDAhi_SCLlo;                   // Set the values
            buffer[bytesToSend++] = I2C_Dir_SDAout_SCLout;                  // Set the directions

            // CLOCK IN ACK
            buffer[bytesToSend++] = MSB_RISING_EDGE_CLOCK_BIT_IN;           // clock data bits in
            buffer[bytesToSend++] = 0x00;                                   // Length of 0 means 1 bit

            // This command then tells the MPSSE to send any results gathered (in this case the ack bit) back immediately
            buffer[bytesToSend++] = 0x87;

            // send commands to chip
            byte[] msg = buffer.Take(bytesToSend).ToArray();
            _device.Write(msg).ThrowIfNotOK();

            byte[] rx1 = _device.ReadBytes(1, out FT_STATUS status);
            status.ThrowIfNotOK();

            // if ack bit is 0 then sensor acked the transfer, otherwise it nak'd the transfer
            bool ack = (rx1[0] & 0x01) == 0;

            return ack;
        }

        private byte ReadDataByte(bool ACK = true)
        {
            const byte MSB_RISING_EDGE_CLOCK_BYTE_IN = 0x20;
            const byte MSB_FALLING_EDGE_CLOCK_BIT_OUT = 0x13;
            const byte I2C_Data_SDAhi_SCLlo = 0x02;
            const byte I2C_Dir_SDAout_SCLout = 0x03;
            int bytesToSend = 0;

            // Clock in one data byte
            byte[] buffer = new byte[100];
            buffer[bytesToSend++] = MSB_RISING_EDGE_CLOCK_BYTE_IN;      // Clock data byte in
            buffer[bytesToSend++] = 0x00;
            buffer[bytesToSend++] = 0x00;                               // Data length of 0x0000 means 1 byte data to clock in

            // clock out one bit as ack/nak bit
            buffer[bytesToSend++] = MSB_FALLING_EDGE_CLOCK_BIT_OUT;     // Clock data bit out
            buffer[bytesToSend++] = 0x00;                               // Length of 0 means 1 bit
            if (ACK == true)
                buffer[bytesToSend++] = 0x00;                           // Data bit to send is a '0'
            else
                buffer[bytesToSend++] = 0xFF;                           // Data bit to send is a '1'

            // I2C lines back to idle state 
            buffer[bytesToSend++] = 0x80;                               //       ' Command - set low byte
            buffer[bytesToSend++] = I2C_Data_SDAhi_SCLlo;                            //      ' Set the values
            buffer[bytesToSend++] = I2C_Dir_SDAout_SCLout;                             //     ' Set the directions

            // This command then tells the MPSSE to send any results gathered back immediately
            buffer[bytesToSend++] = 0x87;                                  //    ' Send answer back immediate command

            // send commands to chip
            byte[] msg = buffer.Take(bytesToSend).ToArray();
            _device.Write(msg).ThrowIfNotOK();

            // get the byte which has been read from the driver's receive buffer
            byte[] readBuffer = new byte[1];
            int bytesRead = 0;
            _device.Read(readBuffer, 1, ref bytesRead).ThrowIfNotOK();

            return readBuffer[0];
        }

        public void Dispose()
        {
            _device.Close();
        }

        public void Exchange(byte peripheralAddress, Span<byte> writeBuffer, Span<byte> readBuffer)
        {
            Start();
            CommandWrite(peripheralAddress);
            for (int i = 0; i < writeBuffer.Length; i++)
            {
                SendDataByte(writeBuffer[i]);
            }

            Start();

            CommandRead(peripheralAddress);
            for (int i = 0; i < readBuffer.Length; i++)
            {
                readBuffer[i] = ReadDataByte(ACK: true);
            }

            Stop();
        }

        public void Read(byte peripheralAddress, Span<byte> readBuffer)
        {
            Start();

            CommandRead(peripheralAddress);

            for (int i = 0; i < readBuffer.Length; i++)
            {
                readBuffer[i] = ReadDataByte(ACK: true);
            }

            Stop();
        }

        public void Write(byte peripheralAddress, Span<byte> writeBuffer)
        {
            bool[] ack = new bool[writeBuffer.Length + 1];

            Start();
            ack[0] = CommandWrite(peripheralAddress);
            for (int i = 0; i < writeBuffer.Length; i++)
            {
                ack[i + 1] = SendDataByte(writeBuffer[i]);
            }
            Stop();

            if (ack.Where(x => x == false).Any())
            {
                Debug.WriteLine("WARNING: not all writes were ACK'd");
                Debug.WriteLine(string.Join(",", ack.Select(x => x.ToString())));
            }
        }
    }
}