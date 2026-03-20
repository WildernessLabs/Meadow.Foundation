using Meadow.Hardware;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Meadow.Foundation.Sensors.Hid;

/// <summary>
/// Represents a BBQ10Keyboard Featherwing
/// </summary>
public partial class BBQ10Keyboard : II2cPeripheral, IDisposable
{
    /// <summary>
    /// The default I2C address for the peripheral
    /// </summary>
    public byte DefaultI2cAddress => (byte)Addresses.Default;

    /// <summary>
    /// Is the object disposed
    /// </summary>
    public bool IsDisposed { get; private set; }

    /// <summary>
    /// Did we create the interrupt port used by the peripheral
    /// </summary>
    readonly bool createdPort = false;

    /// <summary>
    /// I2C Communication bus used to communicate with the peripheral
    /// </summary>
    protected readonly II2cCommunications i2cComms;

    private readonly IDigitalInterruptPort? interruptPort;
    private CancellationTokenSource? pollCts;

    /// <summary>
    /// Raised when a key event is detected.
    /// Fires for every key in the FIFO when using interrupt mode,
    /// or on each poll cycle when using <see cref="StartPolling"/>.
    /// </summary>
    public event EventHandler<KeyEvent>? OnKeyEvent;

    /// <summary>
    /// Number of pending key events in the hardware FIFO (0–31)
    /// </summary>
    private byte KeyCount => (byte)(Status & KEY_COUNT_MASK);

    /// <summary>
    /// Raw KEY register value (key count in bits [4:0], lock flags in bits [5:6])
    /// </summary>
    private byte Status => i2cComms.ReadRegister((byte)Registers.KEY);

    /// <summary>
    /// Get or set the backlight level (0–255)
    /// </summary>
    public byte Backlight
    {
        get => i2cComms.ReadRegister((byte)Registers.BKL);
        set => i2cComms.WriteRegister((byte)Registers.BKL, value);
    }

    /// <summary>
    /// Get or set the secondary backlight level (0–255)
    /// </summary>
    public byte Backlight2
    {
        get => i2cComms.ReadRegister((byte)Registers.BK2);
        set => i2cComms.WriteRegister((byte)Registers.BK2, value);
    }

    /// <summary>
    /// Creates a new BBQ10Keyboard using interrupt-driven key detection.
    /// When <paramref name="interruptPin"/> is provided the driver listens for
    /// the active-low interrupt line and drains the full FIFO on each assertion.
    /// When omitted, call <see cref="StartPolling"/> or poll <see cref="GetLastKeyEvent"/> manually.
    /// </summary>
    /// <param name="i2cBus">The I2C bus</param>
    /// <param name="interruptPin">Optional active-low interrupt pin</param>
    /// <param name="address">The I2C address</param>
    public BBQ10Keyboard(II2cBus i2cBus, IPin? interruptPin = null, byte address = (byte)Addresses.Default)
    {
        i2cComms = new I2cCommunications(i2cBus, address);

        if (interruptPin != null)
        {
            createdPort = true;
            interruptPort = interruptPin.CreateDigitalInterruptPort(
                InterruptMode.EdgeFalling,
                ResistorMode.InternalPullUp);
            interruptPort.Changed += InterruptPort_Changed;
        }

        Reset();
    }

    /// <summary>
    /// Reads the oldest key event from the FIFO without checking count first.
    /// Returns <see cref="KeyState.StateIdle"/> if the FIFO is empty.
    /// </summary>
    public KeyEvent GetLastKeyEvent()
    {
        if (KeyCount == 0)
        {
            return new KeyEvent('\0', KeyState.StateIdle);
        }

        return ReadKeyEvent();
    }

    /// <summary>
    /// Starts a background polling loop that drains the FIFO and raises
    /// <see cref="OnKeyEvent"/> at the given interval.
    /// Use this when no interrupt pin is available.
    /// Calling again replaces the previous polling loop.
    /// </summary>
    /// <param name="intervalMs">Poll interval in milliseconds (default 50 ms)</param>
    public void StartPolling(int intervalMs = 50)
    {
        StopPolling();
        pollCts = new CancellationTokenSource();
        var token = pollCts.Token;

        Task.Run(() =>
        {
            while (!token.IsCancellationRequested)
            {
                try { DrainFifo(); }
                catch { /* swallow I2C errors during polling */ }

                if (!token.IsCancellationRequested)
                    Thread.Sleep(intervalMs);
            }
        }, token);
    }

    /// <summary>
    /// Stops the background polling loop started by <see cref="StartPolling"/>.
    /// </summary>
    public void StopPolling()
    {
        pollCts?.Cancel();
        pollCts = null;
    }

    /// <summary>
    /// Resets the keyboard firmware
    /// </summary>
    public void Reset()
    {
        i2cComms.WriteRegister((byte)Registers.RST, 0x00);
        Thread.Sleep(100);
    }

    /// <summary>
    /// Clears the interrupt status register so the interrupt line de-asserts
    /// and subsequent key events can trigger a new interrupt.
    /// </summary>
    protected void ClearInterruptStatus()
    {
        i2cComms.WriteRegister((byte)Registers.INT, 0x00);
    }

    // Reads all pending events from the FIFO and raises OnKeyEvent for each.
    // Always clears the interrupt status register when done.
    private void DrainFifo()
    {
        while (KeyCount > 0)
        {
            OnKeyEvent?.Invoke(this, ReadKeyEvent());
        }
    }

    // Reads a single raw entry from the FIFO register.
    // High byte = ASCII character, low byte = KeyState.
    private KeyEvent ReadKeyEvent()
    {
        var keyData = i2cComms.ReadRegisterAsUShort((byte)Registers.FIF);
        return new KeyEvent((char)(keyData >> 8), (KeyState)(keyData & 0xFF));
    }

    private void InterruptPort_Changed(object sender, DigitalPortResult e)
    {
        DrainFifo();
        ClearInterruptStatus();
    }

    ///<inheritdoc/>
    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Dispose of the object
    /// </summary>
    /// <param name="disposing">Is disposing</param>
    protected virtual void Dispose(bool disposing)
    {
        if (!IsDisposed)
        {
            if (disposing)
            {
                StopPolling();

                if (createdPort)
                {
                    interruptPort?.Dispose();
                }
            }

            IsDisposed = true;
        }
    }
}
