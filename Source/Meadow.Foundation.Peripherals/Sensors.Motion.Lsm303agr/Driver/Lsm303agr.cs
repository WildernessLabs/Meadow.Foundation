using Meadow.Hardware;
using Meadow.Peripherals.Sensors;
using Meadow.Peripherals.Sensors.Motion;
using Meadow.Units;
using System;
using System.Threading.Tasks;

namespace Meadow.Foundation.Sensors.Motion;

/// <summary>
/// Represents a LSM303AGR is a system-in-package (SiP) that combines a 3D linear acceleration sensor and a 3D magnetic sensor
/// </summary>
public partial class Lsm303agr :
    PollingSensorBase<(Acceleration3D? Acceleration3D, MagneticField3D? MagneticField3D)>,
    IMagnetometer, IAccelerometer, II2cPeripheral
{
    private event EventHandler<IChangeResult<Acceleration3D>> _accelerationHandlers;
    private event EventHandler<IChangeResult<MagneticField3D>> _fieldHandlers;

    event EventHandler<IChangeResult<Acceleration3D>> ISamplingSensor<Acceleration3D>.Updated
    {
        add => _accelerationHandlers += value;
        remove => _accelerationHandlers -= value;
    }

    event EventHandler<IChangeResult<MagneticField3D>> ISamplingSensor<MagneticField3D>.Updated
    {
        add => _fieldHandlers += value;
        remove => _fieldHandlers -= value;
    }

    /// <summary>
    /// Current Acceleration 3D
    /// </summary>
    public Acceleration3D? Acceleration3D => Conditions.Acceleration3D;

    /// <summary>
    /// Current Magnetic Field 3D
    /// </summary>
    public MagneticField3D? MagneticField3D => Conditions.MagneticField3D;

    /// <summary>
    /// The default I2C address for the peripheral
    /// </summary>
    public byte DefaultI2cAddress => (byte)Addresses.AddressAccel_0x19;

    /// <summary>
    /// I2C Communication bus used to communicate with the accelerometer
    /// </summary>
    private readonly II2cCommunications i2cCommsAccel;

    /// <summary>
    /// I2C Communication bus used to communicate with the magnetometer
    /// </summary>
    private readonly II2cCommunications i2cCommsMag;

    /// <summary>
    /// Create a new Lsm303agr instance
    /// </summary>
    /// <param name="i2cBus">The I2C bus connected to the sensor</param>
    public Lsm303agr(II2cBus i2cBus)
    {
        i2cCommsAccel = new I2cCommunications(i2cBus, (byte)Addresses.AddressAccel_0x19);
        i2cCommsMag = new I2cCommunications(i2cBus, (byte)Addresses.AddressMag_0x1E);

        Initialize();
    }

    /// <summary>
    /// Initializes the LSM303AGR sensor
    /// </summary>
    private void Initialize()
    {
        i2cCommsAccel.WriteRegister(ACC_CTRL_REG1_A, 0x57);
        i2cCommsMag.WriteRegister(MAG_CTRL_REG1_M, 0x60);
    }

    /// <summary>
    /// Sets the sensitivity of the accelerometer
    /// </summary>
    /// <param name="sensitivity">The desired sensitivity setting, specified by the AccSensitivity enum.</param>
    public void SetAccelerometerSensitivity(AccSensitivity sensitivity)
    {
        byte[] writeBuffer = new byte[] { ACC_CTRL_REG4_A, (byte)sensitivity };
        i2cCommsAccel.Write(writeBuffer);
    }

    /// <summary>
    /// Retrieves the current sensitivity setting of the accelerometer
    /// </summary>
    /// <returns>The current sensitivity setting, represented by the AccSensitivity enum.</returns>
    public AccSensitivity GetAccelerometerSensitivity()
    {
        byte[] readBuffer = new byte[1];
        i2cCommsAccel.ReadRegister(ACC_CTRL_REG4_A, readBuffer);
        byte sensitivity = (byte)(readBuffer[0] & 0x30);
        return (AccSensitivity)sensitivity;
    }

    /// <summary>
    /// Raise events for subscribers and notify of value changes
    /// </summary>
    /// <param name="changeResult">The updated sensor data</param>
    protected override void RaiseEventsAndNotify(IChangeResult<(Acceleration3D? Acceleration3D, MagneticField3D? MagneticField3D)> changeResult)
    {
        if (changeResult.New.MagneticField3D is { } mag)
        {
            _fieldHandlers?.Invoke(this, new ChangeResult<MagneticField3D>(mag, changeResult.Old?.MagneticField3D));
        }
        if (changeResult.New.Acceleration3D is { } accel)
        {
            _accelerationHandlers?.Invoke(this, new ChangeResult<Acceleration3D>(accel, changeResult.Old?.Acceleration3D));
        }
        base.RaiseEventsAndNotify(changeResult);
    }

    /// <summary>
    /// Reads data from the sensor
    /// </summary>
    /// <returns>The latest sensor reading</returns>
    protected override Task<(Acceleration3D? Acceleration3D, MagneticField3D? MagneticField3D)> ReadSensor()
    {
        (Acceleration3D? Acceleration3D, MagneticField3D? MagneticField3D) conditions;

        var accel = ReadAccelerometerRaw();
        var mag = ReadMagnetometerRaw();

        conditions.Acceleration3D = GetAcceleration3D(accel.x, accel.y, accel.z, GetAccelerometerSensitivity());
        conditions.MagneticField3D = GetMagneticField3D(mag.x, mag.y, mag.z);

        return Task.FromResult(conditions);
    }

    private Acceleration3D GetAcceleration3D(short rawX, short rawY, short rawZ, AccSensitivity sensitivity)
    {
        float lsbPerG = 0;
        switch (sensitivity)
        {
            case AccSensitivity.G2:
                lsbPerG = 16384.0f; // 2^16 / (2 * 2)
                break;
            case AccSensitivity.G4:
                lsbPerG = 8192.0f; // 2^16 / (2 * 4)
                break;
            case AccSensitivity.G8:
                lsbPerG = 4096.0f; // 2^16 / (2 * 8)
                break;
            case AccSensitivity.G16:
                lsbPerG = 2048.0f; // 2^16 / (2 * 16)
                break;
        }

        float x = rawX / lsbPerG;
        float y = rawY / lsbPerG;
        float z = rawZ / lsbPerG;

        return new Acceleration3D(x, y, z, Acceleration.UnitType.Gravity);
    }

    private MagneticField3D GetMagneticField3D(short rawX, short rawY, short rawZ)
    {
        var x = rawX * 1500;
        var y = rawY * 1500;
        var z = rawZ * 1500;

        return new MagneticField3D(x, y, z, MagneticField.UnitType.Gauss);
    }

    /// <summary>
    /// Reads raw accelerometer data
    /// </summary>
    /// <returns>A tuple containing the X, Y, and Z values of the accelerometer.</returns>
    private (short x, short y, short z) ReadAccelerometerRaw()
    {
        Span<byte> rawData = stackalloc byte[6];
        i2cCommsAccel.ReadRegister(ACC_OUT_X_L_A, rawData);

        short x = BitConverter.ToInt16(rawData.Slice(0, 2));
        short y = BitConverter.ToInt16(rawData.Slice(2, 2));
        short z = BitConverter.ToInt16(rawData.Slice(4, 2));

        return (x, y, z);
    }

    /// <summary>
    /// Reads raw magnetometer data
    /// </summary>
    /// <returns>A tuple containing the X, Y, and Z values of the magnetometer.</returns>
    private (short x, short y, short z) ReadMagnetometerRaw()
    {
        Span<byte> rawData = stackalloc byte[6];
        i2cCommsMag.ReadRegister(MAG_OUTX_L_REG_M, rawData);

        short x = BitConverter.ToInt16(rawData.Slice(0, 2));
        short y = BitConverter.ToInt16(rawData.Slice(2, 2));
        short z = BitConverter.ToInt16(rawData.Slice(4, 2));

        return (x, y, z);
    }

    /// <summary>
    /// Sets the output data rate for the accelerometer.
    /// </summary>
    /// <param name="dataRate">The desired output data rate setting.</param>
    public void SetAccelerometerOutputDataRate(AccOutputDataRate dataRate)
    {
        byte[] readBuffer = new byte[1];
        i2cCommsAccel.ReadRegister(ACC_CTRL_REG1_A, readBuffer);

        byte newSetting = (byte)((readBuffer[0] & 0x0F) | (byte)dataRate);
        i2cCommsAccel.WriteRegister(ACC_CTRL_REG1_A, newSetting);
    }

    /// <summary>
    /// Retrieves the current output data rate setting for the accelerometer.
    /// </summary>
    /// <returns>The current output data rate setting.</returns>
    public AccOutputDataRate GetAccelerometerOutputDataRate()
    {
        byte[] readBuffer = new byte[1];
        i2cCommsAccel.ReadRegister(ACC_CTRL_REG1_A, readBuffer);

        byte dataRate = (byte)(readBuffer[0] & 0xF0);
        return (AccOutputDataRate)dataRate;
    }

    /// <summary>
    /// Sets the output data rate for the magnetometer.
    /// </summary>
    /// <param name="dataRate">The desired output data rate setting.</param>
    public void SetMagnetometerOutputDataRate(MagOutputDataRate dataRate)
    {
        byte odrByte = i2cCommsMag.ReadRegister(MAG_CTRL_REG1_M);
        odrByte &= 0xF3; // Clear bits 2 and 3
        odrByte |= (byte)dataRate;
        i2cCommsMag.WriteRegister(MAG_CTRL_REG1_M, odrByte);
    }

    /// <summary>
    /// Retrieves the current output data rate setting for the magnetometer.
    /// </summary>
    /// <returns>The current output data rate setting.</returns>
    public MagOutputDataRate GetMagnetometerOutputDataRate()
    {
        byte odrByte = i2cCommsMag.ReadRegister(MAG_CTRL_REG1_M);
        return (MagOutputDataRate)(odrByte & 0x0C);
    }

    async Task<Acceleration3D> ISensor<Acceleration3D>.Read()
    => (await Read()).Acceleration3D!.Value;

    async Task<MagneticField3D> ISensor<MagneticField3D>.Read()
    => (await Read()).MagneticField3D!.Value;
}