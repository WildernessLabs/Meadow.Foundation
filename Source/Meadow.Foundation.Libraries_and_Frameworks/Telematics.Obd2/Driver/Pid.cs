namespace Meadow.Foundation.Telematics.OBD2;

/// <summary>
/// Enumeration for OBD-II PIDs (Parameter IDs).
/// These PIDs are used to request diagnostic data from a vehicle.
/// </summary>
public enum Pid : byte
{
    /// <summary>
    /// PID 0x00 - Supported PIDs 01-20
    /// </summary>
    SupportedPids_01_20 = 0x00,

    /// <summary>
    /// PID 0x01 - Monitor status since DTCs cleared
    /// </summary>
    MonitorStatus = 0x01,

    /// <summary>
    /// PID 0x02 - Freeze DTC
    /// </summary>
    FreezeDtc = 0x02,

    /// <summary>
    /// PID 0x03 - Fuel system status
    /// </summary>
    FuelSystemStatus = 0x03,

    /// <summary>
    /// PID 0x04 - Calculated engine load
    /// </summary>
    CalculatedEngineLoad = 0x04,

    /// <summary>
    /// PID 0x05 - Engine coolant temperature
    /// </summary>
    EngineCoolantTemperature = 0x05,

    /// <summary>
    /// PID 0x06 - Short term fuel trim - Bank 1
    /// </summary>
    ShortTermFuelTrimBank1 = 0x06,

    /// <summary>
    /// PID 0x07 - Long term fuel trim - Bank 1
    /// </summary>
    LongTermFuelTrimBank1 = 0x07,

    /// <summary>
    /// PID 0x08 - Short term fuel trim - Bank 2
    /// </summary>
    ShortTermFuelTrimBank2 = 0x08,

    /// <summary>
    /// PID 0x09 - Long term fuel trim - Bank 2
    /// </summary>
    LongTermFuelTrimBank2 = 0x09,

    /// <summary>
    /// PID 0x0A - Fuel pressure
    /// </summary>
    FuelPressure = 0x0A,

    /// <summary>
    /// PID 0x0B - Intake manifold absolute pressure
    /// </summary>
    IntakeManifoldPressure = 0x0B,

    /// <summary>
    /// PID 0x0C - Engine RPM
    /// </summary>
    EngineRpm = 0x0C,

    /// <summary>
    /// PID 0x0D - Vehicle speed
    /// </summary>
    VehicleSpeed = 0x0D,

    /// <summary>
    /// PID 0x0E - Timing advance
    /// </summary>
    TimingAdvance = 0x0E,

    /// <summary>
    /// PID 0x0F - Intake air temperature
    /// </summary>
    IntakeAirTemperature = 0x0F,

    /// <summary>
    /// PID 0x10 - MAF air flow rate
    /// </summary>
    MafAirFlowRate = 0x10,

    /// <summary>
    /// PID 0x11 - Throttle position
    /// </summary>
    ThrottlePosition = 0x11,

    /// <summary>
    /// PID 0x12 - Commanded secondary air status
    /// </summary>
    CommandedSecondaryAirStatus = 0x12,

    /// <summary>
    /// PID 0x13 - Oxygen sensors present (in 2 banks)
    /// </summary>
    OxygenSensorsPresent = 0x13,

    /// <summary>
    /// PID 0x14 - Oxygen sensor 1 - Short term fuel trim
    /// </summary>
    OxygenSensor1ShortTermFuelTrim = 0x14,

    /// <summary>
    /// PID 0x15 - Oxygen sensor 2 - Short term fuel trim
    /// </summary>
    OxygenSensor2ShortTermFuelTrim = 0x15,

    /// <summary>
    /// PID 0x16 - Oxygen sensor 3 - Short term fuel trim
    /// </summary>
    OxygenSensor3ShortTermFuelTrim = 0x16,

    /// <summary>
    /// PID 0x17 - Oxygen sensor 4 - Short term fuel trim
    /// </summary>
    OxygenSensor4ShortTermFuelTrim = 0x17,

    /// <summary>
    /// PID 0x18 - Oxygen sensor 5 - Short term fuel trim
    /// </summary>
    OxygenSensor5ShortTermFuelTrim = 0x18,

    /// <summary>
    /// PID 0x19 - Oxygen sensor 6 - Short term fuel trim
    /// </summary>
    OxygenSensor6ShortTermFuelTrim = 0x19,

    /// <summary>
    /// PID 0x1A - Oxygen sensor 7 - Short term fuel trim
    /// </summary>
    OxygenSensor7ShortTermFuelTrim = 0x1A,

    /// <summary>
    /// PID 0x1B - Oxygen sensor 8 - Short term fuel trim
    /// </summary>
    OxygenSensor8ShortTermFuelTrim = 0x1B,

    /// <summary>
    /// PID 0x1C - OBD standards this vehicle conforms to
    /// </summary>
    ObdStandards = 0x1C,

    /// <summary>
    /// PID 0x1D - Oxygen sensors present (in 4 banks)
    /// </summary>
    OxygenSensorsPresent_4Banks = 0x1D,

    /// <summary>
    /// PID 0x1E - Auxiliary input status
    /// </summary>
    AuxiliaryInputStatus = 0x1E,

    /// <summary>
    /// PID 0x1F - Run time since engine start
    /// </summary>
    RunTimeSinceEngineStart = 0x1F,

    // Add more PIDs as needed...

    /// <summary>
    /// PID 0x20 - Supported PIDs 21-40
    /// </summary>
    SupportedPids_21_40 = 0x20,

    /// <summary>
    /// PID 0x21 - Distance traveled with malfunction indicator lamp (MIL) on
    /// </summary>
    DistanceWithMilOn = 0x21,

    /// <summary>
    /// PID 0x22 - Fuel rail pressure (relative to manifold vacuum)
    /// </summary>
    FuelRailPressureRelativeToManifold = 0x22,

    /// <summary>
    /// PID 0x23 - Fuel rail gauge pressure (diesel, or gasoline direct injection)
    /// </summary>
    FuelRailGaugePressure = 0x23,

    /// <summary>
    /// PID 0x24 - Oxygen sensor 1 - Fuel/air commanded equivalence ratio
    /// </summary>
    OxygenSensor1EquivalenceRatio = 0x24,

    /// <summary>
    /// PID 0x25 - Oxygen sensor 2 - Fuel/air commanded equivalence ratio
    /// </summary>
    OxygenSensor2EquivalenceRatio = 0x25,

    /// <summary>
    /// PID 0x26 - Oxygen sensor 3 - Fuel/air commanded equivalence ratio
    /// </summary>
    OxygenSensor3EquivalenceRatio = 0x26,

    /// <summary>
    /// PID 0x27 - Oxygen sensor 4 - Fuel/air commanded equivalence ratio
    /// </summary>
    OxygenSensor4EquivalenceRatio = 0x27,

    /// <summary>
    /// PID 0x28 - Oxygen sensor 5 - Fuel/air commanded equivalence ratio
    /// </summary>
    OxygenSensor5EquivalenceRatio = 0x28,

    /// <summary>
    /// PID 0x29 - Oxygen sensor 6 - Fuel/air commanded equivalence ratio
    /// </summary>
    OxygenSensor6EquivalenceRatio = 0x29,

    /// <summary>
    /// PID 0x2A - Oxygen sensor 7 - Fuel/air commanded equivalence ratio
    /// </summary>
    OxygenSensor7EquivalenceRatio = 0x2A,

    /// <summary>
    /// PID 0x2B - Oxygen sensor 8 - Fuel/air commanded equivalence ratio
    /// </summary>
    OxygenSensor8EquivalenceRatio = 0x2B,

    /// <summary>
    /// PID 0x2C - Commanded EGR
    /// </summary>
    CommandedEgr = 0x2C,

    /// <summary>
    /// PID 0x2D - EGR Error
    /// </summary>
    EgrError = 0x2D,

    /// <summary>
    /// PID 0x2E - Commanded evaporative purge
    /// </summary>
    CommandedEvaporativePurge = 0x2E,

    /// <summary>
    /// PID 0x2F - Fuel tank level input
    /// </summary>
    FuelTankLevelInput = 0x2F,

    /// <summary>
    /// PID 0x30 - Warm-ups since codes cleared
    /// </summary>
    WarmUpsSinceCodesCleared = 0x30,

    /// <summary>
    /// PID 0x31 - Distance traveled since codes cleared
    /// </summary>
    DistanceSinceCodesCleared = 0x31,

    /// <summary>
    /// PID 0x32 - Evaporative system vapor pressure
    /// </summary>
    EvaporativeSystemVaporPressure = 0x32,

    /// <summary>
    /// PID 0x33 - Absolute barometric pressure
    /// </summary>
    BarometricPressure = 0x33,

    /// <summary>
    /// PID 0x34 - Oxygen sensor 1 - Fuel/air commanded equivalence ratio (current)
    /// </summary>
    OxygenSensor1EquivalenceRatioCurrent = 0x34,

    /// <summary>
    /// PID 0x35 - Oxygen sensor 2 - Fuel/air commanded equivalence ratio (current)
    /// </summary>
    OxygenSensor2EquivalenceRatioCurrent = 0x35,

    /// <summary>
    /// PID 0x36 - Oxygen sensor 3 - Fuel/air commanded equivalence ratio (current)
    /// </summary>
    OxygenSensor3EquivalenceRatioCurrent = 0x36,

    /// <summary>
    /// PID 0x37 - Oxygen sensor 4 - Fuel/air commanded equivalence ratio (current)
    /// </summary>
    OxygenSensor4EquivalenceRatioCurrent = 0x37,

    /// <summary>
    /// PID 0x38 - Oxygen sensor 5 - Fuel/air commanded equivalence ratio (current)
    /// </summary>
    OxygenSensor5EquivalenceRatioCurrent = 0x38,

    /// <summary>
    /// PID 0x39 - Oxygen sensor 6 - Fuel/air commanded equivalence ratio (current)
    /// </summary>
    OxygenSensor6EquivalenceRatioCurrent = 0x39,

    /// <summary>
    /// PID 0x3A - Oxygen sensor 7 - Fuel/air commanded equivalence ratio (current)
    /// </summary>
    OxygenSensor7EquivalenceRatioCurrent = 0x3A,

    /// <summary>
    /// PID 0x3B - Oxygen sensor 8 - Fuel/air commanded equivalence ratio (current)
    /// </summary>
    OxygenSensor8EquivalenceRatioCurrent = 0x3B,

    /// <summary>
    /// PID 0x3C - Catalyst temperature bank 1, sensor 1
    /// </summary>
    CatalystTemperatureBank1Sensor1 = 0x3C,

    /// <summary>
    /// PID 0x3D - Catalyst temperature bank 2, sensor 1
    /// </summary>
    CatalystTemperatureBank2Sensor1 = 0x3D,

    /// <summary>
    /// PID 0x3E - Catalyst temperature bank 1, sensor 2
    /// </summary>
    CatalystTemperatureBank1Sensor2 = 0x3E,

    /// <summary>
    /// PID 0x3F - Catalyst temperature bank 2, sensor 2
    /// </summary>
    CatalystTemperatureBank2Sensor2 = 0x3F,

    /// <summary>
    /// PID 0x40 - Supported PIDs 41-60
    /// </summary>
    SupportedPids_41_60 = 0x40,

    /// <summary>
    /// PID 0x41 - Monitor status this drive cycle
    /// </summary>
    MonitorStatusThisDriveCycle = 0x41,

    /// <summary>
    /// PID 0x42 - Control module voltage
    /// </summary>
    ControlModuleVoltage = 0x42,

    /// <summary>
    /// PID 0x43 - Absolute load value
    /// </summary>
    AbsoluteLoadValue = 0x43,

    /// <summary>
    /// PID 0x44 - Fuel/air commanded equivalence ratio
    /// </summary>
    FuelAirCommandedEquivalenceRatio = 0x44,

    /// <summary>
    /// PID 0x45 - Relative throttle position
    /// </summary>
    RelativeThrottlePosition = 0x45,

    /// <summary>
    /// PID 0x46 - Ambient air temperature
    /// </summary>
    AmbientAirTemperature = 0x46,

    /// <summary>
    /// PID 0x47 - Absolute throttle position B
    /// </summary>
    AbsoluteThrottlePositionB = 0x47,

    /// <summary>
    /// PID 0x48 - Absolute throttle position C
    /// </summary>
    AbsoluteThrottlePositionC = 0x48,

    /// <summary>
    /// PID 0x49 - Accelerator pedal position D
    /// </summary>
    AcceleratorPedalPositionD = 0x49,

    /// <summary>
    /// PID 0x4A - Accelerator pedal position E
    /// </summary>
    AcceleratorPedalPositionE = 0x4A,

    /// <summary>
    /// PID 0x4B - Accelerator pedal position F
    /// </summary>
    AcceleratorPedalPositionF = 0x4B,

    /// <summary>
    /// PID 0x4C - Commanded throttle actuator
    /// </summary>
    CommandedThrottleActuator = 0x4C,

    /// <summary>
    /// PID 0x4D - Time run with MIL on
    /// </summary>
    TimeRunWithMilOn = 0x4D,

    /// <summary>
    /// PID 0x4E - Time since trouble codes cleared
    /// </summary>
    TimeSinceTroubleCodesCleared = 0x4E,

    /// <summary>
    /// PID 0x4F - Maximum value for equivalence ratio, oxygen sensor voltage, oxygen sensor current, and intake manifold absolute pressure
    /// </summary>
    MaximumValues = 0x4F,

    /// <summary>
    /// PID 0x50 - Maximum value for air flow rate from mass air flow sensor
    /// </summary>
    MaximumAirFlowRate = 0x50,

    /// <summary>
    /// PID 0x51 - Fuel Type
    /// </summary>
    FuelType = 0x51,

    /// <summary>
    /// PID 0x52 - Ethanol fuel percentage
    /// </summary>
    EthanolFuelPercentage = 0x52,

    /// <summary>
    /// PID 0x53 - Absolute evaporative system vapor pressure
    /// </summary>
    AbsoluteEvaporativeSystemVaporPressure = 0x53,

    /// <summary>
    /// PID 0x54 - Evaporative system vapor pressure
    /// </summary>
    EvaporativeSystemVaporPressure2 = 0x54,

    /// <summary>
    /// PID 0x55 - Short term secondary oxygen sensor trim - Bank 1 and Bank 3
    /// </summary>
    ShortTermSecondaryOxygenSensorTrimBank1And3 = 0x55,

    /// <summary>
    /// PID 0x56 - Long term secondary oxygen sensor trim - Bank 1 and Bank 3
    /// </summary>
    LongTermSecondaryOxygenSensorTrimBank1And3 = 0x56,

    /// <summary>
    /// PID 0x57 - Short term secondary oxygen sensor trim - Bank 2 and Bank 4
    /// </summary>
    ShortTermSecondaryOxygenSensorTrimBank2And4 = 0x57,

    /// <summary>
    /// PID 0x58 - Long term secondary oxygen sensor trim - Bank 2 and Bank 4
    /// </summary>
    LongTermSecondaryOxygenSensorTrimBank2And4 = 0x58,

    /// <summary>
    /// PID 0x59 - Fuel rail pressure (absolute)
    /// </summary>
    FuelRailPressureAbsolute = 0x59,

    /// <summary>
    /// PID 0x5A - Relative accelerator pedal position
    /// </summary>
    RelativeAcceleratorPedalPosition = 0x5A,

    /// <summary>
    /// PID 0x5B - Hybrid battery pack remaining life
    /// </summary>
    HybridBatteryPackRemainingLife = 0x5B,

    /// <summary>
    /// PID 0x5C - Engine oil temperature
    /// </summary>
    EngineOilTemperature = 0x5C,

    /// <summary>
    /// PID 0x5D - Fuel injection timing
    /// </summary>
    FuelInjectionTiming = 0x5D,

    /// <summary>
    /// PID 0x5E - Engine fuel rate
    /// </summary>
    EngineFuelRate = 0x5E,

    /// <summary>
    /// PID 0x5F - Emission requirements to which vehicle is designed
    /// </summary>
    EmissionRequirements = 0x5F,

    /// <summary>
    /// PID 0x60 - Supported PIDs 61-80
    /// </summary>
    SupportedPids_61_80 = 0x60,

    /// <summary>
    /// PID 0x61 - Driver's demand engine - percent torque
    /// </summary>
    DriversDemandEnginePercentTorque = 0x61,

    /// <summary>
    /// PID 0x62 - Actual engine - percent torque
    /// </summary>
    ActualEnginePercentTorque = 0x62,

    /// <summary>
    /// PID 0x63 - Engine reference torque
    /// </summary>
    EngineReferenceTorque = 0x63,

    /// <summary>
    /// PID 0x64 - Engine percent torque data
    /// </summary>
    EnginePercentTorqueData = 0x64,

    /// <summary>
    /// PID 0x65 - Auxiliary input/output supported
    /// </summary>
    AuxiliaryInputOutputSupported = 0x65,

    /// <summary>
    /// PID 0x66 - Mass air flow sensor
    /// </summary>
    MassAirFlowSensor = 0x66,

    /// <summary>
    /// PID 0x67 - Engine coolant temperature
    /// </summary>
    EngineCoolantTemperature2 = 0x67,

    /// <summary>
    /// PID 0x68 - Intake air temperature sensor
    /// </summary>
    IntakeAirTemperatureSensor = 0x68,

    /// <summary>
    /// PID 0x69 - Commanded EGR and EGR error
    /// </summary>
    CommandedEgrAndEgrError = 0x69,

    /// <summary>
    /// PID 0x6A - Commanded Diesel intake air flow control and relative intake air flow position
    /// </summary>
    CommandedDieselIntakeAirFlow = 0x6A,

    /// <summary>
    /// PID 0x6B - Exhaust gas recirculation temperature
    /// </summary>
    ExhaustGasRecirculationTemperature = 0x6B,

    /// <summary>
    /// PID 0x6C - Commanded throttle actuator control and relative throttle position
    /// </summary>
    CommandedThrottleActuatorControl = 0x6C,

    /// <summary>
    /// PID 0x6D - Fuel pressure control system
    /// </summary>
    FuelPressureControlSystem = 0x6D,

    /// <summary>
    /// PID 0x6E - Injection pressure control system
    /// </summary>
    InjectionPressureControlSystem = 0x6E,

    /// <summary>
    /// PID 0x6F - Turbocharger compressor inlet pressure
    /// </summary>
    TurbochargerCompressorInletPressure = 0x6F,

    /// <summary>
    /// PID 0x70 - Boost pressure control
    /// </summary>
    BoostPressureControl = 0x70,

    /// <summary>
    /// PID 0x71 - Variable Geometry Turbo (VGT) control
    /// </summary>
    VariableGeometryTurboControl = 0x71,

    /// <summary>
    /// PID 0x72 - Wastegate control
    /// </summary>
    WastegateControl = 0x72,

    /// <summary>
    /// PID 0x73 - Exhaust pressure
    /// </summary>
    ExhaustPressure = 0x73,

    /// <summary>
    /// PID 0x74 - Turbocharger RPM
    /// </summary>
    TurbochargerRpm = 0x74,

    /// <summary>
    /// PID 0x75 - Turbocharger temperature 1
    /// </summary>
    TurbochargerTemperature1 = 0x75,

    /// <summary>
    /// PID 0x76 - Turbocharger temperature 2
    /// </summary>
    TurbochargerTemperature2 = 0x76,

    /// <summary>
    /// PID 0x77 - Charge air cooler temperature (CACT)
    /// </summary>
    ChargeAirCoolerTemperature = 0x77,

    /// <summary>
    /// PID 0x78 - Exhaust gas temperature (EGT) Bank 1
    /// </summary>
    ExhaustGasTemperatureBank1 = 0x78,

    /// <summary>
    /// PID 0x79 - Exhaust gas temperature (EGT) Bank 2
    /// </summary>
    ExhaustGasTemperatureBank2 = 0x79,

    /// <summary>
    /// PID 0x7A - Diesel particulate filter (DPF) 1 - differential pressure
    /// </summary>
    Dpf1DifferentialPressure = 0x7A,

    /// <summary>
    /// PID 0x7B - Diesel particulate filter (DPF) 2 - differential pressure
    /// </summary>
    Dpf2DifferentialPressure = 0x7B,

    /// <summary>
    /// PID 0x7C - Diesel particulate filter (DPF) temperature
    /// </summary>
    DpfTemperature = 0x7C,

    /// <summary>
    /// PID 0x7D - NOx NTE (not-to-exceed) control area status
    /// </summary>
    NoxNteControlAreaStatus = 0x7D,

    /// <summary>
    /// PID 0x7E - PM NTE (not-to-exceed) control area status
    /// </summary>
    PmNteControlAreaStatus = 0x7E,

    /// <summary>
    /// PID 0x7F - Engine run time
    /// </summary>
    EngineRunTime = 0x7F,

    /// <summary>
    /// PID 0x80 - Supported PIDs 81-A0
    /// </summary>
    SupportedPids_81_A0 = 0x80,

    /// <summary>
    /// PID 0x81 - Engine run time for Auxiliary Emissions Control Device (AECD)
    /// </summary>
    EngineRunTimeForAecd = 0x81,

    /// <summary>
    /// PID 0x82 - Engine run time for Auxiliary Emissions Control Device (AECD) #2
    /// </summary>
    EngineRunTimeForAecd2 = 0x82,

    /// <summary>
    /// PID 0x83 - NOx sensor
    /// </summary>
    NoxSensor = 0x83,

    /// <summary>
    /// PID 0x84 - Manifold surface temperature
    /// </summary>
    ManifoldSurfaceTemperature = 0x84,

    /// <summary>
    /// PID 0x85 - NOx reagent system
    /// </summary>
    NoxReagentSystem = 0x85,

    /// <summary>
    /// PID 0x86 - Particulate matter (PM) sensor
    /// </summary>
    ParticulateMatterSensor = 0x86,

    /// <summary>
    /// PID 0x87 - Intake manifold absolute pressure
    /// </summary>
    IntakeManifoldAbsolutePressure2 = 0x87,

    /// <summary>
    /// PID 0x88 - SCR Induce System
    /// </summary>
    ScrInduceSystem = 0x88,

    /// <summary>
    /// PID 0x89 - Run time for AECD 3
    /// </summary>
    RunTimeForAecd3 = 0x89,

    /// <summary>
    /// PID 0x8A - Run time for AECD 4
    /// </summary>
    RunTimeForAecd4 = 0x8A,

    /// <summary>
    /// PID 0x8B - Run time for AECD 5
    /// </summary>
    RunTimeForAecd5 = 0x8B,

    /// <summary>
    /// PID 0x8C - Run time for AECD 6
    /// </summary>
    RunTimeForAecd6 = 0x8C,

    /// <summary>
    /// PID 0x8D - Run time for AECD 7
    /// </summary>
    RunTimeForAecd7 = 0x8D,

    /// <summary>
    /// PID 0x8E - Run time for AECD 8
    /// </summary>
    RunTimeForAecd8 = 0x8E,

    /// <summary>
    /// PID 0x8F - Run time for AECD 9
    /// </summary>
    RunTimeForAecd9 = 0x8F,

    /// <summary>
    /// PID 0x90 - Run time for AECD 10
    /// </summary>
    RunTimeForAecd10 = 0x90,

    /// <summary>
    /// PID 0x91 - Exhaust Gas Temperature (EGT) Sensor Bank 1
    /// </summary>
    EgtSensorBank1 = 0x91,

    /// <summary>
    /// PID 0x92 - Exhaust Gas Temperature (EGT) Sensor Bank 2
    /// </summary>
    EgtSensorBank2 = 0x92,

    /// <summary>
    /// PID 0x93 - Diesel Particulate Filter 1, Differential Pressure Sensor
    /// </summary>
    DieselParticulateFilter1DifferentialPressureSensor = 0x93,

    /// <summary>
    /// PID 0x94 - Diesel Particulate Filter 2, Differential Pressure Sensor
    /// </summary>
    DieselParticulateFilter2DifferentialPressureSensor = 0x94,

    /// <summary>
    /// PID 0x95 - Diesel Particulate Filter 1, Temperature Sensor
    /// </summary>
    DieselParticulateFilter1TemperatureSensor = 0x95,

    /// <summary>
    /// PID 0x96 - Diesel Particulate Filter 2, Temperature Sensor
    /// </summary>
    DieselParticulateFilter2TemperatureSensor = 0x96,

    /// <summary>
    /// PID 0x97 - Selective Catalytic Reduction (SCR) Catalyst Temperature
    /// </summary>
    ScrCatalystTemperature = 0x97,

    /// <summary>
    /// PID 0x98 - NOx Emission Rate
    /// </summary>
    NoxEmissionRate = 0x98,

    /// <summary>
    /// PID 0x99 - Exhaust Gas Recirculation (EGR) Temperature Sensor
    /// </summary>
    EgrTemperatureSensor = 0x99,

    /// <summary>
    /// PID 0x9A - Oxygen Sensor Monitoring Bank 1
    /// </summary>
    OxygenSensorMonitoringBank1 = 0x9A,

    /// <summary>
    /// PID 0x9B - Oxygen Sensor Monitoring Bank 2
    /// </summary>
    OxygenSensorMonitoringBank2 = 0x9B,

    /// <summary>
    /// PID 0x9C - Oxygen Sensor Monitoring Bank 3
    /// </summary>
    OxygenSensorMonitoringBank3 = 0x9C,

    /// <summary>
    /// PID 0x9D - Oxygen Sensor Monitoring Bank 4
    /// </summary>
    OxygenSensorMonitoringBank4 = 0x9D,

    /// <summary>
    /// PID 0x9E - Oxygen Sensor Monitoring Bank 5
    /// </summary>
    OxygenSensorMonitoringBank5 = 0x9E,

    /// <summary>
    /// PID 0x9F - Oxygen Sensor Monitoring Bank 6
    /// </summary>
    OxygenSensorMonitoringBank6 = 0x9F,

    /// <summary>
    /// PID 0xA0 - Supported PIDs A1-C0
    /// </summary>
    SupportedPids_A1_C0 = 0xA0,

    /// <summary>
    /// PID 0xA1 - Fuel System Control Mode
    /// </summary>
    FuelSystemControlMode = 0xA1,

    /// <summary>
    /// PID 0xA2 - Transmission Actual Gear Ratio
    /// </summary>
    TransmissionActualGearRatio = 0xA2,

    /// <summary>
    /// PID 0xA3 - Diesel Particulate Filter Temperature Sensor 1
    /// </summary>
    DpfTemperatureSensor1 = 0xA3,

    /// <summary>
    /// PID 0xA4 - Diesel Particulate Filter Temperature Sensor 2
    /// </summary>
    DpfTemperatureSensor2 = 0xA4,

    /// <summary>
    /// PID 0xA5 - Diesel Particulate Filter Temperature Sensor 3
    /// </summary>
    DpfTemperatureSensor3 = 0xA5,

    /// <summary>
    /// PID 0xA6 - Diesel Particulate Filter Temperature Sensor 4
    /// </summary>
    DpfTemperatureSensor4 = 0xA6,

    /// <summary>
    /// PID 0xA7 - Selective Catalytic Reduction (SCR) Catalyst Monitoring
    /// </summary>
    ScrCatalystMonitoring = 0xA7,

    /// <summary>
    /// PID 0xA8 - Diesel Particulate Filter (DPF) Monitoring
    /// </summary>
    DpfMonitoring = 0xA8,

    /// <summary>
    /// PID 0xA9 - Exhaust Gas Recirculation (EGR) Monitoring
    /// </summary>
    EgrMonitoring = 0xA9,

    /// <summary>
    /// PID 0xAA - Oxygen Sensor 1, Short-Term Fuel Trim
    /// </summary>
    OxygenSensor1ShortTermFuelTrim_2 = 0xAA,

    /// <summary>
    /// PID 0xAB - Oxygen Sensor 2, Short-Term Fuel Trim
    /// </summary>
    OxygenSensor2ShortTermFuelTrim_2 = 0xAB,

    /// <summary>
    /// PID 0xAC - Oxygen Sensor 3, Short-Term Fuel Trim
    /// </summary>
    OxygenSensor3ShortTermFuelTrim_2 = 0xAC,

    /// <summary>
    /// PID 0xAD - Oxygen Sensor 4, Short-Term Fuel Trim
    /// </summary>
    OxygenSensor4ShortTermFuelTrim_2 = 0xAD,

    /// <summary>
    /// PID 0xAE - Fuel Trim
    /// </summary>
    FuelTrim = 0xAE,

    /// <summary>
    /// PID 0xAF - Commanded Diesel Particulate Filter (DPF) Regeneration
    /// </summary>
    CommandedDpfRegeneration = 0xAF,

    /// <summary>
    /// PID 0xB0 - Requested DPF Regeneration
    /// </summary>
    RequestedDpfRegeneration = 0xB0,

    /// <summary>
    /// PID 0xB1 - DPF Regen Inhibit Switch Status
    /// </summary>
    DpfRegenInhibitSwitchStatus = 0xB1,

    /// <summary>
    /// PID 0xB2 - Diesel Oxidation Catalyst (DOC) Temperature Sensor
    /// </summary>
    DocTemperatureSensor = 0xB2,

    /// <summary>
    /// PID 0xB3 - Diesel Particulate Filter (DPF) Temperature Sensor 5
    /// </summary>
    DpfTemperatureSensor5 = 0xB3,

    /// <summary>
    /// PID 0xB4 - Diesel Particulate Filter (DPF) Temperature Sensor 6
    /// </summary>
    DpfTemperatureSensor6 = 0xB4,

    /// <summary>
    /// PID 0xB5 - Diesel Particulate Filter (DPF) Temperature Sensor 7
    /// </summary>
    DpfTemperatureSensor7 = 0xB5,

    /// <summary>
    /// PID 0xB6 - Diesel Particulate Filter (DPF) Temperature Sensor 8
    /// </summary>
    DpfTemperatureSensor8 = 0xB6,

    /// <summary>
    /// PID 0xB7 - Diesel Particulate Filter (DPF) Temperature Sensor 9
    /// </summary>
    DpfTemperatureSensor9 = 0xB7,

    /// <summary>
    /// PID 0xB8 - Diesel Particulate Filter (DPF) Temperature Sensor 10
    /// </summary>
    DpfTemperatureSensor10 = 0xB8,

    /// <summary>
    /// PID 0xB9 - NOx Reduction Catalyst Monitoring
    /// </summary>
    NoxReductionCatalystMonitoring = 0xB9,

    /// <summary>
    /// PID 0xBA - Fuel Injection Pressure Control
    /// </summary>
    FuelInjectionPressureControl = 0xBA,

    /// <summary>
    /// PID 0xBB - SCR Induce System 2
    /// </summary>
    ScrInduceSystem2 = 0xBB,

    /// <summary>
    /// PID 0xBC - DOC Temperature Sensor 2
    /// </summary>
    DocTemperatureSensor2 = 0xBC,

    /// <summary>
    /// PID 0xBD - Selective Catalytic Reduction (SCR) Catalyst Temperature Sensor 1
    /// </summary>
    ScrCatalystTemperatureSensor1 = 0xBD,

    /// <summary>
    /// PID 0xBE - Selective Catalytic Reduction (SCR) Catalyst Temperature Sensor 2
    /// </summary>
    ScrCatalystTemperatureSensor2 = 0xBE,

    /// <summary>
    /// PID 0xBF - Diesel Particulate Filter (DPF) Differential Pressure Sensor
    /// </summary>
    DpfDifferentialPressureSensor2 = 0xBF,

    /// <summary>
    /// PID 0xC0 - Supported PIDs C1-E0
    /// </summary>
    SupportedPids_C1_E0 = 0xC0,

    /// <summary>
    /// PID 0xC1 - Engine Oil Temperature 2
    /// </summary>
    EngineOilTemperature2 = 0xC1,

    /// <summary>
    /// PID 0xC2 - Fuel Injection Quantity
    /// </summary>
    FuelInjectionQuantity = 0xC2,

    /// <summary>
    /// PID 0xC3 - Diesel Particulate Filter (DPF) Status
    /// </summary>
    DpfStatus = 0xC3,

    /// <summary>
    /// PID 0xC4 - Selective Catalytic Reduction (SCR) Efficiency
    /// </summary>
    ScrEfficiency = 0xC4,

    /// <summary>
    /// PID 0xC5 - Diesel Oxidation Catalyst (DOC) Efficiency
    /// </summary>
    DocEfficiency = 0xC5,

    /// <summary>
    /// PID 0xC6 - Exhaust Gas Temperature (EGT) Sensor 2
    /// </summary>
    EgtSensor2 = 0xC6,

    /// <summary>
    /// PID 0xC7 - Diesel Particulate Filter (DPF) Differential Pressure Sensor 2
    /// </summary>
    DpfDifferentialPressureSensor3 = 0xC7,

    /// <summary>
    /// PID 0xC8 - Selective Catalytic Reduction (SCR) Temperature Sensor
    /// </summary>
    ScrTemperatureSensor = 0xC8,

    /// <summary>
    /// PID 0xC9 - Exhaust Gas Recirculation (EGR) Error
    /// </summary>
    EgrError2 = 0xC9,

    /// <summary>
    /// PID 0xCA - Intake Air Temperature Sensor 2
    /// </summary>
    IntakeAirTemperatureSensor2 = 0xCA,

    /// <summary>
    /// PID 0xCB - Turbocharger Boost Pressure Control
    /// </summary>
    TurbochargerBoostPressureControl = 0xCB,

    /// <summary>
    /// PID 0xCC - Turbocharger Speed
    /// </summary>
    TurbochargerSpeed = 0xCC,

    /// <summary>
    /// PID 0xCD - Diesel Particulate Filter (DPF) Temperature
    /// </summary>
    DpfTemperature3 = 0xCD,

    /// <summary>
    /// PID 0xCE - Diesel Particulate Filter (DPF) Regeneration Inhibit Switch Status
    /// </summary>
    DpfRegenInhibitSwitchStatus2 = 0xCE,

    /// <summary>
    /// PID 0xCF - Exhaust Pressure Control Valve
    /// </summary>
    ExhaustPressureControlValve = 0xCF,

    /// <summary>
    /// PID 0xD0 - Diesel Particulate Filter (DPF) Differential Pressure Sensor 3
    /// </summary>
    DpfDifferentialPressureSensor4 = 0xD0,

    /// <summary>
    /// PID 0xD1 - Exhaust Gas Recirculation (EGR) Temperature Sensor 2
    /// </summary>
    EgrTemperatureSensor2 = 0xD1,

    /// <summary>
    /// PID 0xD2 - Exhaust Gas Recirculation (EGR) Temperature Sensor 3
    /// </summary>
    EgrTemperatureSensor3 = 0xD2,

    /// <summary>
    /// PID 0xD3 - Exhaust Gas Temperature (EGT) Sensor 3
    /// </summary>
    EgtSensor3 = 0xD3,

    /// <summary>
    /// PID 0xD4 - Exhaust Gas Temperature (EGT) Sensor 4
    /// </summary>
    EgtSensor4 = 0xD4,

    /// <summary>
    /// PID 0xD5 - Exhaust Gas Temperature (EGT) Sensor 5
    /// </summary>
    EgtSensor5 = 0xD5,

    /// <summary>
    /// PID 0xD6 - Exhaust Gas Temperature (EGT) Sensor 6
    /// </summary>
    EgtSensor6 = 0xD6,

    /// <summary>
    /// PID 0xD7 - Oxygen Sensor 7
    /// </summary>
    OxygenSensor7 = 0xD7,

    /// <summary>
    /// PID 0xD8 - Oxygen Sensor 8
    /// </summary>
    OxygenSensor8 = 0xD8,

    /// <summary>
    /// PID 0xD9 - Oxygen Sensor 9
    /// </summary>
    OxygenSensor9 = 0xD9,

    /// <summary>
    /// PID 0xDA - Oxygen Sensor 10
    /// </summary>
    OxygenSensor10 = 0xDA,

    /// <summary>
    /// PID 0xDB - Exhaust Gas Recirculation (EGR) Differential Pressure Sensor
    /// </summary>
    EgrDifferentialPressureSensor = 0xDB,

    /// <summary>
    /// PID 0xDC - Exhaust Gas Recirculation (EGR) Differential Pressure Sensor 2
    /// </summary>
    EgrDifferentialPressureSensor2 = 0xDC,

    /// <summary>
    /// PID 0xDD - Diesel Particulate Filter (DPF) Temperature Sensor 11
    /// </summary>
    DpfTemperatureSensor11 = 0xDD,

    /// <summary>
    /// PID 0xDE - Diesel Particulate Filter (DPF) Temperature Sensor 12
    /// </summary>
    DpfTemperatureSensor12 = 0xDE,

    /// <summary>
    /// PID 0xDF - Exhaust Gas Temperature (EGT) Sensor 7
    /// </summary>
    EgtSensor7 = 0xDF,

    /// <summary>
    /// PID 0xE0 - Supported PIDs E1-FF
    /// </summary>
    SupportedPids_E1_FF = 0xE0,

    /// <summary>
    /// PID 0xE1 - Fuel Injection Pressure Regulator
    /// </summary>
    FuelInjectionPressureRegulator = 0xE1,

    /// <summary>
    /// PID 0xE2 - Turbocharger Compressor Inlet Temperature
    /// </summary>
    TurbochargerCompressorInletTemperature = 0xE2,

    /// <summary>
    /// PID 0xE3 - Boost Pressure Control Status
    /// </summary>
    BoostPressureControlStatus = 0xE3,

    /// <summary>
    /// PID 0xE4 - Turbocharger Turbine Outlet Temperature
    /// </summary>
    TurbochargerTurbineOutletTemperature = 0xE4,

    /// <summary>
    /// PID 0xE5 - Variable Geometry Turbo (VGT) Control
    /// </summary>
    VgtControl = 0xE5,

    /// <summary>
    /// PID 0xE6 - Wastegate Control
    /// </summary>
    WastegateControl_E6 = 0xE6,

    /// <summary>
    /// PID 0xE7 - Exhaust Pressure Regulator Valve Control
    /// </summary>
    ExhaustPressureRegulatorValveControl = 0xE7,

    /// <summary>
    /// PID 0xE8 - Exhaust Gas Recirculation (EGR) Cooler Bypass Control
    /// </summary>
    EgrCoolerBypassControl = 0xE8,

    /// <summary>
    /// PID 0xE9 - Fuel Pressure Control Valve
    /// </summary>
    FuelPressureControlValve = 0xE9,

    /// <summary>
    /// PID 0xEA - Fuel Injection Quantity Balance
    /// </summary>
    FuelInjectionQuantityBalance = 0xEA,

    /// <summary>
    /// PID 0xEB - Cylinder Deactivation Status
    /// </summary>
    CylinderDeactivationStatus = 0xEB,

    /// <summary>
    /// PID 0xEC - Exhaust Gas Temperature (EGT) Sensor 8
    /// </summary>
    EgtSensor8 = 0xEC,

    /// <summary>
    /// PID 0xED - Diesel Oxidation Catalyst (DOC) Inlet Temperature
    /// </summary>
    DocInletTemperature = 0xED,

    /// <summary>
    /// PID 0xEE - Diesel Oxidation Catalyst (DOC) Outlet Temperature
    /// </summary>
    DocOutletTemperature = 0xEE,

    /// <summary>
    /// PID 0xEF - Diesel Particulate Filter (DPF) Inlet Temperature
    /// </summary>
    DpfInletTemperature = 0xEF,

    /// <summary>
    /// PID 0xF0 - Diesel Particulate Filter (DPF) Outlet Temperature
    /// </summary>
    DpfOutletTemperature = 0xF0,

    /// <summary>
    /// PID 0xF1 - Selective Catalytic Reduction (SCR) Inlet Temperature
    /// </summary>
    ScrInletTemperature = 0xF1,

    /// <summary>
    /// PID 0xF2 - Selective Catalytic Reduction (SCR) Outlet Temperature
    /// </summary>
    ScrOutletTemperature = 0xF2,

    /// <summary>
    /// PID 0xF3 - Exhaust Gas Recirculation (EGR) Temperature Sensor 4
    /// </summary>
    EgrTemperatureSensor4 = 0xF3,

    /// <summary>
    /// PID 0xF4 - Exhaust Gas Recirculation (EGR) Temperature Sensor 5
    /// </summary>
    EgrTemperatureSensor5 = 0xF4,

    /// <summary>
    /// PID 0xF5 - Turbocharger Speed 2
    /// </summary>
    TurbochargerSpeed2 = 0xF5,

    /// <summary>
    /// PID 0xF6 - Cylinder Pressure
    /// </summary>
    CylinderPressure = 0xF6,

    /// <summary>
    /// PID 0xF7 - Engine Oil Temperature 3
    /// </summary>
    EngineOilTemperature3 = 0xF7,

    /// <summary>
    /// PID 0xF8 - Engine Coolant Temperature 2
    /// </summary>
    EngineCoolantTemperature2_F8 = 0xF8,

    /// <summary>
    /// PID 0xF9 - Transmission Oil Temperature 2
    /// </summary>
    TransmissionOilTemperature2 = 0xF9,

    /// <summary>
    /// PID 0xFA - Fuel Temperature Sensor 2
    /// </summary>
    FuelTemperatureSensor2 = 0xFA,

    /// <summary>
    /// PID 0xFB - Intake Air Temperature Sensor 3
    /// </summary>
    IntakeAirTemperatureSensor3 = 0xFB,

    /// <summary>
    /// PID 0xFC - Exhaust Gas Temperature (EGT) Sensor 9
    /// </summary>
    EgtSensor9 = 0xFC,

    /// <summary>
    /// PID 0xFD - Exhaust Gas Temperature (EGT) Sensor 10
    /// </summary>
    EgtSensor10 = 0xFD,

    /// <summary>
    /// PID 0xFE - Commanded Torque
    /// </summary>
    CommandedTorque = 0xFE,

    /// <summary>
    /// PID 0xFF - Maximum Available Torque
    /// </summary>
    MaximumAvailableTorque = 0xFF,
}
