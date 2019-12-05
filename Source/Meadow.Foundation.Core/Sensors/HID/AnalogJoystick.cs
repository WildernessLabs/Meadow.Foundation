using System.Threading.Tasks;
using Meadow.Hardware;

namespace Meadow.Foundation.Sensors.HID
{
    public class AnalogJoystick
    {
        #region Properties

        public IAnalogInputPort HorizontalInputPort { get; protected set; }
        public IAnalogInputPort VerticalInputPort { get; protected set; }

        public JoystickPosition Position { get; }

        public JoystickCalibration Calibration { get; protected set; }

        public float Horizontal = 0;
        public float Vertical = 0;

        #endregion

        #region Enums

        public enum JoystickPosition
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

        private AnalogJoystick() { }

        public AnalogJoystick(IIODevice device, IPin horizontalPin, IPin verticalPin, JoystickCalibration calibration = null) :
            this(device.CreateAnalogInputPort(horizontalPin), device.CreateAnalogInputPort(verticalPin), calibration)
        { }

        public AnalogJoystick(IAnalogInputPort horizontalInputPort, IAnalogInputPort verticalInputPort, JoystickCalibration calibration = null)
        {
            this.HorizontalInputPort = horizontalInputPort;
            this.VerticalInputPort = verticalInputPort;

            if(calibration == null)
            {
                this.Calibration = new JoystickCalibration();
            }
            else
            {
                this.Calibration = calibration;
            }

        }

        #endregion Constructors

        #region Method

        //call to set the joystick center position
        public async Task SetCenterPosition()
        {
            var hCenter = await HorizontalInputPort.Read();
            var vCenter = await VerticalInputPort.Read();

            Calibration = new JoystickCalibration(hCenter,
                Calibration.HorizontalMin, Calibration.HorizontalMax,
                vCenter,
                Calibration.VerticalMin, Calibration.VerticalMax,
                Calibration.DeadZone);
        }

        public async Task SetRange (int duration)
        {
            var timeoutTask = Task.Delay(duration);

            float h, v;

            while(timeoutTask.IsCompleted == false)
            {
                h = await HorizontalInputPort.Read();
                v = await VerticalInputPort.Read();


            }






        }

        // helper method to check joystick position
        public bool IsJoystickInPosition(JoystickPosition position)
        {
            if (position == Position)
                return true;

            return false;
        }

        public JoystickPosition GetPosition()
        {
            //math time


            return JoystickPosition.Center;
        }

        public void StartUpdating (int sampleCount = 3,
            int sampleIntervalDuration = 40,
            int standbyDuration = 100)
        {
            HorizontalInputPort.StartSampling(sampleCount, sampleIntervalDuration, standbyDuration);
            VerticalInputPort.StartSampling(sampleCount, sampleIntervalDuration, standbyDuration);
        }

        public void StopUpdating ()
        {
            HorizontalInputPort.StopSampling();
            VerticalInputPort.StopSampling();
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

            public JoystickCalibration()
            {
            }

            public JoystickCalibration(float horizontalCenter, float horizontalMin, float horizontalMax,
                float verticalCenter, float verticalMin, float verticalMax, float deadZone)
            {
                this.HorizontalCenter = horizontalCenter;
                this.HorizontalMin = horizontalMin;
                this.HorizontalMax = horizontalMax;

                this.VerticalCenter = verticalCenter;
                this.VerticalMin = verticalMin;
                this.VerticalMax = verticalMax;

                this.DeadZone = deadZone;
            }
        }

        #endregion Local classes
    }
}