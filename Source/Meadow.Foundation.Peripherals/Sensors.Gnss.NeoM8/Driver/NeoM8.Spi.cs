using Meadow.Hardware;
using Meadow.Units;
using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Meadow.Foundation.Sensors.Gnss
{
    public partial class NeoM8 : ISpiPeripheral
    {
        /// <summary>
        /// The default SPI bus speed for the device
        /// </summary>
        public Frequency DefaultSpiBusSpeed => new Frequency(375, Frequency.UnitType.Kilohertz);

        /// <summary>
        /// The SPI bus speed for the device
        /// </summary>
        public Frequency SpiBusSpeed
        {
            get => spiComms!.BusSpeed;
            set => spiComms!.BusSpeed = value;
        }

        /// <summary>
        /// The default SPI bus mode for the device
        /// </summary>
        public SpiClockConfiguration.Mode DefaultSpiBusMode => SpiClockConfiguration.Mode.Mode0;

        /// <summary>
        /// The SPI bus mode for the device
        /// </summary>
        public SpiClockConfiguration.Mode SpiBusMode
        {
            get => spiComms!.BusMode;
            set => spiComms!.BusMode = value;
        }

        /// <summary>
        /// SPI Communication bus used to communicate with the peripheral
        /// </summary>
        protected ISpiCommunications? spiComms;

        private IDigitalOutputPort? chipSelectPort;

        private const byte NULL_VALUE = 0xFF;

        /// <summary>
        /// Create a new NEOM8 object using SPI
        /// </summary>
        public NeoM8(ISpiBus spiBus,
            IDigitalOutputPort chipSelectPort,
            IDigitalOutputPort? resetPort = null,
            IDigitalInputPort? ppsPort = null)
        {
            ResetPort = resetPort;
            PulsePerSecondPort = ppsPort;

            spiComms = new SpiCommunications(spiBus, this.chipSelectPort = chipSelectPort, DefaultSpiBusSpeed, DefaultSpiBusMode);

            _ = InitializeSpi();
        }

        /// <summary>
        /// Create a new NeoM8 object using SPI
        /// </summary>
        public NeoM8(ISpiBus spiBus, IPin? chipSelectPin = null, IPin? resetPin = null, IPin? ppsPin = null)
        {
            createdPorts = true;

            var chipSelectPort = chipSelectPin?.CreateDigitalOutputPort();

            spiComms = new SpiCommunications(spiBus, chipSelectPort, DefaultSpiBusSpeed, DefaultSpiBusMode);

            resetPort = resetPin?.CreateDigitalOutputPort(true);

            ppsPort = ppsPin?.CreateDigitalInterruptPort(InterruptMode.EdgeRising, ResistorMode.InternalPullDown);

            _ = InitializeSpi();
        }

        //ToDo cancellation for sleep aware 
        private async Task InitializeSpi()
        {
            messageProcessor = new SerialMessageProcessor(suffixDelimiter: Encoding.ASCII.GetBytes("\r\n"),
                                                    preserveDelimiter: true,
                                                    readBufferSize: 512);

            communicationMode = CommunicationMode.SPI;
            messageProcessor.MessageReceived += MessageReceived;

            InitDecoders();

            await Reset();

            // Send initialization commands to configure NMEA output
            await SendSpiInitializationCommands();
        }

        private async Task StartUpdatingSpi()
        {
            cts = new CancellationTokenSource();

            var t = new Task(() =>
            {
                while (cts.Token.IsCancellationRequested == false)
                {
                    try
                    {
                        // Check if data is available by reading the status registers first
                        var availableBytesHigh = ReadRegister(Registers.BytesAvailableHigh);
                        var availableBytesLow = ReadRegister(Registers.BytesAvailableLow);
                        var availableBytes = (availableBytesHigh << 8) | availableBytesLow;

                        if (availableBytes > 0)
                        {
                            // Read data from the data stream register
                            var bytesToRead = Math.Min(availableBytes, BUFFER_SIZE);
                            var readData = ReadDataStream(bytesToRead);

                            if (readData[0] != 0xFF)
                            {
                                messageProcessor!.Process(readData);
                            }
                        }
                        else
                        {
                            Thread.Sleep(COMMS_SLEEP_MS);
                        }
                    }
                    catch (Exception ex)
                    {
                        Resolver.Log.Error($"Error reading SPI data: {ex.Message}");
                        Thread.Sleep(COMMS_SLEEP_MS);
                    }
                }
            }, TaskCreationOptions.LongRunning);
            t.Start();
            await t;
        }

        private void StopUpdatingSpi()
        {
            cts?.Cancel();
        }

        /// <summary>
        /// Read a single register from the NeoM8 via SPI
        /// </summary>
        /// <param name="register">The register to read</param>
        /// <returns>The register value</returns>
        private byte ReadRegister(Registers register)
        {
            var writeBuffer = new byte[] { (byte)register };
            var readBuffer = new byte[1];

            spiComms!.Exchange(writeBuffer, readBuffer);

            return readBuffer[0];
        }

        /// <summary>
        /// Read data from the data stream register
        /// </summary>
        /// <param name="length">Number of bytes to read</param>
        /// <returns>The data read from the stream</returns>
        private byte[] ReadDataStream(int length)
        {
            var writeBuffer = new byte[length + 1];
            var readBuffer = new byte[length + 1];

            // First byte is the register address for data stream
            writeBuffer[0] = (byte)Registers.DataStream;

            spiComms!.Exchange(writeBuffer, readBuffer);

            // Skip the first byte (register address response) and return actual data
            var data = new byte[length];
            Array.Copy(readBuffer, 1, data, 0, length);

            return data;
        }

        /// <summary>
        /// Send initialization commands via SPI to configure NMEA output
        /// </summary>
        private async Task SendSpiInitializationCommands()
        {
            try
            {
                // Convert command strings to bytes and send via SPI
                await SendSpiCommand(Commands.PMTK_SET_NMEA_OUTPUT_ALLDATA);
                await Task.Delay(100); // Small delay between commands

                await SendSpiCommand(Commands.PMTK_Q_RELEASE);
                await Task.Delay(100);

                await SendSpiCommand(Commands.PGCMD_ANTENNA);
                await Task.Delay(100);
            }
            catch (Exception ex)
            {
                Resolver.Log.Error($"Error sending SPI initialization commands: {ex.Message}");
            }
        }

        /// <summary>
        /// Send a command string via SPI
        /// </summary>
        /// <param name="command">The command string to send</param>
        private async Task SendSpiCommand(string command)
        {
            var commandBytes = Encoding.ASCII.GetBytes(command + "\r\n");

            // For NeoM8 SPI, we might need to write commands to a specific register or handle differently
            // This is a basic implementation that may need adjustment based on the actual SPI protocol
            var writeBuffer = new byte[commandBytes.Length + 1];
            writeBuffer[0] = (byte)Registers.DataStream; // Assuming commands go to data stream
            Array.Copy(commandBytes, 0, writeBuffer, 1, commandBytes.Length);

            spiComms!.Write(writeBuffer);

            await Task.Delay(10); // Small delay to ensure command is processed
        }
    }
}