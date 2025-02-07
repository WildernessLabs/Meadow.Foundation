namespace Meadow.Foundation.MotorControllers.StepperOnline;

/// <summary>
/// The stepper motor type
/// </summary>
public enum MotorType
{
    /// <summary>
    /// With a sensor
    /// </summary>
    Sensored = 0x0f,
    /// <summary>
    /// Without a sensor
    /// </summary>
    Sensorless = 0x10
}