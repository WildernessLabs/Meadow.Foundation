using Meadow.Hardware;
using Meadow.Peripherals.Displays;
using System;
using System.Threading;

namespace Meadow.Foundation.Displays;

/// <summary>
/// FT800 ITouchScreen implementation
/// </summary>
public partial class Ft800
{
    private const int TouchPollIntervalMs = 50;
    private const int TouchNotDetected = 0x8000;

    private Timer? touchPollTimer;
    private bool isTouched;
    private TouchPoint? lastTouchPoint;
    private bool touchPollingEnabled;

    // ========================================================================
    // ITouchScreen Implementation
    // ========================================================================

    /// <inheritdoc/>
    public event TouchEventHandler? TouchDown;

    /// <inheritdoc/>
    public event TouchEventHandler? TouchUp;

    /// <inheritdoc/>
    public event TouchEventHandler? TouchClick;

    /// <inheritdoc/>
    public event TouchEventHandler? TouchMoved;

    /// <inheritdoc/>
    public bool IsTouched => isTouched;

    /// <inheritdoc/>
    public RotationType Rotation { get; private set; } = RotationType.Normal;

    /// <summary>
    /// Initialize the touch controller
    /// </summary>
    private void InitializeTouch()
    {
        // Configure touch mode to continuous sampling
        WriteRegister(Ft800Defs.REG_TOUCH_MODE, Ft800Defs.TOUCHMODE_FRAME);

        // Configure touch ADC for accurate readings
        WriteRegister(Ft800Defs.REG_TOUCH_ADC_MODE, 1);  // Single-ended
        WriteRegister(Ft800Defs.REG_TOUCH_CHARGE, 5000);
        WriteRegister(Ft800Defs.REG_TOUCH_SETTLE, 1);
        WriteRegister(Ft800Defs.REG_TOUCH_OVERSAMPLE, 7);
        WriteRegister(Ft800Defs.REG_TOUCH_RZTHRESH, 1200);

        // Set up interrupt if available
        if (interruptPort != null)
        {
            WriteRegister(Ft800Defs.REG_INT_MASK, Ft800Defs.INT_TOUCH);
            WriteRegister(Ft800Defs.REG_INT_EN, 1);
            interruptPort.Changed += OnTouchInterrupt;
        }

        // Start polling timer
        StartTouchPolling();
    }

    /// <summary>
    /// Start the touch polling timer
    /// </summary>
    public void StartTouchPolling()
    {
        if (touchPollingEnabled) return;

        touchPollingEnabled = true;
        touchPollTimer = new Timer(TouchPollCallback, null, TouchPollIntervalMs, TouchPollIntervalMs);
    }

    /// <summary>
    /// Stop the touch polling timer
    /// </summary>
    public void StopTouchPolling()
    {
        touchPollingEnabled = false;
        touchPollTimer?.Dispose();
        touchPollTimer = null;
    }

    /// <summary>
    /// Handle interrupt from FT800 touch controller
    /// </summary>
    private void OnTouchInterrupt(object sender, DigitalPortResult e)
    {
        // Clear the interrupt flags
        ReadRegister8(Ft800Defs.REG_INT_FLAGS);

        // Process touch immediately
        ProcessTouch();
    }

    /// <summary>
    /// Timer callback for polling touch state
    /// </summary>
    private void TouchPollCallback(object? state)
    {
        if (!touchPollingEnabled) return;

        try
        {
            ProcessTouch();
        }
        catch
        {
            // Ignore errors during polling to prevent timer from stopping
        }
    }

    /// <summary>
    /// Process the current touch state
    /// </summary>
    private void ProcessTouch()
    {
        // Read transformed touch coordinates
        var touchXY = ReadRegister(Ft800Defs.REG_TOUCH_SCREEN_XY);

        int touchX = (int)((touchXY >> 16) & 0xFFFF);
        int touchY = (int)(touchXY & 0xFFFF);

        // Check if touch is detected (0x8000 in X means no touch)
        bool currentlyTouched = touchX != TouchNotDetected;

        if (currentlyTouched)
        {
            // Apply rotation transformation
            TransformTouchCoordinates(ref touchX, ref touchY);

            // Read touch tag (which object was touched)
            var tag = ReadRegister8(Ft800Defs.REG_TOUCH_TAG);

            var touchPoint = TouchPoint.FromScreenData(touchX, touchY, 100, touchX, touchY, 100);

            if (!isTouched)
            {
                // Touch just started
                isTouched = true;
                lastTouchPoint = touchPoint;
                TouchDown?.Invoke(this, touchPoint);
            }
            else if (lastTouchPoint.HasValue &&
                     (lastTouchPoint.Value.ScreenX != touchX || lastTouchPoint.Value.ScreenY != touchY))
            {
                // Touch moved
                lastTouchPoint = touchPoint;
                TouchMoved?.Invoke(this, touchPoint);
            }
        }
        else if (isTouched)
        {
            // Touch just ended
            isTouched = false;

            if (lastTouchPoint.HasValue)
            {
                TouchUp?.Invoke(this, lastTouchPoint.Value);
                TouchClick?.Invoke(this, lastTouchPoint.Value);
            }

            lastTouchPoint = null;
        }
    }

    /// <summary>
    /// Transform touch coordinates based on current rotation
    /// </summary>
    private void TransformTouchCoordinates(ref int x, ref int y)
    {
        int temp;

        switch (Rotation)
        {
            case RotationType._90Degrees:
                temp = x;
                x = y;
                y = Width - 1 - temp;
                break;

            case RotationType._180Degrees:
                x = Width - 1 - x;
                y = Height - 1 - y;
                break;

            case RotationType._270Degrees:
                temp = x;
                x = Height - 1 - y;
                y = temp;
                break;

            case RotationType.Normal:
            default:
                // No transformation needed
                break;
        }
    }

    /// <summary>
    /// Set the display and touch rotation
    /// </summary>
    /// <param name="rotation">The rotation to apply</param>
    public void SetRotation(RotationType rotation)
    {
        Rotation = rotation;

        // Configure the FT800 display rotation register
        byte rotValue = rotation switch
        {
            RotationType._90Degrees => 2,
            RotationType._180Degrees => 3,
            RotationType._270Degrees => 1,
            _ => 0
        };

        WriteRegister(Ft800Defs.REG_ROTATE, rotValue);
    }

    /// <summary>
    /// Read the raw touch coordinates (before transformation)
    /// </summary>
    /// <returns>Tuple of (x, y) or (-1, -1) if not touched</returns>
    public (int x, int y) ReadRawTouch()
    {
        var rawXY = ReadRegister(Ft800Defs.REG_TOUCH_RAW_XY);

        int rawX = (int)((rawXY >> 16) & 0xFFFF);
        int rawY = (int)(rawXY & 0xFFFF);

        if (rawX == TouchNotDetected)
        {
            return (-1, -1);
        }

        return (rawX, rawY);
    }

    /// <summary>
    /// Read the current touch tag value
    /// </summary>
    /// <returns>The tag value of the touched object, or 0 if not touched</returns>
    public byte ReadTouchTag()
    {
        if (!IsTouched) return 0;
        return ReadRegister8(Ft800Defs.REG_TOUCH_TAG);
    }

    /// <summary>
    /// Run the built-in touch calibration routine
    /// </summary>
    /// <remarks>
    /// This uses the FT800's co-processor calibration command which
    /// displays calibration points on screen and stores the results
    /// in the touch transform registers.
    /// </remarks>
    public void CalibrateTouch()
    {
        // Use the co-processor CALIBRATE command
        WaitForCoprocessorReady();

        var cmdWrite = ReadRegister(Ft800Defs.REG_CMD_WRITE);

        // Write CMD_DLSTART
        WriteMemory32(Ft800Defs.RAM_CMD + cmdWrite, Ft800Defs.CMD_DLSTART);
        cmdWrite = (cmdWrite + 4) % Ft800Defs.RAM_CMD_SIZE;

        // Write CLEAR
        WriteMemory32(Ft800Defs.RAM_CMD + cmdWrite, Ft800Defs.DL_CLEAR_COLOR_RGB | 0x000000);
        cmdWrite = (cmdWrite + 4) % Ft800Defs.RAM_CMD_SIZE;
        WriteMemory32(Ft800Defs.RAM_CMD + cmdWrite, Ft800Defs.DL_CLEAR | Ft800Defs.CLEAR_ALL);
        cmdWrite = (cmdWrite + 4) % Ft800Defs.RAM_CMD_SIZE;

        // Write CMD_CALIBRATE
        WriteMemory32(Ft800Defs.RAM_CMD + cmdWrite, Ft800Defs.CMD_CALIBRATE);
        cmdWrite = (cmdWrite + 4) % Ft800Defs.RAM_CMD_SIZE;

        // Result placeholder (calibration writes result here)
        WriteMemory32(Ft800Defs.RAM_CMD + cmdWrite, 0);
        cmdWrite = (cmdWrite + 4) % Ft800Defs.RAM_CMD_SIZE;

        // Update write pointer to start execution
        WriteRegister(Ft800Defs.REG_CMD_WRITE, cmdWrite);

        // Wait for calibration to complete
        WaitForCoprocessorReady();
    }

    /// <summary>
    /// Wait for the co-processor to finish executing commands
    /// </summary>
    private void WaitForCoprocessorReady()
    {
        int timeout = 5000;  // 5 second timeout
        while (timeout-- > 0)
        {
            var read = ReadRegister(Ft800Defs.REG_CMD_READ);
            var write = ReadRegister(Ft800Defs.REG_CMD_WRITE);

            if (read == write)
            {
                return;
            }

            Thread.Sleep(1);
        }

        throw new TimeoutException("FT800 co-processor timeout");
    }
}
