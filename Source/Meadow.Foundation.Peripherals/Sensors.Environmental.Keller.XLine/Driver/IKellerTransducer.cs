using Meadow.Units;
using System.Threading.Tasks;

namespace Meadow.Foundation.Sensors.Environmental;

public interface IKellerTransducer
{
    Task<Units.Temperature> ReadTemperature(TemperatureChannel channel);
    Task<Pressure> ReadPressure(PressureChannel channel);
    Task<byte> ReadModbusAddress();
    Task<int> ReadSerialNumber();
}
