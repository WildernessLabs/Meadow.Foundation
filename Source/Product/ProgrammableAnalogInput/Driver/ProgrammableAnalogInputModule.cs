using Meadow.Foundation.ICs.ADC;
using Meadow.Foundation.ICs.IOExpanders;
using Meadow.Hardware;
using Meadow.Units;
using System;

namespace Meadow.Foundation;

public partial class ProgrammableAnalogInputModule : ProgrammableAnalogInputModuleBase
{
    public int ChannelCount { get; } = 8;

    private readonly Resistance NtcFixedResistor = 10_000.Ohms();

    private readonly Ads7128 adc;
    private readonly Tca95x5 tca1;
    private readonly Tca95x5 tca2;
    private readonly IDigitalOutputPort[] configBits;
    private readonly IAnalogInputPort[] analogInputs;

    public ProgrammableAnalogInputModule(
        II2cBus bus,
        byte adcAddress,
        byte gpio1Address,
        byte gpio2Address)
        : base(8)
    {
        configBits = new IDigitalOutputPort[ChannelCount * 4];
        analogInputs = new IAnalogInputPort[ChannelCount];

        try
        {
            tca1 = new Tca9535(bus, gpio1Address);
            tca2 = new Tca9535(bus, gpio2Address);
            adc = new Ads7128(
                bus,
                (Ads7128.Addresses)adcAddress);
        }
        catch (Exception ex)
        {
            Resolver.Log.Error("Failed to initialize module ICs", "ProgrammableAnalogInputModule");
            return;
        }

        // configure all as 0-10V inputs
        try
        {
            Resolver.Log.Debug($"Configuring TCA9535-1 at 0x{gpio1Address:X2}...", "ProgrammableAnalogInputModule");
            Resolver.Log.Trace($" P00", "ProgrammableAnalogInputModule");
            configBits[0] = tca1.CreateDigitalOutputPort(tca1.Pins.P00, false);
            Resolver.Log.Trace($" P01", "ProgrammableAnalogInputModule");
            configBits[1] = tca1.CreateDigitalOutputPort(tca1.Pins.P01, false);
            Resolver.Log.Trace($" P02", "ProgrammableAnalogInputModule");
            configBits[2] = tca1.CreateDigitalOutputPort(tca1.Pins.P02, true);
            Resolver.Log.Trace($" P03", "ProgrammableAnalogInputModule");
            configBits[3] = tca1.CreateDigitalOutputPort(tca1.Pins.P03, false);

            Resolver.Log.Trace($" P04", "ProgrammableAnalogInputModule");
            configBits[4] = tca1.CreateDigitalOutputPort(tca1.Pins.P04, true);
            Resolver.Log.Trace($" P05", "ProgrammableAnalogInputModule");
            configBits[5] = tca1.CreateDigitalOutputPort(tca1.Pins.P05, false);
            Resolver.Log.Trace($" P06", "ProgrammableAnalogInputModule");
            configBits[6] = tca1.CreateDigitalOutputPort(tca1.Pins.P06, true);
            Resolver.Log.Trace($" P07", "ProgrammableAnalogInputModule");
            configBits[7] = tca1.CreateDigitalOutputPort(tca1.Pins.P07, false);

            Resolver.Log.Trace($" P10", "ProgrammableAnalogInputModule");
            configBits[8] = tca1.CreateDigitalOutputPort(tca1.Pins.P10, false);
            Resolver.Log.Trace($" P11", "ProgrammableAnalogInputModule");
            configBits[9] = tca1.CreateDigitalOutputPort(tca1.Pins.P11, false);
            Resolver.Log.Trace($" P12", "ProgrammableAnalogInputModule");
            configBits[10] = tca1.CreateDigitalOutputPort(tca1.Pins.P12, true);
            Resolver.Log.Trace($" P13", "ProgrammableAnalogInputModule");
            configBits[11] = tca1.CreateDigitalOutputPort(tca1.Pins.P13, false);

            Resolver.Log.Trace($" P14", "ProgrammableAnalogInputModule");
            configBits[12] = tca1.CreateDigitalOutputPort(tca1.Pins.P14, false);
            Resolver.Log.Trace($" P15", "ProgrammableAnalogInputModule");
            configBits[13] = tca1.CreateDigitalOutputPort(tca1.Pins.P15, false);
            Resolver.Log.Trace($" P16", "ProgrammableAnalogInputModule");
            configBits[14] = tca1.CreateDigitalOutputPort(tca1.Pins.P16, true);
            Resolver.Log.Trace($" P17", "ProgrammableAnalogInputModule");
            configBits[15] = tca1.CreateDigitalOutputPort(tca1.Pins.P17, false);
        }
        catch (Exception ex)
        {
            Resolver.Log.Error($"Failed to configure TCA9535-1: {ex.Message}", "ProgrammableAnalogInputModule");
        }

        try
        {
            Resolver.Log.Debug($"Configuring TCA9535-2 at 0x{gpio2Address:X2}...", "ProgrammableAnalogInputModule");
            Resolver.Log.Trace($" P00", "ProgrammableAnalogInputModule");
            configBits[16] = tca2.CreateDigitalOutputPort(tca2.Pins.P00, true);
            Resolver.Log.Trace($" P01", "ProgrammableAnalogInputModule");
            configBits[17] = tca2.CreateDigitalOutputPort(tca2.Pins.P01, true);
            Resolver.Log.Trace($" P02", "ProgrammableAnalogInputModule");
            configBits[18] = tca2.CreateDigitalOutputPort(tca2.Pins.P02, true);
            Resolver.Log.Trace($" P03", "ProgrammableAnalogInputModule");
            configBits[19] = tca2.CreateDigitalOutputPort(tca2.Pins.P03, false);

            Resolver.Log.Trace($" P04", "ProgrammableAnalogInputModule");
            configBits[20] = tca2.CreateDigitalOutputPort(tca2.Pins.P04, false);
            Resolver.Log.Trace($" P05", "ProgrammableAnalogInputModule");
            configBits[21] = tca2.CreateDigitalOutputPort(tca2.Pins.P05, false);
            Resolver.Log.Trace($" P06", "ProgrammableAnalogInputModule");
            configBits[22] = tca2.CreateDigitalOutputPort(tca2.Pins.P06, true);
            Resolver.Log.Trace($" P07", "ProgrammableAnalogInputModule");
            configBits[23] = tca2.CreateDigitalOutputPort(tca2.Pins.P07, false);

            Resolver.Log.Trace($" P10", "ProgrammableAnalogInputModule");
            configBits[24] = tca2.CreateDigitalOutputPort(tca2.Pins.P10, false);
            Resolver.Log.Trace($" P11", "ProgrammableAnalogInputModule");
            configBits[25] = tca2.CreateDigitalOutputPort(tca2.Pins.P11, false);
            Resolver.Log.Trace($" P12", "ProgrammableAnalogInputModule");
            configBits[26] = tca2.CreateDigitalOutputPort(tca2.Pins.P12, true);
            Resolver.Log.Trace($" P13", "ProgrammableAnalogInputModule");
            configBits[27] = tca2.CreateDigitalOutputPort(tca2.Pins.P13, false);

            configBits[28] = tca2.CreateDigitalOutputPort(tca2.Pins.P14, false);
            configBits[29] = tca2.CreateDigitalOutputPort(tca2.Pins.P15, false);
            configBits[30] = tca2.CreateDigitalOutputPort(tca2.Pins.P16, true);
            configBits[31] = tca2.CreateDigitalOutputPort(tca2.Pins.P17, false);
        }
        catch (Exception ex)
        {
            Resolver.Log.Error($"Failed to configure TCA9535-2: {ex.Message}", "ProgrammableAnalogInputModule");
        }

        for (var i = 0; i < channelConfigs.Length; i++)
        {
            channelConfigs[i] = new ChannelConfig
            {
                ChannelType = ConfigurableAnalogInputChannelType.Voltage_0_10,
                UnitType = "Unknown"
            };
        }

        Resolver.Log.Debug($"Configuring ADS7128 at 0x{adcAddress:X2}...", "ProgrammableAnalogInputModule");
        analogInputs[0] = adc.CreateAnalogInputPort(adc.Pins.AIN0);
        analogInputs[1] = adc.CreateAnalogInputPort(adc.Pins.AIN1);
        analogInputs[2] = adc.CreateAnalogInputPort(adc.Pins.AIN2);
        analogInputs[3] = adc.CreateAnalogInputPort(adc.Pins.AIN3);
        analogInputs[4] = adc.CreateAnalogInputPort(adc.Pins.AIN4);
        analogInputs[5] = adc.CreateAnalogInputPort(adc.Pins.AIN5);
        analogInputs[6] = adc.CreateAnalogInputPort(adc.Pins.AIN6);
        analogInputs[7] = adc.CreateAnalogInputPort(adc.Pins.AIN7);

        Resolver.Log.Debug($"Hardware configuration complete.", "ProgrammableAnalogInputModule");
    }

    /*
    Switch truth table
    +-------------+--------+--------+--------+--------+
    | MODE:       | SW.CH1 | SW.CH2 | SW.CH3 | SW.CH4 |
    +-------------+--------+--------+--------+--------+
    | 0-10V       | LOW    | LOW    | HIGH   | LOW    |
    +-------------+--------+--------+--------+--------+
    | 0-20mA      | LOW    | HIGH   | LOW    | HIGH   |
    +-------------+--------+--------+--------+--------+
    | NTC         | HIGH   | HIGH   | LOW    | LOW    |
    +-------------+--------+--------+--------+--------+    
    */
    public override void ConfigureChannel(ChannelConfig channelConfiguration)
    {
        base.ConfigureChannel(channelConfiguration);

        var offset = channelConfiguration.ChannelNumber * 4;

        switch (channelConfiguration.ChannelType)
        {
            case ConfigurableAnalogInputChannelType.Voltage_0_10:
                configBits[offset + 0].State = false;
                configBits[offset + 1].State = false;
                configBits[offset + 2].State = true;
                configBits[offset + 3].State = false;
                break;
            case ConfigurableAnalogInputChannelType.Current_0_20:
            case ConfigurableAnalogInputChannelType.Current_4_20:
                configBits[offset + 0].State = false;
                configBits[offset + 1].State = true;
                configBits[offset + 2].State = false;
                configBits[offset + 3].State = true;
                break;
            case ConfigurableAnalogInputChannelType.ThermistorNtc:
                configBits[offset + 0].State = true;
                configBits[offset + 1].State = true;
                configBits[offset + 2].State = false;
                configBits[offset + 3].State = false;
                break;
        }
    }

    public override Voltage ReadChannelRaw(int channelNumber)
    {
        if (channelNumber < 0 || channelNumber > ChannelCount - 1)
        {
            throw new ArgumentException("Invalid channelNumber");
        }

        return analogInputs[channelNumber].Read().GetAwaiter().GetResult();
    }

    public override Voltage Read0_10V(int channelNumber)
    {
        if (channelConfigs[channelNumber].ChannelType != ConfigurableAnalogInputChannelType.Voltage_0_10)
        {
            throw new Exception("Channel is not configured for 0-10V input");
        }

        var raw = analogInputs[channelNumber].Read().GetAwaiter().GetResult();

        Resolver.Log.Info($"ADC: {raw.Volts:N2} V");

        return new Voltage((raw.Volts / adc.ReferenceVoltage.Volts) * 10, Voltage.UnitType.Volts);
    }

    private static readonly Voltage AdcVoltageAt20Ma = 2.59.Volts();

    public override Current Read0_20mA(int channelNumber)
    {
        if (channelConfigs[channelNumber].ChannelType != ConfigurableAnalogInputChannelType.Current_4_20)
        {
            throw new Exception("Channel is not configured for 4-20mA input");
        }

        var raw = analogInputs[channelNumber].Read().GetAwaiter().GetResult();

        return new Current((raw.Volts / AdcVoltageAt20Ma.Volts) * 20, Current.UnitType.Milliamps);
        //return new Current((raw.Volts / adc.ReferenceVoltage.Volts) * 20, Current.UnitType.Milliamps);
    }

    public override Current Read4_20mA(int channelNumber)
    {
        return Read0_20mA(channelNumber);
    }

    public override Temperature ReadNtc(int channelNumber, double beta, Temperature referenceTemperature, Resistance resistanceAtRefTemp)
    {
        if (channelConfigs[channelNumber].ChannelType != ConfigurableAnalogInputChannelType.ThermistorNtc)
        {
            throw new Exception("Channel is not configured for NTC thermistor input");
        }

        var raw = analogInputs[channelNumber].Read().GetAwaiter().GetResult();

        Resolver.Log.Info($"ADC: {raw.Volts:N2} V");

        if (raw >= adc.ReferenceVoltage)
        {
            throw new Exception("ADC is saturated");
        }

        //var resistance = NtcFixedResistor.Ohms * raw.Volts / (adc.ReferenceVoltage.Volts - raw.Volts);
        var resistance = NtcFixedResistor.Ohms * raw.Volts / (12 - raw.Volts);

        // Using simplified B-parameter equation (derived from Steinhart-Hart)
        // 1/T = 1/T0 + (1/B) * ln(R/R0)
        double steinhart = 1.0 / referenceTemperature.Kelvin + (1.0 / beta) * Math.Log(resistance / resistanceAtRefTemp.Ohms);

        return new Temperature(1.0 / steinhart, Temperature.UnitType.Kelvin);
    }
}