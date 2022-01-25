using Meadow.Devices;
using Meadow.Hardware;
using Meadow.Peripherals.Sensors.Hid;
using Meadow.Units;
using System;
using System.Threading;
using System.Threading.Tasks;
using VU = Meadow.Units.Voltage.UnitType;

namespace Meadow.Foundation.Sensors.Hid
{
    /// <summary>
    /// 2-axis analog joystick
    /// </summary>
    public partial class AnalogJoystick
        : SensorBase<JoystickPosition>
    {
        //==== events

        //==== internals
        /// <summary>
        /// Number of samples used to calculate position
        /// </summary>
        protected int sampleCount;
        /// <summary>
        /// Interval between samples 
        /// </summary>
        protected int sampleIntervalMs;

        //==== properties
        /// <summary>
        /// Analog port connected to horizonal joystick pin
        /// </summary>
        protected IAnalogInputPort HorizontalInputPort { get; set; }
        /// <summary>
        /// Analog port connected to vertical joystick pin
        /// </summary>
        protected IAnalogInputPort VerticalInputPort { get; set; }

        /// <summary>
        /// Is the joystick inverted 
        /// </summary>
        public bool IsInverted { get; protected set; }

        /// <summary>
        /// Current position of
        /// </summary>
        public JoystickPosition? Position { get; protected set; }

        /// <summary>
        /// 
        /// </summary>
        public DigitalJoystickPosition? DigitalPosition {
            get {
                return TranslateAnalogPositionToDigital();
            }
        }

        /// <summary>
        /// Callibration for 2-axis analog joystick
        /// </summary>
        public JoystickCalibration Calibration { get; protected set; }

        /// <summary>
        /// Creates a 2-axis analog joystick
        /// </summary>
        /// <param name="device">The `IAnalogInputController` to create the port on.</param>
        /// <param name="horizontalPin"></param>
        /// <param name="verticalPin"></param>
        /// <param name="calibration">Calibration for the joystick.</param>
        /// <param name="isInverted">Whether or not the vertical component is inverted.</param>
        public AnalogJoystick(
            IAnalogInputController device, IPin horizontalPin, IPin verticalPin,
            JoystickCalibration? calibration = null, bool isInverted = false)
                : this(device, horizontalPin, verticalPin, calibration, isInverted, 5, TimeSpan.FromMilliseconds(40))
        { }

        /// <summary>
        /// Creates a 2-axis analog joystick
        /// </summary>
        /// <param name="device">The `IAnalogInputController` to create the port on.</param>
        /// <param name="horizontalPin"></param>
        /// <param name="verticalPin"></param>
        /// <param name="calibration">Calibration for the joystick.</param>
        /// <param name="isInverted">Whether or not the vertical component is inverted.</param>
        /// <param name="sampleCount">How many samples to take during a given
        /// reading. These are automatically averaged to reduce noise.</param>
        /// <param name="sampleInterval">The time, in milliseconds,
        /// to wait in between samples during a reading.</param>
        public AnalogJoystick(
            IAnalogInputController device, IPin horizontalPin, IPin verticalPin,
            JoystickCalibration? calibration, bool isInverted,
            int sampleCount, 
            TimeSpan sampleInterval)
                : this(
                      device.CreateAnalogInputPort(horizontalPin, sampleCount, sampleInterval, new Voltage(3.3, VU.Volts)),
                      device.CreateAnalogInputPort(verticalPin, sampleCount, sampleInterval, new Voltage(3.3, VU.Volts)),
                      calibration, isInverted)
        { }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="horizontalInputPort"></param>
        /// <param name="verticalInputPort"></param>
        /// <param name="calibration"></param>
        /// <param name="isInverted"></param>
        public AnalogJoystick(
            IAnalogInputPort horizontalInputPort, IAnalogInputPort verticalInputPort,
            JoystickCalibration? calibration = null, bool isInverted = false)
        {
            HorizontalInputPort = horizontalInputPort;
            VerticalInputPort = verticalInputPort;
            IsInverted = isInverted;

            if (calibration == null) {
                Calibration = new JoystickCalibration(new Voltage(3.3f, VU.Volts));
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
                        if (
                            (((h.Old - Calibration.HorizontalCenter)?.Abs()) < Calibration.DeadZone)
                            &&
                            ((h.New - Calibration.HorizontalCenter).Abs() < Calibration.DeadZone)) {
                            return;
                        }

                        // events are processed on each axis, serially. so when a new
                        // horizontal value comes in, we have to combine it with the last
                        // vertical value, to produce a new, complete joystick position.

                        // capture history
                        var oldPosition = Position;

                        // calculate new horizontal position, combine with previous
                        // vertical (if any)
                        var newH = GetNormalizedPosition(h.New, true);
                        var newV = Position?.Vertical; // old vertical
                        JoystickPosition newPosition = new JoystickPosition(newH, newV);

                        //save state
                        Position = newPosition;

                        var result = new ChangeResult<JoystickPosition>(newPosition, oldPosition);
                        base.RaiseEventsAndNotify(result);
                        
                    }
                )
            );

            VerticalInputPort.Subscribe
            (
               IAnalogInputPort.CreateObserver(
                    v => {
                        //var newVerticalValue = v.New;
                        if (
                            ((v.Old - Calibration.VerticalCenter)?.Abs() < Calibration.DeadZone)
                            &&
                            ((v.New - Calibration.VerticalCenter).Abs() < Calibration.DeadZone)) {
                            return;
                        }
                        // events are processed on each axis, serially. so when a new
                        // vertical value comes in, we have to combine it with the last
                        // horizontal value, to produce a new, complete joystick position.

                        // capture history
                        var oldPosition = Position;

                        // calculate new vertical position, combine with previous
                        // horizontal (if any)
                        var newV = GetNormalizedPosition(v.New, true);
                        var newH = Position?.Horizontal; // old horizontal
                        JoystickPosition newPosition = new JoystickPosition(newH, newV);

                        //save state
                        Position = newPosition;

                        var result = new ChangeResult<JoystickPosition>(newPosition, oldPosition);
                        base.RaiseEventsAndNotify(result);
                    }
                )
           );
        }


        //==== methods

        /// <summary>
        /// sets the current position as the center position and
        /// saves to the calibration.
        /// </summary>
        /// <returns></returns>
        public async Task SetCenterPosition()
        {
            var hCenter = await HorizontalInputPort.Read();
            var vCenter = await VerticalInputPort.Read();

            Calibration = new JoystickCalibration(
                hCenter, Calibration.HorizontalMin, Calibration.HorizontalMax,
                vCenter, Calibration.VerticalMin, Calibration.VerticalMax,
                Calibration.DeadZone);
        }

        ///// <summary>
        ///// todo: this doesn't do anything today
        ///// </summary>
        ///// <param name="duration"></param>
        ///// <returns></returns>
        //public async Task SetRange(int duration)
        //{
        //    var timeoutTask = Task.Delay(duration);

        //    Voltage h, v;

        //    while (timeoutTask.IsCompleted == false) {
        //        h = await HorizontalInputPort.Read();
        //        v = await VerticalInputPort.Read();
        //    }
        //}

        /// <summary>
        /// Translates an analog position into a digital position, taking into
        /// account the calibration information.
        /// </summary>
        /// <returns></returns>
        protected DigitalJoystickPosition TranslateAnalogPositionToDigital()
        {
            var h = Position?.Horizontal;
            var v = Position?.Vertical;

            if (h > (Calibration.HorizontalCenter + Calibration.DeadZone).Volts) {
                if (v > (Calibration.VerticalCenter + Calibration.DeadZone).Volts) {
                    return IsInverted ? DigitalJoystickPosition.DownLeft : DigitalJoystickPosition.UpRight;
                }
                if (v < (Calibration.VerticalCenter - Calibration.DeadZone).Volts) {
                    return IsInverted ? DigitalJoystickPosition.UpLeft : DigitalJoystickPosition.DownRight;
                }
                return IsInverted ? DigitalJoystickPosition.Left : DigitalJoystickPosition.Right;
            } else if (h < (Calibration.HorizontalCenter - Calibration.DeadZone).Volts) {
                if (v > (Calibration.VerticalCenter + Calibration.DeadZone).Volts) {
                    return IsInverted ? DigitalJoystickPosition.DownRight : DigitalJoystickPosition.UpLeft;
                }
                if (v < (Calibration.VerticalCenter - Calibration.DeadZone).Volts) {
                    return IsInverted ? DigitalJoystickPosition.UpRight : DigitalJoystickPosition.DownLeft;
                }
                return IsInverted ? DigitalJoystickPosition.Right : DigitalJoystickPosition.Left;
            } else if (v > (Calibration.VerticalCenter + Calibration.DeadZone).Volts) {
                return IsInverted ? DigitalJoystickPosition.Down : DigitalJoystickPosition.Up;
            } else if (v < (Calibration.VerticalCenter - Calibration.DeadZone).Volts) {
                return IsInverted ? DigitalJoystickPosition.Up : DigitalJoystickPosition.Down;
            }

            return DigitalJoystickPosition.Center;

        }

        /// <summary>
        /// Convenience method to get the current temperature. For frequent reads, use
        /// StartSampling() and StopSampling() in conjunction with the SampleBuffer.
        /// </summary>
        protected override async Task<JoystickPosition> ReadSensor()
        {
            var h = await HorizontalInputPort.Read();
            var v = await VerticalInputPort.Read();

            JoystickPosition position = new JoystickPosition( GetNormalizedPosition(h, true), GetNormalizedPosition(v, false));
            return position;
        }

        /// <summary>
        /// Starts continuously sampling the sensor.
        ///
        /// This method also starts raising `Changed` events and IObservable
        /// subscribers getting notified. Use the `readIntervalDuration` parameter
        /// to specify how often events and notifications are raised/sent.
        /// </summary>
        /// <param name="updateInterval">A `TimeSpan` that specifies how long to
        /// wait between readings. This value influences how often `*Updated`
        /// events are raised and `IObservable` consumers are notified.
        /// The default is 5 seconds.</param>
        public void StartUpdating(TimeSpan? updateInterval)
        {
            // thread safety
            lock (samplingLock) {
                if (IsSampling) return;
                IsSampling = true;

                base.SamplingTokenSource = new CancellationTokenSource();
                CancellationToken ct = SamplingTokenSource.Token;

                HorizontalInputPort.StartUpdating(updateInterval);
                VerticalInputPort.StartUpdating(updateInterval);
            }
        }

        /// <summary>
        /// Stops sampling the joystick position.
        /// </summary>
        public void StopUpdating()
        {
            lock (samplingLock) {
                if (!IsSampling) return;

                HorizontalInputPort.StopUpdating();
                VerticalInputPort.StopUpdating();

                SamplingTokenSource?.Cancel();

                // state muh-cheen
                IsSampling = false;
            }
        }

        /// <summary>
        /// Converts a voltage value to positional data, taking into account the
        /// calibration info.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="isHorizontal"></param>
        /// <returns>A postion value between -1.0 and 1.0</returns>
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

    }
}