using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Meadow.Hardware;
using Meadow.Peripherals.Sensors.Light;
using Meadow.Units;

namespace Meadow.Foundation.Sensors.Light
{
    /// <summary>
    /// High Accuracy Ambient Light Sensor 
    /// </summary>
    public partial class Veml7700 : ByteCommsSensorBase<Illuminance>, ILightSensor,  IDisposable
    {
        //==== events
        public event EventHandler<IChangeResult<Illuminance>> LuminosityUpdated = delegate { };

        //==== internals
        private ushort _config;

        //==== properties
        /// <summary>
        /// Luminosity reading from the TSL2561 sensor.
        /// </summary>
        public Illuminance? Illuminance { get; protected set; }

        public SensorTypes DataSource { get; set; } = SensorTypes.White;

        public Veml7700(II2cBus i2cBus)
            : base(i2cBus, (byte)Addresses.Default)
        {
        }

        protected async override Task<Illuminance> ReadSensor()
        {
            return await Task.Run(async () => {
                Illuminance illuminance = new Illuminance(0);
                
                int integrationTime = 0;
                int gain = 1;
                int scaleA;
                int scaleB;

                // this loop will tune the integration time (effectively shutter speed)
                // of the sensor until it gets to a place where the data is good.
                // once the data coming is good, then the actual reading is multiplied
                // by the integration time scale factor.
                // see the Application Note pdf for more information
                while (true) {
                    WriteRegister(Registers.AlsConf0, 0);

                    // wait > 2.5ms
                    await Task.Delay(5);

                    SetPower(false);
                    scaleA = SetGain(gain);
                    scaleB = SetIntegrationTime(integrationTime);
                    SetPower(true);

                    // read data
                    var data = Peripheral.ReadRegisterAsUShort(DataSource == SensorTypes.Ambient ? (byte)Registers.Als : (byte)Registers.White);

                    // manually looking:
                    //Peripheral.ReadRegister((byte)Registers.Als, ReadBuffer.Span[0..2]);
                    //Console.WriteLine($"1: {ReadBuffer.Span[0]}, 2: {ReadBuffer.Span[1]}");
                    //ushort data = (ushort)(ReadBuffer.Span[0] | (ReadBuffer.Span[1] << 8));

                    //Console.WriteLine($"Gain: {gain}, integrationTime: {integrationTime}");

                    if (data < 100) { // Too dark!
                        //Console.WriteLine("Too dark");
                        // increase gain
                        if (++gain > 4) {
                            gain = 4;

                            // increase integration time
                            if (++integrationTime >= 4) {
                                // everything is maxed out, so return the value.
                                //Console.WriteLine("Maxed out.");
                                return(new Illuminance(scaleA * scaleB * 0.0036f * (float)data));
                            } else {
                                //Console.WriteLine("Increasing integration time.");
                                // power down (we're changing config)
                                SetPower(false);
                                scaleB = SetIntegrationTime(integrationTime);
                                SetPower(true);
                            }
                        } else {
                            //Console.WriteLine("Increasing gain.");
                            // power down (we're changing config)
                            SetPower(false);
                            scaleA = SetGain(gain);
                            SetPower(true);
                        }
                    } else if (data > 10000) { // Too bright!
                        //Console.WriteLine("Too bright.");
                        // decrease integration time
                        if (--integrationTime <= -2) {
                            // can't go lower
                            return CalculateCorrectedLux(scaleA * scaleB * 0.0036f * (float)data);
                        } else {
                            //Console.WriteLine("Decreasing integration time.");
                            // power down (we're changing config)
                            SetPower(false);
                            scaleB = SetIntegrationTime(integrationTime);
                            SetPower(true);
                        }
                    } else { // Just right!
                        return (CalculateCorrectedLux(0.0036f * scaleA * scaleB * (float)data));
                    }
                    // give some time for the sensor to accumulate light
                    await Task.Delay(GetDelayTime(integrationTime));
                }
            });
        }

        protected override void RaiseEventsAndNotify(IChangeResult<Illuminance> changeResult)
        {
            LuminosityUpdated?.Invoke(this, changeResult);
            base.RaiseEventsAndNotify(changeResult);
        }

        private Illuminance CalculateCorrectedLux(float lux)
        {
            // per the App Note
            return new Illuminance(6.0135E-13 * Math.Pow(lux, 4) - 9.3924E-09 * Math.Pow(lux, 3) + 8.1488E-05 * Math.Pow(lux, 2) + 1.0023E+00 * lux);
        }

        private void SetPower(bool on)
        {
            ushort cfg;

            if (on)
            {
                cfg = (ushort)(_config & 0xfffe);
            }
            else
            {
                cfg = (ushort)(_config | 0x0001);
            }

            WriteRegister(Registers.AlsConf0, cfg);
            _config = cfg;
        }

        private int SetGain(int gain)
        {
            var scale = 1;
            ushort cfg;

            // bits 11 & 12
            cfg = (ushort)(_config & ~0x1800); // clear bits

            switch (gain)
            {
                case 1: // 1/8
                    cfg |= (0x02 << 11);
                    scale = 8;
                    break;
                case 2: // 1/4
                    cfg |= (0x03 << 11);
                    scale = 4;
                    break;
                case 3: // 1
                    // nothing set
                    break;
                case 4: // 2
                    cfg |= (0x01 << 11);
                    scale = 2;
                    break;
                default:
                    return 1;
            }

            WriteRegister(Registers.AlsConf0, cfg);
            _config = cfg;
            return scale;
        }

        private int SetIntegrationTime(int it)
        {
            ushort cfg;
            var scale = 1;

            // bits 6-9

            cfg = (ushort)(_config & ~0x03C0); // clear bits
            switch (it)
            {
                case -2: // 25ms
                    cfg |= (0b1100 << 6);
                    scale = 32;
                    break;
                case -1: // 50ms
                    cfg |= (0b1000 << 6);
                    scale = 16;
                    break;
                case 0: // 100ms
                    // nothing set
                    scale = 8;
                    break;
                case 1: // 200ms
                    cfg |= (0b0001 << 6);
                    scale = 4;
                    break;
                case 2: // 400ms
                    cfg |= (0b0010 << 6);
                    scale = 2;
                    break;
                case 3: // 800ms
                    cfg |= (0b0011 << 6);
                    scale = 1;
                    break;
                default:
                    return scale;
            }

            WriteRegister(Registers.AlsConf0, cfg);
            _config = cfg;

            return scale;
        }

        private int GetDelayTime(int it)
        {
            var delay = 500; // TODO: seed this based on power saving mode (PSM)
            switch (it)
            {
                case -2: // 25ms
                    delay += 25;
                    break;
                case -1: // 50ms
                    delay += 50;
                    break;
                case 0: // 100ms
                    delay += 100;
                    break;
                case 1: // 200ms
                    delay += 200;
                    break;
                case 2: // 400ms
                    delay += 400;
                    break;
                case 3: // 800ms
                    delay += 800;
                    break;
                default:
                    return delay;
            }

            return delay;
        }

        private void WriteRegister(Registers register, ushort value)
        {
            // VEML registers are LSB|MSB
            lock (samplingLock)
            {
                WriteBuffer.Span[0] = (byte)register;
                WriteBuffer.Span[1] = (byte)(value & 0x00ff);
                WriteBuffer.Span[2] = (byte)((value & 0xff00) >> 8);

                Peripheral.Write(WriteBuffer.Span[0..3]);
            }
        }

    }
}