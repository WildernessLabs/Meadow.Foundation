using System;
using System.Text;
using System.Threading;

namespace Meadow.Foundation.Displays;

/// <summary>
/// FT800Graphics widget drawing methods (buttons, sliders, gauges, etc.)
/// </summary>
/// <remarks>
/// Widget methods use the FT800 co-processor for rendering. They must be
/// called within a BeginCoprocessor/EndCoprocessor block, or use the
/// simplified DrawXxxImmediate methods which handle this automatically.
/// </remarks>
public partial class Ft800Graphics
{
    private uint cmdWritePointer;

    // ========================================================================
    // Co-processor Management
    // ========================================================================

    /// <summary>
    /// Begin a co-processor command sequence
    /// </summary>
    public void BeginCoprocessor()
    {
        WaitForCoprocessorReady();
        cmdWritePointer = display.ReadRegister(Ft800Defs.REG_CMD_WRITE);

        // Start a new display list
        WriteCoprocessorCommand(Ft800Defs.CMD_DLSTART);
    }

    /// <summary>
    /// End a co-processor command sequence and execute
    /// </summary>
    public void EndCoprocessor()
    {
        // Swap display list
        WriteCoprocessorCommand(Ft800Defs.CMD_SWAP);

        // Update write pointer to execute commands
        display.WriteRegister(Ft800Defs.REG_CMD_WRITE, cmdWritePointer);

        WaitForCoprocessorReady();
    }

    /// <summary>
    /// Write a command to the co-processor FIFO
    /// </summary>
    private void WriteCoprocessorCommand(uint command)
    {
        display.WriteMemory32(Ft800Defs.RAM_CMD + cmdWritePointer, command);
        cmdWritePointer = (cmdWritePointer + 4) % Ft800Defs.RAM_CMD_SIZE;
    }

    /// <summary>
    /// Write a string to the co-processor FIFO (null-terminated, 4-byte aligned)
    /// </summary>
    private void WriteCoprocessorString(string text)
    {
        var bytes = Encoding.ASCII.GetBytes(text + "\0");
        // Pad to 4-byte alignment
        int paddedLength = (bytes.Length + 3) & ~3;
        var padded = new byte[paddedLength];
        Array.Copy(bytes, padded, bytes.Length);

        display.WriteMemory(Ft800Defs.RAM_CMD + cmdWritePointer, padded);
        cmdWritePointer = (cmdWritePointer + (uint)paddedLength) % Ft800Defs.RAM_CMD_SIZE;
    }

    /// <summary>
    /// Wait for the co-processor to finish
    /// </summary>
    private void WaitForCoprocessorReady()
    {
        int timeout = 1000;
        while (timeout-- > 0)
        {
            var read = display.ReadRegister(Ft800Defs.REG_CMD_READ);
            var write = display.ReadRegister(Ft800Defs.REG_CMD_WRITE);
            if (read == write) return;
            Thread.Sleep(1);
        }
        throw new TimeoutException("FT800 co-processor timeout");
    }

    // ========================================================================
    // Text
    // ========================================================================

    /// <summary>
    /// Draw text using an FT800 ROM font
    /// </summary>
    /// <param name="x">X coordinate</param>
    /// <param name="y">Y coordinate</param>
    /// <param name="text">Text to display</param>
    /// <param name="color">Text color</param>
    /// <param name="font">ROM font to use (default Size25)</param>
    /// <param name="options">Text options (centering, etc.)</param>
    public void DrawText(int x, int y, string text, Color color, Ft800Font font = Ft800Font.Size25, ushort options = 0)
    {
        BeginCoprocessor();

        // Set color
        WriteCoprocessorCommand(Ft800Defs.DL_CLEAR_COLOR_RGB | 0x000000);
        WriteCoprocessorCommand(Ft800Defs.DL_CLEAR | Ft800Defs.CLEAR_ALL);
        WriteCoprocessorCommand(Ft800Defs.DL_COLOR_RGB |
            ((uint)color.R << 16) | ((uint)color.G << 8) | color.B);

        // CMD_TEXT: cmd, x, y, font, options, string
        WriteCoprocessorCommand(Ft800Defs.CMD_TEXT);
        WriteCoprocessorCommand((uint)((y << 16) | (x & 0xFFFF)));
        WriteCoprocessorCommand((uint)((options << 16) | ((int)font & 0xFFFF)));
        WriteCoprocessorString(text);

        WriteCoprocessorCommand(Ft800Defs.DL_DISPLAY);
        EndCoprocessor();
    }

    /// <summary>
    /// Draw text to the current display list (must be within Begin/End block)
    /// </summary>
    public void DrawTextInline(int x, int y, string text, Color color, Ft800Font font = Ft800Font.Size25, ushort options = 0)
    {
        SetColor(color);
        WriteCoprocessorCommand(Ft800Defs.CMD_TEXT);
        WriteCoprocessorCommand((uint)((y << 16) | (x & 0xFFFF)));
        WriteCoprocessorCommand((uint)((options << 16) | ((int)font & 0xFFFF)));
        WriteCoprocessorString(text);
    }

    // ========================================================================
    // Button
    // ========================================================================

    /// <summary>
    /// Draw a 3D-styled button with text
    /// </summary>
    /// <param name="x">X coordinate</param>
    /// <param name="y">Y coordinate</param>
    /// <param name="width">Button width</param>
    /// <param name="height">Button height</param>
    /// <param name="font">Font for button text</param>
    /// <param name="text">Button label text</param>
    /// <param name="options">Button options (OPT_FLAT for flat style)</param>
    /// <param name="tag">Touch tag for this button (1-255)</param>
    public void DrawButton(int x, int y, int width, int height, Ft800Font font, string text, ushort options = 0, byte tag = 0)
    {
        BeginCoprocessor();

        WriteCoprocessorCommand(Ft800Defs.DL_CLEAR_COLOR_RGB | 0x000000);
        WriteCoprocessorCommand(Ft800Defs.DL_CLEAR | Ft800Defs.CLEAR_ALL);

        if (tag > 0)
        {
            WriteCoprocessorCommand(Ft800Defs.DL_TAG | tag);
        }

        // CMD_BUTTON: cmd, x, y, w, h, font, options, string
        WriteCoprocessorCommand(Ft800Defs.CMD_BUTTON);
        WriteCoprocessorCommand((uint)((y << 16) | (x & 0xFFFF)));
        WriteCoprocessorCommand((uint)((height << 16) | (width & 0xFFFF)));
        WriteCoprocessorCommand((uint)((options << 16) | ((int)font & 0xFFFF)));
        WriteCoprocessorString(text);

        WriteCoprocessorCommand(Ft800Defs.DL_DISPLAY);
        EndCoprocessor();
    }

    // ========================================================================
    // Keys (Keyboard Row)
    // ========================================================================

    /// <summary>
    /// Draw a row of keyboard-style keys
    /// </summary>
    /// <param name="x">X coordinate</param>
    /// <param name="y">Y coordinate</param>
    /// <param name="width">Total width</param>
    /// <param name="height">Height</param>
    /// <param name="font">Font for key labels</param>
    /// <param name="keys">String of key characters (e.g., "QWERTY")</param>
    /// <param name="options">Key options</param>
    public void DrawKeys(int x, int y, int width, int height, Ft800Font font, string keys, ushort options = 0)
    {
        BeginCoprocessor();

        WriteCoprocessorCommand(Ft800Defs.DL_CLEAR_COLOR_RGB | 0x000000);
        WriteCoprocessorCommand(Ft800Defs.DL_CLEAR | Ft800Defs.CLEAR_ALL);

        WriteCoprocessorCommand(Ft800Defs.CMD_KEYS);
        WriteCoprocessorCommand((uint)((y << 16) | (x & 0xFFFF)));
        WriteCoprocessorCommand((uint)((height << 16) | (width & 0xFFFF)));
        WriteCoprocessorCommand((uint)((options << 16) | ((int)font & 0xFFFF)));
        WriteCoprocessorString(keys);

        WriteCoprocessorCommand(Ft800Defs.DL_DISPLAY);
        EndCoprocessor();
    }

    // ========================================================================
    // Progress Bar
    // ========================================================================

    /// <summary>
    /// Draw a progress bar
    /// </summary>
    /// <param name="x">X coordinate</param>
    /// <param name="y">Y coordinate</param>
    /// <param name="width">Bar width</param>
    /// <param name="height">Bar height</param>
    /// <param name="value">Current value</param>
    /// <param name="range">Maximum value</param>
    /// <param name="options">Progress bar options</param>
    public void DrawProgress(int x, int y, int width, int height, ushort value, ushort range, ushort options = 0)
    {
        BeginCoprocessor();

        WriteCoprocessorCommand(Ft800Defs.DL_CLEAR_COLOR_RGB | 0x000000);
        WriteCoprocessorCommand(Ft800Defs.DL_CLEAR | Ft800Defs.CLEAR_ALL);

        WriteCoprocessorCommand(Ft800Defs.CMD_PROGRESS);
        WriteCoprocessorCommand((uint)((y << 16) | (x & 0xFFFF)));
        WriteCoprocessorCommand((uint)((height << 16) | (width & 0xFFFF)));
        WriteCoprocessorCommand((uint)((value << 16) | options));
        WriteCoprocessorCommand(range);

        WriteCoprocessorCommand(Ft800Defs.DL_DISPLAY);
        EndCoprocessor();
    }

    // ========================================================================
    // Slider
    // ========================================================================

    /// <summary>
    /// Draw a slider control
    /// </summary>
    /// <param name="x">X coordinate</param>
    /// <param name="y">Y coordinate</param>
    /// <param name="width">Slider width</param>
    /// <param name="height">Slider height</param>
    /// <param name="value">Current value</param>
    /// <param name="range">Maximum value</param>
    /// <param name="options">Slider options</param>
    /// <param name="tag">Touch tag</param>
    public void DrawSlider(int x, int y, int width, int height, ushort value, ushort range, ushort options = 0, byte tag = 0)
    {
        BeginCoprocessor();

        WriteCoprocessorCommand(Ft800Defs.DL_CLEAR_COLOR_RGB | 0x000000);
        WriteCoprocessorCommand(Ft800Defs.DL_CLEAR | Ft800Defs.CLEAR_ALL);

        if (tag > 0)
        {
            WriteCoprocessorCommand(Ft800Defs.DL_TAG | tag);
        }

        WriteCoprocessorCommand(Ft800Defs.CMD_SLIDER);
        WriteCoprocessorCommand((uint)((y << 16) | (x & 0xFFFF)));
        WriteCoprocessorCommand((uint)((height << 16) | (width & 0xFFFF)));
        WriteCoprocessorCommand((uint)((value << 16) | options));
        WriteCoprocessorCommand(range);

        WriteCoprocessorCommand(Ft800Defs.DL_DISPLAY);
        EndCoprocessor();
    }

    // ========================================================================
    // Scrollbar
    // ========================================================================

    /// <summary>
    /// Draw a scrollbar control
    /// </summary>
    /// <param name="x">X coordinate</param>
    /// <param name="y">Y coordinate</param>
    /// <param name="width">Scrollbar width</param>
    /// <param name="height">Scrollbar height</param>
    /// <param name="value">Current position</param>
    /// <param name="size">Handle size</param>
    /// <param name="range">Total range</param>
    /// <param name="options">Scrollbar options</param>
    public void DrawScrollbar(int x, int y, int width, int height, ushort value, ushort size, ushort range, ushort options = 0)
    {
        BeginCoprocessor();

        WriteCoprocessorCommand(Ft800Defs.DL_CLEAR_COLOR_RGB | 0x000000);
        WriteCoprocessorCommand(Ft800Defs.DL_CLEAR | Ft800Defs.CLEAR_ALL);

        WriteCoprocessorCommand(Ft800Defs.CMD_SCROLLBAR);
        WriteCoprocessorCommand((uint)((y << 16) | (x & 0xFFFF)));
        WriteCoprocessorCommand((uint)((height << 16) | (width & 0xFFFF)));
        WriteCoprocessorCommand((uint)((value << 16) | options));
        WriteCoprocessorCommand((uint)((range << 16) | size));

        WriteCoprocessorCommand(Ft800Defs.DL_DISPLAY);
        EndCoprocessor();
    }

    // ========================================================================
    // Toggle Switch
    // ========================================================================

    /// <summary>
    /// Draw a toggle switch
    /// </summary>
    /// <param name="x">X coordinate</param>
    /// <param name="y">Y coordinate</param>
    /// <param name="width">Toggle width</param>
    /// <param name="font">Font for labels</param>
    /// <param name="state">Switch state (true = on)</param>
    /// <param name="labels">Labels separated by 0xFF (e.g., "OFF\xFFON")</param>
    /// <param name="options">Toggle options</param>
    /// <param name="tag">Touch tag</param>
    public void DrawToggle(int x, int y, int width, Ft800Font font, bool state, string labels, ushort options = 0, byte tag = 0)
    {
        BeginCoprocessor();

        WriteCoprocessorCommand(Ft800Defs.DL_CLEAR_COLOR_RGB | 0x000000);
        WriteCoprocessorCommand(Ft800Defs.DL_CLEAR | Ft800Defs.CLEAR_ALL);

        if (tag > 0)
        {
            WriteCoprocessorCommand(Ft800Defs.DL_TAG | tag);
        }

        ushort stateValue = state ? (ushort)65535 : (ushort)0;

        WriteCoprocessorCommand(Ft800Defs.CMD_TOGGLE);
        WriteCoprocessorCommand((uint)((y << 16) | (x & 0xFFFF)));
        WriteCoprocessorCommand((uint)(((int)font << 16) | (width & 0xFFFF)));
        WriteCoprocessorCommand((uint)((stateValue << 16) | options));
        WriteCoprocessorString(labels);

        WriteCoprocessorCommand(Ft800Defs.DL_DISPLAY);
        EndCoprocessor();
    }

    // ========================================================================
    // Dial
    // ========================================================================

    /// <summary>
    /// Draw a rotary dial
    /// </summary>
    /// <param name="x">Center X coordinate</param>
    /// <param name="y">Center Y coordinate</param>
    /// <param name="radius">Dial radius</param>
    /// <param name="value">Current value (0-65535 = 0-360 degrees)</param>
    /// <param name="options">Dial options</param>
    /// <param name="tag">Touch tag</param>
    public void DrawDial(int x, int y, int radius, ushort value, ushort options = 0, byte tag = 0)
    {
        BeginCoprocessor();

        WriteCoprocessorCommand(Ft800Defs.DL_CLEAR_COLOR_RGB | 0x000000);
        WriteCoprocessorCommand(Ft800Defs.DL_CLEAR | Ft800Defs.CLEAR_ALL);

        if (tag > 0)
        {
            WriteCoprocessorCommand(Ft800Defs.DL_TAG | tag);
        }

        WriteCoprocessorCommand(Ft800Defs.CMD_DIAL);
        WriteCoprocessorCommand((uint)((y << 16) | (x & 0xFFFF)));
        WriteCoprocessorCommand((uint)((options << 16) | (radius & 0xFFFF)));
        WriteCoprocessorCommand(value);

        WriteCoprocessorCommand(Ft800Defs.DL_DISPLAY);
        EndCoprocessor();
    }

    // ========================================================================
    // Gauge
    // ========================================================================

    /// <summary>
    /// Draw an analog gauge
    /// </summary>
    /// <param name="x">Center X coordinate</param>
    /// <param name="y">Center Y coordinate</param>
    /// <param name="radius">Gauge radius</param>
    /// <param name="major">Number of major tick marks</param>
    /// <param name="minor">Number of minor tick marks between major marks</param>
    /// <param name="value">Current value</param>
    /// <param name="range">Maximum value</param>
    /// <param name="options">Gauge options</param>
    public void DrawGauge(int x, int y, int radius, ushort major, ushort minor, ushort value, ushort range, ushort options = 0)
    {
        BeginCoprocessor();

        WriteCoprocessorCommand(Ft800Defs.DL_CLEAR_COLOR_RGB | 0x000000);
        WriteCoprocessorCommand(Ft800Defs.DL_CLEAR | Ft800Defs.CLEAR_ALL);

        WriteCoprocessorCommand(Ft800Defs.CMD_GAUGE);
        WriteCoprocessorCommand((uint)((y << 16) | (x & 0xFFFF)));
        WriteCoprocessorCommand((uint)((options << 16) | (radius & 0xFFFF)));
        WriteCoprocessorCommand((uint)((minor << 16) | major));
        WriteCoprocessorCommand((uint)((range << 16) | value));

        WriteCoprocessorCommand(Ft800Defs.DL_DISPLAY);
        EndCoprocessor();
    }

    // ========================================================================
    // Clock
    // ========================================================================

    /// <summary>
    /// Draw an analog clock
    /// </summary>
    /// <param name="x">Center X coordinate</param>
    /// <param name="y">Center Y coordinate</param>
    /// <param name="radius">Clock radius</param>
    /// <param name="hours">Hours (0-23)</param>
    /// <param name="minutes">Minutes (0-59)</param>
    /// <param name="seconds">Seconds (0-59)</param>
    /// <param name="options">Clock options (OPT_NOSECS to hide second hand, etc.)</param>
    public void DrawClock(int x, int y, int radius, int hours, int minutes, int seconds, ushort options = 0)
    {
        BeginCoprocessor();

        WriteCoprocessorCommand(Ft800Defs.DL_CLEAR_COLOR_RGB | 0x000000);
        WriteCoprocessorCommand(Ft800Defs.DL_CLEAR | Ft800Defs.CLEAR_ALL);

        WriteCoprocessorCommand(Ft800Defs.CMD_CLOCK);
        WriteCoprocessorCommand((uint)((y << 16) | (x & 0xFFFF)));
        WriteCoprocessorCommand((uint)((options << 16) | (radius & 0xFFFF)));
        WriteCoprocessorCommand((uint)((minutes << 16) | hours));
        WriteCoprocessorCommand((uint)((0 << 16) | seconds));  // ms in high word

        WriteCoprocessorCommand(Ft800Defs.DL_DISPLAY);
        EndCoprocessor();
    }

    // ========================================================================
    // Spinner
    // ========================================================================

    /// <summary>
    /// Start a spinner animation
    /// </summary>
    /// <param name="x">Center X coordinate</param>
    /// <param name="y">Center Y coordinate</param>
    /// <param name="style">Spinner style</param>
    /// <param name="scale">Size scale (0=small, 1=medium, 2=large)</param>
    public void DrawSpinner(int x, int y, Ft800SpinnerStyle style, int scale = 1)
    {
        BeginCoprocessor();

        WriteCoprocessorCommand(Ft800Defs.DL_CLEAR_COLOR_RGB | 0x000000);
        WriteCoprocessorCommand(Ft800Defs.DL_CLEAR | Ft800Defs.CLEAR_ALL);

        WriteCoprocessorCommand(Ft800Defs.CMD_SPINNER);
        WriteCoprocessorCommand((uint)((y << 16) | (x & 0xFFFF)));
        WriteCoprocessorCommand((uint)((scale << 16) | (int)style));

        WriteCoprocessorCommand(Ft800Defs.DL_DISPLAY);
        EndCoprocessor();
    }

    /// <summary>
    /// Stop any running spinner animation
    /// </summary>
    public void StopSpinner()
    {
        BeginCoprocessor();
        WriteCoprocessorCommand(Ft800Defs.CMD_STOP);
        EndCoprocessor();
    }

    // ========================================================================
    // Number Display
    // ========================================================================

    /// <summary>
    /// Draw a number
    /// </summary>
    /// <param name="x">X coordinate</param>
    /// <param name="y">Y coordinate</param>
    /// <param name="value">Number value to display</param>
    /// <param name="color">Text color</param>
    /// <param name="font">Font to use</param>
    /// <param name="options">Display options (OPT_SIGNED for negative numbers, OPT_CENTERX, etc.)</param>
    public void DrawNumber(int x, int y, int value, Color color, Ft800Font font = Ft800Font.Size25, ushort options = 0)
    {
        BeginCoprocessor();

        WriteCoprocessorCommand(Ft800Defs.DL_CLEAR_COLOR_RGB | 0x000000);
        WriteCoprocessorCommand(Ft800Defs.DL_CLEAR | Ft800Defs.CLEAR_ALL);
        WriteCoprocessorCommand(Ft800Defs.DL_COLOR_RGB |
            ((uint)color.R << 16) | ((uint)color.G << 8) | color.B);

        WriteCoprocessorCommand(Ft800Defs.CMD_NUMBER);
        WriteCoprocessorCommand((uint)((y << 16) | (x & 0xFFFF)));
        WriteCoprocessorCommand((uint)((options << 16) | ((int)font & 0xFFFF)));
        WriteCoprocessorCommand((uint)value);

        WriteCoprocessorCommand(Ft800Defs.DL_DISPLAY);
        EndCoprocessor();
    }

    // ========================================================================
    // Colors (Co-processor specific)
    // ========================================================================

    /// <summary>
    /// Set the foreground color for widgets
    /// </summary>
    /// <param name="color">Foreground color</param>
    public void SetForegroundColor(Color color)
    {
        WriteCoprocessorCommand(Ft800Defs.CMD_FGCOLOR);
        WriteCoprocessorCommand(((uint)color.R << 16) | ((uint)color.G << 8) | color.B);
    }

    /// <summary>
    /// Set the background color for widgets
    /// </summary>
    /// <param name="color">Background color</param>
    public void SetBackgroundColor(Color color)
    {
        WriteCoprocessorCommand(Ft800Defs.CMD_BGCOLOR);
        WriteCoprocessorCommand(((uint)color.R << 16) | ((uint)color.G << 8) | color.B);
    }

    /// <summary>
    /// Set the gradient color for widgets
    /// </summary>
    /// <param name="color">Gradient color</param>
    public void SetGradientColor(Color color)
    {
        WriteCoprocessorCommand(Ft800Defs.CMD_GRADCOLOR);
        WriteCoprocessorCommand(((uint)color.R << 16) | ((uint)color.G << 8) | color.B);
    }
}
