using Meadow.Hardware;
using System;

namespace Meadow.Foundation.Sensors.Gnss
{
    /// <summary>
    /// A serial message buffer class to accept data and raise notifcations when complete messages are received
    /// </summary>
    public class SerialMessageBuffer
    {
        /// <summary>
        /// The buffer size, in bytes
        /// </summary>
        public virtual int ReceiveBufferSize { get; protected set; } = 128;

        /// <summary>
        /// Optional null data value 
        /// Value will be removed from data before being added to the buffer
        /// </summary>
        public byte? NullDataValue { get; set; }

        /// <summary>
        /// Raised when a message, as defined in the constructor, arrives.
        /// </summary>
        public event EventHandler<SerialMessageData> MessageReceived = delegate { };

        protected SerialMessageMode messageMode;
        protected byte[] messageDelimiterTokens;
        protected int messageLength;
        protected bool preserveDelimiter;

        protected CircularBuffer<byte> readBuffer;
        protected object msgParseLock = new object();

        /// <summary>
        /// Create a new SerialMessageBuffer using a suffix delimiter
        /// </summary>
        /// <param name="readBufferSize"></param>
        /// <param name="suffixDelimiter"></param>
        /// <param name="preserveDelimiter"></param>
        public SerialMessageBuffer(
            int readBufferSize,
            byte[] suffixDelimiter,
            bool preserveDelimiter)
        {
            this.messageMode = SerialMessageMode.SuffixDelimited;
            this.preserveDelimiter = preserveDelimiter;
            this.messageDelimiterTokens = suffixDelimiter;

            readBuffer = new CircularBuffer<byte>(ReceiveBufferSize = readBufferSize);
        }

        /// <summary>
        /// Create a new SerialMessageBuffer using a prefix delimiter
        /// </summary>
        /// <param name="readBufferSize"></param>
        /// <param name="prefixDelimiter"></param>
        /// <param name="preserveDelimiter"></param>
        /// <param name="messageLength"></param>
        public SerialMessageBuffer(
            int readBufferSize,
            byte[] prefixDelimiter,
            bool preserveDelimiter,
            int messageLength)
        {
            this.messageMode = SerialMessageMode.PrefixDelimited;
            this.preserveDelimiter = preserveDelimiter;
            this.messageDelimiterTokens = prefixDelimiter;
            this.messageLength = messageLength;

            readBuffer = new CircularBuffer<byte>(ReceiveBufferSize = readBufferSize);
        }

        /// <summary>
        /// Add data to the buffer to be parsed
        /// </summary>
        public void AddData(byte[] data)
        {
            if (readBuffer == null) return;

            lock (msgParseLock)
            {
                AddDataAndFilterIgnoreValue(data);

                if (messageMode == SerialMessageMode.PrefixDelimited)
                {
                    ProcessPrefixDelimited();
                }
                else
                {
                    ProcessSuffixDelimited();
                }
            }
        }

        void AddDataAndFilterIgnoreValue(byte[] data)
        {
            if(NullDataValue != null)
            {
                for(int i = 0; i < data.Length; i++)
                {
                    if (data[i] != NullDataValue)
                    {
                        readBuffer.Append(data[i]);
                    }
                }
            }
            else
            {
                readBuffer.Append(data);
            }
        }

        void ProcessPrefixDelimited()
        {
            int firstIndex = readBuffer.FirstIndexOf(messageDelimiterTokens);

            while (firstIndex >= 0)
            {
                int totalMsgLength = messageDelimiterTokens.Length + messageLength;
                int returnMsgLength = (preserveDelimiter ? totalMsgLength : messageLength);

                byte[] msg = new byte[returnMsgLength];

                for (int i = 0; i < firstIndex; i++)
                {
                    readBuffer.Remove();
                }

                if (preserveDelimiter)
                {
                    for (int i = 0; i < totalMsgLength; i++)
                    {
                        msg[i] = readBuffer.Remove();
                    }
                }
                else
                {
                    for (int i = 0; i < messageDelimiterTokens.Length; i++)
                    {
                        readBuffer.Remove();
                    }
                    for (int i = 0; i < returnMsgLength; i++)
                    {
                        msg[i] = readBuffer.Remove();
                    }
                }

                RaiseMessageReceivedAndNotify(new SerialMessageData() { Message = msg });

                firstIndex = readBuffer.FirstIndexOf(messageDelimiterTokens);
            }
        }


        void ProcessSuffixDelimited()
        {
            var firstIndex = readBuffer.FirstIndexOf(messageDelimiterTokens);

            while (firstIndex >= 0)
            {
                var bytesToRemove = firstIndex + messageDelimiterTokens.Length;
                byte[] msg = new byte[(preserveDelimiter ? bytesToRemove : (bytesToRemove - messageDelimiterTokens.Length))];

                for (int i = 0; i < firstIndex; i++)
                {
                    msg[i] = readBuffer.Remove();
                }

                for (int i = firstIndex; i < bytesToRemove; i++)
                {
                    if (preserveDelimiter)
                    {
                        msg[i] = readBuffer.Remove();
                    }
                    else
                    {
                        readBuffer.Remove();
                    }
                }

                RaiseMessageReceivedAndNotify(new SerialMessageData() { Message = msg });

                firstIndex = readBuffer.FirstIndexOf(messageDelimiterTokens);
            }
        }

        /// <summary>
        /// Raise message received notifcation to subscribers
        /// </summary>
        /// <param name="messageData"></param>
        protected void RaiseMessageReceivedAndNotify(SerialMessageData messageData)
        {
            MessageReceived?.Invoke(this, messageData);
        }

        /// <summary>
        /// Whether we're defining messages by prefix + length, or suffix.
        /// </summary>
        protected enum SerialMessageMode
        {
            /// <summary>
            /// Data is prefix delimited
            /// </summary>
            PrefixDelimited,
            /// <summary>
            /// Data is suffix delimited
            /// </summary>
            SuffixDelimited
        }
    }
}