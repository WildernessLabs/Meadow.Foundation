using Meadow.Hardware;
using Meadow.Units;
using System;
using System.Threading.Tasks;

namespace Meadow.Foundation.Sensors.Motion;

/// <summary>
/// Represents the APDS9960 Proximity, Light, RGB, and Gesture Sensor
/// </summary>
public partial class Apds9960 : ByteCommsSensorBase<(Color? Color, Illuminance? AmbientLight)>,
    II2cPeripheral, IDisposable
{
    /// <summary>
    /// Raised when the ambient light value changes
    /// </summary>
    public event EventHandler<IChangeResult<Illuminance>> AmbientLightUpdated = default!;

    /// <summary>
    /// Raised when the color value changes
    /// </summary>
    public event EventHandler<IChangeResult<Color>> ColorUpdated = default!;

    /// <summary>
    /// The default I2C address for the peripheral
    /// </summary>
    public byte DefaultI2cAddress => (byte)Addresses.Default;

    /// <summary>
    /// Is the object disposed
    /// </summary>
    public bool IsDisposed { get; private set; }

    /// <summary>
    /// Did we create the port(s) used by the peripheral
    /// </summary>
    private readonly bool createdPort = false;

    private readonly IDigitalInterruptPort? interruptPort;
    private static readonly byte ERROR = 0xFF;
    private static readonly byte FIFO_PAUSE_TIME = 30;      // Wait period (ms) between FIFO reads

    /// <summary>
    /// The current color value
    /// </summary>
    public Color? Color => Conditions.Color;

    /// <summary>
    /// The current ambient light value
    /// </summary>
    public Illuminance? AmbientLight => Conditions.AmbientLight;

    private readonly Memory<byte> readBuffer = new byte[256];

    /// <summary>
    /// Create a new instance of the APDS9960 communicating over the I2C interface.
    /// </summary>
    /// <param name="i2cBus">SI2C bus object</param>
    /// <param name="interruptPin">The interrupt pin</param>
    public Apds9960(II2cBus i2cBus, IPin? interruptPin = null)
        : base(i2cBus, (byte)Addresses.Default)
    {
        if (interruptPin != null)
        {
            createdPort = true;
            interruptPort = interruptPin.CreateDigitalInterruptPort(InterruptMode.EdgeRising, ResistorMode.Disabled);
            interruptPort.Changed += InterruptPort_Changed;
        }

        gestureData = new GestureData();

        gestureUdDelta = 0;
        gestureLrDelta = 0;

        gestureUdCount = 0;
        gestureLrCount = 0;

        gestureNearCount = 0;
        gestureFarCount = 0;

        gestureState = 0;

        Initialize();
    }

    /// <summary>
    /// Reads data from the sensor
    /// </summary>
    /// <returns>The latest sensor reading</returns>
    protected override Task<(Color? Color, Illuminance? AmbientLight)> ReadSensor()
    {
        (Color? Color, Illuminance? AmbientLight) conditions;

        // TODO: before each of these readings, we need to check to see
        // if that feature is enabled, and if it's not, skip it and set
        // the `conditions.[feature] = null;`

        //---- ambient light
        // TODO: someone needs to verify this
        // have no idea if this conversion is correct. the extent of the datasheet documentation is:
        // "RGBC results can be used to calculate ambient light levels (i.e. Lux) and color temperature (i.e. Kelvin)."
        // NOTE: looks correct, actually. reading ~600 lux in my office and went to 4k LUX when i moved the sensor to the window
        var ambient = ReadAmbientLight();
        conditions.AmbientLight = new Illuminance(ambient, Illuminance.UnitType.Lux);

        //---- color
        // TODO: someone needs to verify this.
        var rgbDivisor = 65536 / 256; // come back as 16-bit values (ushorts). need to be byte.
        var r = ReadRedLight() / rgbDivisor;
        var g = ReadGreenLight() / rgbDivisor;
        var b = ReadBlueLight() / rgbDivisor;
        var a = ambient / rgbDivisor;

        conditions.Color = Meadow.Color.FromRgba(r, g, b, a);

        return Task.FromResult(conditions);
    }

    /// <summary>
    /// Raise events for subscribers and notify of value changes
    /// </summary>
    /// <param name="changeResult">The updated sensor data</param>
    protected override void RaiseEventsAndNotify(IChangeResult<(Color? Color, Illuminance? AmbientLight)> changeResult)
    {
        if (changeResult.New.AmbientLight is { } ambient)
        {
            AmbientLightUpdated?.Invoke(this, new ChangeResult<Illuminance>(ambient, changeResult.Old?.AmbientLight));
        }
        if (changeResult.New.Color is { } color)
        {
            ColorUpdated?.Invoke(this, new ChangeResult<Color>(color, changeResult.Old?.Color));
        }
        base.RaiseEventsAndNotify(changeResult);
    }

    private void InterruptPort_Changed(object sender, DigitalPortResult e)
    {
        //    throw new NotImplementedException();
    }

    private void Initialize()
    {
        var id = BusComms.ReadRegister(Registers.APDS9960_ID);

        SetMode(OperatingModes.ALL, BooleanValues.OFF);

        /* Set default values for ambient light and proximity registers */
        BusComms.WriteRegister(Registers.ATIME, DefaultValues.DEFAULT_ATIME);
        BusComms.WriteRegister(Registers.WTIME, DefaultValues.DEFAULT_WTIME);
        BusComms.WriteRegister(Registers.APDS9960_PPULSE, DefaultValues.DEFAULT_PROX_PPULSE);
        BusComms.WriteRegister(Registers.APDS9960_POFFSET_UR, DefaultValues.DEFAULT_POFFSET_UR);
        BusComms.WriteRegister(Registers.APDS9960_POFFSET_DL, DefaultValues.DEFAULT_POFFSET_DL);
        BusComms.WriteRegister(Registers.APDS9960_CONFIG1, DefaultValues.DEFAULT_CONFIG1);
        SetLEDDrive(DefaultValues.DEFAULT_LDRIVE);

        SetProximityGain(DefaultValues.DEFAULT_PGAIN);
        SetAmbientLightGain(DefaultValues.DEFAULT_AGAIN);
        SetProxIntLowThresh(DefaultValues.DEFAULT_PILT);
        SetProxIntHighThresh(DefaultValues.DEFAULT_PIHT);

        SetLightIntLowThreshold(DefaultValues.DEFAULT_AILT);

        SetLightIntHighThreshold(DefaultValues.DEFAULT_AIHT);

        BusComms.WriteRegister(Registers.APDS9960_PERS, DefaultValues.DEFAULT_PERS);

        BusComms.WriteRegister(Registers.APDS9960_CONFIG2, DefaultValues.DEFAULT_CONFIG2);

        BusComms.WriteRegister(Registers.APDS9960_CONFIG3, DefaultValues.DEFAULT_CONFIG3);

        SetGestureEnterThresh(DefaultValues.DEFAULT_GPENTH);

        SetGestureExitThresh(DefaultValues.DEFAULT_GEXTH);

        BusComms.WriteRegister(Registers.APDS9960_GCONF1, DefaultValues.DEFAULT_GCONF1);

        SetGestureGain(DefaultValues.DEFAULT_GGAIN);

        SetGestureLEDDrive(DefaultValues.DEFAULT_GLDRIVE);

        SetGestureWaitTime(DefaultValues.DEFAULT_GWTIME);

        BusComms.WriteRegister(Registers.APDS9960_GOFFSET_U, DefaultValues.DEFAULT_GOFFSET);
        BusComms.WriteRegister(Registers.APDS9960_GOFFSET_D, DefaultValues.DEFAULT_GOFFSET);
        BusComms.WriteRegister(Registers.APDS9960_GOFFSET_L, DefaultValues.DEFAULT_GOFFSET);
        BusComms.WriteRegister(Registers.APDS9960_GOFFSET_R, DefaultValues.DEFAULT_GOFFSET);
        BusComms.WriteRegister(Registers.APDS9960_GPULSE, DefaultValues.DEFAULT_GPULSE);
        BusComms.WriteRegister(Registers.APDS9960_GCONF3, DefaultValues.DEFAULT_GCONF3);
        SetGestureIntEnable(DefaultValues.DEFAULT_GIEN);
    }

    /**
     * @brief Reads and returns the contents of the ENABLE register
     *
     * @return Contents of the ENABLE register. 0xFF if error.
     */
    private byte GetMode()
    {
        return BusComms.ReadRegister(Registers.ENABLE);
    }

    /**
     * @brief Enables or disables a feature in the APDS-9960
     *
     * @param[in] mode which feature to enable
     * @param[in] enable ON (1) or OFF (0)
     * @return True if operation success. False otherwise.
     */
    private void SetMode(byte mode, byte enable)
    {
        byte reg_val;

        /* Read current ENABLE register */
        reg_val = GetMode();

        if (reg_val == ERROR)
        {
            return; //ToDo exception
        }

        /* Change bit(s) in ENABLE register */
        enable = (byte)(enable & 0x01);
        if (mode >= 0 && mode <= 6)
        {
            if (enable > 0)
            {
                reg_val |= (byte)(1 << mode);
            }
            else
            {
                reg_val &= (byte)~(1 << mode);
            }
        }
        else if (mode == OperatingModes.ALL)
        {
            if (enable > 0)
            {
                reg_val = 0x7F;
            }
            else
            {
                reg_val = 0x00;
            }
        }

        /* Write value back to ENABLE register */
        BusComms.WriteRegister(Registers.ENABLE, reg_val);
    }

    /// <summary>
    /// Enable light sensor
    /// </summary>
    /// <param name="interrupts">True to enable interrupts for light</param>
    public void EnableLightSensor(bool interrupts)
    {
        /* Set default gain, interrupts, enable power, and enable sensor */
        SetAmbientLightGain(DefaultValues.DEFAULT_AGAIN);

        SetAmbientLightIntEnable(interrupts);

        EnablePower(true);
        SetMode(OperatingModes.AMBIENT_LIGHT, 1);
    }

    /// <summary>
    /// Disable light sensor
    /// </summary>
    public void DisableLightSensor()
    {
        SetAmbientLightIntEnable(false);
        SetMode(OperatingModes.AMBIENT_LIGHT, 0);
    }

    /// <summary>
    /// Enable proximity sensor
    /// </summary>
    /// <param name="interrupts">True to enable interrupts for proximity</param>
    public void EnableProximitySensor(bool interrupts)
    {
        /* Set default gain, LED, interrupts, enable power, and enable sensor */
        SetProximityGain(DefaultValues.DEFAULT_PGAIN);
        SetLEDDrive(DefaultValues.DEFAULT_LDRIVE);

        if (interrupts)
        {
            SetProximityIntEnable(1);
        }
        else
        {
            SetProximityIntEnable(0);
        }
        EnablePower(true);
        SetMode(OperatingModes.PROXIMITY, 1);
    }

    /// <summary>
    /// Disable proximity sensor
    /// </summary>
    public void DisableProximitySensor()
    {
        SetProximityIntEnable(0);
        SetMode(OperatingModes.PROXIMITY, 0);
    }

    /// <summary>
    /// Enable power
    /// </summary>
    /// <param name="enable">True to enable, false to disable</param>
    public void EnablePower(bool enable)
    {
        SetMode(OperatingModes.POWER, (byte)(enable ? 1 : 0));
    }

    /// <summary>
    /// Read ambient light value
    /// </summary>
    /// <returns></returns>
    protected ushort ReadAmbientLight()
    {
        byte val = BusComms.ReadRegister(Registers.APDS9960_CDATAL);

        byte val_byte = BusComms.ReadRegister(Registers.APDS9960_CDATAH);

        return (ushort)(val + (val_byte << 8));
    }

    /// <summary>
    /// Read red light value
    /// </summary>
    /// <returns></returns>
    protected ushort ReadRedLight()
    {
        byte val = BusComms.ReadRegister(Registers.APDS9960_RDATAL);

        byte val_byte = BusComms.ReadRegister(Registers.APDS9960_RDATAH);

        return (ushort)(val + (val_byte << 8));
    }

    /// <summary>
    /// Read green light value
    /// </summary>
    /// <returns></returns>
    protected ushort ReadGreenLight()
    {
        byte val = BusComms.ReadRegister(Registers.APDS9960_GDATAL);

        byte val_byte = BusComms.ReadRegister(Registers.APDS9960_GDATAH);

        return (ushort)(val + (val_byte << 8));
    }

    /// <summary>
    /// Read blue light value
    /// </summary>
    /// <returns></returns>
    protected ushort ReadBlueLight()
    {
        byte val = BusComms.ReadRegister(Registers.APDS9960_BDATAL);

        byte val_byte = BusComms.ReadRegister(Registers.APDS9960_BDATAH);

        return (ushort)(val + (val_byte << 8));
    }

    /// <summary>
    /// Read proximity
    /// </summary>
    /// <returns></returns>
    public byte ReadProximity()
    {
        return BusComms.ReadRegister(Registers.APDS9960_PDATA);
    }

    /*******************************************************************************
     * Getters and setters for register values
     ******************************************************************************/

    /**
     * @brief Returns the lower threshold for proximity detection
     *
     * @return lower threshold
     */
    public byte GetProxIntLowThresh()
    {
        return BusComms.ReadRegister(Registers.APDS9960_PILT);
    }

    /**
     * @brief Sets the lower threshold for proximity detection
     *
     * @param[in] threshold the lower proximity threshold
     * @return True if operation successful. False otherwise.
     */
    public void SetProxIntLowThresh(byte threshold)
    {
        BusComms.WriteRegister(Registers.APDS9960_PILT, threshold);
    }

    /**
     * @brief Returns the high threshold for proximity detection
     *
     * @return high threshold
     */
    public byte GetProxIntHighThresh()
    {
        return BusComms.ReadRegister(Registers.APDS9960_PIHT);
    }

    /**
     * @brief Sets the high threshold for proximity detection
     *
     * @param[in] threshold the high proximity threshold
     * @return True if operation successful. False otherwise.
     */
    public void SetProxIntHighThresh(byte threshold)
    {
        BusComms.WriteRegister(Registers.APDS9960_PIHT, threshold);
    }

    /**
     * @brief Returns LED drive strength for proximity and ALS
     *
     * Value    LED Current
     *   0        100 mA
     *   1         50 mA
     *   2         25 mA
     *   3         12.5 mA
     *
     * @return the value of the LED drive strength. 0xFF on failure.
     */
    public byte GetLEDDrive()
    {
        byte val = BusComms.ReadRegister(Registers.APDS9960_CONTROL);

        /* Shift and mask out LED drive bits */
        val = (byte)((val >> 6) & 0b00000011);

        return val;
    }

    /**
     * @brief Sets the LED drive strength for proximity and ALS
     *
     * Value    LED Current
     *   0        100 mA
     *   1         50 mA
     *   2         25 mA
     *   3         12.5 mA
     *
     * @param[in] drive the value (0-3) for the LED drive strength
     * @return True if operation successful. False otherwise.
     */
    public bool SetLEDDrive(byte drive)
    {
        byte val = BusComms.ReadRegister(Registers.APDS9960_CONTROL);

        /* Set bits in register to given value */
        drive &= 0b00000011;
        drive = (byte)(drive << 6);
        val &= 0b00111111;
        val |= drive;

        /* Write register value back into CONTROL register */
        BusComms.WriteRegister(Registers.APDS9960_CONTROL, val);

        return true;
    }

    /**
     * @brief Returns receiver gain for proximity detection
     *
     * Value    Gain
     *   0       1x
     *   1       2x
     *   2       4x
     *   3       8x
     *
     * @return the value of the proximity gain. 0xFF on failure.
     */
    public byte GetProximityGain()
    {
        byte val = BusComms.ReadRegister(Registers.APDS9960_CONTROL);

        /* Shift and mask out PDRIVE bits */
        val = (byte)((val >> 2) & 0b00000011);

        return val;
    }

    /**
     * @brief Sets the receiver gain for proximity detection
     *
     * Value    Gain
     *   0       1x
     *   1       2x
     *   2       4x
     *   3       8x
     *
     * @param[in] drive the value (0-3) for the gain
     * @return True if operation successful. False otherwise.
     */
    public void SetProximityGain(byte drive)
    {
        byte val = BusComms.ReadRegister(Registers.APDS9960_CONTROL);

        /* Set bits in register to given value */
        drive &= 0b00000011;
        drive = (byte)(drive << 2);
        val &= 0b11110011;
        val |= drive;

        /* Write register value back into CONTROL register */
        BusComms.WriteRegister(Registers.APDS9960_CONTROL, val);
    }

    /**
     * @brief Returns receiver gain for the ambient light sensor (ALS)
     *
     * Value    Gain
     *   0        1x
     *   1        4x
     *   2       16x
     *   3       64x
     *
     * @return the value of the ALS gain. 0xFF on failure.
     */
    private byte GetAmbientLightGain()
    {
        byte val = BusComms.ReadRegister(Registers.APDS9960_CONTROL);

        /* Shift and mask out ADRIVE bits */
        val &= 0b00000011;

        return val;
    }

    /**
     * @brief Sets the receiver gain for the ambient light sensor (ALS)
     *
     * Value    Gain
     *   0        1x
     *   1        4x
     *   2       16x
     *   3       64x
     *
     * @param[in] drive the value (0-3) for the gain
     * @return True if operation successful. False otherwise.
     */
    private void SetAmbientLightGain(byte drive)
    {
        byte val = BusComms.ReadRegister(Registers.APDS9960_CONTROL);

        drive &= 0b00000011;
        val &= 0b11111100;
        val |= drive;

        BusComms.WriteRegister(Registers.APDS9960_CONTROL, val);
    }

    /**
     * @brief Get the current LED boost value
     * 
     * Value  Boost Current
     *   0        100%
     *   1        150%
     *   2        200%
     *   3        300%
     *
     * @return The LED boost value. 0xFF on failure.
     */
    private byte GetLEDBoost()
    {
        byte val = BusComms.ReadRegister(Registers.APDS9960_CONTROL);

        /* Shift and mask out LED_BOOST bits */
        val = (byte)((val >> 4) & 0b00000011);

        return val;
    }

    /**
     * @brief Sets the LED current boost value
     *
     * Value  Boost Current
     *   0        100%
     *   1        150%
     *   2        200%
     *   3        300%
     *
     * @param[in] drive the value (0-3) for current boost (100-300%)
     * @return True if operation successful. False otherwise.
     */
    private void SetLEDBoost(byte boost)
    {
        byte val = BusComms.ReadRegister(Registers.APDS9960_CONFIG2);

        /* Set bits in register to given value */
        boost &= 0b00000011;
        boost = (byte)(boost << 4);
        val &= 0b11001111;
        val |= boost;

        BusComms.WriteRegister(Registers.APDS9960_CONFIG2, val);
    }

    /**
     * @brief Gets proximity gain compensation enable
     *
     * @return 1 if compensation is enabled. 0 if not. 0xFF on error.
     */
    private byte GetProxGainCompEnable()
    {
        byte val = BusComms.ReadRegister(Registers.APDS9960_CONFIG3);

        /* Shift and mask out PCMP bits */
        val = (byte)((val >> 5) & 0b00000001);

        return val;
    }

    /**
     * @brief Sets the proximity gain compensation enable
     *
     * @param[in] enable 1 to enable compensation. 0 to disable compensation.
     * @return True if operation successful. False otherwise.
     */
    private void SetProxGainCompEnable(byte enable)
    {
        byte val = BusComms.ReadRegister(Registers.APDS9960_CONFIG3);

        /* Set bits in register to given value */
        enable &= 0b00000001;
        enable = (byte)(enable << 5);
        val &= 0b11011111;
        val |= enable;

        /* Write register value back into CONFIG3 register */
        BusComms.WriteRegister(Registers.APDS9960_CONFIG3, val);
    }

    /**
     * @brief Gets the current mask for enabled/disabled proximity photodiodes
     *
     * 1 = disabled, 0 = enabled
     * Bit    Photodiode
     *  3       UP
     *  2       DOWN
     *  1       LEFT
     *  0       RIGHT
     *
     * @return Current proximity mask for photodiodes. 0xFF on error.
     */
    private byte GetProxPhotoMask()
    {
        byte val = BusComms.ReadRegister(Registers.APDS9960_CONFIG3);

        /* Mask out photodiode enable mask bits */
        val &= 0b00001111;

        return val;
    }

    /**
     * @brief Sets the mask for enabling/disabling proximity photodiodes
     *
     * 1 = disabled, 0 = enabled
     * Bit    Photodiode
     *  3       UP
     *  2       DOWN
     *  1       LEFT
     *  0       RIGHT
     *
     * @param[in] mask 4-bit mask value
     * @return True if operation successful. False otherwise.
     */
    private void SetProxPhotoMask(byte mask)
    {
        byte val = BusComms.ReadRegister(Registers.APDS9960_CONFIG3);

        /* Set bits in register to given value */
        mask &= 0b00001111;
        val &= 0b11110000;
        val |= mask;

        BusComms.WriteRegister(Registers.APDS9960_CONFIG3, val);
    }

    /**
     * @brief Gets the low threshold for ambient light interrupts
     *
     * @param[out] threshold current low threshold stored on the APDS-9960
     * @return True if operation successful. False otherwise.
     */
    private ushort GetLightIntLowThreshold()
    {
        var threshold = BusComms.ReadRegister(Registers.AILTL);

        var val_byte = BusComms.ReadRegister(Registers.AILTH);

        return (byte)(threshold + (val_byte << 8));
    }

    /**
     * @brief Sets the low threshold for ambient light interrupts
     *
     * @param[in] threshold low threshold value for interrupt to trigger
     * @return True if operation successful. False otherwise.
     */
    private void SetLightIntLowThreshold(ushort threshold)
    {
        byte val_low;
        byte val_high;

        /* Break 16-bit threshold into 2 8-bit values */
        val_low = (byte)(threshold & 0x00FF);
        val_high = (byte)((threshold & 0xFF00) >> 8);

        BusComms.WriteRegister(Registers.AILTL, val_low);
        BusComms.WriteRegister(Registers.AILTL, val_high);
    }

    /**
     * @brief Gets the high threshold for ambient light interrupts
     *
     * @param[out] threshold current low threshold stored on the APDS-9960
     * @return True if operation successful. False otherwise.
     */
    private ushort GetLightIntHighThreshold()
    {
        var threshold = BusComms.ReadRegister(Registers.AIHTL);

        var val_byte = BusComms.ReadRegister(Registers.APDS9960_AIHTH);

        return (byte)(threshold + (val_byte << 8));
    }

    /**
     * @brief Sets the high threshold for ambient light interrupts
     *
     * @param[in] threshold high threshold value for interrupt to trigger
     * @return True if operation successful. False otherwise.
     */
    private void SetLightIntHighThreshold(ushort threshold)
    {
        /* Break 16-bit threshold into 2 8-bit values */
        byte val_low = (byte)(threshold & 0x00FF);
        byte val_high = (byte)((threshold & 0xFF00) >> 8);

        BusComms.WriteRegister(Registers.AIHTL, val_low);
        BusComms.WriteRegister(Registers.APDS9960_AIHTH, val_high);
    }

    /**
     * @brief Gets the low threshold for proximity interrupts
     *
     * @param[out] threshold current low threshold stored on the APDS-9960
     * @return True if operation successful. False otherwise.
     */
    private byte GetProximityIntLowThreshold()
    {
        return BusComms.ReadRegister(Registers.APDS9960_PILT);
    }

    /**
     * @brief Sets the low threshold for proximity interrupts
     *
     * @param[in] threshold low threshold value for interrupt to trigger
     * @return True if operation successful. False otherwise.
     */
    private void SetProximityIntLowThreshold(byte threshold)
    {
        BusComms.WriteRegister(Registers.APDS9960_PILT, threshold);
    }

    /**
     * @brief Gets the high threshold for proximity interrupts
     *
     * @param[out] threshold current low threshold stored on the APDS-9960
     * @return True if operation successful. False otherwise.
     */
    private byte GetProximityIntHighThreshold()
    {
        return BusComms.ReadRegister(Registers.APDS9960_PIHT);
    }

    /**
     * @brief Sets the high threshold for proximity interrupts
     *
     * @param[in] threshold high threshold value for interrupt to trigger
     * @return True if operation successful. False otherwise.
     */
    private void SetProximityIntHighThreshold(byte threshold)
    {
        BusComms.WriteRegister(Registers.APDS9960_PIHT, threshold);
    }

    /**
     * @brief Gets if ambient light interrupts are enabled or not
     *
     * @return 1 if interrupts are enabled, 0 if not. 0xFF on error.
     */
    private byte GetAmbientLightIntEnable()
    {
        byte val = BusComms.ReadRegister(Registers.ENABLE);

        /* Shift and mask out AIEN bit */
        val = (byte)((val >> 4) & 0b00000001);

        return val;
    }

    /**
     * @brief Turns ambient light interrupts on or off
     *
     * @param[in] enable 1 to enable interrupts, 0 to turn them off
     * @return True if operation successful. False otherwise.
     */
    private void SetAmbientLightIntEnable(bool enable)
    {
        byte val = BusComms.ReadRegister(Registers.ENABLE);

        /* Set bits in register to given value */
        byte data = (byte)(enable ? 0x1 : 0x0);
        data &= 0b00000001;
        data = (byte)(data << 4);
        val &= 0b11101111;
        val |= data;

        BusComms.WriteRegister(Registers.ENABLE, val);
    }

    /**
     * @brief Gets if proximity interrupts are enabled or not
     *
     * @return 1 if interrupts are enabled, 0 if not. 0xFF on error.
     */
    private byte GetProximityIntEnable()
    {
        byte val = BusComms.ReadRegister(Registers.ENABLE);

        /* Shift and mask out PIEN bit */
        val = (byte)((val >> 5) & 0b00000001);

        return val;
    }

    /**
     * @brief Turns proximity interrupts on or off
     *
     * @param[in] enable 1 to enable interrupts, 0 to turn them off
     * @return True if operation successful. False otherwise.
     */
    private void SetProximityIntEnable(byte enable)
    {
        byte val = BusComms.ReadRegister(Registers.ENABLE);

        /* Set bits in register to given value */
        enable &= 0b00000001;
        enable = (byte)(enable << 5);
        val &= 0b11011111;
        val |= enable;

        BusComms.WriteRegister(Registers.ENABLE, val);
    }

    /**
     * @brief Clears the ambient light interrupt
     *
     * @return True if operation completed successfully. False otherwise.
     */
    private void ClearAmbientLightInt()
    {
        BusComms.WriteRegister(Registers.APDS9960_AICLEAR, 0);
    }

    /**
     * @brief Clears the proximity interrupt
     *
     * @return True if operation completed successfully. False otherwise.
     */
    private void ClearProximityInt()
    {
        BusComms.WriteRegister(Registers.APDS9960_PICLEAR, 0);
    }

    ///<inheritdoc/>
    public override void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Dispose of the object
    /// </summary>
    /// <param name="disposing">Is disposing</param>
    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);

        if (!IsDisposed)
        {
            if (disposing && createdPort)
            {
                interruptPort?.Dispose();
            }

            IsDisposed = true;
        }
    }
}