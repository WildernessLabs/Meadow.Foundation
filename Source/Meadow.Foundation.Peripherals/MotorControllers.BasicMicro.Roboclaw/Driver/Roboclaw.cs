using Meadow.Hardware;
using Meadow.Units;

namespace Meadow.MotorControllers.BasicMicro;

public partial class Roboclaw
{
    public const byte DefaultAddress = 0x80;
    public const int DefaultBaudRate = 38400;

    internal ControllerComms Comms { get; private set; }
    public MotorInfo[] Motors { get; }

    public Roboclaw(ISerialPort port, byte address = DefaultAddress)
    {
        Comms = new ControllerComms(port, address);
        Comms.Open();
        Motors = new MotorInfo[]
            {
                new MotorInfo(this, 1),
                new MotorInfo(this, 2),
            };

    }

    public Temperature GetTemperature1()
    {
        var register = Comms.ReadShort(ControllerCommand.GETTEMP);
        return new Temperature(register / 10d, Temperature.UnitType.Celsius);
    }

    public Temperature GetTemperature2()
    {
        var register = Comms.ReadShort(ControllerCommand.GETTEMP2);
        return new Temperature(register / 10d, Temperature.UnitType.Celsius);
    }

    public Voltage GetMainBatteryVoltage()
    {
        var register = Comms.ReadShort(ControllerCommand.GETMBATT);
        return new Voltage(register / 10d, Voltage.UnitType.Volts);
    }

    public Voltage GetLogicBatteryVoltage()
    {
        var register = Comms.ReadShort(ControllerCommand.GETLBATT);
        return new Voltage(register / 10d, Voltage.UnitType.Volts);
    }
}
