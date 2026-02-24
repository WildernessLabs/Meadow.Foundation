using Meadow.Foundation.Graphics.Buffers;
using Meadow.Hardware;
using Meadow.Peripherals.Displays;
using System;

namespace Meadow.Foundation.Displays;

/// <summary>
/// Represents an WaveShare Epd7in5 v2 ePaper display
/// 800x480, 7.5inch e-Ink display, SPI interface
/// </summary>
public class Epd7in5V2 : EPaperBase, IPixelDisplay, IRefreshableDisplay
{
    /// <inheritdoc/>
    public ColorMode ColorMode { get; protected set; } = ColorMode.Format1bpp;

    /// <inheritdoc/>
    public ColorMode SupportedColorModes => ColorMode.Format1bpp | ColorMode.Format2bppGray;

    /// <inheritdoc/>
    public IPixelBuffer PixelBuffer => imageBuffer;

    /// <inheritdoc/>
    public Color EnabledColor => Color.Black;

    /// <inheritdoc/>
    public Color DisabledColor => Color.White;

    /// <summary>
    /// Buffer to hold display data
    /// </summary>
    protected IPixelBuffer imageBuffer;

    /// <summary>
    /// Width of display in pixels
    /// </summary>
    public int Width => 800;

    /// <summary>
    /// Height of display in pixels
    /// </summary>
    public int Height => 480;

    /// <summary>
    /// Current refresh mode
    /// </summary>
    public RefreshMode CurrentRefreshMode { get; set; } = RefreshMode.Full;

    /// <summary>
    /// Number of partial refreshes since last full refresh
    /// Use IsFullRefreshRecommended to check if full refresh is advised
    /// </summary>
    public int PartialRefreshCount { get; private set; } = 0;

    /// <summary>
    /// Maximum number of partial refreshes before a full refresh is recommended
    /// </summary>
    public int MaxPartialRefreshes { get; set; } = 10;

    /// <summary>
    /// Indicates whether a full refresh is recommended based on partial refresh count
    /// This is advisory only - the application decides when to perform full refresh
    /// </summary>
    public bool IsFullRefreshRecommended => PartialRefreshCount >= MaxPartialRefreshes;

    /// <summary>
    /// Event raised when a fast refresh is recommended due to multiple partial refreshes
    /// </summary>
    public event EventHandler<RefreshRecommendedEventArgs>? FastRefreshRecommended;

    /// <summary>
    /// Event raised when a full refresh is recommended to clear accumulated ghosting
    /// </summary>
    public event EventHandler<RefreshRecommendedEventArgs>? FullRefreshRecommended;

    private bool fastRefreshEventRaised = false;
    private bool fullRefreshEventRaised = false;

    /// <summary>
    /// The minimum delay required by the hardware between screen redraws
    /// </summary>
    public TimeSpan MinimumRefreshInterval
    {
        get
        {
            return CurrentRefreshMode switch
            {
                RefreshMode.Partial => TimeSpan.FromMilliseconds(400),
                RefreshMode.Fast => TimeSpan.FromMilliseconds(1500),
                _ => TimeSpan.FromSeconds(4)
            };
        }
    }

    private int lastUpdatedTick = -1;
    private RefreshMode? lastInitializedMode = null;

    /// <summary>
    /// Create a new WaveShare Epd7in5 v2 800x480 pixel display object
    /// </summary>
    /// <param name="spiBus">SPI bus connected to display</param>
    /// <param name="chipSelectPin">Chip select pin</param>
    /// <param name="dcPin">Data command pin</param>
    /// <param name="resetPin">Reset pin</param>
    /// <param name="busyPin">Busy pin</param>
    /// <param name="colorMode">The display color mode - either 1bpp or 2bpp</param>
    public Epd7in5V2(ISpiBus spiBus, IPin chipSelectPin, IPin dcPin, IPin resetPin, IPin busyPin, ColorMode colorMode = ColorMode.Format1bpp) :
        this(spiBus,
            chipSelectPin.CreateDigitalOutputPort(),
            dcPin.CreateDigitalOutputPort(),
            resetPin.CreateDigitalOutputPort(),
            busyPin.CreateDigitalInputPort(),
            colorMode)
    {
        createdPorts = true;
    }

    /// <summary>
    /// Create a new WaveShare Epd7in5 v2 ePaper 800x480 pixel display object
    /// </summary>
    /// <param name="spiBus">SPI bus connected to display</param>
    /// <param name="chipSelectPort">Chip select output port</param>
    /// <param name="dataCommandPort">Data command output port</param>
    /// <param name="resetPort">Reset output port</param>
    /// <param name="busyPort">Busy input port</param>
    /// <param name="colorMode">The display color mode - either 1bpp or 2bpp</param>
    public Epd7in5V2(ISpiBus spiBus,
        IDigitalOutputPort chipSelectPort,
        IDigitalOutputPort dataCommandPort,
        IDigitalOutputPort resetPort,
        IDigitalInputPort busyPort,
        ColorMode colorMode = ColorMode.Format1bpp)
    {
        this.dataCommandPort = dataCommandPort;
        this.chipSelectPort = chipSelectPort;
        this.resetPort = resetPort;
        this.busyPort = busyPort;

        spiComms = new SpiCommunications(spiBus, chipSelectPort, DefaultSpiBusSpeed, DefaultSpiBusMode);

        if ((SupportedColorModes & colorMode) == 0)
        {
            throw new ArgumentException($"ColorMode {colorMode} is not supported");
        }

        ColorMode = colorMode;

        if (ColorMode == ColorMode.Format1bpp)
        {
            imageBuffer = new Buffer1bppV(Width, Height);
        }
        else
        {
            imageBuffer = new Buffer2bppGreyEPaper(Width, Height);
        }

        imageBuffer.Clear();

        Initialize();
    }

    /// <summary>
    /// Initialize the display driver
    /// </summary>
    private void Initialize()
    {
        if (ColorMode == ColorMode.Format1bpp)
        {
            Initialize1bpp();
        }
        else
        {
            Initialize2bpp();
        }
    }

    private void Initialize1bpp()
    {
        Reset();

        SendCommand(POWER_SETTING);
        SendData(0x07);
        SendData(0x07);
        SendData(0x3f);
        SendData(0x3f);

        SendCommand(BOOSTER_SOFT_START);
        SendData(0x17);
        SendData(0x17);
        SendData(0x28);
        SendData(0x17);

        SendCommand(POWER_ON);
        DelayMs(100);
        WaitUntilIdle();

        SendCommand(PANEL_SETTING);
        SendData(0x1F);

        SendCommand(RESOLUTION_SETTING);
        SendData(0x03);     //source 800
        SendData(0x20);
        SendData(0x01);     //gate 480
        SendData(0xE0);

        SendCommand(0x15);
        SendData(0x00);

        SendCommand(VCOM_AND_DATA_INTERVAL_SETTING);
        SendData(0x10);
        SendData(0x07);

        SendCommand(TCON_SETTING);
        SendData(0x22);
    }

    private void Initialize2bpp()
    {
        Reset();

        SendCommand(PANEL_SETTING);
        SendData(0x1F);

        SendCommand(VCOM_AND_DATA_INTERVAL_SETTING);
        SendData(0x10);
        SendData(0x07);
        SendCommand(0x52);
        SendData(0x03);

        SendCommand(POWER_ON);
        DelayMs(100);
        WaitUntilIdle();

        SendCommand(BOOSTER_SOFT_START);
        SendData(0x27);
        SendData(0x27);
        SendData(0x18);
        SendData(0x17);

        SendCommand(0xE0);
        SendData(0x02);
        SendCommand(0xE5);
        SendData(0x5F);
    }

    /// <summary>
    /// Initialize the display for fast refresh mode
    /// </summary>
    protected void InitializeFast()
    {
        Reset();

        SendCommand(PANEL_SETTING);
        SendData(0x1F);

        SendCommand(VCOM_AND_DATA_INTERVAL_SETTING);
        SendData(0x10);
        SendData(0x07);

        SendCommand(POWER_ON);
        DelayMs(100);
        WaitUntilIdle();

        SendCommand(BOOSTER_SOFT_START);
        SendData(0x27);
        SendData(0x27);
        SendData(0x18);
        SendData(0x17);

        SendCommand(0xE0);
        SendData(0x02);
        SendCommand(0xE5);
        SendData(0x5A);

        CurrentRefreshMode = RefreshMode.Fast;
    }

    /// <summary>
    /// Initialize the display for partial refresh mode
    /// </summary>
    private void InitializePartial()
    {
        Reset();

        SendCommand(PANEL_SETTING);
        SendData(0x1F);

        SendCommand(VCOM_AND_DATA_INTERVAL_SETTING);
        SendData(0x10);
        SendData(0x07);

        SendCommand(POWER_ON);
        DelayMs(100);
        WaitUntilIdle();

        SendCommand(0xE0);
        SendData(0x02);
        SendCommand(0xE5);
        SendData(0x6E);

        CurrentRefreshMode = RefreshMode.Partial;
    }

    /// <summary>
    /// Clear display buffer
    /// </summary>
    /// <param name="updateDisplay">force display update</param>
    public void Clear(bool updateDisplay = false)
    {
        Clear(false, updateDisplay);
    }

    /// <summary>
    /// Clear the display
    /// </summary>
    /// <param name="enabled">Set the display to the enabled or disabled color (defaults are black and white)</param>
    /// <param name="updateDisplay">Update the display once the buffer has been cleared when true</param>
    public void Clear(bool enabled, bool updateDisplay = false)
    {
        if (imageBuffer is Buffer1bppV buf)
        {
            buf.Clear(enabled);
        }
        else if (imageBuffer is Buffer2bppGreyEPaper buf2)
        {
            buf2.Clear();
        }

        if (updateDisplay)
        {
            Show();
        }
    }

    /// <summary>
    /// Clear the display
    /// </summary>
    /// <param name="color">Color to set the display</param>
    /// <param name="updateDisplay">Update the display once the buffer has been cleared when true</param>
    public void Fill(Color color, bool updateDisplay = false)
    {
        if (imageBuffer is Buffer1bppV buf)
        {
            buf.Clear(color.Color1bpp);
        }
        else if (imageBuffer is Buffer2bppGreyEPaper buf2)
        {
            buf2.Fill(color);
        }
    }

    /// <summary>
    /// Fill the display buffer with a color
    /// </summary>
    /// <param name="x">x location in pixels to start fill</param>
    /// <param name="y">y location in pixels to start fill</param>
    /// <param name="width">width in pixels to fill</param>
    /// <param name="height">height in pixels to fill</param>
    /// <param name="color">color to fill</param>
    public void Fill(int x, int y, int width, int height, Color color)
    {
        imageBuffer.Fill(x, y, width, height, color);
    }

    /// <summary>
    /// Draw a single pixel 
    /// </summary>
    /// <param name="x">x location</param>
    /// <param name="y">y location</param>
    /// <param name="enabled">Turn the pixel on (true) or off (false)</param>
    public void DrawPixel(int x, int y, bool enabled)
    {
        if (imageBuffer is Buffer1bppV buf)
        {
            buf.SetPixel(x, y, enabled);
        }
        else
        {
            imageBuffer.SetPixel(x, y, enabled ? Color.Black : Color.White);
        }
    }

    /// <summary>
    /// Draw a single pixel 
    /// </summary>
    /// <param name="x">x location </param>
    /// <param name="y">y location</param>
    /// <param name="color">Color of pixel</param>
    public void DrawPixel(int x, int y, Color color)
    {
        imageBuffer.SetPixel(x, y, color);
    }

    /// <summary>
    /// Invert color of pixel
    /// </summary>
    /// <param name="x">x coordinate of pixel</param>
    /// <param name="y">y coordinate of pixel</param>
    public void InvertPixel(int x, int y)
    {
        imageBuffer.InvertPixel(x, y);
    }

    /// <summary>
    /// Draw a buffer at a specific location
    /// </summary>
    /// <param name="x">x location in pixels</param>
    /// <param name="y">y location in pixels</param>
    /// <param name="displayBuffer"></param>
    public void WriteBuffer(int x, int y, IPixelBuffer displayBuffer)
    {
        imageBuffer.WriteBuffer(x, y, displayBuffer);
    }

    /// <summary>
    /// Reset the display
    /// </summary>
    protected override void Reset()
    {
        if (resetPort != null)
        {
            resetPort.State = true;
            DelayMs(20);
            resetPort.State = false;
            DelayMs(2);
            resetPort.State = true;
            DelayMs(20);
        }
    }

    /// <summary>
    /// Set partial address window for fast partial refresh
    /// </summary>
    /// <param name="x">X start position in pixels</param>
    /// <param name="y">Y start position in pixels</param>
    /// <param name="width">Width in pixels</param>
    /// <param name="height">Height in pixels</param>
    private void SetPartialWindowForRefresh(int x, int y, int width, int height)
    {
        SendCommand(VCOM_AND_DATA_INTERVAL_SETTING);
        SendData(0xA9);
        SendData(0x07);

        SendCommand(PARTIAL_IN);
        SendCommand(PARTIAL_WINDOW);

        SendData(x >> 8);
        SendData(x & 0xFF);
        SendData((x + width - 1) >> 8);
        SendData((x + width - 1) & 0xFF);
        SendData(y >> 8);
        SendData(y & 0xFF);
        SendData((y + height - 1) >> 8);
        SendData((y + height - 1) & 0xFF);

        SendData(0x01);
    }

    /// <summary>
    /// Copy the display buffer to the display
    /// If called more frequently than the minimum refresh interval, a not supported exception will be thrown
    /// </summary>
    /// <exception cref="NotSupportedException">Thrown if called more frequently than the minimum refresh interval</exception>
    public void Show()
    {
        if (Environment.TickCount - lastUpdatedTick < MinimumRefreshInterval.TotalMilliseconds)
        {
            throw new NotSupportedException($"The minimum update interval for this display is {MinimumRefreshInterval.TotalMilliseconds} milliseconds");
        }
        lastUpdatedTick = Environment.TickCount64;

        // Re-initialize only if switching refresh modes

        if (lastInitializedMode != CurrentRefreshMode)
        {
            if (CurrentRefreshMode == RefreshMode.Fast)
            {
                InitializeFast();
                lastInitializedMode = RefreshMode.Fast;
            }
            else if (CurrentRefreshMode == RefreshMode.Partial)
            {
                InitializePartial();
                lastInitializedMode = RefreshMode.Partial;
            }
            else
            {
                // Full mode - initialize on first call or mode switch
                if (lastInitializedMode != null)
                {
                    Initialize();
                }
                lastInitializedMode = RefreshMode.Full;
            }
        }

        if (ColorMode == ColorMode.Format1bpp)
        {
            Show1bpp();
        }
        else
        {
            Show2bpp();
        }

        // Reset counter and event flags after full refresh
        if (CurrentRefreshMode == RefreshMode.Full)
        {
            PartialRefreshCount = 0;
            fastRefreshEventRaised = false;
            fullRefreshEventRaised = false;
        }
    }

    private void Show1bpp()
    {
        var buffer = imageBuffer.Buffer;

        if (CurrentRefreshMode == RefreshMode.Partial)
        {
            // Hardware partial mode: send only DATA_START_TRANSMISSION_2 with non-inverted buffer
            SendCommand(DATA_START_TRANSMISSION_2);
            dataCommandPort!.State = DataState;
            spiComms?.Write(buffer);
        }
        else
        {
            // Full/Fast mode: send both buffers
            SendCommand(DATA_START_TRANSMISSION_1);
            dataCommandPort!.State = DataState;
            spiComms?.Write(buffer);

            SendCommand(DATA_START_TRANSMISSION_2);
            for (int i = 0; i < buffer.Length; i++)
            {
                SendData(~buffer[i]);
            }
        }

        DisplayFrame();
    }

    private void Show2bpp()
    {
        var buffer = imageBuffer as Buffer2bppGreyEPaper;

        if (CurrentRefreshMode == RefreshMode.Partial)
        {
            // Hardware partial mode: send only DATA_START_TRANSMISSION_2
            SendCommand(DATA_START_TRANSMISSION_2);
            for (int i = 0; i < buffer!.DarkBuffer.Length; i++)
            {
                SendData(buffer.DarkBuffer[i]);
            }
        }
        else
        {
            // Full/Fast mode: send both buffers
            SendCommand(DATA_START_TRANSMISSION_1);
            for (int i = 0; i < buffer!.DarkBuffer.Length; i++)
            {
                SendData(buffer.LightBuffer[i]);
            }

            SendCommand(DATA_START_TRANSMISSION_2);
            for (int i = 0; i < buffer.DarkBuffer.Length; i++)
            {
                SendData(buffer.DarkBuffer[i]);
            }
        }

        DisplayFrame();
    }

    /// <summary>
    /// Copy the display buffer to the display for a set region
    /// Automatically uses partial refresh mode for better performance
    /// If called more frequently than the minimum refresh interval, a not supported exception will be thrown
    /// </summary>
    /// <param name="left">left bounds of region in pixels</param>
    /// <param name="top">top bounds of region in pixels</param>
    /// <param name="right">right bounds of region in pixels</param>
    /// <param name="bottom">bottom bounds of region in pixels</param>
    /// <exception cref="NotSupportedException">Thrown if called more frequently than the minimum refresh interval</exception>
    public void Show(int left, int top, int right, int bottom)
    {
        if (Environment.TickCount64 - lastUpdatedTick < MinimumRefreshInterval.TotalMilliseconds)
        {
            throw new NotSupportedException($"The minimum update interval for this display is {MinimumRefreshInterval.TotalMilliseconds} milliseconds");
        }
        lastUpdatedTick = Environment.TickCount64;

        // Initialize partial refresh if not already in that mode
        switch (CurrentRefreshMode)
        {
            //case RefreshMode.Full:
            //    Initialize();
            //    break;
            case RefreshMode.Fast:
                InitializeFast();
                break;
            case RefreshMode.Partial:
                InitializePartial();
                break;
        }

        // Align to 8-pixel boundaries
        left &= ~7;
        right = (right + 7) & ~7;

        int width = right - left;
        int height = bottom - top;

        SetPartialWindowForRefresh(left, top, width, height);

        if (ColorMode == ColorMode.Format1bpp)
        {
            Show1bppPartial(left, top, right, bottom);
        }
        else
        {
            Show2bppPartial(left, top, right, bottom);
        }

        PartialRefreshCount++;

        // Raise events based on thresholds
        RaiseRefreshRecommendationEvents();
    }

    private void RaiseRefreshRecommendationEvents()
    {
        // Check for fast refresh recommendation (at 50% threshold)
        if (!fastRefreshEventRaised && PartialRefreshCount >= MaxPartialRefreshes * 0.5)
        {
            FastRefreshRecommended?.Invoke(this, new RefreshRecommendedEventArgs(
                RefreshMode.Fast,
                PartialRefreshCount,
                $"Performed {PartialRefreshCount} partial refreshes. Consider using fast refresh for better performance."
            ));
            fastRefreshEventRaised = true;
        }

        // Check for full refresh recommendation (at 100% threshold)
        if (!fullRefreshEventRaised && PartialRefreshCount >= MaxPartialRefreshes)
        {
            FullRefreshRecommended?.Invoke(this, new RefreshRecommendedEventArgs(
                RefreshMode.Full,
                PartialRefreshCount,
                $"Performed {PartialRefreshCount} partial refreshes. Full refresh recommended to clear ghosting."
            ));
            fullRefreshEventRaised = true;
        }
    }

    private void Show1bppPartial(int left, int top, int right, int bottom)
    {
        var buffer = imageBuffer.Buffer;

        SendCommand(DATA_START_TRANSMISSION_2);
        for (int i = top; i < bottom; i++)
        {
            for (int j = left / 8; j < (right / 8); j++)
            {
                SendData(buffer[j + i * Width / 8]);
            }
        }

        SendCommand(DISPLAY_REFRESH);
        DelayMs(100);
        WaitUntilIdle();

        SendCommand(PARTIAL_OUT);
    }

    private void Show2bppPartial(int left, int top, int right, int bottom)
    {
        var buffer = imageBuffer as Buffer2bppGreyEPaper;

        SendCommand(DATA_START_TRANSMISSION_2);
        for (int i = top; i < bottom; i++)
        {
            for (int j = left / 8; j < (right / 8); j++)
            {
                SendData(buffer!.DarkBuffer[j + i * Width / 8]);
            }
        }

        SendCommand(DISPLAY_REFRESH);
        DelayMs(100);
        WaitUntilIdle();

        SendCommand(PARTIAL_OUT);
    }

    /// <summary>
    /// Copy the display buffer to the display using full refresh mode
    /// </summary>
    public void ShowFull()
    {
        var savedMode = CurrentRefreshMode;
        CurrentRefreshMode = RefreshMode.Full;
        Show();
        CurrentRefreshMode = savedMode;
    }

    /// <summary>
    /// Copy the display buffer to the display using fast refresh mode
    /// </summary>
    public void ShowFast()
    {
        var savedMode = CurrentRefreshMode;
        CurrentRefreshMode = RefreshMode.Fast;
        Show();
        CurrentRefreshMode = savedMode;
    }

    /// <summary>
    /// Copy a display region using partial refresh mode
    /// </summary>
    /// <param name="left">left bounds of region in pixels</param>
    /// <param name="top">top bounds of region in pixels</param>
    /// <param name="right">right bounds of region in pixels</param>
    /// <param name="bottom">bottom bounds of region in pixels</param>
    public void ShowPartial(int left, int top, int right, int bottom)
    {
        Show(left, top, right, bottom);
    }

    /// <summary>
    /// Send a refresh command to the display
    /// Does not transfer new data
    /// </summary>
    public void DisplayFrame()
    {
        SendCommand(DISPLAY_REFRESH);
        DelayMs(100);
        WaitUntilIdle();
    }

    /// <summary>
    /// Set the device to low power mode
    /// </summary>
    protected void Sleep()
    {
        SendCommand(VCOM_AND_DATA_INTERVAL_SETTING);
        SendData(0XF7);
        SendCommand(POWER_OFF);
        WaitUntilIdle();
        SendCommand(DEEP_SLEEP);
        SendData(0xA5);
    }

    /// <summary>
    /// Reset the partial refresh counter
    /// Call this after performing a full refresh to reset the recommendation
    /// </summary>
    public void ResetPartialRefreshCount()
    {
        PartialRefreshCount = 0;
        fastRefreshEventRaised = false;
        fullRefreshEventRaised = false;
    }

    private readonly byte PANEL_SETTING = 0x00;
    private readonly byte POWER_SETTING = 0x01;
    private readonly byte POWER_OFF = 0x02;
    private readonly byte POWER_ON = 0x04;
    private readonly byte BOOSTER_SOFT_START = 0x06;
    private readonly byte DEEP_SLEEP = 0x07;
    private readonly byte DATA_START_TRANSMISSION_1 = 0x10;
    private readonly byte DISPLAY_REFRESH = 0x12;
    private readonly byte DATA_START_TRANSMISSION_2 = 0x13;
    private readonly byte VCOM_AND_DATA_INTERVAL_SETTING = 0x50;
    private readonly byte TCON_SETTING = 0x60;
    private readonly byte RESOLUTION_SETTING = 0x61;
    private readonly byte PARTIAL_WINDOW = 0x90;
    private readonly byte PARTIAL_IN = 0x91;
    private readonly byte PARTIAL_OUT = 0x92;
}