using Meadow.Hardware;
using System;
using System.Threading;

namespace Meadow.Foundation.Sensors.Hid
{
    /// <summary>
    /// Represents an XPT2046 4-wire touch screen controller
    /// </summary>
    public partial class Xpt2046 : ITouchScreen
    {
        public event TouchEventHandler? TouchDown = null;
        public event TouchEventHandler? TouchUp = null;
        public event TouchEventHandler? TouchClick = null;

        private readonly ISpiBus spiBus;
        private readonly IDigitalInterruptPort touchInterrupt;
        private readonly IDigitalOutputPort? chipSelect = null;

        private bool isCollecting = false;

        public Xpt2046(ISpiBus spiBus, IDigitalInterruptPort touchInterrupt, IDigitalOutputPort? chipSelect)
        {
            this.spiBus = spiBus;
            this.touchInterrupt = touchInterrupt;
            this.chipSelect = chipSelect;

            if (touchInterrupt.InterruptMode != InterruptMode.EdgeBoth)
            {
                throw new ArgumentException("Interrupt port must be InterruptMode.EdgeBoth");
            }

            touchInterrupt.Changed += OnTouchInterrupt;
        }

        private void OnTouchInterrupt(object sender, DigitalPortResult e)
        {
            if (e.New.State)
            {
                new Thread(CollectTouchInfo).Start();
            }
            else
            {
                TouchUp?.Invoke(0, 0);
                Resolver.Log.Info($"touch up");
                isCollecting = false;
            }
        }

        private const int SamplePeriodMilliseconds = 1000;

        private void CollectTouchInfo()
        {
            if (isCollecting) return;
            isCollecting = true;

            TouchDown?.Invoke(0, 0);
            Resolver.Log.Info($"touch down");

            while (isCollecting)
            {
                var z = ReadZ();

                var x = ReadX();
                var y = ReadY();

                Resolver.Log.Info($"Z:{z}  ({x},{y})");

                Thread.Sleep(SamplePeriodMilliseconds);
            }
        }

        private ushort ReadX()
        {
            return ReadChannel(0x91);
        }

        private ushort ReadY()
        {
            return ReadChannel(0xD1);
        }

        private ushort ReadZ()
        {
            return ReadChannel(0xC1);
        }

        private ushort ReadChannel(byte channelIdentifier)
        {
            Span<byte> txBuffer = stackalloc byte[1];
            Span<byte> rxBuffer = stackalloc byte[2];

            txBuffer[0] = channelIdentifier;

            spiBus.Exchange(chipSelect, txBuffer, rxBuffer);

            return (ushort)((rxBuffer[0] >> 3) << 8 | (rxBuffer[1] >> 3));
        }

    }
}