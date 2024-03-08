namespace MicroJson_Sample;

public struct CalibrationPoint
{
    /// <summary>
    /// Creates a CalibrationPoint instance
    /// </summary>
    /// <param name="rawX">The raw touchscreen X value</param>
    /// <param name="screenX">The equivalent screen X coordinate for the raw X value</param>
    /// <param name="rawY">The raw touchscreen Y value</param>
    /// <param name="screenY">The equivalent screen Y coordinate for the raw Y value</param>
    public CalibrationPoint(int rawX, int screenX, int rawY, int screenY)
    {
        ScreenX = screenX;
        ScreenY = screenY;
        RawX = rawX;
        RawY = rawY;
    }

    /// <summary>
    /// The equivalent screen X coordinate for the raw X value
    /// </summary>
    public int ScreenX { get; set; }
    /// <summary>
    /// The equivalent screen Y coordinate for the raw Y value
    /// </summary>
    public int ScreenY { get; set; }
    /// <summary>
    /// The raw touchscreen X value
    /// </summary>
    public int RawX { get; set; }
    /// <summary>
    /// The raw touchscreen Y value
    /// </summary>
    public int RawY { get; set; }

    /// <inheritdoc/>
    public override string ToString()
    {
        return $"({RawX}->{ScreenX}, {RawY}->{ScreenY})";
    }
}