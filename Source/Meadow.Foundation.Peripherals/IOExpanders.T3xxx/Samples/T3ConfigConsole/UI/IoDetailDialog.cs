using T3ConfigConsole.Devices;
using Terminal.Gui;

namespace T3ConfigConsole.UI;

/// <summary>
/// Modal detail view for a single output point.
/// Shows decoded config, register addresses, and raw register values.
/// Runs via Application.Run() so it acts as a nested modal event loop.
/// </summary>
public class IoDetailDialog : Window
{
    public IoDetailDialog(OutputPoint point, T38i8o6doConsoleClient client)
        : base($" Output {point.Index + 1} — Details ")
    {
        X      = Pos.Center();
        Y      = Pos.Center();
        Width  = 66;
        Height = 24;

        var content = BuildContent(point);

        var textView = new TextView
        {
            X        = 1,
            Y        = 1,
            Width    = Dim.Fill() - 1,
            Height   = Dim.Fill() - 3,
            ReadOnly = true,
            Text     = content
        };

        var closeBtn = new Button("Close") { X = Pos.Center(), Y = Pos.AnchorEnd(1) };
        closeBtn.Clicked += () => Application.RequestStop();

        Add(textView, closeBtn);
    }

    private static string BuildContent(OutputPoint p)
    {
        int aoReg     = T38i8o6doRegisters.AoChannelBase     + p.Index;
        int modeReg   = T38i8o6doRegisters.AutoManualOutBase + p.Index;
        int rangeReg  = T38i8o6doRegisters.OutputRangeBase   + p.Index;
        int statusReg = T38i8o6doRegisters.OutputStatusBase  + p.Index;
        int adReg     = T38i8o6doRegisters.OutputAdBase       + p.Index;

        return string.Join("\n", new[]
        {
            $"  Output:       {p.Index + 1}",
            $"  Type:         {p.TypeLabel}",
            $"  Mode:         {p.ModeLabel}",
            $"  Range:        {p.RangeLabel}",
            $"  Value:        {p.ValueLabel}",
            "",
            "  Register Addresses:",
            $"    AO Value:     {aoReg,-6}  (0x{aoReg:X4})",
            $"    Mode:         {modeReg,-6}  (0x{modeReg:X4})",
            $"    Range:        {rangeReg,-6}  (0x{rangeReg:X4})",
            $"    Status:       {statusReg,-6}  (0x{statusReg:X4})",
            $"    AD Select:    {adReg,-6}  (0x{adReg:X4})",
            "",
            "  Raw Register Values:",
            $"    Value:    0x{p.RawValue:X4}  (signed: {(short)p.RawValue})",
            $"    Mode:     0x{p.RawMode:X4}",
            $"    Range:    0x{p.RawRange:X4}  (type={p.RawRange >> 8}, code={p.RawRange & 0xFF})",
            $"    Status:   0x{p.RawStatus:X4}",
        });
    }
}
