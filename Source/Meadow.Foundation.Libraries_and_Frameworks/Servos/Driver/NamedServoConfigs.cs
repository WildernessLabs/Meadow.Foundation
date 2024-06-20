using Meadow.Units;
using AU = Meadow.Units.Angle.UnitType;

namespace Meadow.Foundation.Servos
{
    /// <summary>
    /// NamedServoConfigs class to hold names servo configurations
    /// </summary>
    public static class NamedServoConfigs
    {
        /// <summary>
        /// Represents an ideal 180? servo that has a minimum pulse of 1ms and a max of 2ms
        /// </summary>
        public static ServoConfig Ideal180Servo = new ServoConfig();

        /// <summary>
        /// Represents an ideal 270? servo that has a minimum pulse of 1ms and a max of 2ms
        /// </summary>
        public static ServoConfig Ideal270Servo = new ServoConfig(maximumAngle: new Angle(270, AU.Degrees));

        /// <summary>
        /// Represents an ideal continuous rotation servo that has a min and max pulse of 1ms and 2ms
        /// </summary>
        public static ServoConfig IdealContinuousRotationServo = new ServoConfig();

        /// <summary>
        /// Represents the BlueBird BMS models. Maximum angle is 120. Pulse range is 900?s - 2,100?s
        /// See: https://www.blue-bird-model.com/products_detail/309.htm
        /// </summary>
        public static ServoConfig BlueBirdBMS120 = new ServoConfig(minimumPulseDuration: 900, maximumPulseDuration: 2100, maximumAngle: new Angle(120, AU.Degrees));

        /// <summary>
        /// Represents the HiTec "Standard" servo models. Angle: 0-180, Pulse: 900 - 1,200
        /// </summary>0
        public static ServoConfig HiTecStandard = new ServoConfig(minimumPulseDuration: 900, maximumPulseDuration: 2100, maximumAngle: new Angle(180, AU.Degrees));

        /// <summary>
        /// Represents HiTec Digital servo models with their default settings. Angle: 0-180, Pulse: 900 - 1,200
        /// </summary>0
        public static ServoConfig HiTechDigital = new ServoConfig(minimumPulseDuration: 900, maximumPulseDuration: 1200, maximumAngle: new Angle(180, AU.Degrees));

        /// <summary>
        /// Represents the JX HV 180 degree servo models. Angle: 0-180, Pulse: 500 - 2,500
        /// </summary>0
        public static ServoConfig JXHV180 = new ServoConfig(minimumPulseDuration: 500, maximumPulseDuration: 2500, maximumAngle: new Angle(180, AU.Degrees));


        /// <summary>
        /// Represents the MG996R 180 degree servo models. Angle: 0-180, Pulse: 500 - 2,200
        /// </summary>0
        public static ServoConfig MG996R = new ServoConfig(minimumAngle: new Angle(0, AU.Degrees),
                                                        maximumAngle: new Angle(180, AU.Degrees),
                                                        minimumPulseDuration: 500,
                                                        maximumPulseDuration: 2350,
                                                        frequency: new Frequency(50, Frequency.UnitType.Hertz));

        private static ServoConfig? fs90R;
        private static ServoConfig? sg90;

        /// <summary>
        /// Represents the SG90 180 degree servo models. Angle: 0-180, Pulse: 500 - 2,200
        /// </summary>0
        public static ServoConfig SG90 => sg90 ??=
            new ServoConfig(
                new Frequency(50, Frequency.UnitType.Hertz),
                minimumAngle: new Angle(-90, AU.Degrees),
                maximumAngle: new Angle(90, AU.Degrees),
                minimumPulseDuration: 1000,
                maximumPulseDuration: 2000);

        /// <summary>
        /// Represents the FS90 continuous rotation servo models. Angle: 0-180, Pulse: 500 - 2,200
        /// </summary>0
        public static ServoConfig FS90R => fs90R ??=
            new ServoConfig(
                new Frequency(50, Frequency.UnitType.Hertz),
                minimumPulseDuration: 900,
                maximumPulseDuration: 2100);
    }
}