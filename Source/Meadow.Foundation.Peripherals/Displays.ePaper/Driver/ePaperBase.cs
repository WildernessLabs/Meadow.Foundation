using Meadow.Hardware;
using System.Threading;

namespace Meadow.Foundation.Displays
{
    public abstract class EPaperBase
    {
        protected readonly byte[] commandBuffer = new byte[1];

        protected IDigitalOutputPort dataCommandPort;
        protected IDigitalOutputPort resetPort;
        protected IDigitalOutputPort chipSelectPort;
        protected IDigitalInputPort busyPort;
        protected ISpiPeripheral spiPeripheral;

        protected const bool DataState = true;
        protected const bool CommandState = false;

        protected void Write(byte value)
        {
            commandBuffer[0] = value;
            spiPeripheral.Write(commandBuffer);
        }

        protected virtual void Reset()
        {
            resetPort.State = false;
            DelayMs(200);
            resetPort.State = true;
            DelayMs(200);
        }

        protected void DelayMs(int millseconds)
        {
            Thread.Sleep(millseconds);
        }

        protected void SendCommand(byte command)
        {
            dataCommandPort.State = CommandState;
            Write(command);
        }

        protected void SendData(int data)
        {
            SendData((byte)data);
        }

        protected void SendData(byte data)
        {
            dataCommandPort.State = DataState;
            Write(data);
        }

        protected void SendData(byte[] data)
        {
            dataCommandPort.State = DataState;
            spiPeripheral.Write(data);
        }

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