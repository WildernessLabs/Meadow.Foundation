using Meadow;
using Meadow.Devices;
using Meadow.Foundation.VFDs;
using Meadow.Modbus;
using System.Threading.Tasks;

namespace Motors.CerusXDrive_Sample;

public class MeadowApp : App<F7FeatherV2>
{
    //<!=SNIP=>
    public async override Task Run()
    {
        var port = new SerialPortShim("COM12", 9600, Meadow.Hardware.Parity.None, 8, Meadow.Hardware.StopBits.One);
        var modbus = new ModbusRtuClient(port);
        var vfd = new CerusXDrive(modbus, 1);
        await vfd.Connect();

        var v = await vfd.ReadOutputVoltage();
        var i = await vfd.ReadOutputCurrent();
        var f = await vfd.ReadOutputFrequency();
        var dcb = await vfd.ReadDCBusVoltage();

        var o = await vfd.ReadOperationalStatus();
        var e = await vfd.ReadErrorCodes();

        var t1 = await vfd.ReadIGBTTemperature();
        var t2 = await vfd.ReadAmbientTemperature();

        var s = await vfd.ReadDriveStatus();
        var cm = await vfd.ReadControlMode();

        var dis = await vfd.ReadDigitalInputStatus();
        var dos = await vfd.ReadDigitalOutputStatus();
    }
}