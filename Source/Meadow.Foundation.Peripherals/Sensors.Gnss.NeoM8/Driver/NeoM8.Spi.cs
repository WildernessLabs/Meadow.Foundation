﻿using Meadow.Hardware;
using Meadow.Units;
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

        IDigitalOutputPort? chipSelectPort;

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
        }

        private async Task StartUpdatingSpi()
        {
            cts = new CancellationTokenSource();

            byte[] data = new byte[BUFFER_SIZE];

            static bool HasMoreData(byte[] data)
            {
                bool hasNullValue = false;
                for (int i = 1; i < data.Length; i++)
                {
                    if (data[i] == NULL_VALUE) { hasNullValue = true; }
                    if (data[i - 1] == NULL_VALUE && data[i] != NULL_VALUE)
                    {
                        return true;
                    }
                }
                return !hasNullValue;
            }

            var t = new Task(() =>
            {
                while (cts.Token.IsCancellationRequested == false) { }
                {
                    spiComms!.Read(data);
                    messageProcessor!.Process(data);

                    if (HasMoreData(data) == false)
                    {
                        Thread.Sleep(COMMS_SLEEP_MS);
                    }
                }
            }, TaskCreationOptions.LongRunning);
            await t;
        }

        void StopUpdatingSpi()
        {
            cts?.Cancel();
        }
    }
}