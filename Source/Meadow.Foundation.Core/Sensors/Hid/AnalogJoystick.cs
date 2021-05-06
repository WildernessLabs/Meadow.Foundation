using Meadow.Devices;
using Meadow.Hardware;
using Meadow.Peripherals.Sensors.Hid;
using Meadow.Units;
using System;
using System.Threading.Tasks;

namespace Meadow.Foundation.Sensors.Hid
{
    /// <summary>
    /// 2-axis analog joystick
    /// </summary>
    public class AnalogJoystick
        : FilterableChangeObservable<ChangeResult<JoystickPosition>, JoystickPosition>
    {
        /// <summary>
        /// Raised when the value of the reading changes.
        /// </summary>
        public event EventHandler<ChangeResult<JoystickPosition>> Updated = delegate { };

        #region Properties

        public IAnalogInputPort HorizontalInputPort { get; protected set; }

        public IAnalogInputPort VerticalInputPort { get; protected set; }

        public DigitalJoystickPosition Position { get; }

        public JoystickCalibration Calibration { get; protected set; }

        public bool IsInverted { get; protected set; }

        public Voltage HorizontalValue { get; protected set; }

        public Voltage VerticalValue { get; protected set; }

        #endregion

        #region Enums

        public enum DigitalJoystickPosition
        {
            Center,
            Up,
            Down,
            Left,
            Right,
            UpRight,
            UpLeft,
            DownRight,
            DownLeft,
        }

        #endregion Enums

        #region Member variables / fields

        #endregion Member variables / fields

        #region Constructors

        public AnalogJoystick(IAnalogInputController device, IPin horizontalPin, IPin verticalPin, JoystickCalibration calibration = null, bool isInverted = false) :
            this(device.CreateAnalogInputPort(horizontalPin), device.CreateAnalogInputPort(verticalPin), calibration, isInverted)
        { }

        public AnalogJoystick(IAnalogInputPort horizontalInputPort, IAnalogInputPort verticalInputPort,
            JoystickCalibration calibration = null, bool isInverted = false)
        {
            HorizontalInputPort = horizontalInputPort;
            VerticalInputPort = verticalInputPort;
            IsInverted = isInverted;

            if (calibration == null) {
                Calibration = new JoystickCalibration(3.3f);
            } else {
                Calibration = calibration;
            }

            InitSubscriptions();
        }

        void InitSubscriptions()
        {

            _ = HorizontalInputPort.Subscribe
            (
                IAnalogInputPort.CreateObserver(
                    h => {
                        HorizontalValue = h.New;
                        if (
                            (((h.Old - Calibration.HorizontalCenter)?.Abs()) < Calibration.DeadZone)
                            &&
                            ((h.New - Calibration.HorizontalCenter).Abs() < Calibration.DeadZone)) {
                            return;
                        }

                        // TODO: wouldn't it be better to store the old position on the class
                        // rather than compute each time?

                        // if we have an old position
                        float? oldH = null;
                        if (h.Old is { } old) { oldH = GetNormalizedPosition(old, true); }

                        var newH = GetNormalizedPosition(h.New, true);
                        var v = GetNormalizedPosition(VerticalValue, false);

                        JoystickPosition? oldPosition = null;
                        if(oldH is { } oldPos) { oldPosition = new JoystickPosition(oldPos, v); }
                        // in C#9, we'll be able to do this directly in the method:
                        //(oldH is { } oldPos) ? new JoystickPosition(oldPos, v) : null;

                        RaiseEventsAndNotify
                        (
                            new ChangeResult<JoystickPosition>(
                                new JoystickPosition(newH, v),
                                oldPosition
                                )
                        );
                    }
                )
            );

            VerticalInputPort.Subscribe
            (
               IAnalogInputPort.CreateObserver(
                    v => {
                        VerticalValue = v.New;
                        if (
                            ((v.Old - Calibration.VerticalCenter)?.Abs() < Calibration.DeadZone)
                            &&
                            ((v.New - Calibration.VerticalCenter).Abs() < Calibration.DeadZone)) {
                            return;
                        }

                        float? oldV = null;
                        if (v.Old is { } old) { oldV = GetNormalizedPosition(old, false); }

                        var newV = GetNormalizedPosition(v.New, false);
                        var h = GetNormalizedPosition(HorizontalValue, true);

                        JoystickPosition? oldPosition = null;
                        if (oldV is { } oldPos) { oldPosition = new JoystickPosition(oldPos, h); }

                        RaiseEventsAndNotify
                        (
                            new ChangeResult<JoystickPosition>(
                                new JoystickPosition(h, newV),
                                oldPosition
                            )
                        );
                    }
                )
           );
        }

        #endregion Constructors

        #region Methods

        //call to set the joystick center position
        public async Task SetCenterPosition()
        {
            var hCenter = await HorizontalInputPort.Read(1);
            var vCenter = await VerticalInputPort.Read(1);

            Calibration = new JoystickCalibration(
                hCenter, Calibration.HorizontalMin, Calibration.HorizontalMax,
                vCenter, Calibration.VerticalMin, Calibration.VerticalMax,
                Calibration.DeadZone);
        }

        public async Task SetRange(int duration)
        {
            var timeoutTask = Task.Delay(duration);

            Voltage h, v;

            while (timeoutTask.IsCompleted == false) {
                h = await HorizontalInputPort.Read();
                v = await VerticalInputPort.Read();
            }
        }

        // helper method to check joystick position
        public bool IsJoystickInPosition(DigitalJoystickPosition position)
        {
            if (position == Position)
                return true;

            return false;
        }

        public async Task<DigitalJoystickPosition> GetPosition()
        {
            var h = await GetHorizontalValue();
            var v = await GetVerticalValue();

            if (h > Calibration.HorizontalCenter + Calibration.DeadZone) {
                if (v > Calibration.VerticalCenter + Calibration.DeadZone) {
                    return IsInverted ? DigitalJoystickPosition.DownLeft : DigitalJoystickPosition.UpRight;
                }
                if (v < Calibration.VerticalCenter - Calibration.DeadZone) {
                    return IsInverted ? DigitalJoystickPosition.UpLeft : DigitalJoystickPosition.DownRight;
                }
                return IsInverted ? DigitalJoystickPosition.Left : DigitalJoystickPosition.Right;
            } else if (h < Calibration.HorizontalCenter - Calibration.DeadZone) {
                if (v > Calibration.VerticalCenter + Calibration.DeadZone) {
                    return IsInverted ? DigitalJoystickPosition.DownRight : DigitalJoystickPosition.UpLeft;
                }
                if (v < Calibration.VerticalCenter - Calibration.DeadZone) {
                    return IsInverted ? DigitalJoystickPosition.UpRight : DigitalJoystickPosition.DownLeft;
                }
                return IsInverted ? DigitalJoystickPosition.Right : DigitalJoystickPosition.Left;
            } else if (v > Calibration.VerticalCenter + Calibration.DeadZone) {
                return IsInverted ? DigitalJoystickPosition.Down : DigitalJoystickPosition.Up;
            } else if (v < Calibration.VerticalCenter - Calibration.DeadZone) {
                return IsInverted ? DigitalJoystickPosition.Up : DigitalJoystickPosition.Down;
            }

            return DigitalJoystickPosition.Center;
        }

        public Task<Voltage> GetHorizontalValue()
        {
            return HorizontalInputPort.Read(1);
        }

        public Task<Voltage> GetVerticalValue()
        {
            return VerticalInputPort.Read(1);
        }

        public void StartUpdating(int sampleCount = 3,
            int sampleIntervalDuration = 40,
            int standbyDuration = 100)
        {
            HorizontalInputPort.StartSampling(sampleCount, sampleIntervalDuration, standbyDuration);
            VerticalInputPort.StartSampling(sampleCount, sampleIntervalDuration, standbyDuration);
        }

        public void StopUpdating()
        {
            HorizontalInputPort.StopSampling();
            VerticalInputPort.StopSampling();
        }

        protected void RaiseEventsAndNotify(ChangeResult<JoystickPosition> changeResult)
        {
            Updated?.Invoke(this, changeResult);
            base.NotifyObservers(changeResult);
        }

        float GetNormalizedPosition(Voltage value, bool isHorizontal)
        {
            double normalized;

            if (isHorizontal) {
                if (value <= Calibration.HorizontalCenter) {
                    normalized = (value.Volts - Calibration.HorizontalCenter.Volts) / (Calibration.HorizontalCenter.Volts - Calibration.HorizontalMin.Volts);
                } else {
                    normalized = (value.Volts - Calibration.HorizontalCenter.Volts) / (Calibration.HorizontalMax.Volts - Calibration.HorizontalCenter.Volts);
                }
            } else {
                if (value <= Calibration.VerticalCenter) {
                    normalized = (value.Volts - Calibration.VerticalCenter.Volts) / (Calibration.VerticalCenter.Volts - Calibration.VerticalMin.Volts);
                } else {
                    normalized = (value.Volts - Calibration.VerticalCenter.Volts) / (Calibration.VerticalMax.Volts - Calibration.VerticalCenter.Volts);
                }
            }

            return (float)(IsInverted ? -1 * normalized : normalized);
        }

        #endregion Methods

        #region Local classes

        /// <summary>
        ///     Calibration class for new sensor types.  This allows new sensors
        ///     to be used with this class.
        /// </summary>
        /// <remarks>
        ///     
        /// </remarks>
        public class JoystickCalibration
        {
            public Voltage HorizontalCenter { get; protected set; }
            public Voltage HorizontalMin { get; protected set; }
            public Voltage HorizontalMax { get; protected set; }

            public Voltage VerticalCenter { get; protected set; }
            public Voltage VerticalMin { get; protected set; }
            public Voltage VerticalMax { get; protected set; }

            public Voltage DeadZone { get; protected set; }

            public JoystickCalibration(Voltage voltage)
            {
                HorizontalCenter = voltage / 2;
                HorizontalMin = 0;
                HorizontalMax = voltage;

                VerticalCenter = voltage / 2;
                VerticalMin = 0;
                VerticalMax = voltage;

                DeadZone = 0.2f;
            }

            public JoystickCalibration(Voltage horizontalCenter, Voltage horizontalMin, Voltage horizontalMax,
                Voltage verticalCenter, Voltage verticalMin, Voltage verticalMax, Voltage deadZone)
            {
                HorizontalCenter = horizontalCenter;
                HorizontalMin = horizontalMin;
                HorizontalMax = horizontalMax;

                VerticalCenter = verticalCenter;
                VerticalMin = verticalMin;
                VerticalMax = verticalMax;

                DeadZone = deadZone;
            }
        }

        #endregion Local classes
    }
}