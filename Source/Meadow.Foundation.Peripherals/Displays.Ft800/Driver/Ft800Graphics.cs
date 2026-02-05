using System;
using System.Collections.Generic;
using System.Text;

namespace Meadow.Foundation.Displays;

/// <summary>
/// GPU-accelerated graphics class for the FT800 display controller
/// </summary>
/// <remarks>
/// Ft800Graphics provides native FT800 display list commands for hardware-accelerated
/// rendering. Use this class instead of MicroGraphics when you want to leverage the
/// FT800's GPU capabilities including anti-aliased primitives, built-in widgets,
/// ROM fonts, and efficient rendering.
/// </remarks>
public partial class Ft800Graphics
{
    private readonly Ft800 display;
    private readonly List<uint> displayList;
    private readonly int screenWidth;
    private readonly int screenHeight;

    private Color currentColor = Color.White;
    private byte currentAlpha = 255;
    private int currentLineWidth = 16;  // In 1/16 pixel units (1 pixel default)
    private int currentPointSize = 16;  // In 1/16 pixel units (1 pixel default)

    /// <summary>
    /// Gets the display width in pixels
    /// </summary>
    public int Width => screenWidth;

    /// <summary>
    /// Gets the display height in pixels
    /// </summary>
    public int Height => screenHeight;

    /// <summary>
    /// Gets or sets the current drawing color
    /// </summary>
    public Color PenColor
    {
        get => currentColor;
        set => currentColor = value;
    }

    /// <summary>
    /// Gets or sets the current alpha value (0-255)
    /// </summary>
    public byte Alpha
    {
        get => currentAlpha;
        set => currentAlpha = value;
    }

    /// <summary>
    /// Gets or sets the current line width in pixels
    /// </summary>
    public float LineWidth
    {
        get => currentLineWidth / 16f;
        set => currentLineWidth = (int)(value * 16);
    }

    /// <summary>
    /// Gets or sets the current point size in pixels
    /// </summary>
    public float PointSize
    {
        get => currentPointSize / 16f;
        set => currentPointSize = (int)(value * 16);
    }

    /// <summary>
    /// Creates a new Ft800Graphics instance
    /// </summary>
    /// <param name="display">The FT800 display driver</param>
    public Ft800Graphics(Ft800 display)
    {
        this.display = display ?? throw new ArgumentNullException(nameof(display));
        this.screenWidth = display.Width;
        this.screenHeight = display.Height;
        this.displayList = new List<uint>(256);
    }

    // ========================================================================
    // Display List Management
    // ========================================================================

    /// <summary>
    /// Begin a new display list
    /// </summary>
    /// <remarks>
    /// Call this before drawing any primitives. After drawing, call
    /// EndDisplayList() and then SwapDisplayList() to show the result.
    /// </remarks>
    public void BeginDisplayList()
    {
        displayList.Clear();
    }

    /// <summary>
    /// End the current display list
    /// </summary>
    /// <remarks>
    /// This adds the DISPLAY command to mark the end of the display list.
    /// </remarks>
    public void EndDisplayList()
    {
        AddCommand(Ft800Defs.DL_DISPLAY);
    }

    /// <summary>
    /// Write the display list to FT800 and swap it to the screen
    /// </summary>
    public void SwapDisplayList()
    {
        // Convert display list to byte array
        var data = new byte[displayList.Count * 4];
        for (int i = 0; i < displayList.Count; i++)
        {
            var cmd = displayList[i];
            data[i * 4 + 0] = (byte)(cmd & 0xFF);
            data[i * 4 + 1] = (byte)((cmd >> 8) & 0xFF);
            data[i * 4 + 2] = (byte)((cmd >> 16) & 0xFF);
            data[i * 4 + 3] = (byte)((cmd >> 24) & 0xFF);
        }

        // Write to RAM_DL
        display.WriteMemory(Ft800Defs.RAM_DL, data);

        // Swap the display list
        display.WriteRegister(Ft800Defs.REG_DLSWAP, Ft800Defs.DLSWAP_FRAME);
    }

    /// <summary>
    /// Add a command to the display list
    /// </summary>
    /// <param name="command">The 32-bit display list command</param>
    protected void AddCommand(uint command)
    {
        displayList.Add(command);
    }

    // ========================================================================
    // Clearing
    // ========================================================================

    /// <summary>
    /// Clear the display with a color
    /// </summary>
    /// <param name="color">The clear color</param>
    public void Clear(Color color)
    {
        AddCommand(Ft800Defs.DL_CLEAR_COLOR_RGB |
            ((uint)color.R << 16) | ((uint)color.G << 8) | color.B);
        AddCommand(Ft800Defs.DL_CLEAR_COLOR_A | color.A);
        AddCommand(Ft800Defs.DL_CLEAR | Ft800Defs.CLEAR_ALL);
    }

    /// <summary>
    /// Clear the display with black
    /// </summary>
    public void Clear()
    {
        Clear(Color.Black);
    }

    // ========================================================================
    // State Management
    // ========================================================================

    /// <summary>
    /// Set the current drawing color
    /// </summary>
    /// <param name="color">The color to use for subsequent drawing operations</param>
    public void SetColor(Color color)
    {
        currentColor = color;
        AddCommand(Ft800Defs.DL_COLOR_RGB |
            ((uint)color.R << 16) | ((uint)color.G << 8) | color.B);
        AddCommand(Ft800Defs.DL_COLOR_A | color.A);
    }

    /// <summary>
    /// Set the current drawing color
    /// </summary>
    /// <param name="r">Red component (0-255)</param>
    /// <param name="g">Green component (0-255)</param>
    /// <param name="b">Blue component (0-255)</param>
    public void SetColor(byte r, byte g, byte b)
    {
        SetColor(new Color(r, g, b));
    }

    /// <summary>
    /// Set the current alpha value
    /// </summary>
    /// <param name="alpha">Alpha value (0=transparent, 255=opaque)</param>
    public void SetAlpha(byte alpha)
    {
        currentAlpha = alpha;
        AddCommand(Ft800Defs.DL_COLOR_A | alpha);
    }

    /// <summary>
    /// Set the line width for subsequent line drawing
    /// </summary>
    /// <param name="width">Line width in pixels (supports fractional values)</param>
    public void SetLineWidth(float width)
    {
        currentLineWidth = (int)(width * 16);
        AddCommand(Ft800Defs.DL_LINE_WIDTH | (uint)currentLineWidth);
    }

    /// <summary>
    /// Set the point size for subsequent point drawing
    /// </summary>
    /// <param name="size">Point radius in pixels (supports fractional values)</param>
    public void SetPointSize(float size)
    {
        currentPointSize = (int)(size * 16);
        AddCommand(Ft800Defs.DL_POINT_SIZE | (uint)currentPointSize);
    }

    /// <summary>
    /// Save the current graphics context
    /// </summary>
    public void SaveContext()
    {
        AddCommand(Ft800Defs.DL_SAVE_CONTEXT);
    }

    /// <summary>
    /// Restore a previously saved graphics context
    /// </summary>
    public void RestoreContext()
    {
        AddCommand(Ft800Defs.DL_RESTORE_CONTEXT);
    }

    // ========================================================================
    // Scissor / Clipping
    // ========================================================================

    /// <summary>
    /// Set the scissor (clipping) rectangle
    /// </summary>
    /// <param name="x">X position</param>
    /// <param name="y">Y position</param>
    /// <param name="width">Width</param>
    /// <param name="height">Height</param>
    public void SetScissor(int x, int y, int width, int height)
    {
        AddCommand(Ft800Defs.DL_SCISSOR_XY | ((uint)x << 10) | (uint)y);
        AddCommand(Ft800Defs.DL_SCISSOR_SIZE | ((uint)width << 10) | (uint)height);
    }

    /// <summary>
    /// Reset the scissor rectangle to full screen
    /// </summary>
    public void ResetScissor()
    {
        SetScissor(0, 0, screenWidth, screenHeight);
    }

    // ========================================================================
    // Tagging (for touch detection)
    // ========================================================================

    /// <summary>
    /// Set the tag value for subsequent drawing operations
    /// </summary>
    /// <param name="tag">Tag value (1-255, 0 means no tag)</param>
    /// <remarks>
    /// Use tags to identify which object was touched. When the user touches
    /// the screen, you can read the tag value with Ft800.ReadTouchTag().
    /// </remarks>
    public void SetTag(byte tag)
    {
        AddCommand(Ft800Defs.DL_TAG | tag);
    }

    /// <summary>
    /// Clear the tag (disable tagging for subsequent objects)
    /// </summary>
    public void ClearTag()
    {
        AddCommand(Ft800Defs.DL_TAG | 0);
    }

    // ========================================================================
    // Blend Functions
    // ========================================================================

    /// <summary>
    /// Set the blend function
    /// </summary>
    /// <param name="src">Source blend factor</param>
    /// <param name="dst">Destination blend factor</param>
    public void SetBlendFunc(byte src, byte dst)
    {
        AddCommand(Ft800Defs.DL_BLEND_FUNC | ((uint)src << 3) | dst);
    }

    /// <summary>
    /// Reset blend function to default (src alpha, one minus src alpha)
    /// </summary>
    public void ResetBlendFunc()
    {
        SetBlendFunc(Ft800Defs.BLEND_SRC_ALPHA, Ft800Defs.BLEND_ONE_MINUS_SRC_ALPHA);
    }

    // ========================================================================
    // Helper Methods
    // ========================================================================

    /// <summary>
    /// Convert pixel coordinates to VERTEX2F format (1/16 pixel precision)
    /// </summary>
    protected uint Vertex2F(int x, int y)
    {
        // VERTEX2F format: [01][x:15][y:15] where x,y are in 1/16 pixel units
        return Ft800Defs.DL_VERTEX2F |
            (((uint)(x * 16) & 0x7FFF) << 15) |
            ((uint)(y * 16) & 0x7FFF);
    }

    /// <summary>
    /// Convert pixel coordinates to VERTEX2II format
    /// </summary>
    protected uint Vertex2II(int x, int y, byte handle = 0, byte cell = 0)
    {
        // VERTEX2II format: [10][x:9][y:9][handle:5][cell:7]
        return Ft800Defs.DL_VERTEX2II |
            ((uint)(x & 0x1FF) << 21) |
            ((uint)(y & 0x1FF) << 12) |
            ((uint)(handle & 0x1F) << 7) |
            (uint)(cell & 0x7F);
    }
}
