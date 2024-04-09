using Meadow;
using Meadow.Devices;
using Meadow.Foundation.ICs.IOExpanders;
using Meadow.Hardware;
using System;
using System.Text;
using System.Threading.Tasks;

#nullable enable

namespace ICs.IOExpanders.Sc16is752_Sample
{
    public class MeadowApp : App<F7FeatherV2>
    {
        private Sc16is752? _expander = null;
        private ISerialPort? _sc16is7x2Port = null;
        private IDigitalInterruptPort? _sharedIrqPort = null;

        // LONG MESSAGE TESTS (using extra IRQ driven buffer):
        // The following speed tests have been performed with Meadow v2.d and OS v1.10.0:
        // Test data sent from a second Meadow running the SerialTestGenerator app.
        // NB! These tests are using VERY LONG messages. (300 bytes) You may achieve much higher speeds with
        // short messages only. (<=64 bytes, the size of the SC16IS7x2 FIFO buffer)
        // Continous reading driven by IRQ works, but NOT the FIRST time the callback is invoked.
        // Probably because of this: https://github.com/WildernessLabs/Meadow_Issues/issues/74
        private const int Rate = 9600;      // Continous IRQ read works, but I2C bus speed must be <= 1MHz (FastPlus).
        //private const int Rate = 19200;     // Continous IRQ read works, but I2C bus speed must be 400KHz (Fast) or 1MHz (FastPlus).
        //private const int Rate = 38400;     // Continous IRQ read DOES NOT WORK(!), regardless of I2C bus speed.
        //private const int Rate = 57600;     // Continous IRQ read DOES NOT WORK(!), regardless of I2C bus speed.

        private const I2cBusSpeed I2cSpeed = I2cBusSpeed.Standard;

        // Some GPIO tests, too. GPIO0-3 are output pins. GPIO4-7 are input pins.
        // Connect pin 0 to 4, 1 to 5, 2 to 6, 3 to 7.
        private Sc16is7x2.DigitalOutputPort?[] _expOutputs = new Sc16is7x2.DigitalOutputPort[4];

        public override async Task Initialize()
        {
            // NOTE: This sample connects PortA on the expander back to COM1 on the F7Feather
            Resolver.Log.Info("Initialize...");

            var address = Sc16is7x2.Addresses.Address_0x48;     // Set A0 ans A1 to 3.3V to get this address.
            try
            {
                _sharedIrqPort = Device.CreateDigitalInterruptPort(Device.Pins.D00, InterruptMode.EdgeFalling, ResistorMode.Disabled);

                var i2c = Device.CreateI2cBus(I2cSpeed);
                var frequency = new Meadow.Units.Frequency(1.8432, Meadow.Units.Frequency.UnitType.Megahertz);
                _expander = new Sc16is752(i2c, frequency, address, _sharedIrqPort);
                _sc16is7x2Port = _expander.PortA.CreateSerialPort(Rate, readBufferSize: 8192);

                var outPins = new IPin[] { _expander.Pins.GP0, _expander.Pins.GP1, _expander.Pins.GP2, _expander.Pins.GP3 };
                var inPins = new IPin[] { _expander.Pins.GP4, _expander.Pins.GP5, _expander.Pins.GP6, _expander.Pins.GP7 };
                for(int i = 0; i < 4; i++)
                {
                    _expOutputs[i] = _expander.CreateDigitalOutputPort(outPins[i], false, OutputType.PushPull) as Sc16is7x2.DigitalOutputPort;
                    var inPort = _expander.CreateDigitalInputPort(inPins[i]) as Sc16is7x2.DigitalInputPort;
                    inPort!.StateChanged += (s, e) =>
                    {
                        Resolver.Log.Info($"---> StateChanged {inPort.Pin.Name}: {e.OldState} -> {e.NewState}");
                    };
                }

                // We (this main program) can also handle the IRQ's, but should we should be last in the queue.
                _sharedIrqPort.Changed += (s, e) =>
                {
                    string oldValue = e.Old != null ? e.Old.Value.State.ToString() : "None";
                    //Resolver.Log.Info($"---> IRQ: {oldValue} -> {e.New.State}");
                    //if (e.New.State == false) Resolver.Log.Info($"---> IRQ. Main app can also subscribe.");
                };
            }
            catch (Exception ex)
            {
                Resolver.Log.Error($"Failed to initialize 0x{(byte)address:X2}: {ex.Message}");
                await Task.Delay(1000);
            }
        }

        string _irqReceiveText = string.Empty;
        string _sc16is7x2Name = "SC16IS752";

        public async override Task Run()
        {
            if (_expander == null || _sc16is7x2Port == null)
                return;

            // Test IRQ callbacks for serial port. This will called AFTER the driver has read the data.
            _sc16is7x2Port.DataReceived += (s, e) =>
            {
                var bytes = _sc16is7x2Port.ReadAll();
                _irqReceiveText += Encoding.ASCII.GetString(bytes);
                // Disabling this. It may be too slow inside an interrupt handler...
                //Resolver.Log.Info($" ---> {_sc16is7x2Name} port read: irqReceiveText={NiceMsg(_irqReceiveText)} [{_sc16is7x2Name} IRQ callback is working!]");
            };
            _sc16is7x2Port.BufferOverrun += (s, e) =>
            {
                Resolver.Log.Error($" ---> {_sc16is7x2Name} buffer overrun!");
            };

            Resolver.Log.Info($"Opening port {_sc16is7x2Name}...");
            _sc16is7x2Port.Open();

            for (int i = 0; i < 100; i++)
            {
                // Test receiving message
                await Receive(_sc16is7x2Name, _sc16is7x2Port);
            }

            Resolver.Log.Info($"ALL DONE.");
            while (true)
            {
                await Task.Delay(1000);
            }
        }

        /// <summary>
        /// The RX/TX FIFO's on SC16IS7x2 are 64 bytes each. So, we can read max 64 bytes at once.
        /// KW: I'm trying to implement support for reading more than 64 bytes, but this requires IRQ callback to work.
        /// </summary>
        /// <param name="senderName"></param>
        /// <param name="senderPort"></param>
        /// <param name="receiverName"></param>
        /// <param name="receiverPort"></param>
        /// <returns></returns>
        private async Task Receive(
            //string senderName, ISerialPort senderPort, 
            string receiverName, ISerialPort receiverPort)
        {
            try
            {
                _irqReceiveText = string.Empty;  // Clear the IRQ receive buffer.

                // Wait for possible IRQ callback's to receive data.
                Resolver.Log.Info($"Waiting for IRQ initiated external messages (and changing some GPIO's)...");

                for (int i = 0; i < 4; i++)
                {
                    bool newState = !_expOutputs[i]!.State; // NB! The two !'s have quite different meaning.
                    Resolver.Log.Info($"*** Setting output {_expOutputs[i]!.Pin.Name} to {newState} ***");
                    _expOutputs[i]!.State = newState;
                    await Task.Delay(4000);
                }

                if (_irqReceiveText.Length > 0)
                {
                    Resolver.Log.Info($"Received {_irqReceiveText.Length} bytes on {receiverName} using IRQ callback: {NiceMsg(_irqReceiveText)}");
                }

                // Read any left over bytes. (Text received without IRQ callback)
                var leftOvers = receiverPort.ReadAll();
                if (leftOvers.Length == 0)
                {
                    Resolver.Log.Info("No left over bytes! :-)");
                }
                else
                {
                    string leftOverText = Encoding.ASCII.GetString(leftOvers);
                    Resolver.Log.Info($"Read {leftOvers.Length} left over bytes on {receiverName} WITHOUT IRQ callback: {NiceMsg(leftOverText)}");
                }

                Resolver.Log.Info($"Done.");
            }
            catch (Exception ex)
            {
                Resolver.Log.Error($"Test error: {ex.Message}");
            }
        }

        private string NiceMsg(string message)
        {
            return $"\"{message.Replace("\n", "\\n")}\"";
        }
    }
}