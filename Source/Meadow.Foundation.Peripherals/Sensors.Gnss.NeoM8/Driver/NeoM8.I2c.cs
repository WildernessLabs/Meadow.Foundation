using Meadow.Hardware;
using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Meadow.Foundation.Sensors.Gnss
{
    public partial class NeoM8
    {
        I2cPeripheral i2CPeripheral;

        readonly Memory<byte> i2cBuffer = new byte[BUFFER_SIZE];

        IDigitalOutputPort resetPort;
        IDigitalInputPort ppsPort;

        /// <summary>
        /// Create a new NeoM8 object using I2C
        /// </summary>
        public NeoM8(II2cBus i2cBus, byte address = (byte)Addresses.Default, IPin resetPin = null, IPin ppsPin = null)  
        {
            if(resetPin != null)
            {
                resetPort = resetPin.CreateDigitalOutputPort(true);
            }

            if(ppsPin != null)
            {
                ppsPort = ppsPin.CreateDigitalInputPort(InterruptMode.EdgeRising, ResistorMode.InternalPullDown);
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

        async Task InitializeI2c(II2cBus i2cBus, byte address)
        {
            i2CPeripheral = new I2cPeripheral(i2cBus, address, 128);

            messageProcessor = new SerialMessageProcessor(suffixDelimiter: Encoding.ASCII.GetBytes("\r\n"),
                                        preserveDelimiter: true,
                                        readBufferSize: 512);

            communicationMode = CommunicationMode.I2C;
            messageProcessor.MessageReceived += MessageReceived;

            InitDecoders();

            await Reset();
        }

        async Task StartUpdatingI2c()
        {
            await Task.Run(() =>
            {
                int len;

                while (true)
                {
                    len = i2CPeripheral.ReadRegisterAsUShort(0xFD, ByteOrder.BigEndian);

                    if(len > 0)
                    {
                        if(len > 0)
                        {
                            var data = i2cBuffer.Slice(0, Math.Min(len, BUFFER_SIZE)).Span;

                            i2CPeripheral.ReadRegister(0xFF, data);
                            messageProcessor.Process(data.ToArray());
                        }
                    }
                    Thread.Sleep(COMMS_SLEEP_MS);
                }
            });
        }
    }
}