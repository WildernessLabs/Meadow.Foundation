using Meadow.Foundation.Graphics.Buffers;
using Meadow.Peripherals.Displays;
using System;

namespace Meadow.Foundation.Displays;

/// <summary>
/// FT800 IPixelDisplay implementation for MicroGraphics compatibility
/// </summary>
public partial class Ft800
{
    private readonly BufferRgb565 imageBuffer;
    private uint bitmapAddress = 0;  // Address in RAM_G where bitmap is stored

    // ========================================================================
    // IPixelDisplay Implementation
    // ========================================================================

    /// <inheritdoc/>
    public int Width { get; }

    /// <inheritdoc/>
    public int Height { get; }

    /// <inheritdoc/>
    public ColorMode ColorMode => ColorMode.Format16bppRgb565;

    /// <inheritdoc/>
    public ColorMode SupportedColorModes => ColorMode.Format16bppRgb565;

    /// <inheritdoc/>
    public IPixelBuffer PixelBuffer => imageBuffer;

    /// <inheritdoc/>
    public void Clear(bool updateDisplay = false)
    {
        imageBuffer.Clear();
        if (updateDisplay)
        {
            Show();
        }
    }

    /// <inheritdoc/>
    public void Fill(Color fillColor, bool updateDisplay = false)
    {
        imageBuffer.Fill(fillColor);
        if (updateDisplay)
        {
            Show();
        }
    }

    /// <inheritdoc/>
    public void Fill(int x, int y, int width, int height, Color fillColor)
    {
        imageBuffer.Fill(x, y, width, height, fillColor);
    }

    /// <inheritdoc/>
    public void DrawPixel(int x, int y, Color color)
    {
        imageBuffer.SetPixel(x, y, color);
    }

    /// <inheritdoc/>
    public void DrawPixel(int x, int y, bool enabled)
    {
        imageBuffer.SetPixel(x, y, enabled ? Color.White : Color.Black);
    }

    /// <inheritdoc/>
    public void InvertPixel(int x, int y)
    {
        imageBuffer.InvertPixel(x, y);
    }

    /// <inheritdoc/>
    public void WriteBuffer(int x, int y, IPixelBuffer displayBuffer)
    {
        imageBuffer.WriteBuffer(x, y, displayBuffer);
    }

    /// <inheritdoc/>
    public void Show()
    {
        // Upload the pixel buffer to FT800 graphics RAM
        UploadBitmapToRam();

        // Build a display list that shows the bitmap
        BuildBitmapDisplayList();
    }

    /// <inheritdoc/>
    public void Show(int left, int top, int right, int bottom)
    {
        // For simplicity, just do a full update
        // A more optimized version could upload only the changed region
        Show();
    }

    /// <summary>
    /// Upload the pixel buffer to FT800 graphics RAM
    /// </summary>
    private void UploadBitmapToRam()
    {
        // Upload to RAM_G at address 0
        bitmapAddress = Ft800Defs.RAM_G;

        // The FT800 expects RGB565 in little-endian format, which matches our buffer
        WriteMemory(bitmapAddress, imageBuffer.Buffer);
    }

    /// <summary>
    /// Build a display list that renders the uploaded bitmap
    /// </summary>
    private void BuildBitmapDisplayList()
    {
        uint dlOffset = 0;

        // Clear the screen
        WriteMemory32(Ft800Defs.RAM_DL + dlOffset, Ft800Defs.DL_CLEAR_COLOR_RGB | 0x000000);
        dlOffset += 4;
        WriteMemory32(Ft800Defs.RAM_DL + dlOffset, Ft800Defs.DL_CLEAR | Ft800Defs.CLEAR_ALL);
        dlOffset += 4;

        // Set up bitmap handle 0 for our pixel buffer
        // BITMAP_SOURCE - point to RAM_G address
        WriteMemory32(Ft800Defs.RAM_DL + dlOffset, Ft800Defs.DL_BITMAP_SOURCE | bitmapAddress);
        dlOffset += 4;

        // BITMAP_LAYOUT - format, stride, height
        // Format: RGB565 (7), Stride: Width * 2 bytes, Height: display height
        uint stride = (uint)(Width * 2);
        uint layout = ((uint)Ft800Defs.FORMAT_RGB565 << 19) | ((stride & 0x3FF) << 9) | ((uint)Height & 0x1FF);
        WriteMemory32(Ft800Defs.RAM_DL + dlOffset, Ft800Defs.DL_BITMAP_LAYOUT | layout);
        dlOffset += 4;

        // BITMAP_SIZE - filter, wrap, width, height
        // Filter: NEAREST (0), Wrap: BORDER (0), Width, Height
        uint size = ((uint)Width << 9) | (uint)Height;
        WriteMemory32(Ft800Defs.RAM_DL + dlOffset, Ft800Defs.DL_BITMAP_SIZE | size);
        dlOffset += 4;

        // Begin drawing bitmaps
        WriteMemory32(Ft800Defs.RAM_DL + dlOffset, Ft800Defs.DL_BEGIN | Ft800Defs.PRIM_BITMAPS);
        dlOffset += 4;

        // Draw at position (0, 0) using VERTEX2II for pixel-aligned positioning
        // VERTEX2II format: [10][x:9][y:9][handle:5][cell:7]
        uint vertex = Ft800Defs.DL_VERTEX2II | (0 << 21) | (0 << 12) | (0 << 7) | 0;
        WriteMemory32(Ft800Defs.RAM_DL + dlOffset, vertex);
        dlOffset += 4;

        // End drawing
        WriteMemory32(Ft800Defs.RAM_DL + dlOffset, Ft800Defs.DL_END);
        dlOffset += 4;

        // Display command (end of display list)
        WriteMemory32(Ft800Defs.RAM_DL + dlOffset, Ft800Defs.DL_DISPLAY);
        dlOffset += 4;

        // Swap the display list
        WriteRegister(Ft800Defs.REG_DLSWAP, Ft800Defs.DLSWAP_FRAME);
    }
}
