namespace T3ConfigConsole.Devices;

public class InputPoint
{
    public int Index { get; set; }
    public ushort ValueRegister { get; set; }
    public ushort TypeRegister { get; set; }
    public ushort ModeRegister { get; set; }
    public ushort RangeRegister { get; set; }
    public ushort StatusRegister { get; set; }

    public bool IsAnalog { get; set; }
    public bool IsManual { get; set; }
    public ushort RangeCode { get; set; }
    public ushort RawValue { get; set; }
    public ushort RawStatus { get; set; }

    public string TypeLabel  => IsAnalog ? "Analog" : "Digital";
    public string ModeLabel  => IsManual ? "Manual" : "Auto";
    public string RangeLabel => IsAnalog ? DescribeRange(RangeCode) : "Digital";

    public string ValueLabel => IsAnalog
        ? $"{(short)RawValue / 100.0:F2}"
        : RawValue > 0 ? "ON" : "OFF";

    public static string[] RangeOptions = new string[]
    {
        "Not Used",
        "Y3K_40_150C",
        "Y3K_40_300F",
        "R10K_40_120C",
        "R10K_40_250F",
        "R3K_40_150C",
        "R3K_40_300F",
        "KM10K_40_120C",
        "KM10K_40_250F",
        "A10K_50_110C",
        "A10K_60_200F",
        "0-5V",
        "0-100Amps",
        "4-20mA",
        "0-20mA",
        "0-10V"
    };

    private static string DescribeRange(ushort code)
    {
        if (code < RangeOptions.Length) return RangeOptions[code];
        return $"Code {code}";
    }
}
