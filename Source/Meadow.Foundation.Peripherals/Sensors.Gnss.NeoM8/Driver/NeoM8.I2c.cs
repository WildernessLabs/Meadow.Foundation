using Meadow.Hardware;
using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Meadow.Foundation.Sensors.Gnss
{
    public partial class NeoM8 : II2cPeripheral
    {
        /// <summary>
        /// The default I2C address for the peripheral
        /// </summary>
        public byte DefaultI2cAddress => (byte)Addresses.Default;

        /// <summary>
        /// I2C Communication bus used to communicate with the peripheral
        /// </summary>
        protected II2cCommunications i2cComms;
        private readonly Memory<byte> i2cBuffer = new byte[BUFFER_SIZE];
        private readonly IDigitalOutputPort resetPort;
        private readonly IDigitalInputPort ppsPort;

        /// <summary>
        /// Create a new NeoM8 object using I2C
        /// </summary>
        public NeoM8(II2cBus i2cBus, byte address = (byte)Addresses.Default, IPin resetPin = null, IPin ppsPin = null)
        {
            if (resetPin != null)
            {
                resetPort = resetPin.CreateDigitalOutputPort(true);
            }

            if (ppsPin != null)
            {
                ppsPort = ppsPin.CreateDigitalInterruptPort(InterruptMode.EdgeRising, ResistorMode.InternalPullDown);
            }

            _ = InitializeI2c(i2cBus, address);
        }

        /// <summary>
        /// Create a new NeoM8 object using I2C
        /// </summary>
        public NeoM8(II2cBus i2cBus, byte address = (byte)Addresses.Default, IDigitalOutputPort resetPort = null, IDigitalInputPort ppsPort = null)
        {
            ResetPort = resetPort;
            PulsePerSecondPort = ppsPort;

            _ = InitializeI2c(i2cBus, address);
        }

        private async Task InitializeI2c(II2cBus i2cBus, byte address)
        {
            i2cComms = new I2cCommunications(i2cBus, address, 128);

            messageProcessor = new SerialMessageProcessor(suffixDelimiter: Encoding.ASCII.GetBytes("\r\n"),
                                        preserveDelimiter: true,
                                        readBufferSize: 512);

            communicationMode = CommunicationMode.I2C;
            messageProcessor.MessageReceived += MessageReceived;

            InitDecoders();

            await Reset();
        }

        private async Task StartUpdatingI2c()
        {
            var t = new Task(() =>
            {
                int len;

                while (true)
                {
                    len = i2cComms.ReadRegisterAsUShort(0xFD, ByteOrder.BigEndian);

                    if (len > 0)
                    {
                        if (len > 0)
                        {
                            var data = i2cBuffer.Slice(0, Math.Min(len, BUFFER_SIZE)).Span;

                            i2cComms.ReadRegister(0xFF, data);
                            messageProcessor.Process(data.ToArray());
                        }
                    }
                    Thread.Sleep(COMMS_SLEEP_MS);
                }
            }, TaskCreationOptions.LongRunning);
            t.Start();
            await t;
        }
    }
}