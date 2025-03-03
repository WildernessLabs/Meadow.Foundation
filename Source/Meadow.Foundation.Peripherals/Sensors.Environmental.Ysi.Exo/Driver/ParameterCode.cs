namespace Meadow.Foundation.Sensors.Environmental.Ysi;

/// <summary>
/// Parameter codes for YSI EXO sonde measurements.
/// These codes are used to identify different types of sensor readings
/// transmitted by YSI EXO multiparameter water quality sondes.
/// </summary>
public enum ParameterCode
{
    /// <summary>
    /// Temperature in degrees Celsius.
    /// </summary>
    TemperatureC = 1,

    /// <summary>
    /// Temperature in degrees Fahrenheit.
    /// </summary>
    TemperatureF = 2,

    /// <summary>
    /// Temperature in degrees Kelvin.
    /// </summary>
    TemperatureK = 3,

    /// <summary>
    /// Conductivity in millisiemens per centimeter (mS/cm).
    /// </summary>
    ConductivitymScm = 4,

    /// <summary>
    /// Conductivity in microsiemens per centimeter (μS/cm).
    /// </summary>
    ConductivityuScm = 5,

    /// <summary>
    /// Specific conductance in millisiemens per centimeter (mS/cm).
    /// Temperature-compensated conductivity.
    /// </summary>
    SpecificConductancemScm = 6,

    /// <summary>
    /// Specific conductance in microsiemens per centimeter (μS/cm).
    /// Temperature-compensated conductivity.
    /// </summary>
    SpecificConductanceuScm = 7,

    /// <summary>
    /// Total dissolved solids in grams per liter (g/L).
    /// </summary>
    TDSgL = 10,

    /// <summary>
    /// Salinity in Practical Salinity Units (PSU or PPT).
    /// </summary>
    Salinity = 12,

    /// <summary>
    /// pH millivolts (mV).
    /// </summary>
    pHmV = 17,

    /// <summary>
    /// pH in standard units.
    /// </summary>
    pH = 18,

    /// <summary>
    /// Oxidation-reduction potential in millivolts (mV).
    /// </summary>
    ORP = 19,

    /// <summary>
    /// Pressure in pounds per square inch absolute (psia).
    /// </summary>
    PressurePsia = 20,

    /// <summary>
    /// Pressure in pounds per square inch gauge (psig).
    /// </summary>
    PressurePsig = 21,

    /// <summary>
    /// Depth in meters (m).
    /// </summary>
    DepthMeters = 22,

    /// <summary>
    /// Depth in feet (ft).
    /// </summary>
    DepthFeet = 23,

    /// <summary>
    /// Battery voltage (V).
    /// </summary>
    BatteryVoltage = 28,

    /// <summary>
    /// Turbidity in Nephelometric Turbidity Units (NTU).
    /// </summary>
    TurbidityNTU = 37,

    /// <summary>
    /// Ammonia in milligrams per liter (mg/L).
    /// </summary>
    NH3 = 47,

    /// <summary>
    /// Ammonium in milligrams per liter (mg/L).
    /// </summary>
    NH4 = 48,

    /// <summary>
    /// Date in DDMMYY format.
    /// </summary>
    DateDDMMYY = 51,

    /// <summary>
    /// Date in MMDDYY format.
    /// </summary>
    DateMMDDYY = 52,

    /// <summary>
    /// Date in YYMMDD format.
    /// </summary>
    DateYYMMDD = 53,

    /// <summary>
    /// Time in HHMMSS format.
    /// </summary>
    TimeHHMMSS = 54,

    /// <summary>
    /// Total dissolved solids in kilograms per liter (kg/L).
    /// </summary>
    TDSkgL = 95,

    /// <summary>
    /// Nitrate millivolts (mV).
    /// </summary>
    NO3mV = 101,

    /// <summary>
    /// Nitrate in milligrams per liter (mg/L).
    /// </summary>
    NO3 = 106,

    /// <summary>
    /// Ammonium millivolts (mV).
    /// </summary>
    NH4mV = 108,

    /// <summary>
    /// Total dissolved solids in milligrams per liter (mg/L).
    /// </summary>
    TDSmgL = 110,

    /// <summary>
    /// Chloride in milligrams per liter (mg/L).
    /// </summary>
    Chloride = 112,

    /// <summary>
    /// Chloride millivolts (mV).
    /// </summary>
    ChlorideMV = 145,

    /// <summary>
    /// Total suspended solids in milligrams per liter (mg/L).
    /// </summary>
    TSSmgL = 190,

    /// <summary>
    /// Total suspended solids in grams per liter (g/L).
    /// </summary>
    TSSgL = 191,

    /// <summary>
    /// Chlorophyll in micrograms per liter (μg/L).
    /// </summary>
    ChlorophyllugL = 193,

    /// <summary>
    /// Chlorophyll in Relative Fluorescence Units (RFU).
    /// </summary>
    ChlorophyllRFU = 194,

    /// <summary>
    /// PAR Channel 1 in μmol·s-1·m-2.
    /// </summary>
    PARChannel1 = 201,

    /// <summary>
    /// PAR Channel 2 in μmol·s-1·m-2.
    /// </summary>
    PARChannel2 = 202,

    /// <summary>
    /// Rhodamine in micrograms per liter (μg/L).
    /// </summary>
    RhodamineugL = 204,

    /// <summary>
    /// Dissolved oxygen in percent saturation (%).
    /// </summary>
    ODOPercentSat = 211,

    /// <summary>
    /// Dissolved oxygen in milligrams per liter (mg/L).
    /// </summary>
    ODOmgL = 212,

    /// <summary>
    /// Dissolved oxygen in percent local saturation (%).
    /// </summary>
    ODOPercentLocal = 214,

    /// <summary>
    /// Total Algae-PC in cells per milliliter (cells/mL).
    /// </summary>
    TALPCcellsmL = 215,

    /// <summary>
    /// Blue-green algae phycocyanin in Relative Fluorescence Units (RFU).
    /// </summary>
    BGAPCrfu = 216,

    /// <summary>
    /// Total Algae-PE in cells per milliliter (cells/mL).
    /// </summary>
    TALPEcellsmL = 217,

    /// <summary>
    /// Blue-green algae phycoerythrin in Relative Fluorescence Units (RFU).
    /// </summary>
    BGAPErfu = 218,

    /// <summary>
    /// Turbidity in Formazin Nephelometric Units (FNU).
    /// </summary>
    TurbidityFNU = 223,

    /// <summary>
    /// Turbidity raw signal.
    /// </summary>
    TurbidityRaw = 224,

    /// <summary>
    /// Blue-green algae phycocyanin in micrograms per liter (μg/L).
    /// </summary>
    BGAPCugL = 225,

    /// <summary>
    /// Blue-green algae phycoerythrin in micrograms per liter (μg/L).
    /// </summary>
    BGAPEugL = 226,

    /// <summary>
    /// Fluorescent dissolved organic matter in Relative Fluorescence Units (RFU).
    /// </summary>
    fDOMrfu = 227,

    /// <summary>
    /// Fluorescent dissolved organic matter in Quinine Sulfate Units (QSU).
    /// </summary>
    fDOMqsu = 228,

    /// <summary>
    /// Wiper position in volts (V).
    /// </summary>
    WiperPosition = 229,

    /// <summary>
    /// External power in volts (V).
    /// </summary>
    ExternalPower = 230,

    /// <summary>
    /// Blue-green algae phycocyanin raw signal.
    /// </summary>
    BGAPCRaw = 231,

    /// <summary>
    /// Blue-green algae phycoerythrin raw signal.
    /// </summary>
    BGAPERaw = 232,

    /// <summary>
    /// Fluorescent dissolved organic matter raw signal.
    /// </summary>
    fDOMRaw = 233,

    /// <summary>
    /// Chlorophyll raw signal.
    /// </summary>
    ChlorophyllRaw = 234,

    /// <summary>
    /// Potassium millivolts (mV).
    /// </summary>
    PotassiummV = 235,

    /// <summary>
    /// Potassium in milligrams per liter (mg/L).
    /// </summary>
    PotassiummgL = 236,

    /// <summary>
    /// Non-linear function conductivity in millisiemens per centimeter (mS/cm).
    /// </summary>
    nLFConductivitymScm = 237,

    /// <summary>
    /// Non-linear function conductivity in microsiemens per centimeter (μS/cm).
    /// </summary>
    nLFConductivityuScm = 238,

    /// <summary>
    /// Wiper peak current in milliamps (mA).
    /// </summary>
    WiperPeakCurrent = 239,

    /// <summary>
    /// Vertical position in meters (m).
    /// </summary>
    VerticalPositionm = 240,

    /// <summary>
    /// Vertical position in feet (ft).
    /// </summary>
    VerticalPositionft = 241,

    /// <summary>
    /// Chlorophyll in cells per milliliter (cells/mL).
    /// </summary>
    ChlorophyllcellsmL = 242
}
