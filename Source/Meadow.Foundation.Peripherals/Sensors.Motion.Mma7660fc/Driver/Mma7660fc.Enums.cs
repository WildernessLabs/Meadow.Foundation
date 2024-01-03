namespace Meadow.Foundation.Sensors.Motion
{
    public partial class Mma7660fc
    {
        /// <summary>
        /// Sensor power mode
        /// </summary>
        public enum SensorPowerMode : byte
        {
            /// <summary>
            /// Device is in standby mode
            /// </summary>
            Standby = 0,
            /// <summary>
            /// Device is active
            /// </summary>
            Active = 1,
        }

        /// <summary>
        /// Represents the number samples per second
        /// </summary>
        public enum SampleRate : byte
        {
            /// <summary>
            /// 120 samples per second
            /// </summary>
            _120 = 0,
            /// <summary>
            /// 64 sample per second
            /// </summary>
            _64 = 1,
            /// <summary>
            /// 32 samples per second
            /// </summary>
            _32 = 2,
            /// <summary>
            /// 16 samples per second
            /// </summary>
            _16 = 3,
            /// <summary>
            /// 8 samples per second
            /// </summary>
            _8 = 4,
            /// <summary>
            /// 4 samples per second
            /// </summary>
            _4 = 5,
            /// <summary>
            /// 2 samples per second
            /// </summary>
            _2 = 6,
            /// <summary>
            /// 1 samples per second
            /// </summary>
            _1 = 7,
        }

        /// <summary>
        /// Direction/orientation of the device
        /// UP/DOWN/LEFT/RIGHT
        /// </summary>
        public enum DirectionType : byte
        {
            /// <summary>
            /// Direction unknown
            /// </summary>
            Unknown = 0,
            /// <summary>
            /// Device oriented up
            /// </summary>
            Up = 0b00011000,
            /// <summary>
            /// Device oriented down
            /// </summary>
            Down = 0b00010100,
            /// <summary>
            /// Device oriented right
            /// </summary>
            Right = 0b00001000,
            /// <summary>
            /// Device oriented left
            /// </summary>
            Left = 0b00000100,
        }

        /// <summary>
        /// Is device face down or face up
        /// </summary>
        public enum OrientationType : byte
        {
            /// <summary>
            /// Orientation unknown
            /// </summary>
            Unknown = 0,
            /// <summary>
            /// Device is face up (on back)
            /// </summary>
            Back = 0b00000010,
            /// <summary>
            /// Device is face down (on front)
            /// </summary>
            Front = 0b00000001,
        }

        /// <summary>
        /// Device Tilt status
        /// </summary>
        public enum Tilt : byte
        {
            /// <summary>
            /// Has the device been shaken
            /// </summary>
            Shake = 0b10000000,
            /// <summary>
            /// Has the device been tapped
            /// </summary>
            Tap = 0b00100000,
        }
    }
}
