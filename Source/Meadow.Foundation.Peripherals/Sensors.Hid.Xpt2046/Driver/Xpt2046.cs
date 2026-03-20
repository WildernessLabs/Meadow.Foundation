using Meadow.Hardware;
using Meadow.Peripherals.Displays;
using Meadow.Units;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Meadow.Foundation.Sensors.Hid;

/// <summary>
/// XPT2046 4-wire resistive touchscreen controller driver.
///
/// This class:
/// - Listens to the PENIRQ line via an interrupt
/// - Samples X/Y/Z over SPI on demand while the screen is touched
/// - Applies a simple 2-point linear calibration to map raw ADC values into
///   screen coordinates (matching the display / MicroGraphics orientation)
/// - Raises TouchDown / TouchUp / TouchMoved / TouchClick events
/// </summary>
public partial class Xpt2046 : ICalibratableTouchscreen
{
    /// <inheritdoc/>
    public event TouchEventHandler? TouchDown = null;

    /// <inheritdoc/>
    public event TouchEventHandler? TouchUp = null;

    /// <inheritdoc/>
    public event TouchEventHandler? TouchClick = null;

    /// <inheritdoc/>
    public event TouchEventHandler? TouchMoved = null;

    /// <summary>
    /// How often (in ms) we re-sample the touchscreen while it is being touched.
    /// </summary>
    private const int SamplePeriodMilliseconds = 100;

    /// <summary>
    /// Start bit for XPT2046 command byte.
    /// </summary>
    private const byte StartBit = 0x80;

    /// <summary>
    /// Interrupt port connected to the PENIRQ (touch) line.
    /// High = not touched, Low = touched.
    /// </summary>
    private readonly IDigitalInterruptPort touchInterrupt;

    /// <summary>
    /// SPI communications helper that wraps the shared ISpiBus and per-device CS.
    /// </summary>
    private readonly SpiCommunications comms;

    /// <summary>
    /// Logical screen width (in pixels) for clamping calibrated coordinates.
    /// </summary>
    private readonly int screenWidth;

    /// <summary>
    /// Logical screen height (in pixels) for clamping calibrated coordinates.
    /// </summary>
    private readonly int screenHeight;

    /// <summary>
    /// Timer used to periodically sample X/Y/Z while the screen is held down.
    /// </summary>
    private Timer sampleTimer;

    /// <summary>
    /// True while we are actively sampling a touch sequence.
    /// </summary>
    private bool isSampling = false;

    /// <summary>
    /// True until we see the first valid sample of a new touch sequence.
    /// </summary>
    private bool firstTouch = true;

    /// <summary>
    /// The last reported touch position.
    /// </summary>
    private TouchPoint? lastTouchPosition;

    /// <summary>
    /// The second-to-last reported touch position.
    /// Used because the final sample before PENIRQ goes high is often noisy.
    /// </summary>
    private TouchPoint? penultimatePosition;

    /// <summary>
    /// Linear calibration coefficients:
    ///   screenX = rawX * mX + cX
    ///   screenY = rawY * mY + cY
    /// These are computed from two calibration points.
    /// </summary>
    private float mX;
    private float mY;
    private float cX;
    private float cY;

    /// <inheritdoc/>
    public RotationType Rotation { get; }

    /// <inheritdoc/>
    public bool IsCalibrated { get; private set; }

    /// <inheritdoc/>
    public bool IsTouched => isSampling;

    /// <summary>
    /// Creates a new Xpt2046 touchscreen driver.
    /// </summary>
    /// <param name="spiBus">
    /// Shared SPI bus connected to the XPT2046 controller.
    /// </param>
    /// <param name="touchInterrupt">
    /// Interrupt port connected to the XPT2046 PENIRQ output.
    /// Must be configured for <see cref="InterruptMode.EdgeBoth"/>.
    /// </param>
    /// <param name="chipSelect">
    /// Optional chip select port for the XPT2046 on the shared SPI bus.
    /// </param>
    /// <param name="rotation">
    /// Touchscreen rotation. Currently retained for compatibility; actual
    /// orientation is encoded by calibration and display / graphics rotation.
    /// </param>
    /// <param name="screenWidth">
    /// Logical screen width (pixels) used to clamp calibrated X coordinates.
    /// </param>
    /// <param name="screenHeight">
    /// Logical screen height (pixels) used to clamp calibrated Y coordinates.
    /// </param>
    public Xpt2046(
        ISpiBus spiBus,
        IDigitalInterruptPort touchInterrupt,
        IDigitalOutputPort? chipSelect,
        RotationType rotation = RotationType.Normal,
        int screenWidth = 320,
        int screenHeight = 240)
    {
        // Ensure the interrupt port is configured correctly for PENIRQ
        if (touchInterrupt.InterruptMode != InterruptMode.EdgeBoth)
        {
            throw new ArgumentException("The interrupt port must be set to EdgeBoth");
        }

        this.screenWidth = screenWidth;
        this.screenHeight = screenHeight;
        this.touchInterrupt = touchInterrupt;

        // Timer is created stopped; we start it on first PENIRQ transition.
        sampleTimer = new Timer(SampleTimerProc, null, -1, -1);

        // XPT2046 supports up to ~2.5MHz; we run at 1MHz here.
        comms = new SpiCommunications(spiBus, chipSelect, 1_000_000.Hertz());

        Rotation = rotation;

        // Subscribe to PENIRQ changes; this drives sampling on touch / release.
        touchInterrupt.Changed += OnTouchInterrupt;
    }

    /// <summary>
    /// Converts a single raw X/Y/Z sample from the controller into an
    /// optionally calibrated <see cref="TouchPoint"/>.
    /// </summary>
    /// <param name="rawX">Raw ADC X value from XPT2046.</param>
    /// <param name="rawY">Raw ADC Y value from XPT2046.</param>
    /// <param name="rawZ">Raw pressure (Z) value from XPT2046.</param>
    /// <returns>
    /// A <see cref="TouchPoint"/> containing either raw data (if not calibrated)
    /// or screen coordinates (if calibration has been applied).
    /// </returns>
    private TouchPoint ConvertRawToTouchPoint(ushort rawX, ushort rawY, ushort rawZ)
    {
        Resolver.Log.Trace($"Xpt2046 converting raw to touch point...");

        // We let calibration handle any orientation; no rotation is applied here.
        int x = rawX;
        int y = rawY;

        // If no calibration is set, return the raw data wrapped in a TouchPoint.
        if (!IsCalibrated)
        {
            return TouchPoint.FromRawData(x, y, rawZ);
        }

        // Apply simple 2-point linear calibration to map raw X/Y into screen space.
        var scaledX = (x * mX) + cX;
        var scaledY = (y * mY) + cY;

        // Clamp to screen bounds to avoid negative or out-of-range coordinates
        // due to minor calibration / noise errors.
        scaledX = Math.Clamp(scaledX, 0, screenWidth - 1);
        scaledY = Math.Clamp(scaledY, 0, screenHeight - 1);

        Resolver.Log.Trace($"...scaled to screen X={scaledX}, Y={scaledY}");

        // Preserve raw values for debugging / diagnostics along with the
        // calibrated screen coordinates.
        return TouchPoint.FromScreenData((int)scaledX, (int)scaledY, rawZ, rawX, rawY, rawZ);
    }

    /// <summary>
    /// Interrupt handler for PENIRQ changes.
    /// When the line goes active (low), we schedule a sampling pass.
    /// </summary>
    private void OnTouchInterrupt(object sender, DigitalPortResult e)
    {
        // Note: PENIRQ semantics:
        //   High  = not touched
        //   Low   = touched
        // We kick off sampling on first transition; the SampleTimerProc
        // will decide if we should keep sampling or send TouchUp.
        if (!isSampling)
        {
            sampleTimer.Change(0, -1);
        }
    }

    /// <summary>
    /// Timer callback used to periodically sample the touchscreen
    /// while a touch is in progress.
    /// </summary>
    private void SampleTimerProc(object o)
    {
        // If PENIRQ is high, the pen is up and we should end the touch sequence.
        if (touchInterrupt.State)
        {
            // The actual last reading at pen-up is often garbage, so
            // prefer the second-to-last sample if we have it.
            if (penultimatePosition != null)
            {
                TouchUp?.Invoke(this, penultimatePosition.Value);
            }
            else if (lastTouchPosition != null)
            {
                TouchUp?.Invoke(this, lastTouchPosition.Value);
            }

            // Reset sampling state for the next touch sequence.
            isSampling = false;
            firstTouch = true;
            lastTouchPosition = null;
            penultimatePosition = null;
            return;
        }

        // Pen is down; we are in an active sampling cycle.
        isSampling = true;

        // Read pressure then X and Y. The XPT2046 protocol expects Z to be
        // read first, then X/Y, with a final dummy read to re-enable IRQ.
        var z = ReadZ();
        var x = ReadX();
        var y = ReadY();

        // Ensure PENIRQ is re-armed after sampling.
        EnableIrq();

        // Convert raw ADC readings into a TouchPoint (raw or calibrated).
        var position = ConvertRawToTouchPoint(x, y, z);

        try
        {
            if (firstTouch)
            {
                // First valid sample of this touch; fire TouchDown.
                firstTouch = false;
                lastTouchPosition = position;

                if (lastTouchPosition != null)
                {
                    TouchDown?.Invoke(this, lastTouchPosition.Value);
                }
            }
            else
            {
                // Subsequent samples: if position changed, fire TouchMoved.
                if (!position.Equals(lastTouchPosition))
                {
                    penultimatePosition = lastTouchPosition;
                    lastTouchPosition = position;

                    if (lastTouchPosition != null)
                    {
                        TouchMoved?.Invoke(this, lastTouchPosition.Value);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            // Protect against user event handlers throwing exceptions.
            Resolver.Log.Warn($"Touchscreen event handler error: {ex.Message}");
        }

        // Schedule the next sample in this touch sequence.
        sampleTimer.Change(SamplePeriodMilliseconds, -1);
    }

    /// <summary>
    /// Reads a raw X (horizontal) ADC sample from the touchscreen.
    /// </summary>
    private ushort ReadX()
    {
        return ReadChannel(Channel.X);
    }

    /// <summary>
    /// Reads a raw Y (vertical) ADC sample from the touchscreen.
    /// </summary>
    private ushort ReadY()
    {
        return ReadChannel(Channel.Y);
    }

    /// <summary>
    /// Reads a raw Z (pressure) ADC sample from the touchscreen.
    /// </summary>
    private ushort ReadZ()
    {
        return ReadChannel(Channel.Z2);
    }

    /// <summary>
    /// Issues a dummy conversion on the temperature channel with power-down
    /// to re-enable the PENIRQ (touch) interrupt line.
    /// </summary>
    private void EnableIrq()
    {
        ReadChannel(Channel.Temp, PowerState.PowerDown);
    }

    /// <summary>
    /// Reads a single ADC channel from the XPT2046 using the given configuration.
    /// This constructs the XPT2046 command byte and performs a 3-byte SPI transfer,
    /// then converts the returned 12 bits into a ushort.
    /// </summary>
    /// <param name="channel">ADC channel (X, Y, Z1, Z2, Temp).</param>
    /// <param name="postSamplePowerState">ADC power-state after the sample.</param>
    /// <param name="mode">ADC resolution mode (10/12 bits).</param>
    /// <param name="vref">Voltage reference configuration.</param>
    /// <returns>12-bit converted value as a UInt16.</returns>
    private ushort ReadChannel(
        Channel channel,
        PowerState postSamplePowerState = PowerState.Adc,
        Mode mode = Mode.Bits_12,
        VoltageReference vref = VoltageReference.Differential)
    {
        // 3-byte command/response transaction:
        //   tx[0] = command (start bit + channel + mode + vref + power state)
        //   tx[1], tx[2] = don't care (clock out ADC result)
        //   rx[1:2] contain the 12-bit result (MSB aligned, we need bits [11:0]).
        Span<byte> txBuffer = stackalloc byte[3];
        Span<byte> rxBuffer = stackalloc byte[3];

        txBuffer[0] = (byte)(StartBit | (byte)channel | (byte)mode | (byte)vref | (byte)postSamplePowerState);

        comms.Exchange(txBuffer, rxBuffer, DuplexType.Full);

        // The XPT2046 returns the result left-justified; shift and assemble.
        return (ushort)(((rxBuffer[1] >> 3) << 8) | (rxBuffer[2] >> 3));
    }

    /// <inheritdoc/>
    /// <summary>
    /// Applies a simple 2-point linear calibration to map raw ADC values into
    /// screen coordinates. The calibration points should be collected using
    /// the same display / graphics rotation as the running system.
    ///
    /// Given two points:
    ///   (rawX0, rawY0) -> (screenX0, screenY0)
    ///   (rawX1, rawY1) -> (screenX1, screenY1)
    ///
    /// We compute:
    ///   mX, cX such that screenX = rawX * mX + cX
    ///   mY, cY such that screenY = rawY * mY + cY
    /// </summary>
    public void SetCalibrationData(IEnumerable<CalibrationPoint> data)
    {
        var points = data.ToArray();

        if (points.Length != 2)
            throw new ArgumentException("This touchscreen requires exactly 2 calibration points");

        // Basic validation to avoid divide-by-zero and degenerate calibration.
        if (points[1].RawX - points[0].RawX == 0 ||
            points[1].RawY - points[0].RawY == 0 ||
            points[1].ScreenX - points[0].ScreenX == 0 ||
            points[1].ScreenY - points[0].ScreenY == 0)
        {
            throw new ArgumentOutOfRangeException("Invalid calibration data");
        }

        // Compute X calibration: screenX = rawX * mX + cX
        mX = (points[1].ScreenX - points[0].ScreenX) /
            (float)(points[1].RawX - points[0].RawX);
        cX = points[0].ScreenX - (points[0].RawX * mX);

        // Compute Y calibration: screenY = rawY * mY + cY
        mY = (points[1].ScreenY - points[0].ScreenY) /
            (float)(points[1].RawY - points[0].RawY);
        cY = points[0].ScreenY - (points[0].RawY * mY);

        IsCalibrated = true;
    }

    /// <summary>
    /// XPT2046 ADC channels.
    /// </summary>
    private enum Channel : byte
    {
        Temp = 0x00,
        X = 1 << 4,
        Z1 = 3 << 4,
        Z2 = 4 << 4,
        Y = 5 << 4,
    }
}
