using System;
using System.Buffers;
using System.Threading;
using Meadow.Hardware;
using Meadow.Utilities;

namespace Meadow.Foundation.Sensors.Atmospheric
{
    public partial class Bme680
    {
        public class SensorReading
        {
            private SensorReading(double pressure, double temperature, double humidity, double voc, SensorSettings settings)
            {
                switch (settings.PressureUnit) {
                    case PressureUnit.Pa:
                        Pressure = pressure;
                        break;
                    case PressureUnit.Psia:
                        Pressure = pressure / 6894.75728;
                        break;
                    case PressureUnit.Atm:
                        Pressure = pressure / 101325;
                        break;
                }

                switch (settings.TemperatureUnit) {
                    case TemperatureUnit.C:
                        Temperature = temperature;
                        break;
                    case TemperatureUnit.F:
                        Temperature = (temperature * (9D / 5D)) + 32;
                        break;
                }

                Humidity = humidity;
                VOC = voc;
            }

            public double Temperature { get; }
            public double Pressure { get; }
            public double Humidity { get; }
            public double VOC { get; }

            public override string ToString()
            {
                return $"Temperature: {Temperature}, Pressure: {Pressure}, Humidity: {Humidity}, VOC: {VOC}";
            }

            public static SensorReading CreateFromDevice(I2cPeripheral device, SensorSettings sensorSettings)
            {
                // Read the current control register
                var status = device.ReadRegister(RegisterAddresses.ControlTemperatureAndPressure.Address);
                // Force a sample
                status = BitHelpers.SetBit(status, 0x00, true);
                device.WriteRegister(RegisterAddresses.ControlTemperatureAndPressure.Address, status);
                // Wait for the sample to be taken.
                do {
                    status = device.ReadRegister(RegisterAddresses.ControlTemperatureAndPressure.Address);
                } while (BitHelpers.GetBitValue(status, 0x00));

                var sensorData = device.ReadRegisters(
                    RegisterAddresses.AllSensors.Address,
                    RegisterAddresses.AllSensors.Length)
                    .AsSpan();

                var rawPressure = GetRawValue(sensorData.Slice(0, 3));
                var rawTemperature = GetRawValue(sensorData.Slice(3, 3));
                var rawHumidity = GetRawValue(sensorData.Slice(6, 2));
                //var rawVoc = GetRawValue(sensorData.Slice(8, 2));
                var compensationData1 = device.ReadRegisters(RegisterAddresses.CompensationData1.Address,
                    RegisterAddresses.CompensationData1.Length);
                var compensationData2 = device.ReadRegisters(RegisterAddresses.CompensationData2.Address,
                    RegisterAddresses.CompensationData2.Length);
                var compensationData = ArrayPool<byte>.Shared.Rent(64);
                try {
                    Array.Copy(compensationData1, 0, compensationData, 0, compensationData1.Length);
                    Array.Copy(compensationData2, 0, compensationData, 25, compensationData2.Length);

                    var temp = RawToTemp(rawTemperature,
                        new TemperatureCompensation(compensationData));

                    var pressure = RawToPressure(temp, rawPressure,
                        new PressureCompensation(compensationData));
                    var humidity = RawToHumidity(temp, rawHumidity,
                        new HumidityCompensation(compensationData));

                    return new SensorReading(pressure, temp, humidity, 0, sensorSettings);
                } finally {
                    ArrayPool<byte>.Shared.Return(compensationData, true);
                }
            }

            private static int GetRawValue(Span<byte> data)
            {
                if (data.Length == 3) {
                    return (data[0] << 12) | (data[1] << 4) | ((data[2] >> 4) & 0x0f);
                }
                if (data.Length == 2) {
                    return (data[0] << 8) | data[1];
                }
                return 0;
            }

            private static double RawToTemp(int adcTemperature, TemperatureCompensation temperatureCompensation)
            {
                var var1 = ((adcTemperature / 16384.0M) - (temperatureCompensation.T1 / 1024.0M)) *
                           temperatureCompensation.T2;
                var var2 = ((adcTemperature / 131072M) - (temperatureCompensation.T1 / 8192.0M));
                var var3 = var2 * ((adcTemperature / 131072.0M) - (temperatureCompensation.T1 / 8192.0M));
                var var4 = var3 * (temperatureCompensation.T3 * 16.0M);
                var tFine = var1 + var4;
                return Convert.ToDouble(tFine / 5120.0M);
            }

            private static double RawToPressure(double temp, int adcPressure, PressureCompensation pressureCompensation)
            {
                var tFine = temp * 5120;
                var var1 = (tFine / 2.0) - 64000.0;
                var var2 = var1 * var1 * (pressureCompensation.P6 / 131072.0);
                var2 += (var1 * pressureCompensation.P5 * 2.0);
                var2 = (var2 / 4.0) + (pressureCompensation.P4 * 65536.0);
                var1 = (((pressureCompensation.P3 * var1 * var1) / 16384.0) +
                        (pressureCompensation.P2 * var1)) / 524288.0;
                var1 = (1.0 + (var1 / 32768.0)) * pressureCompensation.P1;
                var calcPress = 1048576.0 - adcPressure;
                calcPress = ((calcPress - (var2 / 4096.0)) * 6250.0) / var1;
                var1 = (pressureCompensation.P9 * calcPress * calcPress) / 2147483648.0;
                var2 = calcPress * (pressureCompensation.P8 / 32768.0);
                var var3 = (calcPress / 256.0) * (calcPress / 256.0) * (calcPress / 256.0) *
                           (pressureCompensation.P10 / 131072.0);
                calcPress += (var1 + var2 + var3 + (pressureCompensation.P7 * 128.0)) / 16.0;
                return calcPress;
            }

            private static double RawToHumidity(double temp, int adcHumidity, HumidityCompensation humidityCompensation)
            {
                var var1 = adcHumidity - ((humidityCompensation.H1 * 16.0) + ((humidityCompensation.H3 / 2.0) * temp));
                var var2 = var1 * ((humidityCompensation.H2 / 262144.0) * (1.0 + ((humidityCompensation.H4 / 16384.0) * temp) + ((humidityCompensation.H5 / 1048576.0) * temp * temp)));
                var var3 = humidityCompensation.H6 / 16384.0;
                var var4 = humidityCompensation.H7 / 2097152.0;
                return var2 + ((var3 + (var4 * temp)) * var2 * var2);
            }
        }
    }
}