using Meadow.Hardware;
using Meadow.Peripherals.Sensors.Hid;
using System;
using System.Threading.Tasks;

namespace Meadow.Foundation.Sensors.Hid
{
    /// <summary>
    /// 2-axis analog joystick
    /// </summary>
    public class AnalogJoystick
        : FilterableChangeObservableBase<JoystickPositionChangeResult, JoystickPosition>
    {
        /// <summary>
        /// Raised when the value of the reading changes.
        /// </summary>
        public event EventHandler<JoystickPositionChangeResult> Updated = delegate { };

        #region Properties

        public IAnalogInputPort HorizontalInputPort { get; protected set; }

        public IAnalogInputPort VerticalInputPort { get; protected set; }

        public DigitalJoystickPosition Position { get; }

        public JoystickCalibration Calibration { get; protected set; }

        public bool IsInverted { get; protected set; }

        public float HorizontalValue { get; protected set; }

        public float VerticalValue { get; protected set; }

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

        public AnalogJoystick(IIODevice device, IPin horizontalPin, IPin verticalPin, JoystickCalibration calibration = null, bool isInverted = false) :
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

            HorizontalInputPort.Subscribe
            (
                new FilterableChangeObserver<FloatChangeResult, float>(
                    h => {
                        HorizontalValue = h.New;
                        if ((Math.Abs(h.Old - Calibration.HorizontalCenter) < Calibration.DeadZone) &&
                            (Math.Abs(h.New - Calibration.HorizontalCenter) < Calibration.DeadZone)) {
                            return;
                        }

                        var oldH = GetNormalizedPosition(h.Old, true);
                        var newH = GetNormalizedPosition(h.New, true);
                        var v = GetNormalizedPosition(VerticalValue, false);

                        RaiseEventsAndNotify
                        (
                            new JoystickPositionChangeResult(
                                new JoystickPosition(newH, v),
                                new JoystickPosition(oldH, v)
                            )
                        );
                    }
                )
            );

            VerticalInputPort.Subscribe
            (
                new FilterableChangeObserver<FloatChangeResult, float>(
                    v => {
                        VerticalValue = v.New;
                        if ((Math.Abs(v.Old - Calibration.VerticalCenter) < Calibration.DeadZone) &&
                            (Math.Abs(v.New - Calibration.VerticalCenter) < Calibration.DeadZone)) {
                            return;
                        }

                        var oldV = GetNormalizedPosition(v.Old, false);
                        var newV = GetNormalizedPosition(v.New, false);
                        var h = GetNormalizedPosition(HorizontalValue, true);

                        RaiseEventsAndNotify
                        (
                            new JoystickPositionChangeResult(
                                new JoystickPosition(h, newV),
                                new JoystickPosition(h, oldV)
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

            float h, v;

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

        public Task<float> GetHorizontalValue()
        {
            return HorizontalInputPort.Read(1);
        }

        public Task<float> GetVerticalValue()
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

        protected void RaiseEventsAndNotify(JoystickPositionChangeResult changeResult)
        {
            Updated?.Invoke(this, changeResult);
            base.NotifyObservers(changeResult);
        }

        float GetNormalizedPosition(float value, bool isHorizontal)
        {
            float normalized;

            if (isHorizontal) {
                if (value <= Calibration.HorizontalCenter) {
                    normalized = (value - Calibration.HorizontalCenter) / (Calibration.HorizontalCenter - Calibration.HorizontalMin);
                } else {
                    normalized = (value - Calibration.HorizontalCenter) / (Calibration.HorizontalMax - Calibration.HorizontalCenter);
                }
            } else {
                if (value <= Calibration.VerticalCenter) {
                    normalized = (value - Calibration.VerticalCenter) / (Calibration.VerticalCenter - Calibration.VerticalMin);
                } else {
                    normalized = (value - Calibration.VerticalCenter) / (Calibration.VerticalMax - Calibration.VerticalCenter);
                }
            }

            return IsInverted ? -1 * normalized : normalized;
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
            public float HorizontalCenter { get; protected set; }
            public float HorizontalMin { get; protected set; }
            public float HorizontalMax { get; protected set; }

            public float VerticalCenter { get; protected set; }
            public float VerticalMin { get; protected set; }
            public float VerticalMax { get; protected set; }

            public float DeadZone { get; protected set; }

            public JoystickCalibration(float analogVoltage)
            {
                HorizontalCenter = analogVoltage / 2;
                HorizontalMin = 0;
                HorizontalMax = analogVoltage;

                VerticalCenter = analogVoltage / 2;
                VerticalMin = 0;
                VerticalMax = analogVoltage;

                DeadZone = 0.2f;
            }

            public JoystickCalibration(float horizontalCenter, float horizontalMin, float horizontalMax,
                float verticalCenter, float verticalMin, float verticalMax, float deadZone)
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