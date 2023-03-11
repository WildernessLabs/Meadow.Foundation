using Meadow.Units;
using System;

namespace Meadow.MotorControllers.BasicMicro;

public class MotorInfo
{
    private Roboclaw Controller { get; }
    public int Number { get; }

    internal MotorInfo(Roboclaw controller, int motorNumber)
    {
        Controller = controller;
        Number = motorNumber;
    }

    public Current GetCurrentDraw()
    {
        var currents = Controller.Comms.ReadUInt32(ControllerCommand.GETCURRENTS);
        switch (Number)
        {
            case 1:
                var c = currents >> 16;
                return new Current(c, Current.UnitType.Amps);
            case 2:
                var c2 = currents & 0xffff;
                return new Current(c2, Current.UnitType.Amps);
        }

        throw new NotSupportedException();

    }

    public int GetSpeed()
    {
        switch (Number)
        {
            case 1:
                var r1 = Controller.Comms.ReadInt32(ControllerCommand.GETM1ISPEED);
                return r1;
            case 2:
                var r2 = Controller.Comms.ReadInt32(ControllerCommand.GETM2ISPEED);
                return r2;
        }
        throw new NotSupportedException();
    }

    public int GetEncoderValue()
    {
        switch (Number)
        {
            case 1:
                // these reads send back a singed 32-bit int, plus a byte. No idea wtf the byte is
                var r1 = Controller.Comms.ReadInt32_byte(ControllerCommand.GETM1ENC);
                return r1.Item1;
            case 2:
                var r2 = Controller.Comms.ReadInt32_byte(ControllerCommand.GETM2ENC);
                return r2.Item1;
        }
        throw new NotSupportedException();
    }

}
