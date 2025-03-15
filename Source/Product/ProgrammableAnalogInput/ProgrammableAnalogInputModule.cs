using Meadow.Foundation.ICs.ADC;
using Meadow.Foundation.ICs.IOExpanders;
using Meadow.Hardware;
using Meadow.Units;
using System;

namespace Meadow.Foundation;

public partial class ProgrammableAnalogInputModule
{
    public const int ChannelCount = 8;
    private readonly Resistance NtcFixedResistor = 10_000.Ohms();

    public enum ChannelType
    {
        Voltage_0_10,
        Current_4_20,
        ThermistorNtc
    }

    private readonly Ads7128 adc;
    private readonly Pcf8575 pcf1;
    private readonly Pcf8575 pcf2;
    private readonly ChannelType[] channelConfigs = new ChannelType[ChannelCount];
    private readonly IDigitalOutputPort[] configBits = new IDigitalOutputPort[ChannelCount * 4];
    private readonly IAnalogInputPort[] analogInputs = new IAnalogInputPort[ChannelCount];

    public ProgrammableAnalogInputModule(
        II2cBus bus,
        byte adcAddress,
        byte gpio1Address,
        byte gpio2Address)
    {
        pcf1 = new Pcf8575(bus, gpio1Address);
        pcf2 = new Pcf8575(bus, gpio2Address);
        adc = new Ads7128(
            bus,
            (Ads7128.Addresses)adcAddress);

        // configure all as 0-10V inputs
        configBits[0] = pcf1.CreateDigitalOutputPort(pcf1.Pins.P00, false);
        configBits[1] = pcf1.CreateDigitalOutputPort(pcf1.Pins.P01, false);
        configBits[2] = pcf1.CreateDigitalOutputPort(pcf1.Pins.P02, true);
        configBits[3] = pcf1.CreateDigitalOutputPort(pcf1.Pins.P03, false);

        configBits[4] = pcf1.CreateDigitalOutputPort(pcf1.Pins.P04, false);
        configBits[5] = pcf1.CreateDigitalOutputPort(pcf1.Pins.P05, false);
        configBits[6] = pcf1.CreateDigitalOutputPort(pcf1.Pins.P06, true);
        configBits[7] = pcf1.CreateDigitalOutputPort(pcf1.Pins.P07, false);

        configBits[8] = pcf1.CreateDigitalOutputPort(pcf1.Pins.P10, false);
        configBits[9] = pcf1.CreateDigitalOutputPort(pcf1.Pins.P11, false);
        configBits[10] = pcf1.CreateDigitalOutputPort(pcf1.Pins.P12, true);
        configBits[11] = pcf1.CreateDigitalOutputPort(pcf1.Pins.P13, false);

        configBits[12] = pcf1.CreateDigitalOutputPort(pcf1.Pins.P14, false);
        configBits[13] = pcf1.CreateDigitalOutputPort(pcf1.Pins.P15, false);
        configBits[14] = pcf1.CreateDigitalOutputPort(pcf1.Pins.P16, true);
        configBits[15] = pcf1.CreateDigitalOutputPort(pcf1.Pins.P17, false);

        configBits[16] = pcf2.CreateDigitalOutputPort(pcf2.Pins.P00, false);
        configBits[17] = pcf2.CreateDigitalOutputPort(pcf2.Pins.P01, false);
        configBits[18] = pcf2.CreateDigitalOutputPort(pcf2.Pins.P02, true);
        configBits[19] = pcf2.CreateDigitalOutputPort(pcf2.Pins.P03, false);

        configBits[20] = pcf2.CreateDigitalOutputPort(pcf2.Pins.P04, false);
        configBits[21] = pcf2.CreateDigitalOutputPort(pcf2.Pins.P05, false);
        configBits[22] = pcf2.CreateDigitalOutputPort(pcf2.Pins.P06, true);
        configBits[23] = pcf2.CreateDigitalOutputPort(pcf2.Pins.P07, false);

        configBits[24] = pcf2.CreateDigitalOutputPort(pcf2.Pins.P10, false);
        configBits[25] = pcf2.CreateDigitalOutputPort(pcf2.Pins.P11, false);
        configBits[26] = pcf2.CreateDigitalOutputPort(pcf2.Pins.P12, true);
        configBits[27] = pcf2.CreateDigitalOutputPort(pcf2.Pins.P13, false);

        configBits[28] = pcf2.CreateDigitalOutputPort(pcf2.Pins.P14, false);
        configBits[29] = pcf2.CreateDigitalOutputPort(pcf2.Pins.P15, false);
        configBits[30] = pcf2.CreateDigitalOutputPort(pcf2.Pins.P16, true);
        configBits[31] = pcf2.CreateDigitalOutputPort(pcf2.Pins.P17, false);

        for (var i = 0; i < channelConfigs.Length; i++)
        {
            channelConfigs[i] = ChannelType.Voltage_0_10;
        }

        Resolver.Log.Info($"Analogs...");
        analogInputs[0] = adc.CreateAnalogInputPort(adc.Pins.AIN0);
        Resolver.Log.Info($"Analogs2...");
        analogInputs[1] = adc.CreateAnalogInputPort(adc.Pins.AIN1);
        analogInputs[2] = adc.CreateAnalogInputPort(adc.Pins.AIN2);
        analogInputs[3] = adc.CreateAnalogInputPort(adc.Pins.AIN3);
        analogInputs[4] = adc.CreateAnalogInputPort(adc.Pins.AIN4);
        analogInputs[5] = adc.CreateAnalogInputPort(adc.Pins.AIN5);
        analogInputs[6] = adc.CreateAnalogInputPort(adc.Pins.AIN6);
        analogInputs[7] = adc.CreateAnalogInputPort(adc.Pins.AIN7);
    }

    /*
    Switch truth table
    +-------------+--------+--------+--------+--------+
    | MODE:       | SW.CH1 | SW.CH2 | SW.CH3 | SW.CH4 |
    +-------------+--------+--------+--------+--------+
    | 0-10V       | LOW    | LOW    | HIGH   | LOW    |
    +-------------+--------+--------+--------+--------+
    | 0-20mA      | LOW    | HIGH   | HIGH   | HIGH   |
    +-------------+--------+--------+--------+--------+
    | NTC         | HIGH   | HIGH   | LOW    | LOW    |
    +-------------+--------+--------+--------+--------+    
    */
    public void ConfigureChannel(int channelNumber, ChannelType channelType)
    {
        if (channelNumber < 0 || channelNumber > ChannelCount - 1)
        {
            throw new ArgumentException("Invalid channelNumber");
        }

        channelConfigs[channelNumber] = channelType;

        var offset = channelNumber * 4;

        switch (channelType)
        {
            case ChannelType.Voltage_0_10:
                configBits[offset + 0].State = false;
                configBits[offset + 1].State = false;
                configBits[offset + 2].State = true;
                configBits[offset + 3].State = false;
                break;
            case ChannelType.Current_4_20:
                configBits[offset + 0].State = false;
                configBits[offset + 1].State = true;
                configBits[offset + 2].State = true;
                configBits[offset + 3].State = true;
                break;
            case ChannelType.ThermistorNtc:
                configBits[offset + 0].State = true;
                configBits[offset + 1].State = true;
                configBits[offset + 2].State = false;
                configBits[offset + 3].State = false;
                break;
        }
    }

    public Voltage ReadChannelRaw(int channelNumber)
    {
        if (channelNumber < 0 || channelNumber > ChannelCount - 1)
        {
            throw new ArgumentException("Invalid channelNumber");
        }

        return analogInputs[channelNumber].Read().GetAwaiter().GetResult();
    }

    public Voltage Read0_10V(int channelNumber)
    {
        if (channelConfigs[channelNumber] != ChannelType.Voltage_0_10)
        {
            throw new Exception("Channel is not configured for 0-10V input");
        }

        var raw = analogInputs[channelNumber].Read().GetAwaiter().GetResult();

        Resolver.Log.Info($"RAW: {raw.Volts}  REF: {adc.ReferenceVoltage.Volts}");

        return new Voltage((raw.Volts / adc.ReferenceVoltage.Volts) * 10, Voltage.UnitType.Volts);
    }

    public Current Read4_20mA(int channelNumber)
    {
        if (channelConfigs[channelNumber] != ChannelType.Current_4_20)
        {
            throw new Exception("Channel is not configured for 4-20mA input");
        }

        var raw = analogInputs[channelNumber].Read().GetAwaiter().GetResult();

        return new Current((raw.Volts / adc.ReferenceVoltage.Volts) * 20, Current.UnitType.Milliamps);
    }

    public Temperature ReadNtc(int channelNumber)
    {
        return ReadNtc(channelNumber, 3950, new Temperature(25, Temperature.UnitType.Celsius), new Resistance(10_000, Resistance.UnitType.Ohms));
    }

    public Temperature ReadNtc(int channelNumber, double beta, Temperature referenceTemperature, Resistance resistanceAtRefTemp)
    {
        if (channelConfigs[channelNumber] != ChannelType.ThermistorNtc)
        {
            throw new Exception("Channel is not configured for NTC thermistor input");
        }

        var raw = analogInputs[channelNumber].Read().GetAwaiter().GetResult();

        if (raw >= adc.ReferenceVoltage)
        {
            throw new Exception("ADC is saturated");
        }

        var resistance = NtcFixedResistor.Ohms * raw.Volts / (adc.ReferenceVoltage.Volts - raw.Volts);

        // Using simplified B-parameter equation (derived from Steinhart-Hart)
        // 1/T = 1/T0 + (1/B) * ln(R/R0)
        double steinhart = 1.0 / referenceTemperature.Kelvin + (1.0 / beta) * Math.Log(resistance / resistanceAtRefTemp.Ohms);

        return new Temperature(1.0 / steinhart, Temperature.UnitType.Kelvin);
    }
}