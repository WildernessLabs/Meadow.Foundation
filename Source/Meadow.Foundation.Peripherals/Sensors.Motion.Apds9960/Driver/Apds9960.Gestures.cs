using System;
using System.Threading;

namespace Meadow.Foundation.Sensors.Motion;

/// <summary>
/// Represents the APDS9960 Proximity, Light, RGB, and Gesture Sensor
/// </summary>
public partial class Apds9960
{
    private readonly GestureData gestureData;
    private int gestureUdDelta;
    private int gestureLrDelta;
    private int gestureUdCount;
    private int gestureLrCount;
    private int gestureNearCount;
    private int gestureFarCount;
    private States gestureState;


    /// <summary>
    /// Starts the gesture recognition engine on the APDS-9960
    /// </summary>
    /// <param name="interrupts"></param>
    /// <returns>Enable interrupts for gestures</returns>
    public bool EnableGestureSensor(bool interrupts)
    {
        /* Enable gesture mode
           Set ENABLE to 0 (power off)
           Set WTIME to 0xFF
           Set AUX to LED_BOOST_100
           Enable PON, WEN, PEN, GEN in ENABLE 
        */
        ResetGestureParameters();
        BusComms.WriteRegister(Registers.WTIME, 0xFF);
        BusComms.WriteRegister(Registers.APDS9960_PPULSE, DefaultValues.DEFAULT_GESTURE_PPULSE);

        SetLEDBoost(GainValues.LED_BOOST_100);

        SetGestureIntEnable((byte)(interrupts ? 1 : 0));

        SetGestureMode(1);
        EnablePower(true);
        SetMode(OperatingModes.WAIT, 1);
        SetMode(OperatingModes.PROXIMITY, 1);
        SetMode(OperatingModes.GESTURE, 1);

        return true;
    }

    /// <summary>
    /// Disable gestures
    /// </summary>
    public void DisableGestureSensor()
    {
        ResetGestureParameters();
        SetGestureIntEnable(0);
        SetGestureMode(0);
        SetMode(OperatingModes.GESTURE, 0);
    }

    /// <summary>
    /// Is a gesture reading available
    /// </summary>
    /// <returns>True if available</returns>
    public bool IsGestureAvailable()
    {
        byte val = BusComms.ReadRegister(Registers.APDS9960_GSTATUS);

        /* Shift and mask out GVALID bit */
        val &= BitFields.APDS9960_GVALID;

        /* Return true/false based on GVALID bit */
        return val == 1;
    }

    /// <summary>
    /// Read the current gesture
    /// </summary>
    /// <returns>The direction</returns>
    /// <exception cref="Exception">Throws if reading gesture data failed</exception>
    public Direction ReadGesture()
    {
        /* Make sure that power and gesture is on and data is valid */
        if (!IsGestureAvailable() || (GetMode() & 0b01000001) == 0x0)
        {
            return (int)Direction.NONE;
        }

        /* Keep looping as long as gesture data is valid */
        while (true)
        {
            byte fifo_level;
            byte bytes_read;

            /* Wait some time to collect next batch of FIFO data */
            Thread.Sleep(FIFO_PAUSE_TIME);

            var gstatus = ReadGestureStatusRegister();

            /* If we have valid data, read in FIFO */
            if ((gstatus & GestureStatus.Valid) == GestureStatus.Valid)
            {
                fifo_level = BusComms.ReadRegister(Registers.APDS9960_GFLVL);

                /* If there's stuff in the FIFO, read it into our data block */
                if (fifo_level > 0)
                {
                    byte len = (byte)(fifo_level * 4);

                    BusComms.ReadRegister(Registers.APDS9960_GFIFO_U, readBuffer.Span[0..len]);

                    bytes_read = len; //ToDo should we have a check> (byte)fifo_data.Length;

                    if (bytes_read < 1)
                    {
                        throw new Exception();
                    }

                    /* If at least 1 set of data, sort the data into U/D/L/R */
                    if (bytes_read >= 4)
                    {
                        for (int i = 0; i < bytes_read; i += 4)
                        {
                            gestureData.UData[gestureData.Index] = readBuffer.Span[i + 0];
                            gestureData.DData[gestureData.Index] = readBuffer.Span[i + 1];
                            gestureData.LData[gestureData.Index] = readBuffer.Span[i + 2];
                            gestureData.RData[gestureData.Index] = readBuffer.Span[i + 3];
                            gestureData.Index++;
                            gestureData.TotalGestures++;
                        }

                        /* Filter and process gesture data. Decode near/far state */
                        if (ProcessGestureData())
                        {
                            if (DecodeGesture().success)
                            {
                                //***TODO: U-Turn Gestures
                            }
                        }

                        /* Reset data */
                        gestureData.Index = 0;
                        gestureData.TotalGestures = 0;
                    }
                }
            }
            else
            {
                /* Determine best guessed gesture and clean up */
                Thread.Sleep(FIFO_PAUSE_TIME);
                var result = DecodeGesture();

                ResetGestureParameters();

                if (result.success)
                {
                    return result.direction;
                }

                return Direction.NONE;
            }
        }
    }

    /// <summary>
    /// Reset all gesture data parameters
    /// </summary>
    private void ResetGestureParameters()
    {
        gestureData.Index = 0;
        gestureData.TotalGestures = 0;

        gestureUdDelta = 0;
        gestureLrDelta = 0;

        gestureUdCount = 0;
        gestureLrCount = 0;

        gestureNearCount = 0;
        gestureFarCount = 0;

        gestureState = 0;
    }

    /// <summary>
    /// Processes the raw gesture data to determine swipe direction
    /// </summary>
    /// <returns>True if near or far state seen, false otherwise</returns>
    private bool ProcessGestureData()
    {
        byte u_first = 0;
        byte d_first = 0;
        byte l_first = 0;
        byte r_first = 0;
        byte u_last = 0;
        byte d_last = 0;
        byte l_last = 0;
        byte r_last = 0;
        int ud_ratio_first;
        int lr_ratio_first;
        int ud_ratio_last;
        int lr_ratio_last;
        int ud_delta;
        int lr_delta;
        int i;

        /* If we have less than 4 total gestures, that's not enough */
        if (gestureData.TotalGestures <= 4)
        {
            return false;
        }

        /* Check to make sure our data isn't out of bounds */
        if ((gestureData.TotalGestures <= 32) &&
            (gestureData.TotalGestures > 0))
        {
            /* Find the first value in U/D/L/R above the threshold */
            for (i = 0; i < gestureData.TotalGestures; i++)
            {
                if ((gestureData.UData[i] > GestureParameters.THRESHOLD_OUT) &&
                    (gestureData.DData[i] > GestureParameters.THRESHOLD_OUT) &&
                    (gestureData.LData[i] > GestureParameters.THRESHOLD_OUT) &&
                    (gestureData.RData[i] > GestureParameters.THRESHOLD_OUT))
                {

                    u_first = gestureData.UData[i];
                    d_first = gestureData.DData[i];
                    l_first = gestureData.LData[i];
                    r_first = gestureData.RData[i];
                    break;
                }
            }

            /* If one of the _first values is 0, then there is no good data */
            if ((u_first == 0) || (d_first == 0) ||
                (l_first == 0) || (r_first == 0))
            {
                return false;
            }
            /* Find the last value in U/D/L/R above the threshold */
            for (i = gestureData.TotalGestures - 1; i >= 0; i--)
            {
                if ((gestureData.UData[i] > GestureParameters.THRESHOLD_OUT) &&
                    (gestureData.DData[i] > GestureParameters.THRESHOLD_OUT) &&
                    (gestureData.LData[i] > GestureParameters.THRESHOLD_OUT) &&
                    (gestureData.RData[i] > GestureParameters.THRESHOLD_OUT))
                {

                    u_last = gestureData.UData[i];
                    d_last = gestureData.DData[i];
                    l_last = gestureData.LData[i];
                    r_last = gestureData.RData[i];
                    break;
                }
            }
        }

        /* Calculate the first vs. last ratio of up/down and left/right */
        ud_ratio_first = (u_first - d_first) * 100 / (u_first + d_first);
        lr_ratio_first = (l_first - r_first) * 100 / (l_first + r_first);
        ud_ratio_last = (u_last - d_last) * 100 / (u_last + d_last);
        lr_ratio_last = (l_last - r_last) * 100 / (l_last + r_last);

        /* Determine the difference between the first and last ratios */
        ud_delta = ud_ratio_last - ud_ratio_first;
        lr_delta = lr_ratio_last - lr_ratio_first;

        /* Accumulate the UD and LR delta values */
        gestureUdDelta += ud_delta;
        gestureLrDelta += lr_delta;

        /* Determine U/D gesture */
        if (gestureUdDelta >= GestureParameters.SENSITIVITY_1)
        {
            gestureUdCount = 1;
        }
        else if (gestureUdDelta <= -GestureParameters.SENSITIVITY_1)
        {
            gestureUdCount = -1;
        }
        else
        {
            gestureUdCount = 0;
        }

        /* Determine L/R gesture */
        if (gestureLrDelta >= GestureParameters.SENSITIVITY_1)
        {
            gestureLrCount = 1;
        }
        else if (gestureLrDelta <= -GestureParameters.SENSITIVITY_1)
        {
            gestureLrCount = -1;
        }
        else
        {
            gestureLrCount = 0;
        }

        /* Determine Near/Far gesture */
        if ((gestureUdCount == 0) && (gestureLrCount == 0))
        {
            if ((Math.Abs(ud_delta) < GestureParameters.SENSITIVITY_2) &&
                (Math.Abs(lr_delta) < GestureParameters.SENSITIVITY_2))
            {
                if (Math.Abs(ud_delta) <= 2 && Math.Abs(lr_delta) <= 2)
                {
                    gestureNearCount++;
                }
                else
                {
                    gestureFarCount++;
                }

                if (gestureNearCount >= 8)
                {
                    gestureState = States.NEAR_STATE;
                    return true;
                }
                else if (gestureFarCount >= 2)
                {
                    gestureState = States.FAR_STATE;
                    return true;
                }
            }
        }
        else
        {
            if ((Math.Abs(ud_delta) < GestureParameters.SENSITIVITY_2) && (Math.Abs(lr_delta) < GestureParameters.SENSITIVITY_2))
            {
                if ((ud_delta == 0) && (lr_delta == 0))
                {
                    gestureNearCount++;
                }

                if (gestureNearCount >= 10)
                {
                    gestureUdCount = 0;
                    gestureLrCount = 0;
                    gestureUdDelta = 0;
                    gestureLrDelta = 0;
                }
            }
        }

        return false;
    }

    private (bool success, Direction direction) DecodeGesture()
    {
        // Check proximity gestures first
        if (gestureState == States.NEAR_STATE)
        {
            return (true, Direction.NEAR);
        }

        if (gestureState == States.FAR_STATE)
        {
            return (true, Direction.FAR);
        }

        // Handle pure directional gestures
        if (IsSimpleDirectionalGesture())
        {
            return DecodeSimpleDirection();
        }

        // Handle diagonal gestures
        if (IsDiagonalGesture())
        {
            return DecodeDiagonalDirection();
        }

        return (false, Direction.NONE);
    }

    private bool IsSimpleDirectionalGesture()
    {
        return (Math.Abs(gestureUdCount) == 1 && gestureLrCount == 0) ||
               (gestureUdCount == 0 && Math.Abs(gestureLrCount) == 1);
    }

    private (bool success, Direction direction) DecodeSimpleDirection()
    {
        if (gestureUdCount == -1) return (true, Direction.UP);
        if (gestureUdCount == 1) return (true, Direction.DOWN);
        if (gestureLrCount == 1) return (true, Direction.RIGHT);
        if (gestureLrCount == -1) return (true, Direction.LEFT);

        return (false, Direction.NONE); // Should never reach here if IsSimpleDirectionalGesture() is true
    }

    private bool IsDiagonalGesture()
    {
        return Math.Abs(gestureUdCount) == 1 && Math.Abs(gestureLrCount) == 1;
    }

    private (bool success, Direction direction) DecodeDiagonalDirection()
    {
        bool isVerticalDominant = Math.Abs(gestureUdDelta) > Math.Abs(gestureLrDelta);

        // Map diagonal movements to their dominant direction
        return (true, (gestureUdCount, gestureLrCount, isVerticalDominant) switch
        {
            (-1, 1, true) => Direction.UP,
            (-1, 1, false) => Direction.RIGHT,
            (1, -1, true) => Direction.DOWN,
            (1, -1, false) => Direction.LEFT,
            (-1, -1, true) => Direction.UP,
            (-1, -1, false) => Direction.LEFT,
            (1, 1, true) => Direction.DOWN,
            (1, 1, false) => Direction.RIGHT,
            _ => Direction.NONE
        });
    }

    private byte GetGestureEnterThresh()
    {
        byte val = BusComms.ReadRegister(Registers.APDS9960_GPENTH);

        return val;
    }

    /**
     * @brief Sets the entry proximity threshold for gesture sensing
     *
     * @param[in] threshold proximity value needed to start gesture mode
     * @return True if operation successful. False otherwise.
     */
    private void SetGestureEnterThresh(byte threshold)
    {
        BusComms.WriteRegister(Registers.APDS9960_GPENTH, threshold);
    }

    /**
     * @brief Gets the exit proximity threshold for gesture sensing
     *
     * @return Current exit proximity threshold.
     */
    private byte GetGestureExitThresh()
    {
        byte val = BusComms.ReadRegister(Registers.APDS9960_GEXTH);

        return val;
    }

    /**
     * @brief Sets the exit proximity threshold for gesture sensing
     *
     * @param[in] threshold proximity value needed to end gesture mode
     * @return True if operation successful. False otherwise.
     */
    private void SetGestureExitThresh(byte threshold)
    {
        BusComms.WriteRegister(Registers.APDS9960_GEXTH, threshold);
    }

    /**
     * @brief Gets the gain of the photodiode during gesture mode
     *
     * Value    Gain
     *   0       1x
     *   1       2x
     *   2       4x
     *   3       8x
     *
     * @return the current photodiode gain. 0xFF on error.
     */
    private byte GetGestureGain()
    {
        byte val = BusComms.ReadRegister(Registers.APDS9960_GCONF2);

        /* Shift and mask out GGAIN bits */
        val = (byte)((val >> 5) & 0b00000011);

        return val;
    }

    /**
     * @brief Sets the gain of the photodiode during gesture mode
     *
     * Value    Gain
     *   0       1x
     *   1       2x
     *   2       4x
     *   3       8x
     *
     * @param[in] gain the value for the photodiode gain
     * @return True if operation successful. False otherwise.
     */
    private void SetGestureGain(byte gain)
    {
        byte val = BusComms.ReadRegister(Registers.APDS9960_GCONF2);

        /* Set bits in register to given value */
        gain &= 0b00000011;
        gain = (byte)(gain << 5);
        val &= 0b10011111;
        val |= gain;

        /* Write register value back into GCONF2 register */
        BusComms.WriteRegister(Registers.APDS9960_GCONF2, val);
    }

    /**
     * @brief Gets the drive current of the LED during gesture mode
     *
     * Value    LED Current
     *   0        100 mA
     *   1         50 mA
     *   2         25 mA
     *   3         12.5 mA
     *
     * @return the LED drive current value. 0xFF on error.
     */
    private byte GetGestureLEDDrive()
    {
        /* Read value from GCONF2 register */
        byte val = BusComms.ReadRegister(Registers.APDS9960_GCONF2);

        /* Shift and mask out GLDRIVE bits */
        val = (byte)((val >> 3) & 0b00000011);

        return val;
    }

    /**
     * @brief Sets the LED drive current during gesture mode
     *
     * Value    LED Current
     *   0        100 mA
     *   1         50 mA
     *   2         25 mA
     *   3         12.5 mA
     *
     * @param[in] drive the value for the LED drive current
     * @return True if operation successful. False otherwise.
     */
    private void SetGestureLEDDrive(byte drive)
    {
        byte val = BusComms.ReadRegister(Registers.APDS9960_GCONF2);

        /* Set bits in register to given value */
        drive &= 0b00000011;
        drive = (byte)(drive << 3);
        val &= 0b11100111;
        val |= drive;

        BusComms.WriteRegister(Registers.APDS9960_GCONF2, val);
    }

    /**
     * @brief Gets the time in low power mode between gesture detections
     *
     * Value    Wait time
     *   0          0 ms
     *   1          2.8 ms
     *   2          5.6 ms
     *   3          8.4 ms
     *   4         14.0 ms
     *   5         22.4 ms
     *   6         30.8 ms
     *   7         39.2 ms
     *
     * @return the current wait time between gestures. 0xFF on error.
     */
    private byte GetGestureWaitTime()
    {
        byte val = BusComms.ReadRegister(Registers.APDS9960_GCONF2);

        /* Mask out GWTIME bits */
        val &= 0b00000111;

        return val;
    }

    /**
     * @brief Sets the time in low power mode between gesture detections
     *
     * Value    Wait time
     *   0          0 ms
     *   1          2.8 ms
     *   2          5.6 ms
     *   3          8.4 ms
     *   4         14.0 ms
     *   5         22.4 ms
     *   6         30.8 ms
     *   7         39.2 ms
     *
     * @param[in] the value for the wait time
     * @return True if operation successful. False otherwise.
     */
    private void SetGestureWaitTime(byte time)
    {
        byte val = BusComms.ReadRegister(Registers.APDS9960_GCONF2);

        /* Set bits in register to given value */
        time &= 0b00000111;
        val &= 0b11111000;
        val |= time;

        BusComms.WriteRegister(Registers.APDS9960_GCONF2, val);
    }

    /**
     * @brief Gets if gesture interrupts are enabled or not
     *
     * @return 1 if interrupts are enabled, 0 if not. 0xFF on error.
     */
    private byte GetGestureIntEnable()
    {
        byte val = BusComms.ReadRegister(Registers.APDS9960_GCONF4);

        /* Shift and mask out GIEN bit */
        val = (byte)((val >> 1) & 0b00000001);

        return val;
    }

    /**
     * @brief Turns gesture-related interrupts on or off
     *
     * @param[in] enable 1 to enable interrupts, 0 to turn them off
     * @return True if operation successful. False otherwise.
     */
    private void SetGestureIntEnable(byte enable)
    {
        byte val = BusComms.ReadRegister(Registers.APDS9960_GCONF4);

        /* Set bits in register to given value */
        enable &= 0b00000001;
        enable = (byte)(enable << 1);
        val &= 0b11111101;
        val |= enable;

        BusComms.WriteRegister(Registers.APDS9960_GCONF4, val);
    }

    /**
     * @brief Tells if the gesture state machine is currently running
     *
     * @return 1 if gesture state machine is running, 0 if not. 0xFF on error.
     */
    private byte GetGestureMode()
    {
        byte val = BusComms.ReadRegister(Registers.APDS9960_GCONF4);

        /* Mask out GMODE bit */
        val &= 0b00000001;

        return val;
    }

    /**
     * @brief Tells the state machine to either enter or exit gesture state machine
     *
     * @param[in] mode 1 to enter gesture state machine, 0 to exit.
     * @return True if operation successful. False otherwise.
     */
    private void SetGestureMode(byte mode)
    {
        byte val = BusComms.ReadRegister(Registers.APDS9960_GCONF4);

        /* Set bits in register to given value */
        mode &= 0b00000001;
        val &= 0b11111110;
        val |= mode;

        BusComms.WriteRegister(Registers.APDS9960_GCONF4, val);
    }

    /* Container for gesture data */
    private class GestureData
    {
        public byte[] UData { get; set; } = new byte[32];
        public byte[] DData { get; set; } = new byte[32];
        public byte[] LData { get; set; } = new byte[32];
        public byte[] RData { get; set; } = new byte[32];
        public byte Index { get; set; }
        public byte TotalGestures { get; set; }
        public byte InThreshold { get; set; }
        public byte OutThreshold { get; set; }
    }

    // results of register 0xAF (GSTATUS)
    [Flags]
    internal enum GestureStatus : byte
    {
        Valid = 1 << 0,
        Overflow = 1 << 1
    }

    internal GestureStatus ReadGestureStatusRegister()
    {
        return (GestureStatus)(BusComms.ReadRegister(Registers.APDS9960_GSTATUS) & 0x03);
    }
}