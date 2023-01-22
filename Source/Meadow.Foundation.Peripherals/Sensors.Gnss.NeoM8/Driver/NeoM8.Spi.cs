using Meadow.Hardware;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Meadow.Foundation.Sensors.Gnss
{
    public partial class NeoM8
    {
        readonly ISpiPeripheral spiPeripheral;
        readonly SerialMessageProcessor messageProcessor;

        const byte NULL_VALUE = 0xFF;
        const byte BUFFER_SIZE = 128;
        const byte SPI_SLEEP_MS = 200;

        /// <summary>
        /// Create a new NEOM8 object using SPI
        /// </summary>
        public NeoM8(ISpiBus spiBus, IDigitalOutputPort chipSelectPort, IDigitalOutputPort resetPort = null)
        {
            ResetPort = resetPort;
            spiPeripheral = new SpiPeripheral(spiBus, chipSelectPort);

            messageProcessor = new SerialMessageProcessor(suffixDelimiter: Encoding.ASCII.GetBytes("\r\n"),
                                                    preserveDelimiter: true,
                                                    readBufferSize: 512);

            _ = InitializeSpi();
        }

        //ToDo cancellation for sleep aware 
        async Task InitializeSpi()
        {
            messageProcessor.MessageReceived += MessageReceived;

            InitDecoders();

            await Reset();

            Resolver.Log.Debug("Finish NeoM8 SPI initialization");
        }

        async Task StartUpdatingSpi()
        { 
            byte[] data = new byte[BUFFER_SIZE];

            bool HasMoreData(byte[] data)
            {
                bool hasNullValue = false;
                for(int i = 1; i < data.Length; i++)
                {
                    if (data[i] == NULL_VALUE) { hasNullValue = true; }
                    if (data[i - 1] == NULL_VALUE && data[i] != NULL_VALUE)
                    {
                        return true;
                    }
                }
                return !hasNullValue;
            }

            await Task.Run(() =>
            {
                while (true)
                {
                    spiPeripheral.Read(data);
                    messageProcessor.Process(data);

                    if(HasMoreData(data) == false)
                    {
                        Thread.Sleep(SPI_SLEEP_MS);
                    }
                }
            });
        }
    }
}