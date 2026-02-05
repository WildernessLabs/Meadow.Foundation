using Meadow.Foundation.Graphics.Buffers;
using Meadow.Hardware;
using Meadow.Peripherals.Displays;
using Meadow.Units;
using System;
using System.Threading;

namespace Meadow.Foundation.Displays;

/// <summary>
/// Represents an FT800/FT800CB Embedded Video Engine (EVE) display controller
/// </summary>
/// <remarks>
/// The FT800 is a GPU-based display controller that supports hardware-accelerated
/// 2D graphics, built-in widgets, touch input, and audio. This driver provides
/// both IPixelDisplay compatibility (for use with MicroGraphics) and native
/// GPU-accelerated graphics through Ft800Graphics.
/// </remarks>
public partial class Ft800 : IPixelDisplay, ITouchScreen, ISpiPeripheral, IDisposable
{
    /// <summary>
    /// Default display width for FT800CB modules
    /// </summary>
    public const int DefaultWidth = 480;

    /// <summary>
    /// Default display height for FT800CB modules
    /// </summary>
    public const int DefaultHeight = 272;

    /// <summary>
    /// The default SPI bus speed for the FT800 (20 MHz)
    /// </summary>
    public Frequency DefaultSpiBusSpeed => new(20_000, Frequency.UnitType.Kilohertz);

    /// <summary>
    /// The default SPI bus mode for the FT800 (Mode 0)
    /// </summary>
    public SpiClockConfiguration.Mode DefaultSpiBusMode => SpiClockConfiguration.Mode.Mode0;

    /// <summary>
    /// Current SPI bus speed
    /// </summary>
    public Frequency SpiBusSpeed
    {
        get => spiComms.BusSpeed;
        set => spiComms.BusSpeed = value;
    }

    /// <summary>
    /// Current SPI bus mode
    /// </summary>
    public SpiClockConfiguration.Mode SpiBusMode
    {
        get => spiComms.BusMode;
        set => spiComms.BusMode = value;
    }

    /// <summary>
    /// Is the object disposed
    /// </summary>
    public bool IsDisposed { get; private set; }

    private readonly ISpiCommunications spiComms;
    private readonly IDigitalOutputPort chipSelectPort;
    private readonly IDigitalOutputPort powerDownPort;
    private readonly IDigitalInterruptPort? interruptPort;
    private readonly bool createdPorts;

    // SPI transaction buffers
    private readonly byte[] cmdBuffer = new byte[4];
    private readonly byte[] readBuffer = new byte[8];

    /// <summary>
    /// Creates a new FT800 display driver instance
    /// </summary>
    /// <param name="spiBus">The SPI bus connected to the FT800</param>
    /// <param name="chipSelectPin">Chip select pin</param>
    /// <param name="powerDownPin">Power down pin (directly controls FT800 PD_N)</param>
    /// <param name="interruptPin">Optional interrupt pin for touch events</param>
    /// <param name="width">Display width in pixels (default 480)</param>
    /// <param name="height">Display height in pixels (default 272)</param>
    public Ft800(
        ISpiBus spiBus,
        IPin chipSelectPin,
        IPin powerDownPin,
        IPin? interruptPin = null,
        int width = DefaultWidth,
        int height = DefaultHeight)
        : this(
            spiBus,
            chipSelectPin.CreateDigitalOutputPort(true),
            powerDownPin.CreateDigitalOutputPort(false),
            interruptPin?.CreateDigitalInterruptPort(InterruptMode.EdgeFalling),
            width,
            height)
    {
        createdPorts = true;
    }

    /// <summary>
    /// Creates a new FT800 display driver instance using pre-configured ports
    /// </summary>
    /// <param name="spiBus">The SPI bus connected to the FT800</param>
    /// <param name="chipSelectPort">Chip select output port</param>
    /// <param name="powerDownPort">Power down output port</param>
    /// <param name="interruptPort">Optional interrupt port for touch events</param>
    /// <param name="width">Display width in pixels (default 480)</param>
    /// <param name="height">Display height in pixels (default 272)</param>
    public Ft800(
        ISpiBus spiBus,
        IDigitalOutputPort chipSelectPort,
        IDigitalOutputPort powerDownPort,
        IDigitalInterruptPort? interruptPort = null,
        int width = DefaultWidth,
        int height = DefaultHeight)
    {
        this.chipSelectPort = chipSelectPort;
        this.powerDownPort = powerDownPort;
        this.interruptPort = interruptPort;

        Width = width;
        Height = height;

        spiComms = new SpiCommunications(spiBus, chipSelectPort, DefaultSpiBusSpeed, DefaultSpiBusMode);

        // Create the pixel buffer for IPixelDisplay compatibility
        imageBuffer = new BufferRgb565(width, height);

        Initialize();
    }

    /// <summary>
    /// Initialize the FT800 display
    /// </summary>
    private void Initialize()
    {
        // Power cycle the FT800
        powerDownPort.State = false;  // Assert PD_N low
        Thread.Sleep(20);
        powerDownPort.State = true;   // Release PD_N
        Thread.Sleep(20);

        // Wake up the FT800
        HostCommand(Ft800Defs.CMD_ACTIVE);
        Thread.Sleep(5);

        // Wait for the FT800 to be ready (REG_ID should read 0x7C)
        int timeout = 100;
        while (timeout-- > 0)
        {
            var id = ReadRegister8(Ft800Defs.REG_ID);
            if (id == Ft800Defs.CHIP_ID_FT800)
            {
                break;
            }
            Thread.Sleep(10);
        }

        if (timeout <= 0)
        {
            throw new Exception("FT800 not responding - check connections");
        }

        // Select 48MHz clock
        HostCommand(Ft800Defs.CMD_CLK48M);

        // Configure display timing for 480x272 display
        ConfigureDisplayTiming();

        // Configure GPIO for display enable (active high on GPIO bit 7)
        WriteRegister(Ft800Defs.REG_GPIO_DIR, 0x83);
        WriteRegister(Ft800Defs.REG_GPIO, 0x80);

        // Configure PWM for backlight (full brightness)
        WriteRegister(Ft800Defs.REG_PWM_HZ, 250);
        WriteRegister(Ft800Defs.REG_PWM_DUTY, 128);

        // Write an initial display list (black screen)
        WriteInitialDisplayList();

        // Enable display output
        WriteRegister(Ft800Defs.REG_PCLK, 5);

        // Initialize touch
        InitializeTouch();
    }

    /// <summary>
    /// Configure the display timing registers for standard 480x272 panel
    /// </summary>
    private void ConfigureDisplayTiming()
    {
        // These values are typical for 480x272 displays
        // Adjust if your specific panel requires different timing
        WriteRegister(Ft800Defs.REG_HCYCLE, 548);
        WriteRegister(Ft800Defs.REG_HOFFSET, 43);
        WriteRegister(Ft800Defs.REG_HSIZE, 480);
        WriteRegister(Ft800Defs.REG_HSYNC0, 0);
        WriteRegister(Ft800Defs.REG_HSYNC1, 41);

        WriteRegister(Ft800Defs.REG_VCYCLE, 292);
        WriteRegister(Ft800Defs.REG_VOFFSET, 12);
        WriteRegister(Ft800Defs.REG_VSIZE, 272);
        WriteRegister(Ft800Defs.REG_VSYNC0, 0);
        WriteRegister(Ft800Defs.REG_VSYNC1, 10);

        WriteRegister(Ft800Defs.REG_SWIZZLE, 0);
        WriteRegister(Ft800Defs.REG_PCLK_POL, 1);
        WriteRegister(Ft800Defs.REG_CSPREAD, 1);
        WriteRegister(Ft800Defs.REG_DITHER, 1);
    }

    /// <summary>
    /// Write an initial display list showing a black screen
    /// </summary>
    private void WriteInitialDisplayList()
    {
        // Write to RAM_DL
        WriteMemory32(Ft800Defs.RAM_DL + 0, Ft800Defs.DL_CLEAR_COLOR_RGB | 0x000000);
        WriteMemory32(Ft800Defs.RAM_DL + 4, Ft800Defs.DL_CLEAR | Ft800Defs.CLEAR_ALL);
        WriteMemory32(Ft800Defs.RAM_DL + 8, Ft800Defs.DL_DISPLAY);

        // Swap display list
        WriteRegister(Ft800Defs.REG_DLSWAP, Ft800Defs.DLSWAP_FRAME);
    }

    // ========================================================================
    // Host Commands
    // ========================================================================

    /// <summary>
    /// Send a host command to the FT800
    /// </summary>
    /// <param name="command">The command byte</param>
    public void HostCommand(byte command)
    {
        cmdBuffer[0] = command;
        cmdBuffer[1] = 0x00;
        cmdBuffer[2] = 0x00;

        spiComms.Write(cmdBuffer.AsSpan(0, 3));
    }

    // ========================================================================
    // Memory Access
    // ========================================================================

    /// <summary>
    /// Write data to FT800 memory
    /// </summary>
    /// <param name="address">24-bit memory address</param>
    /// <param name="data">Data to write</param>
    public void WriteMemory(uint address, byte[] data)
    {
        WriteMemory(address, data.AsSpan());
    }

    /// <summary>
    /// Write data to FT800 memory
    /// </summary>
    /// <param name="address">24-bit memory address</param>
    /// <param name="data">Data to write</param>
    public void WriteMemory(uint address, Span<byte> data)
    {
        // Build header: [1][1][addr21:16][addr15:8][addr7:0]
        var header = new byte[3];
        header[0] = (byte)(0x80 | ((address >> 16) & 0x3F));  // Write bit + high address
        header[1] = (byte)((address >> 8) & 0xFF);
        header[2] = (byte)(address & 0xFF);

        // Combine header and data
        var buffer = new byte[3 + data.Length];
        header.CopyTo(buffer, 0);
        data.CopyTo(buffer.AsSpan(3));

        spiComms.Write(buffer);
    }

    /// <summary>
    /// Write a single 32-bit value to FT800 memory
    /// </summary>
    /// <param name="address">24-bit memory address</param>
    /// <param name="value">32-bit value (little endian)</param>
    public void WriteMemory32(uint address, uint value)
    {
        var data = new byte[4];
        data[0] = (byte)(value & 0xFF);
        data[1] = (byte)((value >> 8) & 0xFF);
        data[2] = (byte)((value >> 16) & 0xFF);
        data[3] = (byte)((value >> 24) & 0xFF);
        WriteMemory(address, data);
    }

    /// <summary>
    /// Read data from FT800 memory
    /// </summary>
    /// <param name="address">24-bit memory address</param>
    /// <param name="length">Number of bytes to read</param>
    /// <returns>The data read</returns>
    public byte[] ReadMemory(uint address, int length)
    {
        // Build header: [0][0][addr21:16][addr15:8][addr7:0][dummy]
        var txBuffer = new byte[4 + length];
        txBuffer[0] = (byte)((address >> 16) & 0x3F);  // Read bit (0) + high address
        txBuffer[1] = (byte)((address >> 8) & 0xFF);
        txBuffer[2] = (byte)(address & 0xFF);
        txBuffer[3] = 0x00;  // Dummy byte

        var rxBuffer = new byte[4 + length];
        spiComms.Exchange(txBuffer, rxBuffer);

        var result = new byte[length];
        Array.Copy(rxBuffer, 4, result, 0, length);
        return result;
    }

    /// <summary>
    /// Read a 32-bit value from FT800 memory
    /// </summary>
    /// <param name="address">24-bit memory address</param>
    /// <returns>The 32-bit value (little endian)</returns>
    public uint ReadMemory32(uint address)
    {
        var data = ReadMemory(address, 4);
        return (uint)(data[0] | (data[1] << 8) | (data[2] << 16) | (data[3] << 24));
    }

    // ========================================================================
    // Register Access
    // ========================================================================

    /// <summary>
    /// Write a 32-bit value to an FT800 register
    /// </summary>
    /// <param name="register">Register address</param>
    /// <param name="value">Value to write</param>
    public void WriteRegister(uint register, uint value)
    {
        WriteMemory32(register, value);
    }

    /// <summary>
    /// Write an 8-bit value to an FT800 register
    /// </summary>
    /// <param name="register">Register address</param>
    /// <param name="value">Value to write</param>
    public void WriteRegister8(uint register, byte value)
    {
        WriteMemory(register, new[] { value });
    }

    /// <summary>
    /// Read a 32-bit value from an FT800 register
    /// </summary>
    /// <param name="register">Register address</param>
    /// <returns>The register value</returns>
    public uint ReadRegister(uint register)
    {
        return ReadMemory32(register);
    }

    /// <summary>
    /// Read an 8-bit value from an FT800 register
    /// </summary>
    /// <param name="register">Register address</param>
    /// <returns>The register value</returns>
    public byte ReadRegister8(uint register)
    {
        var data = ReadMemory(register, 1);
        return data[0];
    }

    /// <summary>
    /// Read a 16-bit value from an FT800 register
    /// </summary>
    /// <param name="register">Register address</param>
    /// <returns>The register value</returns>
    public ushort ReadRegister16(uint register)
    {
        var data = ReadMemory(register, 2);
        return (ushort)(data[0] | (data[1] << 8));
    }

    // ========================================================================
    // Backlight Control
    // ========================================================================

    /// <summary>
    /// Set the backlight brightness
    /// </summary>
    /// <param name="brightness">Brightness level (0-128)</param>
    public void SetBacklight(byte brightness)
    {
        WriteRegister(Ft800Defs.REG_PWM_DUTY, brightness);
    }

    // ========================================================================
    // IDisposable
    // ========================================================================

    /// <inheritdoc/>
    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Dispose managed resources
    /// </summary>
    /// <param name="disposing">True if disposing</param>
    protected virtual void Dispose(bool disposing)
    {
        if (!IsDisposed)
        {
            if (disposing)
            {
                StopTouchPolling();

                if (createdPorts)
                {
                    chipSelectPort?.Dispose();
                    powerDownPort?.Dispose();
                    interruptPort?.Dispose();
                }
            }

            IsDisposed = true;
        }
    }
}
