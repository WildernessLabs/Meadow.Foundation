using Meadow.Units;

namespace Meadow.Foundation.Servos
{
    public static class NamedServoConfigs
    {
        /// <summary>
        /// Represents an ideal 180? servo that has a minimum pulse of 1ms and a max of 2ms.
        /// </summary>
        public static ServoConfig Ideal180Servo = new ServoConfig();
        
        /// <summary>
        /// Represents an ideal 270? servo that has a minimum pulse of 1ms and a max of 2ms.
        /// </summary>
        public static ServoConfig Ideal270Servo = new ServoConfig(maximumAngle: new Angle(270, Angle.UnitType.Degrees));

        /// <summary>
        /// Represents an ideal continuous rotation servo that has a min and max pulse of 1ms and 2ms.
        /// </summary>
        public static ServoConfig IdealContinuousRotationServo = new ServoConfig(-1, -1);

        /// <summary>
        /// Represents the BlueBird BMS models. Maximum angle is 120. Pulse range is 900?s - 2,100?s.
        /// See: https://www.blue-bird-model.com/products_detail/309.htm
        /// </summary>
        public static ServoConfig BlueBirdBMS120 = new ServoConfig(minimumPulseDuration: 900, maximumPulseDuration: 2100, maximumAngle: new Angle(120, Angle.UnitType.Degrees));

        /// <summary>
        /// Represents the HiTec "Standard" servo models. Angle: 0-180, Pulse: 900 - 1,200
        /// </summary>0
        public static ServoConfig HiTecStandard = new ServoConfig(minimumPulseDuration: 900, maximumPulseDuration: 2100, maximumAngle: new Angle(180, Angle.UnitType.Degrees));

        /// <summary>
        /// Represents HiTec Digitial servo models with their default settings. Angle: 0-180, Pulse: 900 - 1,200
        /// </summary>0
        public static ServoConfig HiTechDigital = new ServoConfig(minimumPulseDuration: 900, maximumPulseDuration: 1200, maximumAngle: new Angle(180, Angle.UnitType.Degrees));

        /// <summary>
        /// Represents the JX HV 180 degree servo models. Angle: 0-180, Pulse: 500 - 2,500
        /// </summary>0
        public static ServoConfig JXHV180 = new ServoConfig(minimumPulseDuration: 500, maximumPulseDuration: 2500, maximumAngle: new Angle(180, Angle.UnitType.Degrees));

        /// <summary>
        /// Represents the SG90 180 degree servo models. Angle: 0-180, Pulse: 500 - 2,200
        /// </summary>0
        public static ServoConfig SG90 = new ServoConfig(minimumAngle: 0, maximumAngle: new Angle(180, Angle.UnitType.Degrees), minimumPulseDuration: 500, maximumPulseDuration: 2350, frequency: 50);
    }
}