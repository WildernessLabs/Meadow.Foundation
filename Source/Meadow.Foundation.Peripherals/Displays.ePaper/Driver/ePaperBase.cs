using Meadow.Hardware;
using Meadow.Units;
using System.Threading;

namespace Meadow.Foundation.Displays
{
    /// <summary>
    /// Represents a base ePaper display driver
    /// </summary>
    public abstract class EPaperBase
    {
        /// <summary>
        /// The SPI bus speed for the device
        /// </summary>
        public Frequency SpiBusSpeed
        {
            get => _spiBusSpeed;
            set => _spiBusSpeed = spiPeripheral.BusSpeed = value;
        }
        Frequency _spiBusSpeed = new Frequency(375, Frequency.UnitType.Kilohertz);

        /// <summary>
        /// The SPI bus mode for the device
        /// </summary>
        public SpiClockConfiguration.Mode SpiBusMode
        {
            get => _piBusMode;
            set => _piBusMode = spiPeripheral.BusMode = value;
        }
        SpiClockConfiguration.Mode _piBusMode = SpiClockConfiguration.Mode.Mode0;

        /// <summary>
        /// The command buffer
        /// </summary>
        protected readonly byte[] commandBuffer = new byte[1];

        /// <summary>
        /// Data command port
        /// </summary>
        protected IDigitalOutputPort dataCommandPort;

        /// <summary>
        /// Reset port
        /// </summary>
        protected IDigitalOutputPort resetPort;

        /// <summary>
        /// Chip select port
        /// </summary>
        protected IDigitalOutputPort chipSelectPort;

        /// <summary>
        /// Busy indicator port
        /// </summary>
        protected IDigitalInputPort busyPort;

        /// <summary>
        /// The SpiPeripheral object that reprsents the display
        /// </summary>
        protected ISpiPeripheral spiPeripheral;

        /// <summary>
        /// Const bool representing the data state
        /// </summary>
        protected const bool DataState = true;

        /// <summary>
        /// Const bool representing the command state
        /// </summary>
        protected const bool CommandState = false;

        /// <summary>
        /// Write a value to the display
        /// </summary>
        /// <param name="value">The value as a byte</param>
        protected void Write(byte value)
        {
            commandBuffer[0] = value;
            spiPeripheral.Write(commandBuffer);
        }

        /// <summary>
        /// Reset the display
        /// </summary>
        protected virtual void Reset()
        {
            resetPort.State = false;
            DelayMs(200);
            resetPort.State = true;
            DelayMs(200);
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
            dataCommandPort.State = CommandState;
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
            dataCommandPort.State = DataState;
            Write(data);
        }

        /// <summary>
        /// Send data to the display
        /// </summary>
        /// <param name="data">The data</param>
        protected void SendData(byte[] data)
        {
            dataCommandPort.State = DataState;
            spiPeripheral.Write(data);
        }

        /// <summary>
        /// Wait until the display is idle (not busy)
        /// </summary>
        protected virtual void WaitUntilIdle()
        {
            int count = 0;
            while (busyPort.State == false && count < 20)
            {
                DelayMs(50);
                count++;
            }
        }
    }
}