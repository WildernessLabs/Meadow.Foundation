/// <summary>
/// Driver for Adafruit Seesaw
/// Author: Frederick M Meyer
/// Date: 2022-03-03
/// Copyright: 2022 (c) Frederick M Meyer for Wilderness Labs
/// License: MIT
/// </summary>
/// <remarks>
/// For hardware, this works with either Seesaw device:
/// Adafruit ATSAMD09 Breakout with seesaw <see href="https://www.adafruit.com/product/3657"</see>
/// -or-
/// Adafruit ATtiny8x7 Breakout with seesaw - STEMMA QT / Qwiic <see href="https://www.adafruit.com/product/5233"</see>
/// </remarks>

using Meadow.Hardware;
using System;
using System.Threading;

namespace Meadow.Foundation.ICs.IOExpanders.Seesaw
{
    /// <summary>
    /// Provides the base class for driving the Seesaw device
    /// </summary>
    /// <example>
    /// <code>
    ///     var i2cBus = Device.CreateI2cBus(...);
    ///     var seesaw = new Seesaw(i2cBus, ...);
    /// </code>
    /// </example>
    
    public partial class Seesaw
    {
        public  II2cBus I2cBus { get; }
        public byte SeesawBoardAddr { get; }
        private IPin DeviceReadyPin { get; }
        public bool ResetOnInit { get; }

        public II2cPeripheral I2cPeripheral { get; }
        public byte ChipId { get; }

        public Seesaw
        (
            II2cBus i2cBus,              // Bus the Seesaw is connected to
            byte seesawBoardAddr = 0x49, // I2C address of the Seesaw device
            IPin deviceReadyPin = null,  // Pin connected to Seesaw's 'ready' output. Not yet implemented
            bool resetOnInit = true      // Whether to do a software reset on init
        )
        {
            this.I2cBus = i2cBus;
            this.SeesawBoardAddr = seesawBoardAddr;
            this.DeviceReadyPin = deviceReadyPin;
            this.ResetOnInit = resetOnInit;

            // TODO: Add support for DeviceReadyPin
            if (DeviceReadyPin != null)
                throw new NotImplementedException("[Device Ready Pin]");
            
            // devicereadypin.switchtoinput()
            // var d7 = Device.CreateDigitalInputPort(Device.Pins.D07, resistorMode: ResistorMode.InternalPullDown);

            this.I2cPeripheral = new I2cPeripheral(I2cBus, SeesawBoardAddr);

            if (ResetOnInit)
                SwReset();

            byte[] returnValue = new byte[1];
            I2cPeripheral.Exchange(new Span<byte>(new byte[] { (byte)BaseAddresses.Status, (byte)StatusCommands.HwId }), new Span<byte>(returnValue));
            ChipId = returnValue[0];

            if (!Enum.IsDefined(typeof(HwidCodes), ChipId))
                throw new InvalidOperationException(
                    $"Seesaw hardware ID returned (0x{ChipId:x}) is not correct! Expected 0x{HwidCodes.ATSAMD09:x} or 0x{HwidCodes.ATtiny8X7:x}. Please check your wiring.");
        }

        public void SwReset(int postResetDelay = 500)
        {
            I2cPeripheral.Write(new byte[] { (byte)BaseAddresses.Status, (byte)StatusCommands.SwReset, (byte)0xFF });
            Thread.Sleep(postResetDelay);
        }

        public UInt32 GetOptions()
        {
            // Retrieve the 'options' word from the Seesaw board
            byte[] returnValue = new byte[4];
            I2cPeripheral.Exchange(new Span<byte>(new byte[] { (byte)BaseAddresses.Status, (byte)StatusCommands.Options }), new Span<byte>(returnValue));
            return ((UInt32)returnValue[0] << 24) | ((UInt32)returnValue[1] << 16) | ((UInt32)returnValue[2] << 8) | (UInt32)returnValue[3];
        }
        
        public UInt32 GetVersion()
        {
            // Retrieve the 'version' word from the Seesaw board
            byte[] returnValue = new byte[4];
            I2cPeripheral.Exchange(new Span<byte>(new byte[] { (byte)BaseAddresses.Status, (byte)StatusCommands.Version }), new Span<byte>(returnValue));
            return ((UInt32)returnValue[0] << 24) | ((UInt32)returnValue[1] << 16) | ((UInt32)returnValue[2] << 8) | (UInt32)returnValue[3];
        }
        
        public void SetDeviceReadyPin(IPin pin)
        {
            throw new NotImplementedException("[Device Ready Pin]");

            //byte[] pinNumber = BitConverter.GetBytes((int)pin.Key);
            //I2cPeripheral.Write(new Span<byte>(new byte[] { (byte)Seesaw.BaseAddress.Neopixel, (byte)Seesaw.Neopixel.Pin,
            //    pinNumber[0], pinNumber[1], pinNumber[2], pinNumber[3] }));
         }
    }
}
