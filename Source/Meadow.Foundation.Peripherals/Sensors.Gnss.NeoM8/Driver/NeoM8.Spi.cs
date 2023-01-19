using Meadow.Hardware;
using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Meadow.Foundation.Sensors.Gnss
{
    public partial class NeoM8
    {
        readonly ISpiPeripheral spiPeripheral;

        SerialMessageBuffer messageBuffer;

        /// <summary>
        /// Create a new NEOM8 object using SPI
        /// </summary>
        public NeoM8(ISpiBus spiBus, IDigitalOutputPort chipSelectPort, IDigitalOutputPort resetPort = null)
        {
            ResetPort = resetPort;
            spiPeripheral = new SpiPeripheral(spiBus, chipSelectPort);

            messageBuffer = new SerialMessageBuffer(suffixDelimiter: Encoding.ASCII.GetBytes("\r\n"),
                                                    preserveDelimiter: true,
                                                    readBufferSize: 512);

            _ = InitializeSpi();
        }

        //ToDo cancellation for sleep aware 
        async Task InitializeSpi()
        {
            messageBuffer.MessageReceived += MessageReceived;

            InitDecoders();

            await Reset();

            Resolver.Log.Debug("Finish NeoM8 SPI initialization");
        }

        async Task StartUpdatingSpi()
        { 
            byte[] data = new byte[128]; //TODO make consts

            await Task.Run(() =>
            {
                while (true)
                {
                    spiPeripheral.Read(data);
                    messageBuffer.AddData(data);

                    Thread.Sleep(200); //TODO make consts
                }
            });
        }
    }
}