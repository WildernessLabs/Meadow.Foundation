namespace FTD2XX;

/// <summary>
/// Valid values for drive current options on FT2232H, FT4232H and FT232H devices.
/// </summary>
public static class FT_DRIVE_CURRENT
{
    /// <summary>
    /// 4mA drive current
    /// </summary>
    public const byte FT_DRIVE_CURRENT_4MA = 4;

    /// <summary>
    /// 8mA drive current
    /// </summary>
    public const byte FT_DRIVE_CURRENT_8MA = 8;

    /// <summary>
    /// 12mA drive current
    /// </summary>
    public const byte FT_DRIVE_CURRENT_12MA = 12;

    /// <summary>
    /// 16mA drive current
    /// </summary>
    public const byte FT_DRIVE_CURRENT_16MA = 16;
}
