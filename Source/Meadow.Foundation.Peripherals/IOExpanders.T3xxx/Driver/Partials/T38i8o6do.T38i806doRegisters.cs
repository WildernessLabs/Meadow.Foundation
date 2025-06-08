namespace Meadow.Foundation.IOExpanders;

public partial class T38i8o6do
{
    /// <summary>
    /// Register addresses for T38I8O6DO Modbus device
    /// </summary>
    private enum T38i806doRegisters
    {
        /// <summary>
        /// MODBUS_SERIALNUMBER
        /// </summary>
        Serialnumber = 0,

        /// <summary>
        /// MODBUS_FIRMWARE_VERSION
        /// </summary>
        FirmwareVersion = 4,

        /// <summary>
        /// MODBUS_ADDRESS
        /// </summary>
        Address = 6,

        /// <summary>
        /// MODBUS_PRODUCT_MODEL
        /// </summary>
        ProductModel = 7,

        /// <summary>
        /// MODBUS_HARDWARE_REV
        /// </summary>
        HardwareRev = 8,

        /// <summary>
        /// MODBUS_OUTPUT_DEAD_MASTER
        /// </summary>
        OutputDeadMaster = 30,

        /// <summary>
        /// MODBUS_AO_CHANNLE0
        /// </summary>
        AoChannel0 = 100,

        /// <summary>
        /// MODBUS_AO_CHANNLE1
        /// </summary>
        AoChannel1 = 101,

        /// <summary>
        /// MODBUS_AO_CHANNLE2
        /// </summary>
        AoChannel2 = 102,

        /// <summary>
        /// MODBUS_AO_CHANNLE3
        /// </summary>
        AoChannel3 = 103,

        /// <summary>
        /// MODBUS_AO_CHANNLE4
        /// </summary>
        AoChannel4 = 104,

        /// <summary>
        /// MODBUS_AO_CHANNLE5
        /// </summary>
        AoChannel5 = 105,

        /// <summary>
        /// MODBUS_AO_CHANNLE6
        /// </summary>
        AoChannel6 = 106,

        /// <summary>
        /// MODBUS_AO_CHANNLE7
        /// </summary>
        AoChannel7 = 107,

        /// <summary>
        /// MODBUS_DO_CHANNLE0
        /// </summary>
        DoChannel0 = 108,

        /// <summary>
        /// MODBUS_DO_CHANNLE1
        /// </summary>
        DoChannel1 = 109,

        /// <summary>
        /// MODBUS_DO_CHANNLE2
        /// </summary>
        DoChannel2 = 110,

        /// <summary>
        /// MODBUS_DO_CHANNLE3
        /// </summary>
        DoChannel3 = 111,

        /// <summary>
        /// MODBUS_DO_CHANNLE4
        /// </summary>
        DoChannel4 = 112,

        /// <summary>
        /// MODBUS_DO_CHANNLE5
        /// </summary>
        DoChannel5 = 113,

        /// <summary>
        /// MODBUS_AI_CHANNLE0
        /// </summary>
        AiChannel0 = 116,

        /// <summary>
        /// MODBUS_AI_CHANNLE1
        /// </summary>
        AiChannel1 = 118,

        /// <summary>
        /// MODBUS_AI_CHANNLE2
        /// </summary>
        AiChannel2 = 120,

        /// <summary>
        /// MODBUS_AI_CHANNLE3
        /// </summary>
        AiChannel3 = 122,

        /// <summary>
        /// MODBUS_AI_CHANNLE4
        /// </summary>
        AiChannel4 = 124,

        /// <summary>
        /// MODBUS_AI_CHANNLE5
        /// </summary>
        AiChannel5 = 126,

        /// <summary>
        /// MODBUS_AI_CHANNLE6
        /// </summary>
        AiChannel6 = 128,

        /// <summary>
        /// MODBUS_AI_CHANNLE7
        /// </summary>
        AiChannel7 = 130,

        /// <summary>
        /// MODBUS_AI_CHANNEL_JUMP0
        /// </summary>
        AiChannelJump0 = 140,

        /// <summary>
        /// MODBUS_AI_CHANNEL_JUMP1
        /// </summary>
        AiChannelJump1 = 141,

        /// <summary>
        /// MODBUS_AI_CHANNEL_JUMP2
        /// </summary>
        AiChannelJump2 = 142,

        /// <summary>
        /// MODBUS_AI_CHANNEL_JUMP3
        /// </summary>
        AiChannelJump3 = 143,

        /// <summary>
        /// MODBUS_AI_CHANNEL_JUMP4
        /// </summary>
        AiChannelJump4 = 144,

        /// <summary>
        /// MODBUS_AI_CHANNEL_JUMP5
        /// </summary>
        AiChannelJump5 = 145,

        /// <summary>
        /// MODBUS_AI_CHANNEL_JUMP6
        /// </summary>
        AiChannelJump6 = 146,

        /// <summary>
        /// MODBUS_AI_CHANNEL_JUMP7
        /// </summary>
        AiChannelJump7 = 147,

        /// <summary>
        /// MODBUS_AUTO_MANUAL0
        /// </summary>
        AutoManual0 = 149,

        /// <summary>
        /// MODBUS_AUTO_MANUAL1
        /// </summary>
        AutoManual1 = 150,

        /// <summary>
        /// MODBUS_AUTO_MANUAL2
        /// </summary>
        AutoManual2 = 151,

        /// <summary>
        /// MODBUS_AUTO_MANUAL3
        /// </summary>
        AutoManual3 = 152,

        /// <summary>
        /// MODBUS_AUTO_MANUAL4
        /// </summary>
        AutoManual4 = 153,

        /// <summary>
        /// MODBUS_AUTO_MANUAL5
        /// </summary>
        AutoManual5 = 154,

        /// <summary>
        /// MODBUS_AUTO_MANUAL6
        /// </summary>
        AutoManual6 = 155,

        /// <summary>
        /// MODBUS_AUTO_MANUAL7
        /// </summary>
        AutoManual7 = 156,

        /// <summary>
        /// MODBUS_AI_DI_AI0
        /// </summary>
        AiDiAi0 = 157,

        /// <summary>
        /// MODBUS_AI_DI_AI1
        /// </summary>
        AiDiAi1 = 158,

        /// <summary>
        /// MODBUS_AI_DI_AI2
        /// </summary>
        AiDiAi2 = 159,

        /// <summary>
        /// MODBUS_AI_DI_AI3
        /// </summary>
        AiDiAi3 = 160,

        /// <summary>
        /// MODBUS_AI_DI_AI4
        /// </summary>
        AiDiAi4 = 161,

        /// <summary>
        /// MODBUS_AI_DI_AI5
        /// </summary>
        AiDiAi5 = 162,

        /// <summary>
        /// MODBUS_AI_DI_AI6
        /// </summary>
        AiDiAi6 = 163,

        /// <summary>
        /// MODBUS_AI_DI_AI7
        /// </summary>
        AiDiAi7 = 164,

        /// <summary>
        /// MODBUS_CAL_SIGN0
        /// </summary>
        CalSign0 = 165,

        /// <summary>
        /// MODBUS_CAL_SIGN1
        /// </summary>
        CalSign1 = 166,

        /// <summary>
        /// MODBUS_CAL_SIGN2
        /// </summary>
        CalSign2 = 167,

        /// <summary>
        /// MODBUS_CAL_SIGN3
        /// </summary>
        CalSign3 = 168,

        /// <summary>
        /// MODBUS_CAL_SIGN4
        /// </summary>
        CalSign4 = 169,

        /// <summary>
        /// MODBUS_CAL_SIGN5
        /// </summary>
        CalSign5 = 170,

        /// <summary>
        /// MODBUS_CAL_SIGN6
        /// </summary>
        CalSign6 = 171,

        /// <summary>
        /// MODBUS_CAL_SIGN7
        /// </summary>
        CalSign7 = 172,

        /// <summary>
        /// MODBUS_CAL_VALUE0
        /// </summary>
        CalValue0 = 173,

        /// <summary>
        /// MODBUS_CAL_VALUE1
        /// </summary>
        CalValue1 = 175,

        /// <summary>
        /// MODBUS_CAL_VALUE2
        /// </summary>
        CalValue2 = 177,

        /// <summary>
        /// MODBUS_CAL_VALUE3
        /// </summary>
        CalValue3 = 179,

        /// <summary>
        /// MODBUS_CAL_VALUE4
        /// </summary>
        CalValue4 = 181,

        /// <summary>
        /// MODBUS_CAL_VALUE5
        /// </summary>
        CalValue5 = 183,

        /// <summary>
        /// MODBUS_CAL_VALUE6
        /// </summary>
        CalValue6 = 185,

        /// <summary>
        /// MODBUS_CAL_VALUE7
        /// </summary>
        CalValue7 = 187,

        /// <summary>
        /// MODBUS_AI_STATUS_0
        /// </summary>
        AiStatus0 = 189,

        /// <summary>
        /// MODBUS_AI_STATUS_1
        /// </summary>
        AiStatus1 = 190,

        /// <summary>
        /// MODBUS_AI_STATUS_2
        /// </summary>
        AiStatus2 = 191,

        /// <summary>
        /// MODBUS_AI_STATUS_3
        /// </summary>
        AiStatus3 = 192,

        /// <summary>
        /// MODBUS_AI_STATUS_4
        /// </summary>
        AiStatus4 = 193,

        /// <summary>
        /// MODBUS_AI_STATUS_5
        /// </summary>
        AiStatus5 = 194,

        /// <summary>
        /// MODBUS_AI_STATUS_6
        /// </summary>
        AiStatus6 = 195,

        /// <summary>
        /// MODBUS_AI_STATUS_7
        /// </summary>
        AiStatus7 = 196,

        /// <summary>
        /// MODBUS_AI_FILTER0
        /// </summary>
        AiFilter0 = 200,

        /// <summary>
        /// MODBUS_AI_FILTER1
        /// </summary>
        AiFilter1 = 201,

        /// <summary>
        /// MODBUS_AI_FILTER2
        /// </summary>
        AiFilter2 = 202,

        /// <summary>
        /// MODBUS_AI_FILTER3
        /// </summary>
        AiFilter3 = 203,

        /// <summary>
        /// MODBUS_AI_FILTER4
        /// </summary>
        AiFilter4 = 204,

        /// <summary>
        /// MODBUS_AI_FILTER5
        /// </summary>
        AiFilter5 = 205,

        /// <summary>
        /// MODBUS_AI_FILTER6
        /// </summary>
        AiFilter6 = 206,

        /// <summary>
        /// MODBUS_AI_FILTER7
        /// </summary>
        AiFilter7 = 207,

        /// <summary>
        /// MODBUS_AI_RANGE0
        /// </summary>
        AiRange0 = 225,

        /// <summary>
        /// MODBUS_AI_RANGE1
        /// </summary>
        AiRange1 = 226,

        /// <summary>
        /// MODBUS_AI_RANGE2
        /// </summary>
        AiRange2 = 227,

        /// <summary>
        /// MODBUS_AI_RANGE3
        /// </summary>
        AiRange3 = 228,

        /// <summary>
        /// MODBUS_AI_RANGE4
        /// </summary>
        AiRange4 = 229,

        /// <summary>
        /// MODBUS_AI_RANGE5
        /// </summary>
        AiRange5 = 230,

        /// <summary>
        /// MODBUS_AI_RANGE6
        /// </summary>
        AiRange6 = 231,

        /// <summary>
        /// MODBUS_AI_RANGE7
        /// </summary>
        AiRange7 = 232,

        /// <summary>
        /// MODBUS_AUTO_MANUAL_OUT0
        /// </summary>
        AutoManualOut0 = 240,

        /// <summary>
        /// MODBUS_AUTO_MANUAL_OUT1
        /// </summary>
        AutoManualOut1 = 241,

        /// <summary>
        /// MODBUS_AUTO_MANUAL_OUT2
        /// </summary>
        AutoManualOut2 = 242,

        /// <summary>
        /// MODBUS_AUTO_MANUAL_OUT3
        /// </summary>
        AutoManualOut3 = 243,

        /// <summary>
        /// MODBUS_AUTO_MANUAL_OUT4
        /// </summary>
        AutoManualOut4 = 244,

        /// <summary>
        /// MODBUS_AUTO_MANUAL_OUT5
        /// </summary>
        AutoManualOut5 = 245,

        /// <summary>
        /// MODBUS_AUTO_MANUAL_OUT6
        /// </summary>
        AutoManualOut6 = 246,

        /// <summary>
        /// MODBUS_AUTO_MANUAL_OUT7
        /// </summary>
        AutoManualOut7 = 247,

        /// <summary>
        /// MODBUS_AUTO_MANUAL_OUT8
        /// </summary>
        AutoManualOut8 = 248,

        /// <summary>
        /// MODBUS_AUTO_MANUAL_OUT9
        /// </summary>
        AutoManualOut9 = 249,

        /// <summary>
        /// MODBUS_AUTO_MANUAL_OUT10
        /// </summary>
        AutoManualOut10 = 250,

        /// <summary>
        /// MODBUS_AUTO_MANUAL_OUT11
        /// </summary>
        AutoManualOut11 = 251,

        /// <summary>
        /// MODBUS_AUTO_MANUAL_OUT12
        /// </summary>
        AutoManualOut12 = 252,

        /// <summary>
        /// MODBUS_AUTO_MANUAL_OUT13
        /// </summary>
        AutoManualOut13 = 253,

        /// <summary>
        /// MODBUS_OUTPUT_STATUS_0
        /// </summary>
        OutputStatus0 = 254,

        /// <summary>
        /// MODBUS_OUTPUT_STATUS_1
        /// </summary>
        OutputStatus1 = 255,

        /// <summary>
        /// MODBUS_OUTPUT_STATUS_2
        /// </summary>
        OutputStatus2 = 256,

        /// <summary>
        /// MODBUS_OUTPUT_STATUS_3
        /// </summary>
        OutputStatus3 = 257,

        /// <summary>
        /// MODBUS_OUTPUT_STATUS_4
        /// </summary>
        OutputStatus4 = 258,

        /// <summary>
        /// MODBUS_OUTPUT_STATUS_5
        /// </summary>
        OutputStatus5 = 259,

        /// <summary>
        /// MODBUS_OUTPUT_STATUS_6
        /// </summary>
        OutputStatus6 = 260,

        /// <summary>
        /// MODBUS_OUTPUT_STATUS_7
        /// </summary>
        OutputStatus7 = 261,

        /// <summary>
        /// MODBUS_OUTPUT_STATUS_8
        /// </summary>
        OutputStatus8 = 262,

        /// <summary>
        /// MODBUS_OUTPUT_STATUS_9
        /// </summary>
        OutputStatus9 = 263,

        /// <summary>
        /// MODBUS_OUTPUT_STATUS_10
        /// </summary>
        OutputStatus10 = 264,

        /// <summary>
        /// MODBUS_OUTPUT_STATUS_11
        /// </summary>
        OutputStatus11 = 265,

        /// <summary>
        /// MODBUS_OUTPUT_STATUS_12
        /// </summary>
        OutputStatus12 = 266,

        /// <summary>
        /// MODBUS_OUTPUT_STATUS_13
        /// </summary>
        OutputStatus13 = 267,

        /// <summary>
        /// MODBUS_OUTPUT_RANGE_0
        /// </summary>
        OutputRange0 = 268,

        /// <summary>
        /// MODBUS_OUTPUT_RANGE_1
        /// </summary>
        OutputRange1 = 269,

        /// <summary>
        /// MODBUS_OUTPUT_RANGE_2
        /// </summary>
        OutputRange2 = 270,

        /// <summary>
        /// MODBUS_OUTPUT_RANGE_3
        /// </summary>
        OutputRange3 = 271,

        /// <summary>
        /// MODBUS_OUTPUT_RANGE_4
        /// </summary>
        OutputRange4 = 272,

        /// <summary>
        /// MODBUS_OUTPUT_RANGE_5
        /// </summary>
        OutputRange5 = 273,

        /// <summary>
        /// MODBUS_OUTPUT_RANGE_6
        /// </summary>
        OutputRange6 = 274,

        /// <summary>
        /// MODBUS_OUTPUT_RANGE_7
        /// </summary>
        OutputRange7 = 275,

        /// <summary>
        /// MODBUS_OUTPUT_RANGE_8
        /// </summary>
        OutputRange8 = 276,

        /// <summary>
        /// MODBUS_OUTPUT_RANGE_9
        /// </summary>
        OutputRange9 = 277,

        /// <summary>
        /// MODBUS_OUTPUT_RANGE_10
        /// </summary>
        OutputRange10 = 278,

        /// <summary>
        /// MODBUS_OUTPUT_RANGE_11
        /// </summary>
        OutputRange11 = 279,

        /// <summary>
        /// MODBUS_OUTPUT_RANGE_12
        /// </summary>
        OutputRange12 = 280,

        /// <summary>
        /// MODBUS_OUTPUT_RANGE_13
        /// </summary>
        OutputRange13 = 281,

        /// <summary>
        /// MODBUS_OUTPUT_SWITCH1
        /// </summary>
        OutputSwitch1 = 282,

        /// <summary>
        /// MODBUS_OUTPUT_SWITCH2
        /// </summary>
        OutputSwitch2 = 283,

        /// <summary>
        /// MODBUS_OUTPUT_SWITCH3
        /// </summary>
        OutputSwitch3 = 284,

        /// <summary>
        /// MODBUS_OUTPUT_SWITCH4
        /// </summary>
        OutputSwitch4 = 285,

        /// <summary>
        /// MODBUS_OUTPUT_SWITCH5
        /// </summary>
        OutputSwitch5 = 286,

        /// <summary>
        /// MODBUS_OUTPUT_SWITCH6
        /// </summary>
        OutputSwitch6 = 287,

        /// <summary>
        /// MODBUS_OUTPUT_SWITCH7
        /// </summary>
        OutputSwitch7 = 288,

        /// <summary>
        /// MODBUS_OUTPUT_SWITCH8
        /// </summary>
        OutputSwitch8 = 289,

        /// <summary>
        /// MODBUS_OUTPUT_SWITCH9
        /// </summary>
        OutputSwitch9 = 290,

        /// <summary>
        /// MODBUS_OUTPUT_SWITCH10
        /// </summary>
        OutputSwitch10 = 291,

        /// <summary>
        /// MODBUS_OUTPUT_SWITCH11
        /// </summary>
        OutputSwitch11 = 292,

        /// <summary>
        /// MODBUS_OUTPUT_SWITCH12
        /// </summary>
        OutputSwitch12 = 293,

        /// <summary>
        /// MODBUS_OUTPUT_SWITCH13
        /// </summary>
        OutputSwitch13 = 294,

        /// <summary>
        /// MODBUS_OUTPUT_SWITCH14
        /// </summary>
        OutputSwitch14 = 295,

        /// <summary>
        /// MODBUS_OUTPUT_AD0
        /// </summary>
        OutputAd0 = 342,

        /// <summary>
        /// MODBUS_OUTPUT_AD1
        /// </summary>
        OutputAd1 = 343,

        /// <summary>
        /// MODBUS_OUTPUT_AD2
        /// </summary>
        OutputAd2 = 344,

        /// <summary>
        /// MODBUS_OUTPUT_AD3
        /// </summary>
        OutputAd3 = 345,

        /// <summary>
        /// MODBUS_OUTPUT_AD4
        /// </summary>
        OutputAd4 = 346,

        /// <summary>
        /// MODBUS_OUTPUT_AD5
        /// </summary>
        OutputAd5 = 347,

        /// <summary>
        /// MODBUS_OUTPUT_AD6
        /// </summary>
        OutputAd6 = 348,

        /// <summary>
        /// MODBUS_OUTPUT_AD7
        /// </summary>
        OutputAd7 = 349,

        /// <summary>
        /// MODBUS_OUTPUT_AD8
        /// </summary>
        OutputAd8 = 350,

        /// <summary>
        /// MODBUS_OUTPUT_AD9
        /// </summary>
        OutputAd9 = 351,

        /// <summary>
        /// MODBUS_OUTPUT_AD10
        /// </summary>
        OutputAd10 = 352,

        /// <summary>
        /// MODBUS_OUTPUT_AD11
        /// </summary>
        OutputAd11 = 353,

        /// <summary>
        /// MODBUS_OUTPUT_AD12
        /// </summary>
        OutputAd12 = 354,
    }
}