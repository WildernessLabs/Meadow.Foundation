namespace Meadow.Foundation.Sensors.Motion
{
    // TODO: make all these constants and fix casing.
    // e.g. `public const CHIP_ID = 0X00;`
    public partial class Bno055
    {

        /// <summary>
        ///     Register addresses in the sensor.
        /// </summary>
        private static class Registers
        {
            /// <summary>
            ///     Chip ID, read-only, fixed value 0xa0.
            /// </summary>
            public static readonly byte ChipID = 0x00;

            /// <summary>
            ///     Accelerometer ID, read-only, fixed to 0xfb.
            /// </summary>
            public static readonly byte AccelerometerID = 0x01;

            /// <summary>
            ///     Magnetometer ID, read-only, fixed to 0x32.
            /// </summary>
            public static readonly byte MagnetometerID = 0x02;

            /// <summary>
            ///     Gyroscope ID, read-only, fixed to 0x0f.
            /// </summary>
            public static readonly byte GyroscopeID = 0x03;

            /// <summary>
            ///     LSB of the software version number.
            /// </summary>
            public static readonly byte SoftwareRevisionIDLSB = 0x04;

            /// <summary>
            ///     MSB of the software version number.
            /// </summary>
            public static readonly byte SoftwareRevisionIDMSB = 0x05;

            /// <summary>
            ///     Bootloader version number.
            /// </summary>
            public static readonly byte BootloaderRevisionID = 0x06;

            /// <summary>
            ///     Page register.  This determines which set of registers are available, 
            ///     page 0 or page 1.
            /// 
            ///     This defaults to 0 on reset.
            /// </summary>
            public static readonly byte PageID = 0x07;

            /// <summary>
            ///     Register containing the first byte of the sensor readings.
            /// </summary>
            /// <remarks>
            ///     This is used in the calculation of the various sensor readings
            /// in the _sensorReadings member.
            /// </remarks>
            public static readonly byte StartOfSensorData = 0x08;

            /// <summary>
            ///     LSB of the X acceleration data.
            /// </summary>
            /// <remarks>
            ///     The output units can be changed through the Units register and the
            ///     operation mode can be changed through the OperationMode register.
            /// </remarks>
            public static readonly byte AccelerometerXLSB = 0x08;

            /// <summary>
            ///     MSB of the X acceleration data.
            /// </summary>
            /// <remarks>
            ///     The output units can be changed through the Units register and the
            ///     operation mode can be changed through the OperationMode register.
            /// </remarks>
            public static readonly byte AccelerometerXMSB = 0x09;

            /// <summary>
            ///     LSB of the Y acceleration data.
            /// </summary>
            /// <remarks>
            ///     The output units can be changed through the Units register and the
            ///     operation mode can be changed through the OperationMode register.
            /// </remarks>
            public static readonly byte AccelerometerYLSB = 0x0a;

            /// <summary>
            ///     MSB of the Y acceleration data.
            /// </summary>
            /// <remarks>
            ///     The output units can be changed through the Units register and the
            ///     operation mode can be changed through the OperationMode register.
            /// </remarks>
            public static readonly byte AccelerometerYMSB = 0x0b;

            /// <summary>
            ///     LSB of the Z acceleration data.
            /// </summary>
            /// <remarks>
            ///     The output units can be changed through the Units register and the
            ///     operation mode can be changed through the OperationMode register.
            /// </remarks>
            public static readonly byte AccelerometerZLSB = 0x0c;

            /// <summary>
            ///     MSB of the Z acceleration data.
            /// </summary>
            /// <remarks>
            ///     The output units can be changed through the Units register and the
            ///     operation mode can be changed through the OperationMode register.
            /// </remarks>
            public static readonly byte AccelerometerZMSB = 0x0d;

            /// <summary>
            ///     LSB of the X magnetometer data.
            /// </summary>
            /// <remarks>
            ///     The output units can be changed through the Units register and the
            ///     operation mode can be changed through the OperationMode register.
            /// </remarks>
            public static readonly byte MagnetometerXLSB = 0x0e;

            /// <summary>
            ///    MSB of the X magnetometer data.
            /// </summary>
            /// <remarks>
            ///     The output units can be changed through the Units register and the
            ///     operation mode can be changed through the OperationMode register.
            /// </remarks>
            public static readonly byte MagnetometerXMSB = 0x0f;

            /// <summary>
            ///     LSB of the Y magnetometer data.
            /// </summary>
            /// <remarks>
            ///     The output units can be changed through the Units register and the
            ///     operation mode can be changed through the OperationMode register.
            /// </remarks>
            public static readonly byte MagnetometerYLSB = 0x10;

            /// <summary>
            ///     MSB of the Y magnetometer data.
            /// </summary>
            /// <remarks>
            ///     The output units can be changed through the Units register and the
            ///     operation mode can be changed through the OperationMode register.
            /// </remarks>
            public static readonly byte MagnetometerYMSB = 0x11;

            /// <summary>
            ///     LSB of the Z magnetometer data.
            /// </summary>
            /// <remarks>
            ///     The output units can be changed through the Units register and the
            ///     operation mode can be changed through the OperationMode register.
            /// </remarks>
            public static readonly byte MagnetometerZLSB = 0x12;

            /// <summary>
            ///     MSB of the Z magnetometer data.
            /// </summary>
            /// <remarks>
            ///     The output units can be changed through the Units register and the
            ///     operation mode can be changed through the OperationMode register.
            /// </remarks>
            public static readonly byte MagnetometerZMSB = 0x13;

            /// <summary>
            ///     LSB of the X gyroscope data.
            /// </summary>
            /// <remarks>
            ///     The output units can be changed through the Units register and the
            ///     operation mode can be changed through the OperationMode register.
            /// </remarks>
            public static readonly byte GyroscopeXLSB = 0x14;

            /// <summary>
            ///     MSB of the X gyroscope data.
            /// </summary>
            /// <remarks>
            ///     The output units can be changed through the Units register and the
            ///     operation mode can be changed through the OperationMode register.
            /// </remarks>
            public static readonly byte GyroscopeXMSB = 0x15;

            /// <summary>
            ///     LSB of the Y gyroscope data.
            /// </summary>
            /// <remarks>
            ///     The output units can be changed through the Units register and the
            ///     operation mode can be changed through the OperationMode register.
            /// </remarks>
            public static readonly byte GyroscopeYLSB = 0x16;

            /// <summary>
            ///     MSB of the Y gyroscope data.
            /// </summary>
            /// <remarks>
            ///     The output units can be changed through the Units register and the
            ///     operation mode can be changed through the OperationMode register.
            /// </remarks>
            public static readonly byte GyroscopeYMSB = 0x17;

            /// <summary>
            ///     LSB of the Z gyroscope data.
            /// </summary>
            /// <remarks>
            ///     The output units can be changed through the Units register and the
            ///     operation mode can be changed through the OperationMode register.
            /// </remarks>
            public static readonly byte GyroscopeZLSB = 0x18;

            /// <summary>
            ///     MSB of the Z gyroscope data.
            /// </summary>
            /// <remarks>
            ///     The output units can be changed through the Units register and the
            ///     operation mode can be changed through the OperationMode register.
            /// </remarks>
            public static readonly byte GyroscopeZMSB = 0x19;

            /// <summary>
            ///     LSB of the X heading data.
            /// </summary>
            /// <remarks>
            ///     The output units can be changed through the Units register and the
            ///     operation mode can be changed through the OperationMode register.
            /// </remarks>
            public static readonly byte EulerAngleXLSB = 0x1a;

            /// <summary>
            ///     MSB of the X heading data.
            /// </summary>
            /// <remarks>
            ///     The output units can be changed through the Units register and the
            ///     operation mode can be changed through the OperationMode register.
            /// </remarks>
            public static readonly byte EulerAngleXMSB = 0x1b;

            /// <summary>
            ///     LSB of the Y heading data.
            /// </summary>
            /// <remarks>
            ///     The output units can be changed through the Units register and the
            ///     operation mode can be changed through the OperationMode register.
            /// </remarks>
            public static readonly byte EulerAngleYLSB = 0x1c;

            /// <summary>
            ///     MSB of the Y heading data.
            /// </summary>
            /// <remarks>
            ///     The output units can be changed through the Units register and the
            ///     operation mode can be changed through the OperationMode register.
            /// </remarks>
            public static readonly byte EulerAngleYMSB = 0x1d;

            /// <summary>
            ///     LSB of the Z heading data.
            /// </summary>
            /// <remarks>
            ///     The output units can be changed through the Units register and the
            ///     operation mode can be changed through the OperationMode register.
            /// </remarks>            public static readonly byte EulerAngleYMSB = 0x1d;
            public static readonly byte EulerAngleZLSB = 0x1e;

            /// <summary>
            ///     MSB of the Z heading data.
            /// </summary>
            /// <remarks>
            ///     The output units can be changed through the Units register and the
            ///     operation mode can be changed through the OperationMode register.
            /// </remarks>
            public static readonly byte EulerAngleZMSB = 0x1f;

            /// <summary>
            ///     LSB of the W quaternion data.
            /// </summary>
            /// <remarks>
            ///     The output units can be changed through the Units register and the
            ///     operation mode can be changed through the OperationMode register.
            /// </remarks>
            public static readonly byte QuaternionDataWLSB = 0x20;

            /// <summary>
            ///     MSB of the W quaternion data.
            /// </summary>
            /// <remarks>
            ///     The output units can be changed through the Units register and the
            ///     operation mode can be changed through the OperationMode register.
            /// </remarks>
            public static readonly byte QuaternionDataWMSB = 0x21;

            /// <summary>
            ///     LSB of the X quaternion data.
            /// </summary>
            /// <remarks>
            ///     The output units can be changed through the Units register and the
            ///     operation mode can be changed through the OperationMode register.
            /// </remarks>
            public static readonly byte QuaternionDataXLSB = 0x22;

            /// <summary>
            ///     MSB of the X quaternion data.
            /// </summary>
            /// <remarks>
            ///     The output units can be changed through the Units register and the
            ///     operation mode can be changed through the OperationMode register.
            /// </remarks>
            public static readonly byte QuaternionDataXMSB = 0x23;

            /// <summary>
            ///     LSB of the Y quaternion data.
            /// </summary>
            /// <remarks>
            ///     The output units can be changed through the Units register and the
            ///     operation mode can be changed through the OperationMode register.
            /// </remarks>
            public static readonly byte QuaternionDataYLSB = 0x24;

            /// <summary>
            ///     MSB of the Y quaternion data.
            /// </summary>
            /// <remarks>
            ///     The output units can be changed through the Units register and the
            ///     operation mode can be changed through the OperationMode register.
            /// </remarks>
            public static readonly byte QuaternionDataYMSB = 0x25;

            /// <summary>
            ///     LSB of the Z quaternion data.
            /// </summary>
            /// <remarks>
            ///     The output units can be changed through the Units register and the
            ///     operation mode can be changed through the OperationMode register.
            /// </remarks>
            public static readonly byte QuaternionDataZLSB = 0x26;

            /// <summary>
            ///     MSB of the Z quaternion data.
            /// </summary>
            /// <remarks>
            ///     The output units can be changed through the Units register and the
            ///     operation mode can be changed through the OperationMode register.
            /// </remarks>
            public static readonly byte QuaternionDataZMSB = 0x27;

            /// <summary>
            ///     LSB of the X linear acceleration data.
            /// </summary>
            /// <remarks>
            ///     The output units can be changed through the Units register and the
            ///     operation mode can be changed through the OperationMode register.
            /// </remarks>
            public static readonly byte LinearAccelerationXLSB = 0x28;

            /// <summary>
            ///     MSB of the X linear acceleration data.
            /// </summary>
            /// <remarks>
            ///     The output units can be changed through the Units register and the
            ///     operation mode can be changed through the OperationMode register.
            /// </remarks>
            public static readonly byte LinearAccelerationXMSB = 0x29;

            /// <summary>
            ///     LSB of the Y linear acceleration data.
            /// </summary>
            /// <remarks>
            ///     The output units can be changed through the Units register and the
            ///     operation mode can be changed through the OperationMode register.
            /// </remarks>
            public static readonly byte LinearAccelerationYLSB = 0x2a;

            /// <summary>
            ///     MSB of the Y linear acceleration data.
            /// </summary>
            /// <remarks>
            ///     The output units can be changed through the Units register and the
            ///     operation mode can be changed through the OperationMode register.
            /// </remarks>
            public static readonly byte LinearAccelerationYMSB = 0x2b;

            /// <summary>
            ///     LSB of the Z linear acceleration data.
            /// </summary>
            /// <remarks>
            ///     The output units can be changed through the Units register and the
            ///     operation mode can be changed through the OperationMode register.
            /// </remarks>
            public static readonly byte LinearAccelerationZLSB = 0x2c;

            /// <summary>
            ///     MSB of the Z linear acceleration data.
            /// </summary>
            /// <remarks>
            ///     The output units can be changed through the Units register and the
            ///     operation mode can be changed through the OperationMode register.
            /// </remarks>
            public static readonly byte LinearAccelerationZMSB = 0x2d;

            /// <summary>
            ///     LSB of the X gravity vector data.
            /// </summary>
            /// <remarks>
            ///     The output units can be changed through the Units register and the
            ///     operation mode can be changed through the OperationMode register.
            /// </remarks>
            public static readonly byte GravityVectorXLSB = 0x2e;

            /// <summary>
            ///     MSB of the X gravity vector data.
            /// </summary>
            /// <remarks>
            ///     The output units can be changed through the Units register and the
            ///     operation mode can be changed through the OperationMode register.
            /// </remarks>
            public static readonly byte GravityVectorXMSB = 0x2f;

            /// <summary>
            ///     LSB of the Y gravity vector data.
            /// </summary>
            /// <remarks>
            ///     The output units can be changed through the Units register and the
            ///     operation mode can be changed through the OperationMode register.
            /// </remarks>
            public static readonly byte GravityVectorYLSB = 0x30;

            /// <summary>
            ///     MSB of the Y gravity vector data.
            /// </summary>
            /// <remarks>
            ///     The output units can be changed through the Units register and the
            ///     operation mode can be changed through the OperationMode register.
            /// </remarks>
            public static readonly byte GravityVectorYMSB = 0x31;

            /// <summary>
            ///     LSB of the Z gravity vector data.
            /// </summary>
            /// <remarks>
            ///     The output units can be changed through the Units register and the
            ///     operation mode can be changed through the OperationMode register.
            /// </remarks>
            public static readonly byte GravityVectorZLSB = 0x32;

            /// <summary>
            ///     MSB of the Z gravity vector data.
            /// </summary>
            /// <remarks>
            ///     The output units can be changed through the Units register and the
            ///     operation mode can be changed through the OperationMode register.
            /// </remarks>
            public static readonly byte GravityVectorZMSB = 0x33;

            /// <summary>
            ///     Temperature.
            /// 
            ///     1 LSB = 1 degree (F or C).
            /// </summary>
            /// <remarks>
            ///     The output units can be changed through the Units register and the
            ///     operation mode can be changed through the OperationMode register.
            /// </remarks>
            public static readonly byte Temperature = 0x34;

            /// <summary>
            ///     Calibration status.
            /// </summary>
            /// <remarks>
            ///     Read the calibration status for the sensor.
            ///         b0-b1: Magnetometer calibration status.
            ///         b2-b3: Accelerometer calibration status.
            ///         b4-b5: Gyroscope calibration status.
            ///         b6-b7: System calibration status.  This depends upon the 
            ///                calibration status of all of the sensors.
            /// 
            ///     3 indicates the particular sensor is fully calibrated, 0 indicates the
            ///     sensor is not calibrated.
            /// </remarks>
            public static readonly byte CalibrationStatus = 0x35;

            /// <summary>
            ///     Get the system test result.
            /// </summary>
            /// <remarks>
            ///     Allows the 
            ///         b0: Accelerometer test result.
            ///         b1: Magnetometer test result.
            ///         b2: Gyroscope test result.
            ///         b3: Microcontroller test result.
            /// 
            ///     A value of 1 indicates a pass, 0 indicates a failure.
            /// </remarks>
            public static readonly byte SelfTestResult = 0x36;

            /// <summary>
            ///     Indicate which interrupts have triggered.
            /// </summary>
            /// <remarks>
            ///     Decode the interrupt type using the following table:
            ///         b2: Gyroscope any motion
            ///         b3: Gyroscope high rate
            ///         b5: Accelerometer high rate
            ///         b6: Accelerometer any motion
            ///         b7: Accelerometer no motion or slow motion
            /// </remarks>
            public static readonly byte InterruptStatus = 0x37;

            /// <summary>
            ///     System clock statue, 0, the clock can be reconfigured.
            /// </summary>
            public static readonly byte SystemClockStatus = 0x38;

            /// <summary>
            ///     System status:
            ///         0: Idle
            ///         1: Error
            ///         2: Initializing the peripherals
            ///         3: System initialization
            ///         4: Executing self test
            ///         5: Sensor fusion algorithm is running
            ///         6: System running without the fusion algorithm
            /// </summary>
            public static readonly byte SystemStatus = 0x39;

            /// <summary>
            ///     Indicates the type of error (if any) that has occurred.
            ///         0: No error
            ///         1: Peripheral initialization error
            ///         2: System initialization error
            ///         3: Self test failed
            ///         4: Register map value out of range
            ///         5: Register map address out of range
            ///         6: Register map write error
            ///         7: Low power mode not available for selected operation mode
            ///         8: Accelerometer mor not available
            ///         9: Fusion algorithm configuration error
            ///         a: Sensor configuration error
            /// </summary>
            public static readonly byte ErrorCode = 0x3a;

            /// <summary>
            ///     Define the units for the various sensors.
            /// 
            ///         b1: Accelerometer (0 - m/s2, 1 - mg)
            ///         b2: Gyroscope (0 - degrees per seconds, 1 - radians per second)
            ///         b3: Euler units (0 - degrees, 1 - radians)
            ///         b5: Temperature (0 - Celsius, 1 - Fahrenheit)
            ///         b7: Orientation (0 - Windows, 1 - Android)
            /// </summary>
            public static readonly byte Units = 0x3b;

            /// <summary>
            ///     Operation mode.
            /// 
            ///     On power up, the sensor enters configuration mode.
            /// </summary>
            public static readonly byte OperatingMode = 0x3d;

            /// <summary>
            ///     Power mode (Normal, Low power, or Suspended).
            /// </summary>
            public static readonly byte PowerMode = 0x3e;

            /// <summary>
            ///     System trigger
            ///         b0: Trigger self test when set to 1
            ///         b5: Trigger a reset when set to 1
            ///         b6: Reset all of the interrupt status bits when set to 1
            ///         b7: 0 - use internal oscillator, 1 - use external oscillator
            /// </summary>
            public static readonly byte SystemTrigger = 0x3f;

            /// <summary>
            ///     Get / set the temperature source.
            /// 
            ///     00 - Accelerometer provides the temperature data.
            ///     01 - Gyroscope provides the temperature data.
            /// </summary>
            public static readonly byte TemperatureSource = 0x40;

            /// <summary>
            ///     Remap the axis to take into consideration the mlounting of the sensor.
            /// 
            ///     b0-b1: Remap the X axis
            ///     b2-b3: Remap the Y axis
            ///     b4-b5: Remap the Z axis
            /// 
            ///     00 - X axis
            ///     01 - Y axis
            ///     10 - Z axis
            ///     11 - Invalid.
            /// </summary>
            public static readonly byte AxisMapConfiguration = 0x41;

            /// <summary>
            ///     Sign of the axis mapping
            /// 
            ///     b0: Sign of the X axis mapping
            ///     b1: Sign of the Y axis mapping
            ///     b2: Sign of the Z axis mapping
            /// </summary>
            public static readonly byte AxisMapSign = 0x42;

            /// <summary>
            ///     Accelerometer offset X LSB
            /// </summary>
            /// <remarks>
            ///     See section 3.6.4.
            /// </remarks>
            public static readonly byte AccelerometerOffsetXLSB = 0x55;

            /// <summary>
            ///     Accelerometer offset X MSB
            /// </summary>
            /// <remarks>
            ///     See section 3.6.4.
            /// </remarks>
            public static readonly byte AccelerometerOffsetXMSB = 0x56;

            /// <summary>
            ///     Accelerometer offset Y LSB
            /// </summary>
            /// <remarks>
            ///     See section 3.6.4.
            /// </remarks>
            public static readonly byte AccelerometerOffsetYLSB = 0x57;

            /// <summary>
            ///     Accelerometer offset Y MSB
            /// </summary>
            /// <remarks>
            ///     See section 3.6.4.
            /// </remarks>
            public static readonly byte AccelerometerOffsetYMSB = 0x58;

            /// <summary>
            ///     Accelerometer offset Z LSB
            /// </summary>
            /// <remarks>
            ///     See section 3.6.4.
            /// </remarks>
            public static readonly byte AccelerometerOffsetZLSB = 0x59;

            /// <summary>
            ///     Accelerometer offset Z LSB
            /// </summary>
            /// <remarks>
            ///     See section 3.6.4.
            /// </remarks>
            public static readonly byte AccelerometerOffsetZMSB = 0x5a;

            /// <summary>
            ///     Magnetometer offset X LSB
            /// </summary>
            /// <remarks>
            ///     See section 3.6.4.
            /// </remarks>
            public static readonly byte MagnetometerOffsetXLSB = 0x5b;

            /// <summary>
            ///     Magnetometer offset X MSB
            /// </summary>
            /// <remarks>
            ///     See section 3.6.4.
            /// </remarks>
            public static readonly byte MagnetometerOffsetXMSB = 0x5c;

            /// <summary>
            ///     Magnetometer offset Y LSB
            /// </summary>
            /// <remarks>
            ///     See section 3.6.4.
            /// </remarks>
            public static readonly byte MagnetometerOffsetYLSB = 0x5d;

            /// <summary>
            ///     Magnetometer offset Y MSB
            /// </summary>
            /// <remarks>
            ///     See section 3.6.4.
            /// </remarks>
            public static readonly byte MagnetometerOffsetYMSB = 0x5e;

            /// <summary>
            ///     Magnetometer offset Z LSB
            /// </summary>
            /// <remarks>
            ///     See section 3.6.4.
            /// </remarks>
            public static readonly byte MagnetometerOffsetZLSB = 0x5f;

            /// <summary>
            ///     Magnetometer offset Z MSB
            /// </summary>
            /// <remarks>
            ///     See section 3.6.4.
            /// </remarks>
            public static readonly byte MagnetometerOffsetZMSB = 0x60;

            /// <summary>
            ///     Magnetometer offset X LSB
            /// </summary>
            /// <remarks>
            ///     See section 3.6.4.
            /// </remarks>
            public static readonly byte GyroscopeOffsetXLSB = 0x61;

            /// <summary>
            ///     Magnetometer offset X MSB
            /// </summary>
            /// <remarks>
            ///     See section 3.6.4.
            /// </remarks>
            public static readonly byte GyroscopeOffsetXMSB = 0x62;

            /// <summary>
            ///     Magnetometer offset Y LSB
            /// </summary>
            /// <remarks>
            ///     See section 3.6.4.
            /// </remarks>
            public static readonly byte GyroscopeOffsetYLSB = 0x63;

            /// <summary>
            ///     Magnetometer offset Y MSB
            /// </summary>
            /// <remarks>
            ///     See section 3.6.4.
            /// </remarks>
            public static readonly byte GyroscopeOffsetYMSB = 0x64;

            /// <summary>
            ///     Magnetometer offset Z LSB
            /// </summary>
            /// <remarks>
            ///     See section 3.6.4.
            /// </remarks>
            public static readonly byte GyroscopeOffsetZLSB = 0x65;

            /// <summary>
            ///     Magnetometer offset Z MSB
            /// </summary>
            /// <remarks>
            ///     See section 3.6.4.
            /// </remarks>
            public static readonly byte GyroscopeOffsetZMSB = 0x66;

            /// <summary>
            ///     Accelerometer radius LSB
            /// </summary>
            /// <remarks>
            ///     See section 3.6.4.
            /// </remarks>
            public static readonly byte AccelerometerRadiusLSB = 0x67;

            /// <summary>
            ///     Accelerometer radius MSB
            /// </summary>
            /// <remarks>
            ///     See section 3.6.4.
            /// </remarks>
            public static readonly byte AccelerometerRadiusMSB = 0x68;

            /// <summary>
            ///     Magnetometer radius LSB
            /// </summary>
            /// <remarks>
            ///     See section 3.6.4.
            /// </remarks>
            public static readonly byte MagnetometerRadiusLSB = 0x69;

            /// <summary>
            ///     Magnetometer radius MSB
            /// </summary>
            /// <remarks>
            ///     See section 3.6.4.
            /// </remarks>
            public static readonly byte MagnetometerRadiusMSB = 0x6a;

            /// <summary>
            ///     Configure the accelerometer
            ///         b0-b2: 
            ///         b3-b4:
            ///         b5-b7: 
            /// </summary>
            /// <remarks>
            ///     The register is in page 1.
            /// </remarks>
            public static readonly byte AccelerometerConfiguration = 0x08;

            /// <summary>
            ///     Configure the magnetometer.
            ///         b0-b2: 
            ///         b3-b4:
            ///         b5-b6: 
            /// </summary>
            /// <remarks>
            ///     The register is in page 1.
            /// </remarks>
            public static readonly byte MagnetometerConfiguration = 0x09;

            /// <summary>
            ///     Configure the gyroscope.
            ///         b0-b2: 
            ///         b3-b5:
            /// </summary>
            /// <remarks>
            ///     The register is in page 1.
            /// </remarks>
            public static readonly byte GyroscopeConfiguration0 = 0x0a;

            /// <summary>
            ///     Configure the gyroscope
            /// </summary>
            ///  <remarks>
            ///     The register is in page 1.
            ///         b0-b2: 
            /// </remarks>
            public static readonly byte GyroscopeConfiguration1 = 0x0b;

            /// <summary>
            ///     Configure the sleep options for the accelerometer.
            /// 
            ///         b0: Sleep mode,0 - use event driven time-base mode.
            ///         b1-b4: Sleep duration.
            /// 
            ///     Note that the sleep mode can only be configured then the fusion
            ///     engine is not running.
            /// </summary>
            /// <remarks>
            ///     The register is in page 1.
            /// </remarks>
            public static readonly byte AccelerometerSleepConfiguration = 0x0c;

            /// <summary>
            ///     The gyroscope can be configured to sleep in order to save power.
            /// 
            ///     b0-b2: Auto sleep duration.
            ///     b3-b5: Sleep duration.
            /// </summary>
            /// <remarks>
            ///     The register is in page 1.
            /// </remarks>
            public static readonly byte GyrosscopeSleepConfiguration = 0x0d;

            /// <summary>
            ///     Determine which interrupts are enabled / disabled.
            /// 
            ///     The InterruptStatus register will be updated for all interrupts but
            ///     the interrupt pin will only be triggered when the relevant bit in the 
            ///     InterruptMask register is set to 1.
            /// 
            ///     b2: Gyroscope interrupt when any motion is detected.
            ///     b3: Gyroscope high rate interrupt
            ///     b5: Accelerometer high rate interrupt
            ///     b6: Accelerometer, any motion interrupt
            ///     b7: Accelerometer, slow or no motion interrupt
            /// </summary>
            /// <remarks>
            ///     The register is in page 1.
            /// </remarks>
            public static readonly byte InterruptMask = 0x0f;

            /// <summary>
            ///     Enable / disable interrupts.  Setting a bit to 0 will disable interrupt
            ///     a value of 1 will enable the interrupt.
            /// 
            ///     b2: Enable / disable gyroscope interrupt
            ///     b3: Enable / disable gyroscope high rate interrupt
            ///     b5: Enable / disable accelerometer high rate interrupt
            ///     b6: Enable / disable accelerometer, any motion interrupt
            ///     b7: Enable / disable accelerometer, slow or no motion interrupt
            /// </summary>
            /// <remarks>
            ///     The register is in page 1.
            /// </remarks>
            public static readonly byte InterruptEnable = 0x10;

            /// <summary>
            ///     Accelerometer any motion threshold
            /// 
            ///     The threshold value depends upon the value in the 
            ///     AccelerometerConfiguration register.
            /// 
            ///     1 LSB = 3.91mg (2g range)
            ///     1 LSB = 7.81mg (4g range)
            ///     1 LSB = 15.63mg (8g range)
            ///     1 LSB = 31.25mg (16g range)
            /// </summary>
            /// <remarks>
            ///     The register is in page 1.
            /// </remarks>
            public static readonly byte AccelerometerMotionThreshold = 0x11;

            /// <summary>
            ///     Accelerometer interrupt settings.
            /// 
            ///     b0-b1: Any motion interrupt triggers if b1b0+1 consecutive data 
            ///            points exceed the any motion threshold.
            ///     b2: Select X axis for the any motion interrupt.
            ///     b3: Select Y axis for the any motion interrupt.
            ///     b4: Select Y axis for the any motion interrupt.
            ///     b5: Select X axis for the high rate interrupt.
            ///     b6: Select Y axis for the high rate interrupt.
            ///     b7: Select Z axis for the high rate interrupt.
            /// </summary>
            /// <remarks>
            ///     The register is in page 1.
            /// </remarks>
            public static readonly byte AccelerometerInterruptSettings = 0x12;

            /// <summary>
            ///     Accelerometer high-g interrupt duration.
            /// 
            ///     Delay before the high-g interrupt is triggered.  Delay is a
            ///     multiple of 2ms.
            /// </summary>
            /// <remarks>
            ///     The register is in page 1.
            /// </remarks>
            public static readonly byte AccelerometerHighGDuration = 0x013;

            /// <summary>
            ///     High-g threshold, this is dependent upon the setting in the range register.
            /// 
            ///     1 LSB = 7.81mg (2g range)
            ///     1 LSB = 15.63mg (4g range)
            ///     1 LSB = 31.25mg (8g range)
            ///     1 LSB = 62.5mg (16-g range)
            /// </summary>
            /// <remarks>
            ///     The register is in page 1.
            /// </remarks>
            public static readonly byte AccelerometerHighGThreshold = 0x14;

            /// <summary>
            ///     Accelerometer no motion / slow motion threshold.
            /// 
            ///     1 LSB = 3.91mg (2g range)
            ///     1 LSB = 7.81mg (4g range)
            ///     1 LSB = 15.63mg (8g range)
            ///     1 LSB = 31.25mg (16g range)
            /// </summary>
            /// <remarks>
            ///     The register is in page 1.
            /// </remarks>
            public static readonly byte AccelerometerNoMotionThreshold = 0x15;

            /// <summary>
            ///     Accelerometer slow / no motion setting.
            /// 
            ///     b0: 0 - Slow motion selected, 1 - no motion selected.
            ///     b1-b7: Slow / no motion duration
            /// </summary>
            /// <remarks>
            ///     The register is in page 1.
            /// </remarks>
            public static readonly byte AccelerometerNoMotionSetting = 0x16;

            /// <summary>
            ///     Gyroscope interrupt settings
            /// 
            ///     b0: Enable / disable the any motion interrupt on the X axis.
            ///     b1: Enable / disable the any motion interrupt on the Y axis.
            ///     b2: Enable / disable the any motion interrupt on the Z axis.
            ///     b3: Enable / disable the high rate interrupt on the X axis.
            ///     b4: Enable / disable the high rate interrupt on the Y axis.
            ///     b5: Enable / disable the high rate  interrupt on the Z axis.
            ///     b6: Select unfiltered data for the any motion interrupt.
            ///     b7: Select unfiltered data for the high rate interrupt.
            /// </summary>
            /// <remarks>
            ///     The register is in page 1.
            /// </remarks>
            public static readonly byte GyroscopeInterruptSetting = 0x17;

            /// <summary>
            ///     High rate settings for the gyroscope X axis
            /// 
            ///     b0-b4: High rate threshold for the X axis.
            ///            1 LSB - 15.625 degrees per second in 500 degrees per second range.
            ///            1 LSB - 31.25 degrees per second in 1000 degrees per second range.
            ///            1 LSB - 62.5 degrees per second in 2000 degrees per second range.
            /// 
            ///     b5-b5: High rate hysteresis for the X axis.
            ///            1 LSB - 15.625 degrees per second in 500 degrees per second range.
            ///            1 LSB - 31.25 degrees per second in 1000 degrees per second range.
            ///            1 LSB - 62.5 degrees per second in 2000 degrees per second range.
            /// </summary>
            /// <remarks>
            ///     The register is in page 1.
            /// </remarks>
            public static readonly byte GyroscopeHighRateX = 0x18;

            /// <summary>
            ///     High rate duration for the X axis.
            /// 
            ///     Actual time will be (1 + high rate duration) * 2.5 ms.
            /// </summary>
            /// <remarks>
            ///     The register is in page 1.
            /// </remarks>
            public static readonly byte GyroscopeDurationX = 0x19;

            /// <summary>
            ///     High rate settings for the gyroscope Y axis
            /// 
            ///     b0-b4: High rate threshold for the X axis.
            ///            1 LSB - 15.625 degrees per second in 500 degrees per second range.
            ///            1 LSB - 31.25 degrees per second in 1000 degrees per second range.
            ///            1 LSB - 62.5 degrees per second in 2000 degrees per second range.
            /// 
            ///     b5-b5: High rate hysteresis for the X axis.
            ///            1 LSB - 15.625 degrees per second in 500 degrees per second range.
            ///            1 LSB - 31.25 degrees per second in 1000 degrees per second range.
            ///            1 LSB - 62.5 degrees per second in 2000 degrees per second range.
            /// </summary>
            /// <remarks>
            ///     The register is in page 1.
            /// </remarks>
            public static readonly byte GyroscopeHighRateY = 0x1a;

            /// <summary>
            ///     High rate duration for the Y axis.
            /// 
            ///     Actual time will be (1 + high rate duration) * 2.5 ms.
            /// </summary>
            /// <remarks>
            ///     The register is in page 1.
            /// </remarks>
            public static readonly byte GyroscopeDurationY = 0x1b;

            /// <summary>
            ///     High rate settings for the gyroscope Z axis
            /// 
            ///     b0-b4: High rate threshold for the X axis.
            ///            1 LSB - 15.625 degrees per second in 500 degrees per second range.
            ///            1 LSB - 31.25 degrees per second in 1000 degrees per second range.
            ///            1 LSB - 62.5 degrees per second in 2000 degrees per second range.
            /// 
            ///     b5-b5: High rate hysteresis for the X axis.
            ///            1 LSB - 15.625 degrees per second in 500 degrees per second range.
            ///            1 LSB - 31.25 degrees per second in 1000 degrees per second range.
            ///            1 LSB - 62.5 degrees per second in 2000 degrees per second range.
            /// </summary>
            /// <remarks>
            ///     The register is in page 1.
            /// </remarks>
            public static readonly byte GyroscopeHighRateZ = 0x1c;

            /// <summary>
            ///     High rate duration for the Z axis.
            /// 
            ///     Actual time will be (1 + high rate duration) * 2.5 ms.
            /// </summary>
            /// <remarks>
            ///     The register is in page 1.
            /// </remarks>
            public static readonly byte GyroscopeDurationZ = 0x1d;

            /// <summary>
            ///     Gyroscope any motion threshold.
            /// 
            ///     1 LSB = 0.25 degrees per second in 500 degrees per second range.
            ///     1 LSB = 0.5 degrees per second in 1000 degrees per second range.
            ///     1 LSB = 1 degree per second in 2000 degrees per second range.
            /// </summary>
            /// <remarks>
            ///     The register is in page 1.
            /// </remarks>
            public static readonly byte GyroscopeAnyMotionThreshold = 0x1e;

            /// <summary>
            ///     Gyroscope any motion setup.
            /// 
            ///     b0-b1: Any motion trigger slope threshold.  The any motion
            ///            interrupt triggers when (1 + slope samples) * 4 data points
            ///            are above the GyroscopeAnyMotionThreshold.
            ///     b2-b3: Awake duration. 0 = 8 samples, 1 = 16 samples, 
            ///            2 = 32 samples, 3 = 64 samples.
            /// </summary>
            /// <remarks>
            ///     The register is in page 1.
            /// </remarks>
            public static readonly byte GyroscopeAnyMotionSetting = 0x1f;
        }



    }
}
