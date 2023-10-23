using Meadow.Hardware;
using Meadow.Units;

namespace Meadow.Foundation.ICs.DigiPots;

public class Mcp4131 : Mcp4xx1
{
    private const int MaxSteps = 129;

    public Mcp4131(ISpiBus spiBus, IDigitalOutputPort chipSelect, Resistance maxResistance) :
        base(spiBus, chipSelect, 1, maxResistance, MaxSteps)
    {
    }
}

public class Mcp4132 : Mcp4xx2
{
    private const int MaxSteps = 129;

    public Mcp4132(ISpiBus spiBus, IDigitalOutputPort chipSelect, Resistance maxResistance) :
        base(spiBus, chipSelect, 1, maxResistance, MaxSteps)
    {
    }
}

public class Mcp4141 : Mcp4xx1
{
    private const int MaxSteps = 129;

    public Mcp4141(ISpiBus spiBus, IDigitalOutputPort chipSelect, Resistance maxResistance) :
        base(spiBus, chipSelect, 1, maxResistance, MaxSteps)
    {
    }
}

public class Mcp4142 : Mcp4xx2
{
    private const int MaxSteps = 129;

    public Mcp4142(ISpiBus spiBus, IDigitalOutputPort chipSelect, Resistance maxResistance) :
        base(spiBus, chipSelect, 1, maxResistance, MaxSteps)
    {
    }
}

public class Mcp4151 : Mcp4xx1
{
    private const int MaxSteps = 257;

    public Mcp4151(ISpiBus spiBus, IDigitalOutputPort chipSelect, Resistance maxResistance) :
        base(spiBus, chipSelect, 1, maxResistance, MaxSteps)
    {
    }
}

public class Mcp4152 : Mcp4xx2
{
    private const int MaxSteps = 257;

    public Mcp4152(ISpiBus spiBus, IDigitalOutputPort chipSelect, Resistance maxResistance) :
        base(spiBus, chipSelect, 1, maxResistance, MaxSteps)
    {
    }
}

public class Mcp4161 : Mcp4xx1
{
    private const int MaxSteps = 257;

    public Mcp4161(ISpiBus spiBus, IDigitalOutputPort chipSelect, Resistance maxResistance) :
        base(spiBus, chipSelect, 1, maxResistance, MaxSteps)
    {
    }
}

public class Mcp4162 : Mcp4xx2
{
    private const int MaxSteps = 257;

    public Mcp4162(ISpiBus spiBus, IDigitalOutputPort chipSelect, Resistance maxResistance) :
        base(spiBus, chipSelect, 1, maxResistance, MaxSteps)
    {
    }
}

public class Mcp4231 : Mcp4xx1
{
    private const int MaxSteps = 129;

    public Mcp4231(ISpiBus spiBus, IDigitalOutputPort chipSelect, Resistance maxResistance) :
        base(spiBus, chipSelect, 2, maxResistance, MaxSteps)
    {
    }
}

public class Mcp4232 : Mcp4xx2
{
    private const int MaxSteps = 129;

    public Mcp4232(ISpiBus spiBus, IDigitalOutputPort chipSelect, Resistance maxResistance) :
        base(spiBus, chipSelect, 2, maxResistance, MaxSteps)
    {
    }
}

public class Mcp4241 : Mcp4xx1
{
    private const int MaxSteps = 129;

    public Mcp4241(ISpiBus spiBus, IDigitalOutputPort chipSelect, Resistance maxResistance) :
        base(spiBus, chipSelect, 2, maxResistance, MaxSteps)
    {
    }
}

public class Mcp4242 : Mcp4xx2
{
    private const int MaxSteps = 129;

    public Mcp4242(ISpiBus spiBus, IDigitalOutputPort chipSelect, Resistance maxResistance) :
        base(spiBus, chipSelect, 2, maxResistance, MaxSteps)
    {
    }
}

public class Mcp4251 : Mcp4xx1
{
    private const int MaxSteps = 257;

    public Mcp4251(ISpiBus spiBus, IDigitalOutputPort chipSelect, Resistance maxResistance) :
        base(spiBus, chipSelect, 2, maxResistance, MaxSteps)
    {
    }
}

public class Mcp4252 : Mcp4xx2
{
    private const int MaxSteps = 257;

    public Mcp4252(ISpiBus spiBus, IDigitalOutputPort chipSelect, Resistance maxResistance) :
        base(spiBus, chipSelect, 2, maxResistance, MaxSteps)
    {
    }
}

public class Mcp4261 : Mcp4xx1
{
    private const int MaxSteps = 257;

    public Mcp4261(ISpiBus spiBus, IDigitalOutputPort chipSelect, Resistance maxResistance) :
        base(spiBus, chipSelect, 2, maxResistance, MaxSteps)
    {
    }
}

public class Mcp4262 : Mcp4xx2
{
    private const int MaxSteps = 257;

    public Mcp4262(ISpiBus spiBus, IDigitalOutputPort chipSelect, Resistance maxResistance) :
        base(spiBus, chipSelect, 2, maxResistance, MaxSteps)
    {
    }
}
