namespace T3ConfigConsole.Devices;

/// <summary>
/// Public register address constants mirroring T38i8o6do driver internals,
/// for use in the low-level configuration console.
/// </summary>
public static class T38i8o6doRegisters
{
    // ── Device identity (registers 0-8) ─────────────────────────────────────
    public const ushort SerialNumber    = 0;  // 4 regs, 32-bit (low word first)
    public const ushort FirmwareVersion = 4;  // 1 reg  — actual = raw / 10
    public const ushort ModbusAddress   = 6;  // 1 reg
    public const ushort ProductModel    = 7;  // 1 reg
    public const ushort HardwareRev     = 8;  // 1 reg

    // ── Analog input values (registers 10-25, 2 regs per channel) ──────────
    public const ushort AiChannelBase     = 10;   // channels 0-7 → 10-25

    // ── Input auto/manual mode (0 = Auto, 1 = Manual) ──────────────────────
    public const ushort AutoManualInBase  = 149;  // channels 0-7 → 149-156

    // ── Analog / digital type select (0 = digital, 1 = analog) ──────────────
    public const ushort AiDiAiBase        = 157;  // channels 0-7 → 157-164

    // ── Input status flags ──────────────────────────────────────────────────
    public const ushort InputStatusBase   = 189;  // channels 0-7 → 189-196

    // ── Input range register ────────────────────────────────────────────────
    public const ushort InputRangeBase    = 225;  // channels 0-7 → 225-232

    // ── Analog output values (signed int16, actual = raw / 10) ──────────────
    public const ushort AoChannelBase     = 100;  // channels 0-7 → 100-107

    // ── Digital output values (0 = OFF, 1 = ON) ─────────────────────────────
    public const ushort DoChannelBase     = 108;  // channels 0-5 → 108-113

    // ── Output auto/manual mode (0 = Auto, 1 = Manual) ──────────────────────
    public const ushort AutoManualOutBase = 240;  // channels 0-7 → 240-247

    // ── Output status flags ──────────────────────────────────────────────────
    public const ushort OutputStatusBase  = 254;  // channels 0-7 → 254-261

    // ── Output range register ────────────────────────────────────────────────
    // High byte: type (0 = digital, 1 = analog)
    // Low  byte: range code (see OutputPoint.DescribeRange)
    public const ushort OutputRangeBase   = 268;  // channels 0-7 → 268-275

    // ── Analog / digital type select (0 = digital, 1 = analog) ──────────────
    public const ushort OutputAdBase      = 342;  // channels 0-7 → 342-349

    public const int OutputCount = 8;
}
