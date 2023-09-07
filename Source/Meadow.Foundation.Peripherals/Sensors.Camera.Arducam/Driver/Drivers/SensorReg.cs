namespace Meadow.Foundation.Sensors.Camera
{
    public struct SensorReg
    {
        public byte Register;
        public byte Value;

        public SensorReg(byte register, byte value)
        {
            Register = register;
            Value = value;
        }
    }
}