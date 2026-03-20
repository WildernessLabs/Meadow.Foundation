using Meadow.Units;
using System;

namespace Meadow.Foundation.Sensors.Environmental.Ysi;

internal static class Converter
{
    public static (ParameterCode, IUnit) Convert(ParameterCode code, float value)
    {
        switch (code)
        {
            case ParameterCode.TemperatureC:
                return (code, new Units.Temperature(value, Units.Temperature.UnitType.Celsius));
            case ParameterCode.TemperatureF:
                return (code, new Units.Temperature(value, Units.Temperature.UnitType.Fahrenheit));
            case ParameterCode.TemperatureK:
                return (code, new Units.Temperature(value, Units.Temperature.UnitType.Kelvin));
            case ParameterCode.ConductivitymScm:
            case ParameterCode.SpecificConductancemScm:
            case ParameterCode.nLFConductivitymScm:
                return (code, new Conductivity(value, Conductivity.UnitType.MilliSiemensPerCentimeter));
            case ParameterCode.ConductivityuScm:
            case ParameterCode.SpecificConductanceuScm:
            case ParameterCode.nLFConductivityuScm:
                return (code, new Conductivity(value, Conductivity.UnitType.MicroSiemensPerCentimeter));
            case ParameterCode.TDSmgL:
            case ParameterCode.NH3:
            case ParameterCode.NH4:
            case ParameterCode.NO3:
            case ParameterCode.Chloride:
            case ParameterCode.TSSmgL:
            case ParameterCode.ODOmgL:
            case ParameterCode.PotassiummgL:
                return (code, new ConcentrationInWater(value, ConcentrationInWater.UnitType.MilligramsPerLiter));
            case ParameterCode.TDSgL:
            case ParameterCode.TSSgL:
                return (code, new ConcentrationInWater(value, ConcentrationInWater.UnitType.GramsPerLiter));
            case ParameterCode.TDSkgL:
                return (code, new ConcentrationInWater(value, ConcentrationInWater.UnitType.KilogramsPerLiter));
            case ParameterCode.ChlorophyllugL:
            case ParameterCode.BGAPCugL:
            case ParameterCode.BGAPEugL:
                return (code, new ConcentrationInWater(value, ConcentrationInWater.UnitType.MicrogramsPerLiter));
            case ParameterCode.Salinity:
                return (code, new ConcentrationInWater(value, ConcentrationInWater.UnitType.PartsPerThousand));
            case ParameterCode.pH:
                return (code, new PotentialHydrogen(value));
            case ParameterCode.pHmV:
            case ParameterCode.ORP:
            case ParameterCode.NH4mV:
            case ParameterCode.NO3mV:
            case ParameterCode.ChlorideMV:
            case ParameterCode.PotassiummV:
                return (code, new Voltage(value, Voltage.UnitType.Millivolts));
            case ParameterCode.PressurePsia:
            case ParameterCode.PressurePsig:
                return (code, new Pressure(value, Pressure.UnitType.Psi));
            case ParameterCode.DepthFeet:
            case ParameterCode.VerticalPositionft:
                return (code, new Length(value, Length.UnitType.Feet));
            case ParameterCode.DepthMeters:
            case ParameterCode.VerticalPositionm:
                return (code, new Length(value, Length.UnitType.Meters));
            case ParameterCode.BatteryVoltage:
            case ParameterCode.WiperPosition:
            case ParameterCode.ExternalPower:
                return (code, new Voltage(value, Voltage.UnitType.Volts));
            case ParameterCode.TurbidityNTU:
                return (code, new Turbidity(value, Turbidity.UnitType.NTU));
            case ParameterCode.WiperPeakCurrent:
                return (code, new Current(value, Current.UnitType.Milliamps));
            case ParameterCode.DateMMDDYY:
                int dateInt = (int)value;

                int month = dateInt / 10000;
                int day = (dateInt % 10000) / 100;
                int year = dateInt % 100;

                int fullYear = year < 100 ? 2000 + year : year;
                var dt = new DateTime(fullYear, month, day);
                return (code, TimePeriod.FromFileTime(dt.ToFileTime()));
            case ParameterCode.DateDDMMYY:
                int dateInt2 = (int)value;

                int day2 = dateInt2 / 10000;
                int month2 = (dateInt2 % 10000) / 100;
                int year2 = dateInt2 % 100;

                int fullYear2 = year2 < 100 ? 2000 + year2 : year2;
                var dt2 = new DateTime(fullYear2, month2, day2);
                return (code, TimePeriod.FromFileTime(dt2.ToFileTime()));
            case ParameterCode.DateYYMMDD:
                int dateInt3 = (int)value;

                int year3 = dateInt3 / 10000;
                int month3 = (dateInt3 % 10000) / 100;
                int day3 = dateInt3 % 100;

                int fullYear3 = year3 < 100 ? 2000 + year3 : year3;
                var dt3 = new DateTime(fullYear3, month3, day3);
                return (code, TimePeriod.FromFileTime(dt3.ToFileTime()));
            case ParameterCode.TimeHHMMSS:
                int timeInt = (int)value;

                int hour = timeInt / 10000;
                int minute = (timeInt % 10000) / 100;
                int sec = timeInt % 100;
                var ts = new TimeSpan(0, hour, minute, sec);
                return (code, new TimePeriod(ts.TotalSeconds));
            default:
                return (code, new Scalar(value));
        }
    }
}
