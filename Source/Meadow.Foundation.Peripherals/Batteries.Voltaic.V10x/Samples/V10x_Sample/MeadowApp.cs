using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Batteries.Voltaic;
using Meadow.Hardware;
using Meadow.Modbus;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Batteries.Voltaic.V10x_Sample
{
    public class MeadowApp : App<F7FeatherV2>
    {
        //<!=SNIP=>

        public override async Task Run()
        {
            Resolver.Log.Info("Run...");

            using (var port = new SerialPortShim("COM5", V10x.DefaultBaudRate, Parity.None, 8, StopBits.One))
            {
                port.ReadTimeout = TimeSpan.FromSeconds(15);
                port.Open();

                var client = new ModbusRtuClient(port);
                var controller = new V10x(client);

                controller.CommTimeout += (s, e) => Debug.WriteLine("Read Timeout");
                controller.CommError += (s, e) => Debug.WriteLine($"Error: {e.Message}");

                controller.StartPolling();

                var i = 0;

                while (true)
                {
                    await Task.Delay(2000);
                    Debug.WriteLine($"---------------");
                    Debug.WriteLine($"Battery voltage: {controller.BatteryVoltage.Volts:N2} V");
                    Debug.WriteLine($"Input voltage:   {controller.InputVoltage.Volts:N2} V");
                    Debug.WriteLine($"Input current:   {controller.InputCurrent.Amps:N2} A");
                    Debug.WriteLine($"Load voltage:    {controller.LoadVoltage.Volts:N2} V");
                    Debug.WriteLine($"Load current:    {controller.LoadCurrent.Amps:N2} A");
                    Debug.WriteLine($"Environ temp:    {controller.EnvironmentTemp.Fahrenheit:N2} F");
                    Debug.WriteLine($"Controller temp: {controller.ControllerTemp.Fahrenheit:N2} F");

                    controller.BatteryOutput = (i++ % 2 == 0);
                }
            }
        }

        //<!=SNOP=>
    }
}