﻿using Meadow.Hardware;
using Meadow.Peripherals.Sensors.Cameras;
using Meadow.Units;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Meadow.Foundation.Sensors.Camera;

/// <summary>
/// Base class for the Arducam family of cameras
/// </summary>
public abstract partial class Arducam : IPhotoCamera, ISpiPeripheral, II2cPeripheral
{
    /// <summary>
    /// The current image format for capturing photos
    /// </summary>
    protected ImageFormat CurrentImageFormat { get; set; } = ImageFormat.Jpeg;

    /// <summary>
    /// The maximum amount of data that can be read from the camera memory
    /// </summary>
    protected virtual uint MAX_FIFO_SIZE => 0x5FFFF; //384KByte - OV2640 support

    /// <summary>
    /// The default SPI bus speed for the device
    /// </summary>
    public Frequency DefaultSpiBusSpeed => new Frequency(8, Frequency.UnitType.Megahertz);

    /// <summary>
    /// The SPI bus speed for the device
    /// </summary>
    public Frequency SpiBusSpeed
    {
        get => spiComms.BusSpeed;
        set => spiComms.BusSpeed = value;
    }

    /// <summary>
    /// The default SPI bus mode for the device
    /// </summary>
    public SpiClockConfiguration.Mode DefaultSpiBusMode => SpiClockConfiguration.Mode.Mode0;

    /// <summary>
    /// The SPI bus mode for the device
    /// </summary>
    public SpiClockConfiguration.Mode SpiBusMode
    {
        get => spiComms.BusMode;
        set => spiComms.BusMode = value;
    }

    /// <summary>
    /// The default I2C bus for the camera
    /// </summary>
    public byte DefaultI2cAddress => 0x30;

    /// <summary>
    /// SPI Communication bus used to communicate with the peripheral
    /// </summary>
    protected readonly ISpiCommunications spiComms;

    /// <summary>
    /// I2C Communication bus used to communicate with the peripheral
    /// </summary>
    protected readonly II2cCommunications i2cComms;

    /// <summary>
    /// Initializes a new instance of the Arducam class
    /// </summary>
    public Arducam(ISpiBus spiBus, IPin chipSelectPin, II2cBus i2cBus, byte i2cAddress)
        : this(spiBus, chipSelectPin.CreateDigitalOutputPort(), i2cBus, i2cAddress)
    { }

    /// <summary>
    /// Initializes a new instance of the Arducam class
    /// </summary>
    public Arducam(ISpiBus spiBus, IDigitalOutputPort chipSelectPort, II2cBus i2cBus, byte i2cAddress)
    {
        i2cComms = new I2cCommunications(i2cBus, i2cAddress);
        spiComms = new SpiCommunications(spiBus, chipSelectPort, DefaultSpiBusSpeed, DefaultSpiBusMode);

        Reset();

        Validate().Wait();
        Initialize().Wait();
    }

    /// <summary>
    /// Initializes the camera
    /// </summary>
    public abstract Task Initialize();

    /// <summary>
    /// Resets the camera
    /// </summary>
    protected void Reset()
    {
        WriteRegister(0x07, 0x80);
        Thread.Sleep(100);
        WriteRegister(0x07, 0x00);
        Thread.Sleep(100);
    }

    /// <summary>
    /// Validates the camera connection
    /// </summary>
    protected abstract Task Validate();

    /// <summary>
    /// Captures a photo and returns the image data
    /// </summary>
    public async Task<byte[]> CapturePhoto()
    {
        StartCapture();

        bool readData = false;
        for (int i = 0; i < 10; i++)
        {
            if (IsPhotoAvailable() == false)
            {
                await Task.Delay(500);
            }
            else
            {
                readData = true;
                break;
            }
        }

        if (readData == false)
        {
            throw new Exception("Photo data not available");
        }

        await Task.Delay(50);


        return await GetPhotoData();
    }

    private void StartCapture()
    {
        WriteRegister(ARDUCHIP_FIFO, FIFO_START_MASK);
    }

    private bool IsPhotoAvailable()
    {
        return GetBit(ARDUCHIP_TRIG, CAP_DONE_MASK) > 0;
    }

    /// <summary>
    /// Clears the FIFO buffer
    /// </summary>
    protected void FlushFifo()
    {
        WriteRegister(ARDUCHIP_FIFO, FIFO_CLEAR_MASK);
    }

    /// <summary>
    /// Clears the FIFO buffer
    /// </summary>
    protected void ClearFifoFlag()
    {
        WriteRegister(ARDUCHIP_FIFO, FIFO_CLEAR_MASK);
    }

    private Task<byte[]> GetPhotoData()
    {
        uint length = ReadFifoLength();

        if (length >= MAX_FIFO_SIZE)
        {
            throw new Exception($"Camera data - Fifo size {length} is over limit of {MAX_FIFO_SIZE}");
        }
        if (length == 0)
        {
            throw new Exception($"No camera data available");
        }

        var tx = new byte[length + 1];
        tx[0] = 0x3C;
        var rx = new byte[length + 1];

        spiComms.Exchange(tx, rx, DuplexType.Full);

        int header = -1;
        int footer = -1;

        //search for jpeg header and footer
        for (int i = 0; i < rx.Length - 1; i++)
        {
            if (rx[i] == 0xFF && rx[i + 1] == 0xD8)
            {
                header = i;
            }
            if (rx[i] == 0xFF && rx[i + 1] == 0xD9)
            {
                footer = i + 2;
                if (header != -1)
                {
                    break;
                }
            }
        }

        if (header == -1 || footer == -1)
        {
            throw new Exception($"Invalid camera data detected");
        }

        var image = new byte[footer - header];

        Array.Copy(rx, header, image, 0, footer - header);

        ClearFifoFlag();
        return Task.FromResult(image);
    }

    private uint ReadFifoLength()
    {
        uint len1, len2, len3, length;
        len1 = ReadRegister(FIFO_SIZE1);
        len2 = ReadRegister(FIFO_SIZE2);
        len3 = (uint)(ReadRegister(FIFO_SIZE3) & 0x7f);
        length = ((len3 << 16) | (len2 << 8) | len1) & 0x07fffff;
        return length;
    }

    private void SetFifoBurst()
    {
        spiComms.Write(BURST_FIFO_READ);
    }

    private byte ReadFifo()
    {
        return BusReadSpi(SINGLE_FIFO_READ);
    }

    private void SetBit(byte address, byte bit)
    {
        byte temp;
        temp = ReadRegister(address);
        WriteRegister(address, (byte)(temp | bit));
    }

    private void ClearBit(byte address, byte bit)
    {
        byte temp;
        temp = ReadRegister(address);
        WriteRegister(address, (byte)(temp & (~bit)));
    }

    /// <summary>
    /// Read an SPI register
    /// </summary>
    /// <param name="address"></param>
    /// <returns></returns>
    protected byte ReadRegister(byte address)
    {
        return BusReadSpi(address);
    }

    /// <summary>
    /// Get the value of a single bit from a byte
    /// </summary>
    /// <param name="address"></param>
    /// <param name="bit"></param>
    /// <returns></returns>
    protected byte GetBit(byte address, byte bit)
    {
        byte temp;
        temp = ReadRegister(address);
        temp &= bit;
        return temp;
    }

    /// <summary>
    /// Set the camera mode
    /// </summary>
    /// <param name="mode"></param>
    protected void SetMode(byte mode)
    {
        switch (mode)
        {
            case MCU2LCD_MODE:
                WriteRegister(ARDUCHIP_MODE, MCU2LCD_MODE);
                break;
            case CAM2LCD_MODE:
                WriteRegister(ARDUCHIP_MODE, CAM2LCD_MODE);
                break;
            case LCD2MCU_MODE:
                WriteRegister(ARDUCHIP_MODE, LCD2MCU_MODE);
                break;
            default:
                WriteRegister(ARDUCHIP_MODE, MCU2LCD_MODE);
                break;
        }
    }

    /// <summary>
    /// Set the jpeg capture resolution
    /// </summary>
    /// <param name="size"></param>
    /// <returns></returns>
    public abstract Task SetJpegSize(ImageSize size);

    /// <summary>
    /// Set the capture image format
    /// </summary>
    /// <param name="format"></param>
    protected void SetImageFormat(ImageFormat format)
    {
        CurrentImageFormat = format;
    }

    /// <summary>
    /// Write an SPI register
    /// </summary>
    /// <param name="address"></param>
    /// <param name="data"></param>
    protected void WriteRegister(byte address, byte data)
    {
        spiComms.WriteRegister((byte)(address | 0x80), data);
    }

    private byte BusReadSpi(byte address)
    {
        return spiComms.ReadRegister((byte)(address & 0x7F));
    }

    /// <summary>
    /// Read an I2C register
    /// </summary>
    /// <param name="regID"></param>
    /// <returns></returns>
    protected byte ReadSensorRegister(byte regID)
    {
        i2cComms.Write(regID);
        var ret = new byte[1];

        i2cComms.Read(ret);
        return ret[0];
    }

    /// <summary>
    /// Write an I2C register
    /// </summary>
    /// <param name="register"></param>
    /// <param name="value"></param>
    protected void WriteSensorRegister(byte register, byte value)
    {
        i2cComms.WriteRegister(register, value);
    }

    /// <summary>
    /// Write an array of I2C registers
    /// Used for camera configuration
    /// </summary>
    /// <param name="registerList"></param>
    protected internal void WriteSensorRegisters(SensorReg[] registerList)
    {
        for (int i = 0; i < registerList.Length; i++)
        {
            WriteSensorRegister(registerList[i].Register, registerList[i].Value);
        }
    }
}