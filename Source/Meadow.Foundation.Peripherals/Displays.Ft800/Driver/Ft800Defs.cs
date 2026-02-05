namespace Meadow.Foundation.Displays;

/// <summary>
/// FT800 register addresses, commands, and constants
/// </summary>
public static class Ft800Defs
{
    // ========================================================================
    // Memory Regions
    // ========================================================================

    /// <summary>Graphics RAM - 256KB for bitmaps, fonts, etc.</summary>
    public const uint RAM_G = 0x000000;

    /// <summary>Display list RAM - 8KB</summary>
    public const uint RAM_DL = 0x100000;

    /// <summary>Palette RAM - 1KB</summary>
    public const uint RAM_PAL = 0x102000;

    /// <summary>Register space base address</summary>
    public const uint RAM_REG = 0x102400;

    /// <summary>Command FIFO - 4KB for co-processor commands</summary>
    public const uint RAM_CMD = 0x108000;

    /// <summary>Size of command FIFO in bytes</summary>
    public const uint RAM_CMD_SIZE = 4096;

    // ========================================================================
    // Registers - Identification
    // ========================================================================

    /// <summary>Chip ID register - reads 0x7C for FT800</summary>
    public const uint REG_ID = 0x102400;

    /// <summary>Frame buffer row counter</summary>
    public const uint REG_FRAMES = 0x102404;

    /// <summary>Display clock cycles per line</summary>
    public const uint REG_CLOCK = 0x102408;

    /// <summary>Display frequency</summary>
    public const uint REG_FREQUENCY = 0x10240C;

    // ========================================================================
    // Registers - Display Timing
    // ========================================================================

    /// <summary>Horizontal total cycle count</summary>
    public const uint REG_HCYCLE = 0x102428;

    /// <summary>Horizontal display start offset</summary>
    public const uint REG_HOFFSET = 0x10242C;

    /// <summary>Horizontal display pixel count</summary>
    public const uint REG_HSIZE = 0x102430;

    /// <summary>Horizontal sync fall offset</summary>
    public const uint REG_HSYNC0 = 0x102434;

    /// <summary>Horizontal sync rise offset</summary>
    public const uint REG_HSYNC1 = 0x102438;

    /// <summary>Vertical total cycle count</summary>
    public const uint REG_VCYCLE = 0x10243C;

    /// <summary>Vertical display start offset</summary>
    public const uint REG_VOFFSET = 0x102440;

    /// <summary>Vertical display line count</summary>
    public const uint REG_VSIZE = 0x102444;

    /// <summary>Vertical sync fall offset</summary>
    public const uint REG_VSYNC0 = 0x102448;

    /// <summary>Vertical sync rise offset</summary>
    public const uint REG_VSYNC1 = 0x10244C;

    /// <summary>Display list swap control</summary>
    public const uint REG_DLSWAP = 0x102450;

    /// <summary>Display rotation</summary>
    public const uint REG_ROTATE = 0x102454;

    /// <summary>Output RGB signal swizzle</summary>
    public const uint REG_SWIZZLE = 0x102460;

    /// <summary>Dither control</summary>
    public const uint REG_DITHER = 0x102464;

    /// <summary>Output signal spread</summary>
    public const uint REG_OUTBITS = 0x102468;

    /// <summary>Pixel clock edge and polarity</summary>
    public const uint REG_CSPREAD = 0x102468;

    /// <summary>Pixel clock polarity</summary>
    public const uint REG_PCLK_POL = 0x10246C;

    /// <summary>Pixel clock divisor</summary>
    public const uint REG_PCLK = 0x102470;

    // ========================================================================
    // Registers - GPIO
    // ========================================================================

    /// <summary>GPIO direction register</summary>
    public const uint REG_GPIO_DIR = 0x102490;

    /// <summary>GPIO data register</summary>
    public const uint REG_GPIO = 0x102494;

    // ========================================================================
    // Registers - Interrupts
    // ========================================================================

    /// <summary>Interrupt flags register</summary>
    public const uint REG_INT_FLAGS = 0x102498;

    /// <summary>Interrupt enable register</summary>
    public const uint REG_INT_EN = 0x10249C;

    /// <summary>Interrupt mask register</summary>
    public const uint REG_INT_MASK = 0x1024A0;

    // ========================================================================
    // Registers - PWM / Backlight
    // ========================================================================

    /// <summary>PWM output frequency</summary>
    public const uint REG_PWM_HZ = 0x1024C0;

    /// <summary>PWM duty cycle for backlight (0-128)</summary>
    public const uint REG_PWM_DUTY = 0x1024C4;

    // ========================================================================
    // Registers - Audio
    // ========================================================================

    /// <summary>Audio playback volume</summary>
    public const uint REG_VOL_PB = 0x1024C8;

    /// <summary>Sound effect volume</summary>
    public const uint REG_VOL_SOUND = 0x1024CC;

    /// <summary>Sound effect selection</summary>
    public const uint REG_SOUND = 0x1024D0;

    /// <summary>Sound playback control</summary>
    public const uint REG_PLAY = 0x1024D4;

    // ========================================================================
    // Registers - Command FIFO
    // ========================================================================

    /// <summary>Command FIFO read pointer</summary>
    public const uint REG_CMD_READ = 0x1024E4;

    /// <summary>Command FIFO write pointer</summary>
    public const uint REG_CMD_WRITE = 0x1024E8;

    /// <summary>Command FIFO display list offset</summary>
    public const uint REG_CMD_DL = 0x1024EC;

    // ========================================================================
    // Registers - Touch
    // ========================================================================

    /// <summary>Touch engine mode</summary>
    public const uint REG_TOUCH_MODE = 0x1024F0;

    /// <summary>Touch ADC mode</summary>
    public const uint REG_TOUCH_ADC_MODE = 0x1024F4;

    /// <summary>Touch charge current</summary>
    public const uint REG_TOUCH_CHARGE = 0x1024F8;

    /// <summary>Touch settle time</summary>
    public const uint REG_TOUCH_SETTLE = 0x1024FC;

    /// <summary>Touch oversample factor</summary>
    public const uint REG_TOUCH_OVERSAMPLE = 0x102500;

    /// <summary>Touch resistance Z threshold</summary>
    public const uint REG_TOUCH_RZTHRESH = 0x102504;

    /// <summary>Raw touch X/Y values</summary>
    public const uint REG_TOUCH_RAW_XY = 0x102508;

    /// <summary>Raw touch resistance Z</summary>
    public const uint REG_TOUCH_RZ = 0x10250C;

    /// <summary>Transformed touch screen X/Y</summary>
    public const uint REG_TOUCH_SCREEN_XY = 0x102510;

    /// <summary>Touch tag value</summary>
    public const uint REG_TOUCH_TAG = 0x102518;

    /// <summary>Touch tag X/Y position</summary>
    public const uint REG_TOUCH_TAG_XY = 0x102514;

    /// <summary>Touch transform matrix A</summary>
    public const uint REG_TOUCH_TRANSFORM_A = 0x10251C;

    /// <summary>Touch transform matrix B</summary>
    public const uint REG_TOUCH_TRANSFORM_B = 0x102520;

    /// <summary>Touch transform matrix C</summary>
    public const uint REG_TOUCH_TRANSFORM_C = 0x102524;

    /// <summary>Touch transform matrix D</summary>
    public const uint REG_TOUCH_TRANSFORM_D = 0x102528;

    /// <summary>Touch transform matrix E</summary>
    public const uint REG_TOUCH_TRANSFORM_E = 0x10252C;

    /// <summary>Touch transform matrix F</summary>
    public const uint REG_TOUCH_TRANSFORM_F = 0x102530;

    /// <summary>Touch direct X value</summary>
    public const uint REG_TOUCH_DIRECT_XY = 0x102574;

    /// <summary>Touch direct Z value</summary>
    public const uint REG_TOUCH_DIRECT_Z1Z2 = 0x102578;

    // ========================================================================
    // Registers - Tracker
    // ========================================================================

    /// <summary>Tracker control</summary>
    public const uint REG_TRACKER = 0x109000;

    // ========================================================================
    // Host Commands
    // ========================================================================

    /// <summary>Switch to active mode</summary>
    public const byte CMD_ACTIVE = 0x00;

    /// <summary>Switch to standby mode</summary>
    public const byte CMD_STANDBY = 0x41;

    /// <summary>Switch to sleep mode</summary>
    public const byte CMD_SLEEP = 0x42;

    /// <summary>Power down the FT800</summary>
    public const byte CMD_PWRDOWN = 0x50;

    /// <summary>Select external clock</summary>
    public const byte CMD_CLKEXT = 0x44;

    /// <summary>Select 48MHz PLL output</summary>
    public const byte CMD_CLK48M = 0x62;

    /// <summary>Select 36MHz PLL output</summary>
    public const byte CMD_CLK36M = 0x61;

    /// <summary>Core reset</summary>
    public const byte CMD_CORERST = 0x68;

    // ========================================================================
    // Display List Commands
    // ========================================================================

    /// <summary>End the display list</summary>
    public const uint DL_DISPLAY = 0x00000000;

    /// <summary>Set bitmap source address</summary>
    public const uint DL_BITMAP_SOURCE = 0x01000000;

    /// <summary>Set clear color RGB</summary>
    public const uint DL_CLEAR_COLOR_RGB = 0x02000000;

    /// <summary>Set tag value for following objects</summary>
    public const uint DL_TAG = 0x03000000;

    /// <summary>Set drawing color RGB</summary>
    public const uint DL_COLOR_RGB = 0x04000000;

    /// <summary>Set bitmap handle</summary>
    public const uint DL_BITMAP_HANDLE = 0x05000000;

    /// <summary>Set bitmap cell</summary>
    public const uint DL_CELL = 0x06000000;

    /// <summary>Set bitmap layout (format, stride, height)</summary>
    public const uint DL_BITMAP_LAYOUT = 0x07000000;

    /// <summary>Set bitmap size parameters</summary>
    public const uint DL_BITMAP_SIZE = 0x08000000;

    /// <summary>Set alpha test function</summary>
    public const uint DL_ALPHA_FUNC = 0x09000000;

    /// <summary>Set stencil test function</summary>
    public const uint DL_STENCIL_FUNC = 0x0A000000;

    /// <summary>Set blend function</summary>
    public const uint DL_BLEND_FUNC = 0x0B000000;

    /// <summary>Set stencil operation</summary>
    public const uint DL_STENCIL_OP = 0x0C000000;

    /// <summary>Set point size (in 1/16 pixel units)</summary>
    public const uint DL_POINT_SIZE = 0x0D000000;

    /// <summary>Set line width (in 1/16 pixel units)</summary>
    public const uint DL_LINE_WIDTH = 0x0E000000;

    /// <summary>Set clear color alpha</summary>
    public const uint DL_CLEAR_COLOR_A = 0x0F000000;

    /// <summary>Set drawing color alpha</summary>
    public const uint DL_COLOR_A = 0x10000000;

    /// <summary>Set clear stencil value</summary>
    public const uint DL_CLEAR_STENCIL = 0x11000000;

    /// <summary>Set clear tag value</summary>
    public const uint DL_CLEAR_TAG = 0x12000000;

    /// <summary>Set stencil write mask</summary>
    public const uint DL_STENCIL_MASK = 0x13000000;

    /// <summary>Enable/disable tag buffer writes</summary>
    public const uint DL_TAG_MASK = 0x14000000;

    /// <summary>Bitmap transform matrix A coefficient</summary>
    public const uint DL_BITMAP_TRANSFORM_A = 0x15000000;

    /// <summary>Bitmap transform matrix B coefficient</summary>
    public const uint DL_BITMAP_TRANSFORM_B = 0x16000000;

    /// <summary>Bitmap transform matrix C coefficient</summary>
    public const uint DL_BITMAP_TRANSFORM_C = 0x17000000;

    /// <summary>Bitmap transform matrix D coefficient</summary>
    public const uint DL_BITMAP_TRANSFORM_D = 0x18000000;

    /// <summary>Bitmap transform matrix E coefficient</summary>
    public const uint DL_BITMAP_TRANSFORM_E = 0x19000000;

    /// <summary>Bitmap transform matrix F coefficient</summary>
    public const uint DL_BITMAP_TRANSFORM_F = 0x1A000000;

    /// <summary>Set scissor clip rectangle position</summary>
    public const uint DL_SCISSOR_XY = 0x1B000000;

    /// <summary>Set scissor clip rectangle size</summary>
    public const uint DL_SCISSOR_SIZE = 0x1C000000;

    /// <summary>Call a subroutine</summary>
    public const uint DL_CALL = 0x1D000000;

    /// <summary>Jump to a display list address</summary>
    public const uint DL_JUMP = 0x1E000000;

    /// <summary>Begin drawing a graphics primitive</summary>
    public const uint DL_BEGIN = 0x1F000000;

    /// <summary>Set color channel write mask</summary>
    public const uint DL_COLOR_MASK = 0x20000000;

    /// <summary>End drawing a graphics primitive</summary>
    public const uint DL_END = 0x21000000;

    /// <summary>Save current graphics context</summary>
    public const uint DL_SAVE_CONTEXT = 0x22000000;

    /// <summary>Restore saved graphics context</summary>
    public const uint DL_RESTORE_CONTEXT = 0x23000000;

    /// <summary>Return from subroutine</summary>
    public const uint DL_RETURN = 0x24000000;

    /// <summary>Execute a macro</summary>
    public const uint DL_MACRO = 0x25000000;

    /// <summary>Clear buffers (color, stencil, tag)</summary>
    public const uint DL_CLEAR = 0x26000000;

    /// <summary>Supply vertex with subpixel precision</summary>
    public const uint DL_VERTEX2F = 0x40000000;

    /// <summary>Supply vertex with pixel precision and handle/cell</summary>
    public const uint DL_VERTEX2II = 0x80000000;

    // ========================================================================
    // Graphics Primitive Types (for BEGIN command)
    // ========================================================================

    /// <summary>Bitmap primitive</summary>
    public const byte PRIM_BITMAPS = 1;

    /// <summary>Point primitive</summary>
    public const byte PRIM_POINTS = 2;

    /// <summary>Line primitive</summary>
    public const byte PRIM_LINES = 3;

    /// <summary>Line strip primitive</summary>
    public const byte PRIM_LINE_STRIP = 4;

    /// <summary>Edge strip right primitive</summary>
    public const byte PRIM_EDGE_STRIP_R = 5;

    /// <summary>Edge strip left primitive</summary>
    public const byte PRIM_EDGE_STRIP_L = 6;

    /// <summary>Edge strip above primitive</summary>
    public const byte PRIM_EDGE_STRIP_A = 7;

    /// <summary>Edge strip below primitive</summary>
    public const byte PRIM_EDGE_STRIP_B = 8;

    /// <summary>Rectangle primitive</summary>
    public const byte PRIM_RECTS = 9;

    // ========================================================================
    // Co-processor Commands
    // ========================================================================

    /// <summary>Start a new display list</summary>
    public const uint CMD_DLSTART = 0xFFFFFF00;

    /// <summary>Swap the current display list</summary>
    public const uint CMD_SWAP = 0xFFFFFF01;

    /// <summary>Set interrupt flag</summary>
    public const uint CMD_INTERRUPT = 0xFFFFFF02;

    /// <summary>Set co-processor background color</summary>
    public const uint CMD_BGCOLOR = 0xFFFFFF09;

    /// <summary>Set co-processor foreground color</summary>
    public const uint CMD_FGCOLOR = 0xFFFFFF0A;

    /// <summary>Set gradient color</summary>
    public const uint CMD_GRADCOLOR = 0xFFFFFF34;

    /// <summary>Draw text</summary>
    public const uint CMD_TEXT = 0xFFFFFF0C;

    /// <summary>Draw a button</summary>
    public const uint CMD_BUTTON = 0xFFFFFF0D;

    /// <summary>Draw keyboard keys</summary>
    public const uint CMD_KEYS = 0xFFFFFF0E;

    /// <summary>Draw a progress bar</summary>
    public const uint CMD_PROGRESS = 0xFFFFFF0F;

    /// <summary>Draw a slider</summary>
    public const uint CMD_SLIDER = 0xFFFFFF10;

    /// <summary>Draw a scrollbar</summary>
    public const uint CMD_SCROLLBAR = 0xFFFFFF11;

    /// <summary>Draw a toggle switch</summary>
    public const uint CMD_TOGGLE = 0xFFFFFF12;

    /// <summary>Draw a gauge</summary>
    public const uint CMD_GAUGE = 0xFFFFFF13;

    /// <summary>Draw an analog clock</summary>
    public const uint CMD_CLOCK = 0xFFFFFF14;

    /// <summary>Calibrate touch screen</summary>
    public const uint CMD_CALIBRATE = 0xFFFFFF15;

    /// <summary>Start a spinner animation</summary>
    public const uint CMD_SPINNER = 0xFFFFFF16;

    /// <summary>Stop any spinner animation</summary>
    public const uint CMD_STOP = 0xFFFFFF17;

    /// <summary>Set up memory for bitmap writes</summary>
    public const uint CMD_MEMCRC = 0xFFFFFF18;

    /// <summary>Read a register value</summary>
    public const uint CMD_REGREAD = 0xFFFFFF19;

    /// <summary>Write to memory</summary>
    public const uint CMD_MEMWRITE = 0xFFFFFF1A;

    /// <summary>Fill memory with a value</summary>
    public const uint CMD_MEMSET = 0xFFFFFF1B;

    /// <summary>Zero a block of memory</summary>
    public const uint CMD_MEMZERO = 0xFFFFFF1C;

    /// <summary>Copy a block of memory</summary>
    public const uint CMD_MEMCPY = 0xFFFFFF1D;

    /// <summary>Append commands to display list</summary>
    public const uint CMD_APPEND = 0xFFFFFF1E;

    /// <summary>Create a snapshot of the current screen</summary>
    public const uint CMD_SNAPSHOT = 0xFFFFFF1F;

    /// <summary>Draw a bitmap with transformation</summary>
    public const uint CMD_BITMAP_TRANSFORM = 0xFFFFFF21;

    /// <summary>Inflate compressed data</summary>
    public const uint CMD_INFLATE = 0xFFFFFF22;

    /// <summary>Get the end address of decompressed data</summary>
    public const uint CMD_GETPTR = 0xFFFFFF23;

    /// <summary>Load a JPEG image</summary>
    public const uint CMD_LOADIMAGE = 0xFFFFFF24;

    /// <summary>Get properties of the loaded image</summary>
    public const uint CMD_GETPROPS = 0xFFFFFF25;

    /// <summary>Load identity transform matrix</summary>
    public const uint CMD_LOADIDENTITY = 0xFFFFFF26;

    /// <summary>Translate the transform matrix</summary>
    public const uint CMD_TRANSLATE = 0xFFFFFF27;

    /// <summary>Scale the transform matrix</summary>
    public const uint CMD_SCALE = 0xFFFFFF28;

    /// <summary>Rotate the transform matrix</summary>
    public const uint CMD_ROTATE = 0xFFFFFF29;

    /// <summary>Set current transform matrix</summary>
    public const uint CMD_SETMATRIX = 0xFFFFFF2A;

    /// <summary>Set font base address</summary>
    public const uint CMD_SETFONT = 0xFFFFFF2B;

    /// <summary>Track touch input on an object</summary>
    public const uint CMD_TRACK = 0xFFFFFF2C;

    /// <summary>Draw a dial</summary>
    public const uint CMD_DIAL = 0xFFFFFF2D;

    /// <summary>Draw a number</summary>
    public const uint CMD_NUMBER = 0xFFFFFF2E;

    /// <summary>Start a screen saver</summary>
    public const uint CMD_SCREENSAVER = 0xFFFFFF2F;

    /// <summary>Draw a sketch area</summary>
    public const uint CMD_SKETCH = 0xFFFFFF30;

    /// <summary>Display the logo</summary>
    public const uint CMD_LOGO = 0xFFFFFF31;

    /// <summary>Set the cold start flag</summary>
    public const uint CMD_COLDSTART = 0xFFFFFF32;

    /// <summary>Get current display list position</summary>
    public const uint CMD_GETMATRIX = 0xFFFFFF33;

    /// <summary>Draw a gradient</summary>
    public const uint CMD_GRADIENT = 0xFFFFFF0B;

    // ========================================================================
    // Bitmap Formats
    // ========================================================================

    /// <summary>ARGB1555 format - 1 bit alpha, 5 bits each RGB</summary>
    public const byte FORMAT_ARGB1555 = 0;

    /// <summary>L1 format - 1 bit luminance</summary>
    public const byte FORMAT_L1 = 1;

    /// <summary>L4 format - 4 bit luminance</summary>
    public const byte FORMAT_L4 = 2;

    /// <summary>L8 format - 8 bit luminance (grayscale)</summary>
    public const byte FORMAT_L8 = 3;

    /// <summary>RGB332 format - 3 bits R, 3 bits G, 2 bits B</summary>
    public const byte FORMAT_RGB332 = 4;

    /// <summary>ARGB2 format - 2 bits each ARGB</summary>
    public const byte FORMAT_ARGB2 = 5;

    /// <summary>ARGB4 format - 4 bits each ARGB</summary>
    public const byte FORMAT_ARGB4 = 6;

    /// <summary>RGB565 format - 5 bits R, 6 bits G, 5 bits B</summary>
    public const byte FORMAT_RGB565 = 7;

    /// <summary>Paletted format</summary>
    public const byte FORMAT_PALETTED = 8;

    /// <summary>Text 8x8 format</summary>
    public const byte FORMAT_TEXT8X8 = 9;

    /// <summary>Text VGA format</summary>
    public const byte FORMAT_TEXTVGA = 10;

    /// <summary>Bargraph format</summary>
    public const byte FORMAT_BARGRAPH = 11;

    // ========================================================================
    // Widget Options
    // ========================================================================

    /// <summary>No option</summary>
    public const ushort OPT_NONE = 0;

    /// <summary>3D effect for buttons</summary>
    public const ushort OPT_3D = 0;

    /// <summary>Flat appearance (no 3D effect)</summary>
    public const ushort OPT_FLAT = 256;

    /// <summary>Signed number</summary>
    public const ushort OPT_SIGNED = 256;

    /// <summary>Center horizontally</summary>
    public const ushort OPT_CENTERX = 512;

    /// <summary>Center vertically</summary>
    public const ushort OPT_CENTERY = 1024;

    /// <summary>Center both horizontally and vertically</summary>
    public const ushort OPT_CENTER = 1536;

    /// <summary>Right align</summary>
    public const ushort OPT_RIGHTX = 2048;

    /// <summary>No background for text</summary>
    public const ushort OPT_NOBACK = 4096;

    /// <summary>No ticker marks on gauge</summary>
    public const ushort OPT_NOTICKS = 8192;

    /// <summary>No hands on clock/gauge</summary>
    public const ushort OPT_NOHANDS = 49152;

    /// <summary>No hour hand on clock</summary>
    public const ushort OPT_NOHM = 16384;

    /// <summary>No minute hand on clock</summary>
    public const ushort OPT_NOSECS = 32768;

    /// <summary>No pointer on gauge</summary>
    public const ushort OPT_NOPOINTER = 16384;

    // ========================================================================
    // Clear Flags
    // ========================================================================

    /// <summary>Clear color buffer</summary>
    public const byte CLEAR_COLOR = 1;

    /// <summary>Clear stencil buffer</summary>
    public const byte CLEAR_STENCIL = 2;

    /// <summary>Clear tag buffer</summary>
    public const byte CLEAR_TAG = 4;

    /// <summary>Clear all buffers</summary>
    public const byte CLEAR_ALL = 7;

    // ========================================================================
    // Blend Functions
    // ========================================================================

    /// <summary>Blend factor zero</summary>
    public const byte BLEND_ZERO = 0;

    /// <summary>Blend factor one</summary>
    public const byte BLEND_ONE = 1;

    /// <summary>Source alpha blend factor</summary>
    public const byte BLEND_SRC_ALPHA = 2;

    /// <summary>Destination alpha blend factor</summary>
    public const byte BLEND_DST_ALPHA = 3;

    /// <summary>One minus source alpha</summary>
    public const byte BLEND_ONE_MINUS_SRC_ALPHA = 4;

    /// <summary>One minus destination alpha</summary>
    public const byte BLEND_ONE_MINUS_DST_ALPHA = 5;

    // ========================================================================
    // Display List Swap Modes
    // ========================================================================

    /// <summary>Swap display list at end of frame</summary>
    public const byte DLSWAP_FRAME = 2;

    /// <summary>Swap display list immediately</summary>
    public const byte DLSWAP_LINE = 1;

    // ========================================================================
    // Interrupt Flags
    // ========================================================================

    /// <summary>Swap completed interrupt</summary>
    public const byte INT_SWAP = 1;

    /// <summary>Touch detected interrupt</summary>
    public const byte INT_TOUCH = 2;

    /// <summary>Tag value change interrupt</summary>
    public const byte INT_TAG = 4;

    /// <summary>Sound effect completed interrupt</summary>
    public const byte INT_SOUND = 8;

    /// <summary>Audio playback completed interrupt</summary>
    public const byte INT_PLAYBACK = 16;

    /// <summary>Command FIFO empty interrupt</summary>
    public const byte INT_CMDEMPTY = 32;

    /// <summary>Command FIFO flag interrupt</summary>
    public const byte INT_CMDFLAG = 64;

    /// <summary>Co-processor fault interrupt</summary>
    public const byte INT_CONVCOMPLETE = 128;

    // ========================================================================
    // Touch Modes
    // ========================================================================

    /// <summary>Touch sampling off</summary>
    public const byte TOUCHMODE_OFF = 0;

    /// <summary>One-shot touch sampling</summary>
    public const byte TOUCHMODE_ONESHOT = 1;

    /// <summary>Continuous touch sampling at frame rate</summary>
    public const byte TOUCHMODE_FRAME = 2;

    /// <summary>Continuous touch sampling</summary>
    public const byte TOUCHMODE_CONTINUOUS = 3;

    // ========================================================================
    // Expected Values
    // ========================================================================

    /// <summary>Expected chip ID value for FT800</summary>
    public const byte CHIP_ID_FT800 = 0x7C;
}

/// <summary>
/// FT800 built-in ROM font handles
/// </summary>
public enum Ft800Font
{
    /// <summary>8x8 pixel font</summary>
    Size8 = 16,

    /// <summary>8x8 pixel font (alternate)</summary>
    Size8Alt = 17,

    /// <summary>8x16 pixel font</summary>
    Size8x16 = 18,

    /// <summary>8x16 pixel font (alternate)</summary>
    Size8x16Alt = 19,

    /// <summary>8x13 pixel font</summary>
    Size8x13 = 20,

    /// <summary>8x13 pixel font (alternate)</summary>
    Size8x13Alt = 21,

    /// <summary>16x16 pixel font</summary>
    Size16 = 22,

    /// <summary>16x16 pixel font (alternate)</summary>
    Size16Alt = 23,

    /// <summary>16x24 pixel font</summary>
    Size16x24 = 24,

    /// <summary>20x20 pixel font</summary>
    Size20 = 25,

    /// <summary>20x32 pixel font</summary>
    Size20x32 = 26,

    /// <summary>25x40 pixel font</summary>
    Size25 = 27,

    /// <summary>28x48 pixel font</summary>
    Size28 = 28,

    /// <summary>31x52 pixel font</summary>
    Size31 = 29,

    /// <summary>34x56 pixel font</summary>
    Size34 = 30,

    /// <summary>49x64 pixel font (largest)</summary>
    Size49 = 31
}

/// <summary>
/// FT800 spinner animation styles
/// </summary>
public enum Ft800SpinnerStyle
{
    /// <summary>Circular spinner</summary>
    Circle = 0,

    /// <summary>Linear spinner</summary>
    Line = 1,

    /// <summary>Clock-style spinner</summary>
    Clock = 2,

    /// <summary>Orbiting dots</summary>
    Orbiting = 3
}

/// <summary>
/// FT800 bitmap formats
/// </summary>
public enum Ft800BitmapFormat
{
    /// <summary>ARGB1555 - 1 bit alpha, 5 bits each RGB</summary>
    Argb1555 = 0,

    /// <summary>L1 - 1 bit luminance</summary>
    L1 = 1,

    /// <summary>L4 - 4 bit luminance</summary>
    L4 = 2,

    /// <summary>L8 - 8 bit luminance (grayscale)</summary>
    L8 = 3,

    /// <summary>RGB332 - 3-3-2 bits RGB</summary>
    Rgb332 = 4,

    /// <summary>ARGB2 - 2 bits each ARGB</summary>
    Argb2 = 5,

    /// <summary>ARGB4 - 4 bits each ARGB</summary>
    Argb4 = 6,

    /// <summary>RGB565 - 5-6-5 bits RGB</summary>
    Rgb565 = 7,

    /// <summary>Paletted 256 color</summary>
    Paletted = 8
}
