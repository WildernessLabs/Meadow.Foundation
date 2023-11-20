using Meadow.Hardware;
using Meadow.Units;
using System;
using System.Threading;

namespace Meadow.Foundation.Displays
{
    /// <summary>
    /// Represents a base ePaper display driver
    /// </summary>
    public abstract class EPaperBase : ISpiPeripheral, IDisposable
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
        /// Is the object disposed
        /// </summary>
        public bool IsDisposed { get; private set; }

        /// <summary>
        /// The command buffer
        /// </summary>
        protected readonly byte[] commandBuffer = new byte[1];

        /// <summary>
        /// Data command port
        /// </summary>
        protected IDigitalOutputPort? dataCommandPort;

        /// <summary>
        /// Reset port
        /// </summary>
        protected IDigitalOutputPort? resetPort;

        /// <summary>
        /// Chip select port
        /// </summary>
        protected IDigitalOutputPort? chipSelectPort;

        /// <summary>
        /// Busy indicator port
        /// </summary>
        protected IDigitalInputPort? busyPort;

        /// <summary>
        /// SPI Communication bus used to communicate with the peripheral
        /// </summary>
        protected ISpiCommunications? spiComms;

        /// <summary>
        /// Const bool representing the data state
        /// </summary>
        protected const bool DataState = true;

        /// <summary>
        /// Const bool representing the command state
        /// </summary>
        protected const bool CommandState = false;

        /// <summary>
        /// Did we create the port(s) used by the peripheral
        /// </summary>
        protected bool createdPorts = false;

        /// <summary>
        /// Write a value to the display
        /// </summary>
        /// <param name="value">The value as a byte</param>
        protected void Write(byte value)
        {
            commandBuffer[0] = value;
            spiComms?.Write(commandBuffer);
        }

        /// <summary>
        /// Reset the display
        /// </summary>
        protected virtual void Reset()
        {
            if (resetPort != null)
            {
                resetPort.State = false;
                DelayMs(200);
                resetPort.State = true;
                DelayMs(200);
            }
        }

        /// <summary>
        /// Delay for a specified amount of time
        /// </summary>
        /// <param name="millseconds">The time in milliseconds</param>
        protected void DelayMs(int millseconds)
        {
            Thread.Sleep(millseconds);
        }

        /// <summary>
        /// Send a command to the display
        /// </summary>
        /// <param name="command">The command value</param>
        protected void SendCommand(byte command)
        {
            dataCommandPort!.State = CommandState;
            Write(command);
        }

        /// <summary>
        /// Send data to the display
        /// </summary>
        /// <param name="data">The data (is cast to a byte)</param>
        protected void SendData(int data)
        {
            SendData((byte)data);
        }

        /// <summary>
        /// Send data to the display
        /// </summary>
        /// <param name="data">The data</param>
        protected void SendData(byte data)
        {
            dataCommandPort!.State = DataState;
            Write(data);
        }

        /// <summary>
        /// Send data to the display
        /// </summary>
        /// <param name="data">The data</param>
        protected void SendData(byte[] data)
        {
            dataCommandPort!.State = DataState;
            spiComms!.Write(data);
        }

        /// <summary>
        /// Wait until the display is idle (not busy)
        /// </summary>
        protected virtual void WaitUntilIdle()
        {
            int count = 0;

            if (busyPort == null)
            {
                DelayMs(200);
                return;
            }

            while (busyPort.State == false && count < 20)
            {
                DelayMs(50);
                count++;
            }
        }

        ///<inheritdoc/>
        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Dispose of the object
        /// </summary>
        /// <param name="disposing">Is disposing</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!IsDisposed)
            {
                if (disposing && createdPorts)
                {
                    chipSelectPort?.Dispose();
                    resetPort?.Dispose();
                    dataCommandPort?.Dispose();
                    busyPort?.Dispose();
                }

                IsDisposed = true;
            }
        }
    }
}