namespace T3ConfigConsole.Devices;

public class OutputPoint
{
    public int Index { get; set; }
    public bool IsAnalog { get; set; }
    public bool IsManual { get; set; }
    public byte RangeCode { get; set; }
    public ushort RawValue { get; set; }
    public ushort RawMode { get; set; }
    public ushort RawRange { get; set; }
    public ushort RawStatus { get; set; }

    public string TypeLabel  => IsAnalog ? "Analog"  : "Digital";
    public string ModeLabel  => IsManual ? "Manual"  : "Auto";
    public string RangeLabel => IsAnalog ? DescribeRange(RangeCode) : "Digital";

    public string ValueLabel => IsAnalog
        ? $"{(short)RawValue / 10.0:F1}"
        : RawValue > 0 ? "ON" : "OFF";

    private static string DescribeRange(byte code) => code switch
    {
        0 => "Not Used",
        1 => "0-10V",
        2 => "2-10V",
        3 => "0-5V",
        4 => "1-5V",
        5 => "4-20mA",
        6 => "0-20mA",
        _ => $"Code {code}"
    };
}
