using Meadow.Hardware;
using Meadow.Units;
using System;
using System.Threading.Tasks;
using HU = Meadow.Units.RelativeHumidity.UnitType;
using PU = Meadow.Units.Pressure.UnitType;
using TU = Meadow.Units.Temperature.UnitType;

namespace Meadow.Foundation.Sensors.Atmospheric;

/// <summary>
/// Shared logic for the BMx280 family of sensors
/// </summary>
partial class Bmx280
{
    /// <summary>
    /// Update the sensor information from the BMx280
    /// </summary>
    internal static Task<(Units.Temperature? Temperature, RelativeHumidity? Humidity, Pressure? Pressure)>
        ReadSensor(IByteCommunications bmx280Comms, Memory<byte> readBuffer, CompensationData compensationData)
    {
        (Units.Temperature? Temperature, RelativeHumidity? Humidity, Pressure? Pressure) conditions;

        bmx280Comms.ReadRegister(0xf7, readBuffer.Span[0..8]);

        var adcTemperature = (readBuffer.Span[3] << 12) | (readBuffer.Span[4] << 4) | ((readBuffer.Span[5] >> 4) & 0x0f);
        var tvar1 = (((adcTemperature >> 3) - (compensationData.T1 << 1)) * compensationData.T2) >> 11;
        var tvar2 = (((((adcTemperature >> 4) - compensationData.T1) *
                       ((adcTemperature >> 4) - compensationData.T1)) >> 12) * compensationData.T3) >> 14;
        var tfine = tvar1 + tvar2;

        conditions.Temperature = new Units.Temperature((float)(((tfine * 5) + 128) >> 8) / 100, TU.Celsius);

        long pvar1 = tfine - 128000;
        var pvar2 = pvar1 * pvar1 * compensationData.P6;
        pvar2 += (pvar1 * compensationData.P5) << 17;
        pvar2 += (long)compensationData.P4 << 35;
        pvar1 = ((pvar1 * pvar1 * compensationData.P8) >> 8) + ((pvar1 * compensationData.P2) << 12);
        pvar1 = ((((long)1 << 47) + pvar1) * compensationData.P1) >> 33;
        if (pvar1 == 0)
        {
            conditions.Pressure = new Pressure(0, PU.Pascal);
        }
        else
        {
            var adcPressure = (readBuffer.Span[0] << 12) | (readBuffer.Span[1] << 4) | ((readBuffer.Span[2] >> 4) & 0x0f);
            long pressure = 1048576 - adcPressure;
            pressure = (((pressure << 31) - pvar2) * 3125) / pvar1;
            pvar1 = (compensationData.P9 * (pressure >> 13) * (pressure >> 13)) >> 25;
            pvar2 = (compensationData.P8 * pressure) >> 19;
            pressure = ((pressure + pvar1 + pvar2) >> 8) + ((long)compensationData.P7 << 4);
            conditions.Pressure = new Pressure((double)pressure / 256, PU.Pascal);
        }

        var adcHumidity = (readBuffer.Span[6] << 8) | readBuffer.Span[7];
        var v_x1_u32r = tfine - 76800;

        v_x1_u32r = ((((adcHumidity << 14) - (compensationData.H4 << 20) - (compensationData.H5 * v_x1_u32r)) +
                      16384) >> 15) *
                    ((((((((v_x1_u32r * compensationData.H6) >> 10) *
                          (((v_x1_u32r * compensationData.H3) >> 11) + 32768)) >> 10) + 2097152) *
                       compensationData.H2) + 8192) >> 14);
        v_x1_u32r = v_x1_u32r - (((((v_x1_u32r >> 15) * (v_x1_u32r >> 15)) >> 7) * compensationData.H1) >> 4);

        v_x1_u32r = v_x1_u32r < 0 ? 0 : v_x1_u32r;
        v_x1_u32r = v_x1_u32r > 419430400 ? 419430400 : v_x1_u32r;

        conditions.Humidity = new RelativeHumidity((v_x1_u32r >> 12) / 1024, HU.Percent);

        return Task.FromResult(conditions);
    }

    /// <summary>
    /// Reads the sensor compensation data
    /// </summary>
    internal static void ReadCompensationData(IByteCommunications bmx280Comms, Memory<byte> readBuffer, ref CompensationData compensationData)
    {
        // read the temperature and pressure data into the internal read buffer
        bmx280Comms.ReadRegister(0x88, readBuffer.Span[0..24]);

        // Temperature
        compensationData.T1 = (ushort)(readBuffer.Span[0] + (readBuffer.Span[1] << 8));
        compensationData.T2 = (short)(readBuffer.Span[2] + (readBuffer.Span[3] << 8));
        compensationData.T3 = (short)(readBuffer.Span[4] + (readBuffer.Span[5] << 8));
        // Pressure
        compensationData.P1 = (ushort)(readBuffer.Span[6] + (readBuffer.Span[7] << 8));
        compensationData.P2 = (short)(readBuffer.Span[8] + (readBuffer.Span[9] << 8));
        compensationData.P3 = (short)(readBuffer.Span[10] + (readBuffer.Span[11] << 8));
        compensationData.P4 = (short)(readBuffer.Span[12] + (readBuffer.Span[13] << 8));
        compensationData.P5 = (short)(readBuffer.Span[14] + (readBuffer.Span[15] << 8));
        compensationData.P6 = (short)(readBuffer.Span[16] + (readBuffer.Span[17] << 8));
        compensationData.P7 = (short)(readBuffer.Span[18] + (readBuffer.Span[19] << 8));
        compensationData.P8 = (short)(readBuffer.Span[20] + (readBuffer.Span[21] << 8));
        compensationData.P9 = (short)(readBuffer.Span[22] + (readBuffer.Span[23] << 8));

        // Humidity - read twice because it's in non-sequential registers
        bmx280Comms.ReadRegister(0xa1, readBuffer.Span[0..1]);
        compensationData.H1 = readBuffer.Span[0];
        bmx280Comms.ReadRegister(0xe1, readBuffer.Span[0..7]);
        compensationData.H2 = (short)(readBuffer.Span[0] + (readBuffer.Span[1] << 8));
        compensationData.H3 = readBuffer.Span[2];
        compensationData.H4 = (short)((readBuffer.Span[3] << 4) + (readBuffer.Span[4] & 0xf));
        compensationData.H5 = (short)(((readBuffer.Span[4] & 0xf) >> 4) + (readBuffer.Span[5] << 4));
        compensationData.H6 = (sbyte)readBuffer.Span[6];
    }
}