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

            // TODO: figure out how to set bus speed
        }

        public I2cBusSpeed BusSpeed { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        private void Start()
        {
            List<byte> bytes = new();

            const byte I2C_Data_SDAlo_SCLlo = 0x00;
            const byte I2C_Data_SDAlo_SCLhi = 0x01;
            const byte I2C_Data_SDAhi_SCLlo = 0x02;
            const byte I2C_Data_SDAhi_SCLhi = 0x03;

            const byte I2C_ADbus = 0x80;
            const byte I2C_Dir_SDAout_SCLout = 0x03;

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

            const byte I2C_Data_SDAlo_SCLlo = 0x00;
            const byte I2C_Data_SDAlo_SCLhi = 0x01;
            const byte I2C_Data_SDAhi_SCLhi = 0x03;

            const byte I2C_ADbus = 0x80;
            const byte I2C_Dir_SDAout_SCLout = 0x03;

            for (int i = 0; i < 6; i++)
                bytes.AddRange(new byte[] { I2C_ADbus, I2C_Data_SDAlo_SCLlo, I2C_Dir_SDAout_SCLout, });

            for (int i = 0; i < 6; i++)
                bytes.AddRange(new byte[] { I2C_ADbus, I2C_Data_SDAlo_SCLhi, I2C_Dir_SDAout_SCLout, });

            for (int i = 0; i < 6; i++)
                bytes.AddRange(new byte[] { I2C_ADbus, I2C_Data_SDAhi_SCLhi, I2C_Dir_SDAout_SCLout, });

            _device.Write(bytes.ToArray()).ThrowIfNotOK();
        }

        private bool FTDI_CommandWrite(byte address)
        {
            address <<= 1;
            return SendDataByte(address);
        }

        private bool FTDI_CommandRead(byte address)
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
            FTDI_CommandWrite(peripheralAddress);
            for (int i = 0; i < writeBuffer.Length; i++)
            {
                SendDataByte(writeBuffer[i]);
            }

            FTDI_CommandRead(peripheralAddress);
            for (int i = 0; i < readBuffer.Length; i++)
            {
                readBuffer[i] = ReadDataByte(ACK: true);
            }

            Stop();
        }

        public void Read(byte peripheralAddress, Span<byte> readBuffer)
        {
            Start();

            FTDI_CommandRead(peripheralAddress);

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
            ack[0] = FTDI_CommandWrite(peripheralAddress);
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