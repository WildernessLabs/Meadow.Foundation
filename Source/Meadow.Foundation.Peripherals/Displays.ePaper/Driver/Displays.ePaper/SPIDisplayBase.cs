using Meadow;
using Meadow.Hardware;
using System.Threading;

namespace Meadow.Foundation.Displays
{
    public abstract class SPIDisplayBase : DisplayBase
    {
        protected readonly byte[] spiBOneByteBuffer = new byte[1];

        protected IDigitalOutputPort dataCommandPort;
        protected IDigitalOutputPort resetPort;
        protected IDigitalInputPort busyPort;
        protected ISpiPeripheral spi;

        protected Color currentPen = Color.White;

        protected const bool Data = true;
        protected const bool Command = false;

        protected void Write(byte value)
        {
            spiBOneByteBuffer[0] = value;
            spi.WriteBytes(spiBOneByteBuffer);
        }

        public override void SetPenColor(Color pen)
        {
            currentPen = pen;
        }

        protected void Reset()
        {
            resetPort.State = (false);
            DelayMs(200);
            resetPort.State = (true);
            DelayMs(200);
        }

        protected void DelayMs(int millseconds)
        {
            Thread.Sleep(millseconds);
        }

        protected void SendCommand(byte command)
        {
            dataCommandPort.State = (Command);
            Write(command);
        }

        protected void SendData(int data)
        {
            SendData((byte)data);
        }

        protected void SendData(byte data)
        {
            dataCommandPort.State = (Data);
            Write(data);
        }

        protected void SendData(byte[] data)
        {
            dataCommandPort.State = (Data);
            spi.WriteBytes(data);
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