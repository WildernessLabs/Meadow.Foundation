using Meadow.Hardware;
using Meadow.Peripherals.Displays;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Meadow.Foundation.Sensors.Hid
{
    /// <summary>
    /// Represents an XPT2046 4-wire touch screen controller
    /// </summary>
    public partial class Xpt2046 : ICalibratableTouchscreen
    {
        /// <inheritdoc/>
        public event TouchEventHandler? TouchDown = null;
        /// <inheritdoc/>
        public event TouchEventHandler? TouchUp = null;
        /// <inheritdoc/>
        public event TouchEventHandler? TouchClick = null;
        /// <inheritdoc/>
        public event TouchEventHandler? TouchMoved = null;

        private const int SamplePeriodMilliseconds = 100;
        private const byte StartBit = 0x80;

        private readonly ISpiBus spiBus;
        private readonly IDigitalInterruptPort touchInterrupt;
        private readonly IDigitalOutputPort? chipSelect = null;
        private Timer sampleTimer;
        private bool isSampling = false;
        private bool firstTouch = true;
        private TouchPoint? lastTouchPosition;
        private TouchPoint? penultimatePosition;
        private float mX, mY, cX, cY; // linear calibration coefficeints

        /// <inheritdoc/>
        public RotationType Rotation { get; }
        /// <inheritdoc/>
        public bool IsCalibrated { get; private set; }

        /// <inheritdoc/>
        public bool IsTouched => isSampling;

        /// <summary>
        /// Creates an instance of an Xpt2046
        /// </summary>
        /// <param name="spiBus">The ISpiBus connected to the touchscreen controller</param>
        /// <param name="touchInterrupt">The interrupt port connected to the touchscreen controller</param>
        /// <param name="chipSelect">The chip select port for the touchscreen controller</param>
        /// <param name="rotation">The touchscreen rotation (not the display rotation)</param>
        public Xpt2046(
            ISpiBus spiBus,
            IDigitalInterruptPort touchInterrupt,
            IDigitalOutputPort? chipSelect,
            RotationType rotation = RotationType.Normal)
        {
            sampleTimer = new Timer(SampleTimerProc, null, -1, -1);

            this.spiBus = spiBus;
            this.touchInterrupt = touchInterrupt;
            this.chipSelect = chipSelect;
            Rotation = rotation;

            touchInterrupt.Changed += OnTouchInterrupt;
        }

        private TouchPoint ConvertRawToTouchPoint(ushort rawX, ushort rawY, ushort rawZ)
        {
            int x, y;

            // rotate
            switch (Rotation)
            {
                case RotationType._90Degrees:
                    x = 4095 - rawY;
                    y = rawX;
                    break;
                case RotationType._180Degrees:
                    x = 4095 - rawX;
                    y = 4095 - rawY;
                    break;
                case RotationType._270Degrees:
                    x = rawY;
                    y = 4095 - rawX;
                    break;
                default:
                    x = rawX;
                    y = rawY;
                    break;
            }

            if (!IsCalibrated)
            {
                return TouchPoint.FromRawData(x, y, rawZ);
            }

            // scale for calibration
            var scaledX = ((x * mX) + cX);
            var scaledY = ((y * mY) + cY);

            return TouchPoint.FromScreenData((int)scaledX, (int)scaledY, rawZ, rawX, rawY, rawZ);
        }

        private void OnTouchInterrupt(object sender, DigitalPortResult e)
        {
            // high is not touched, low is touched
            if (!isSampling)
            {
                sampleTimer.Change(0, -1);
            }
        }

        private void SampleTimerProc(object o)
        {
            if (touchInterrupt.State)
            {
                // the actual "last" reading for up is often garbage, so go back one before that if we have it
                if (penultimatePosition != null)
                {
                    TouchUp?.Invoke(this, penultimatePosition.Value);
                }
                else if (lastTouchPosition != null)
                {
                    TouchUp?.Invoke(this, lastTouchPosition.Value);
                }
                isSampling = false;
                firstTouch = true;
                lastTouchPosition = null;
                penultimatePosition = null;
                return;
            }

            isSampling = true;

            var z = ReadZ();
            var x = ReadX();
            var y = ReadY();
            EnableIrq();

            var position = ConvertRawToTouchPoint(x, y, z);

            try
            {
                if (firstTouch)
                {
                    firstTouch = false;
                    lastTouchPosition = position;
                    if (lastTouchPosition != null)
                    {
                        TouchDown?.Invoke(this, lastTouchPosition.Value);
                    }
                }
                else
                {
                    if (!position.Equals(lastTouchPosition))
                    {
                        penultimatePosition = lastTouchPosition;
                        lastTouchPosition = position;
                        if (lastTouchPosition != null)
                        {
                            TouchMoved?.Invoke(this, lastTouchPosition.Value);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Resolver.Log.Warn($"Touchscreen event handler error: {ex.Message}");
                // ignore any unhandled handler exceptions
            }

            sampleTimer.Change(SamplePeriodMilliseconds, -1);
        }

        private ushort ReadX()
        {
            return ReadChannel(Channel.X);
        }

        private ushort ReadY()
        {
            return ReadChannel(Channel.Y);
        }

        private ushort ReadZ()
        {
            return ReadChannel(Channel.Z2);
        }

        private void EnableIrq()
        {
            ReadChannel(Channel.Temp, PowerState.PowerDown);
        }

        private ushort ReadChannel(Channel channel, PowerState postSamplePowerState = PowerState.Adc, Mode mode = Mode.Bits_12, VoltageReference vref = VoltageReference.Differential)
        {
            Span<byte> txBuffer = stackalloc byte[3];
            Span<byte> rxBuffer = stackalloc byte[3];

            txBuffer[0] = (byte)(StartBit | (byte)channel | (byte)mode | (byte)vref | (byte)postSamplePowerState);

            spiBus.Exchange(chipSelect, txBuffer, rxBuffer);

            return (ushort)((rxBuffer[1] >> 3) << 8 | (rxBuffer[2] >> 3));
        }

        /// <inheritdoc/>
        public void SetCalibrationData(IEnumerable<CalibrationPoint> data)
        {
            var points = data.ToArray();
            if (points.Length != 2) { throw new ArgumentException("This touchscreen requires exactly 2 calibration points"); }

            // basic point validation
            if (points[1].RawX - points[0].RawX == 0 ||
                points[1].RawY - points[0].RawY == 0 ||
                points[1].ScreenX - points[0].ScreenX == 0 ||
                points[1].ScreenY - points[0].ScreenY == 0)
            {
                throw new ArgumentOutOfRangeException("Invalid calibration data");
            }

            // simple 2-point linear calibration (fine for small screens)

            mX = (points[1].ScreenX - points[0].ScreenX) / (float)(points[1].RawX - points[0].RawX);
            cX = points[0].ScreenX - (points[0].RawX * mX);
            mY = (points[1].ScreenY - points[0].ScreenY) / (float)(points[1].RawY - points[0].RawY);
            cY = points[0].ScreenY - (points[0].RawY * mY);

            IsCalibrated = true;
        }

        private enum Channel : byte
        {
            Temp = 0x00,
            X = 1 << 4,
            Z1 = 3 << 4,
            Z2 = 4 << 4,
            Y = 5 << 4,
        }
    }
}