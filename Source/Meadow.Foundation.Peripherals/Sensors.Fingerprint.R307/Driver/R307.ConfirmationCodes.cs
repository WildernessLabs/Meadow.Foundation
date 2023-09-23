using System;

namespace Meadow.Foundation.Sensors.Fingerprint
{
    public partial class R307
    {
        private class CommandPacket : Packet
        {
            public CommandPacket(CommandCode command, ReadOnlySpan<byte> data, uint address = 0xffffffff)
                : base(command, data, address)
            {

            }

            public CommandPacket(CommandCode command, byte[] data, uint address = 0xffffffff)
                : base(command, data, address)

            {
            }
        }

        private abstract class Packet
        {
            private byte[] _data;

            private const int OffsetHeader = 0x00;
            private const int OffsetAddress = 0x02;
            private const int OffsetPacketID = 0x06;
            private const int OffsetPacketLength = 0x07;
            private const int OffsetPayload = 0x09;

            public Packet(CommandCode command, ReadOnlySpan<byte> data, uint address = 0xffffffff)
            {
                _data = new byte[11 + data.Length];

                _data[0] = 0xEF; // magic number per data sheet
                _data[1] = 0x01; // magic number per data sheet

                _data[2] = (byte)((address >> 24) & 0xff);
                _data[3] = (byte)((address >> 16) & 0xff);
                _data[4] = (byte)((address >> 8) & 0xff);
                _data[5] = (byte)((address >> 0) & 0xff);

                FillCRC(_data);
            }

            public Packet(CommandCode command, byte[] data, uint address = 0xffffffff)
            {
                _data = new byte[11 + data.Length];

                _data[0] = 0xEF; // magic number per data sheet
                _data[1] = 0x01; // magic number per data sheet

                _data[2] = (byte)((address >> 24) & 0xff);
                _data[3] = (byte)((address >> 16) & 0xff);
                _data[4] = (byte)((address >> 8) & 0xff);
                _data[5] = (byte)((address >> 0) & 0xff);

                FillCRC(_data);
            }

            public byte[] Serialize()
            {
                return _data;
            }

            private static ushort Crc(byte[] data, int index, int count)
            {
                ushort crc = 0xFFFF;
                char lsb;

                for (int i = index; i < count; i++)
                {
                    crc = (ushort)(crc ^ data[i]);

                    for (int j = 0; j < 8; j++)
                    {
                        lsb = (char)(crc & 0x0001);
                        crc = (ushort)((crc >> 1) & 0x7fff);

                        if (lsb == 1)
                            crc = (ushort)(crc ^ 0xa001);
                    }
                }

                return crc;
            }

            private static void FillCRC(byte[] message)
            {
                var crc = Crc(message, 0, message.Length - 2);

                // fill in the CRC (last 2 bytes) - big-endian
                message[message.Length - 1] = (byte)((crc >> 8) & 0xff);
                message[message.Length - 2] = (byte)(crc & 0xff);
            }
        }

        private enum PacketIdentifier
        {
            Command = 0x01,
            Data = 0x02,
            Ack = 0x07,
            EndOfTransfer = 0x08
        }

        private enum ConfirmationCode : byte
        {
            ExecutionComplete = 0x00,
            Error = 0x01,
            NoFinger = 0x02,
            FailedToEnroll = 0x03,
            DisorderedImage = 0x04,
            WetImage = 0x05,
            DisorderedImage2 = 0x06, // 2 values, same description in the data sheet
            LackOfPoints = 0x07,
            FingerDoesNotMatch = 0x08,
            FailedToFindMatch = 0x09

        }

        private enum CommandCode : byte
        {
            GenImg = 0x01,
            Img2Tz = 0x02,
            Match = 0x03,
            Search = 0x04,
            RegModel = 0x05,
            Store = 0x06,
            LoadChar = 0x07,
            UploadChar = 0x08,
            DownloadChar = 0x09,
            UploadImage = 0x0A,
            DownloadImage = 0x0B,
            DeleteTemplate = 0x0C,
            EmptyLibrary = 0x0D,
            SetSystemParam = 0x0E,
            GetSystemParameter = 0x0F,
            SetPassword = 0x12,
            VerifyPassword = 0x013,
            GetRandowmCode = 0x014,
            SetAddress = 0x015,
            PortControl = 0x17,
            WriteNotepad = 0x18,
            ReadNotepad = 0x19,
            FastSearch = 0x1B,
            ReadTemplateNumbers = 0x1D
        }
    }

}