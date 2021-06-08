using Meadow.Devices;
using Meadow.Hardware;
using Meadow.Peripherals.Sensors.Hid;
using Meadow.Units;
using System;
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
        /// <summary>
        /// Raised when the value of the reading changes.
        /// </summary>
        public event EventHandler<ChangeResult<JoystickPosition>> Updated = delegate { };

        //==== properties
        protected IAnalogInputPort HorizontalInputPort { get; set; }
        protected IAnalogInputPort VerticalInputPort { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public bool IsInverted { get; protected set; }

        /// <summary>
        /// 
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
        /// 
        /// </summary>
        public JoystickCalibration Calibration { get; protected set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="device"></param>
        /// <param name="horizontalPin"></param>
        /// <param name="verticalPin"></param>
        /// <param name="calibration"></param>
        /// <param name="isInverted"></param>
        public AnalogJoystick(
            IAnalogInputController device, IPin horizontalPin, IPin verticalPin,
            JoystickCalibration? calibration = null, bool isInverted = false)
                : this(device.CreateAnalogInputPort(horizontalPin), device.CreateAnalogInputPort(verticalPin), calibration, isInverted)
        { }

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
                        RaiseEventsAndNotify(result);
                        
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
                        RaiseEventsAndNotify(result);
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
            var hCenter = await HorizontalInputPort.Read(2, 20);
            var vCenter = await VerticalInputPort.Read(2, 20);

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
        /// <param name="sampleCount">The number of sample readings to take. 
        /// Must be greater than 0. These samples are automatically averaged.</param>
        /// <param name="sampleIntervalDuration">The time, in milliseconds,
        /// to wait in between samples during a reading.</param>
        /// <returns>A float value that's ann average value of all the samples taken.</returns>
        public async Task<JoystickPosition> Read(int sampleCount = 2, int sampleIntervalDuration = 20)
        {
            var h = await HorizontalInputPort.Read(sampleCount, sampleIntervalDuration);
            var v = await VerticalInputPort.Read(sampleCount, sampleIntervalDuration);

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
        /// <param name="sampleCount">How many samples to take during a given
        /// reading. These are automatically averaged to reduce noise.</param>
        /// <param name="sampleIntervalDuration">The time, in milliseconds,
        /// to wait in between samples during a reading.</param>
        /// <param name="standbyDuration">The time, in milliseconds, to wait
        /// between sets of sample readings. This value determines how often
        /// `Changed` events are raised and `IObservable` consumers are notified.</param>
        public void StartUpdating(int sampleCount = 3,
            int sampleIntervalDuration = 40,
            int standbyDuration = 100)
        {
            HorizontalInputPort.StartUpdating(sampleCount, sampleIntervalDuration, standbyDuration);
            VerticalInputPort.StartUpdating(sampleCount, sampleIntervalDuration, standbyDuration);
        }

        /// <summary>
        /// Stops sampling the joystick position.
        /// </summary>
        public void StopUpdating()
        {
            HorizontalInputPort.StopUpdating();
            VerticalInputPort.StopUpdating();
        }

        /// <summary>
        /// Inheritance safe way to raise events.
        /// </summary>
        /// <param name="changeResult"></param>
        protected void RaiseEventsAndNotify(ChangeResult<JoystickPosition> changeResult)
        {
            Updated?.Invoke(this, changeResult);
            base.NotifyObservers(changeResult);
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