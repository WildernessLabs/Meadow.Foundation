namespace Meadow.Foundation.IOExpanders;

public partial class T322ai
{
    /// <summary>
    /// Register addresses for T322AI Modbus device
    /// </summary>
    private enum T322aiRegisters
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
        /// Modbus comms baud rate
        /// </summary>
        BaudRate = 15,

        /// <summary>
        /// MODBUS_AI_CHANNLE0_HI
        /// </summary>
        AiChannel0Hi = 100,

        /// <summary>
        /// MODBUS_AI_CHANNLE0_LO
        /// </summary>
        AiChannel0Lo = 101,

        /// <summary>
        /// MODBUS_AI_CHANNLE1_HI
        /// </summary>
        AiChannel1Hi = 102,

        /// <summary>
        /// MODBUS_AI_CHANNLE1_LO
        /// </summary>
        AiChannel1Lo = 103,

        /// <summary>
        /// MODBUS_AI_CHANNLE2_HI
        /// </summary>
        AiChannel2Hi = 104,

        /// <summary>
        /// MODBUS_AI_CHANNLE2_LO
        /// </summary>
        AiChannel2Lo = 105,

        /// <summary>
        /// MODBUS_AI_CHANNLE3_HI
        /// </summary>
        AiChannel3Hi = 106,

        /// <summary>
        /// MODBUS_AI_CHANNLE3_LO
        /// </summary>
        AiChannel3Lo = 107,

        /// <summary>
        /// MODBUS_AI_CHANNLE4_HI
        /// </summary>
        AiChannel4Hi = 108,

        /// <summary>
        /// MODBUS_AI_CHANNLE4_LO
        /// </summary>
        AiChannel4Lo = 109,

        /// <summary>
        /// MODBUS_AI_CHANNLE5_HI
        /// </summary>
        AiChannel5Hi = 110,

        /// <summary>
        /// MODBUS_AI_CHANNLE5_LO
        /// </summary>
        AiChannel5Lo = 111,

        /// <summary>
        /// MODBUS_AI_CHANNLE6_HI
        /// </summary>
        AiChannel6Hi = 112,

        /// <summary>
        /// MODBUS_AI_CHANNLE6_LO
        /// </summary>
        AiChannel6Lo = 113,

        /// <summary>
        /// MODBUS_AI_CHANNLE7_HI
        /// </summary>
        AiChannel7Hi = 114,

        /// <summary>
        /// MODBUS_AI_CHANNLE7_LO
        /// </summary>
        AiChannel7Lo = 115,

        /// <summary>
        /// MODBUS_AI_CHANNLE8_HI
        /// </summary>
        AiChannel8Hi = 116,

        /// <summary>
        /// MODBUS_AI_CHANNLE8_LO
        /// </summary>
        AiChannel8Lo = 117,

        /// <summary>
        /// MODBUS_AI_CHANNLE9_HI
        /// </summary>
        AiChannel9Hi = 118,

        /// <summary>
        /// MODBUS_AI_CHANNLE9_LO
        /// </summary>
        AiChannel9Lo = 119,

        /// <summary>
        /// MODBUS_AI_CHANNLE10_HI
        /// </summary>
        AiChannel10Hi = 120,

        /// <summary>
        /// MODBUS_AI_CHANNLE10_LO
        /// </summary>
        AiChannel10Lo = 121,

        /// <summary>
        /// MODBUS_AI_CHANNLE11_HI
        /// </summary>
        AiChannel11Hi = 122,

        /// <summary>
        /// MODBUS_AI_CHANNLE11_LO
        /// </summary>
        AiChannel11Lo = 123,

        /// <summary>
        /// MODBUS_AI_CHANNLE12_HI
        /// </summary>
        AiChannel12Hi = 124,

        /// <summary>
        /// MODBUS_AI_CHANNLE12_LO
        /// </summary>
        AiChannel12Lo = 125,

        /// <summary>
        /// MODBUS_AI_CHANNLE13_HI
        /// </summary>
        AiChannel13Hi = 126,

        /// <summary>
        /// MODBUS_AI_CHANNLE13_LO
        /// </summary>
        AiChannel13Lo = 127,

        /// <summary>
        /// MODBUS_AI_CHANNLE14_HI
        /// </summary>
        AiChannel14Hi = 128,

        /// <summary>
        /// MODBUS_AI_CHANNLE14_LO
        /// </summary>
        AiChannel14Lo = 129,

        /// <summary>
        /// MODBUS_AI_CHANNLE15_HI
        /// </summary>
        AiChannel15Hi = 130,

        /// <summary>
        /// MODBUS_AI_CHANNLE15_LO
        /// </summary>
        AiChannel15Lo = 131,

        /// <summary>
        /// MODBUS_AI_CHANNLE16_HI
        /// </summary>
        AiChannel16Hi = 132,

        /// <summary>
        /// MODBUS_AI_CHANNLE16_LO
        /// </summary>
        AiChannel16Lo = 133,

        /// <summary>
        /// MODBUS_AI_CHANNLE17_HI
        /// </summary>
        AiChannel17Hi = 134,

        /// <summary>
        /// MODBUS_AI_CHANNLE17_LO
        /// </summary>
        AiChannel17Lo = 135,

        /// <summary>
        /// MODBUS_AI_CHANNLE18_HI
        /// </summary>
        AiChannel18Hi = 136,

        /// <summary>
        /// MODBUS_AI_CHANNLE18_LO
        /// </summary>
        AiChannel18Lo = 137,

        /// <summary>
        /// MODBUS_AI_CHANNLE19_HI
        /// </summary>
        AiChannel19Hi = 138,

        /// <summary>
        /// MODBUS_AI_CHANNLE19_LO
        /// </summary>
        AiChannel19Lo = 139,

        /// <summary>
        /// MODBUS_AI_CHANNLE20_HI
        /// </summary>
        AiChannel20Hi = 140,

        /// <summary>
        /// MODBUS_AI_CHANNLE20_LO
        /// </summary>
        AiChannel20Lo = 141,

        /// <summary>
        /// MODBUS_AI_CHANNLE21_HI
        /// </summary>
        AiChannel21Hi = 142,

        /// <summary>
        /// MODBUS_AI_CHANNLE21_LO
        /// </summary>
        AiChannel21Lo = 143,

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
        /// MODBUS_AI_FILTER8
        /// </summary>
        AiFilter8 = 208,

        /// <summary>
        /// MODBUS_AI_FILTER9
        /// </summary>
        AiFilter9 = 209,

        /// <summary>
        /// MODBUS_AI_FILTER10
        /// </summary>
        AiFilter10 = 210,

        /// <summary>
        /// MODBUS_AI_FILTER11
        /// </summary>
        AiFilter11 = 211,

        /// <summary>
        /// MODBUS_AI_FILTER12
        /// </summary>
        AiFilter12 = 212,

        /// <summary>
        /// MODBUS_AI_FILTER13
        /// </summary>
        AiFilter13 = 213,

        /// <summary>
        /// MODBUS_AI_FILTER14
        /// </summary>
        AiFilter14 = 214,

        /// <summary>
        /// MODBUS_AI_FILTER15
        /// </summary>
        AiFilter15 = 215,

        /// <summary>
        /// MODBUS_AI_FILTER16
        /// </summary>
        AiFilter16 = 216,

        /// <summary>
        /// MODBUS_AI_FILTER17
        /// </summary>
        AiFilter17 = 217,

        /// <summary>
        /// MODBUS_AI_FILTER18
        /// </summary>
        AiFilter18 = 218,

        /// <summary>
        /// MODBUS_AI_FILTER19
        /// </summary>
        AiFilter19 = 219,

        /// <summary>
        /// MODBUS_AI_FILTER20
        /// </summary>
        AiFilter20 = 220,

        /// <summary>
        /// MODBUS_AI_FILTER21
        /// </summary>
        AiFilter21 = 221,

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
        /// MODBUS_AI_RANGE8
        /// </summary>
        AiRange8 = 233,

        /// <summary>
        /// MODBUS_AI_RANGE9
        /// </summary>
        AiRange9 = 234,

        /// <summary>
        /// MODBUS_AI_RANGE10
        /// </summary>
        AiRange10 = 235,

        /// <summary>
        /// MODBUS_AI_RANGE11
        /// </summary>
        AiRange11 = 236,

        /// <summary>
        /// MODBUS_AI_RANGE12
        /// </summary>
        AiRange12 = 237,

        /// <summary>
        /// MODBUS_AI_RANGE13
        /// </summary>
        AiRange13 = 238,

        /// <summary>
        /// MODBUS_AI_RANGE14
        /// </summary>
        AiRange14 = 239,

        /// <summary>
        /// MODBUS_AI_RANGE15
        /// </summary>
        AiRange15 = 240,

        /// <summary>
        /// MODBUS_AI_RANGE16
        /// </summary>
        AiRange16 = 241,

        /// <summary>
        /// MODBUS_AI_RANGE17
        /// </summary>
        AiRange17 = 242,

        /// <summary>
        /// MODBUS_AI_RANGE18
        /// </summary>
        AiRange18 = 243,

        /// <summary>
        /// MODBUS_AI_RANGE19
        /// </summary>
        AiRange19 = 244,

        /// <summary>
        /// MODBUS_AI_RANGE20
        /// </summary>
        AiRange20 = 245,

        /// <summary>
        /// MODBUS_AI_RANGE21
        /// </summary>
        AiRange21 = 246,

        /// <summary>
        /// MODBUS_AUTO_MANUAL_0
        /// </summary>
        AutoManual0 = 247,

        /// <summary>
        /// MODBUS_AUTO_MANUAL_1
        /// </summary>
        AutoManual1 = 248,

        /// <summary>
        /// MODBUS_AUTO_MANUAL_2
        /// </summary>
        AutoManual2 = 249,

        /// <summary>
        /// MODBUS_AUTO_MANUAL_3
        /// </summary>
        AutoManual3 = 250,

        /// <summary>
        /// MODBUS_AUTO_MANUAL_4
        /// </summary>
        AutoManual4 = 251,

        /// <summary>
        /// MODBUS_AUTO_MANUAL_5
        /// </summary>
        AutoManual5 = 252,

        /// <summary>
        /// MODBUS_AUTO_MANUAL_6
        /// </summary>
        AutoManual6 = 253,

        /// <summary>
        /// MODBUS_AUTO_MANUAL_7
        /// </summary>
        AutoManual7 = 254,

        /// <summary>
        /// MODBUS_AUTO_MANUAL_8
        /// </summary>
        AutoManual8 = 255,

        /// <summary>
        /// MODBUS_AUTO_MANUAL_9
        /// </summary>
        AutoManual9 = 256,

        /// <summary>
        /// MODBUS_AUTO_MANUAL_10
        /// </summary>
        AutoManual10 = 257,

        /// <summary>
        /// MODBUS_AUTO_MANUAL_11
        /// </summary>
        AutoManual11 = 258,

        /// <summary>
        /// MODBUS_AUTO_MANUAL_12
        /// </summary>
        AutoManual12 = 259,

        /// <summary>
        /// MODBUS_AUTO_MANUAL_13
        /// </summary>
        AutoManual13 = 260,

        /// <summary>
        /// MODBUS_AUTO_MANUAL_14
        /// </summary>
        AutoManual14 = 261,

        /// <summary>
        /// MODBUS_AUTO_MANUAL_15
        /// </summary>
        AutoManual15 = 262,

        /// <summary>
        /// MODBUS_AUTO_MANUAL_16
        /// </summary>
        AutoManual16 = 263,

        /// <summary>
        /// MODBUS_AUTO_MANUAL_17
        /// </summary>
        AutoManual17 = 264,

        /// <summary>
        /// MODBUS_AUTO_MANUAL_18
        /// </summary>
        AutoManual18 = 265,

        /// <summary>
        /// MODBUS_AUTO_MANUAL_19
        /// </summary>
        AutoManual19 = 266,

        /// <summary>
        /// MODBUS_AUTO_MANUAL_20
        /// </summary>
        AutoManual20 = 267,

        /// <summary>
        /// MODBUS_AUTO_MANUAL_21
        /// </summary>
        AutoManual21 = 268,

        /// <summary>
        /// MODBUS_AI_DI_AI0
        /// </summary>
        AiDiAi0 = 269,

        /// <summary>
        /// MODBUS_AI_DI_AI1
        /// </summary>
        AiDiAi1 = 270,

        /// <summary>
        /// MODBUS_AI_DI_AI2
        /// </summary>
        AiDiAi2 = 271,

        /// <summary>
        /// MODBUS_AI_DI_AI3
        /// </summary>
        AiDiAi3 = 272,

        /// <summary>
        /// MODBUS_AI_DI_AI4
        /// </summary>
        AiDiAi4 = 273,

        /// <summary>
        /// MODBUS_AI_DI_AI5
        /// </summary>
        AiDiAi5 = 274,

        /// <summary>
        /// MODBUS_AI_DI_AI6
        /// </summary>
        AiDiAi6 = 275,

        /// <summary>
        /// MODBUS_AI_DI_AI7
        /// </summary>
        AiDiAi7 = 276,

        /// <summary>
        /// MODBUS_AI_DI_AI8
        /// </summary>
        AiDiAi8 = 277,

        /// <summary>
        /// MODBUS_AI_DI_AI9
        /// </summary>
        AiDiAi9 = 278,

        /// <summary>
        /// MODBUS_AI_DI_AI10
        /// </summary>
        AiDiAi10 = 279,

        /// <summary>
        /// MODBUS_AI_DI_AI11
        /// </summary>
        AiDiAi11 = 280,

        /// <summary>
        /// MODBUS_AI_DI_AI12
        /// </summary>
        AiDiAi12 = 281,

        /// <summary>
        /// MODBUS_AI_DI_AI13
        /// </summary>
        AiDiAi13 = 282,

        /// <summary>
        /// MODBUS_AI_DI_AI14
        /// </summary>
        AiDiAi14 = 283,

        /// <summary>
        /// MODBUS_AI_DI_AI15
        /// </summary>
        AiDiAi15 = 284,

        /// <summary>
        /// MODBUS_AI_DI_AI16
        /// </summary>
        AiDiAi16 = 285,

        /// <summary>
        /// MODBUS_AI_DI_AI17
        /// </summary>
        AiDiAi17 = 286,

        /// <summary>
        /// MODBUS_AI_DI_AI18
        /// </summary>
        AiDiAi18 = 287,

        /// <summary>
        /// MODBUS_AI_DI_AI19
        /// </summary>
        AiDiAi19 = 288,

        /// <summary>
        /// MODBUS_AI_DI_AI20
        /// </summary>
        AiDiAi20 = 289,

        /// <summary>
        /// MODBUS_AI_DI_AI21
        /// </summary>
        AiDiAi21 = 290,

        /// <summary>
        /// MODBUS_AI_JUPER0
        /// </summary>
        AiJuper0 = 291,

        /// <summary>
        /// MODBUS_AI_JUPER1
        /// </summary>
        AiJuper1 = 292,

        /// <summary>
        /// MODBUS_AI_JUPER2
        /// </summary>
        AiJuper2 = 293,

        /// <summary>
        /// MODBUS_AI_JUPER3
        /// </summary>
        AiJuper3 = 294,

        /// <summary>
        /// MODBUS_AI_JUPER4
        /// </summary>
        AiJuper4 = 295,

        /// <summary>
        /// MODBUS_AI_JUPER5
        /// </summary>
        AiJuper5 = 296,

        /// <summary>
        /// MODBUS_AI_JUPER6
        /// </summary>
        AiJuper6 = 297,

        /// <summary>
        /// MODBUS_AI_JUPER7
        /// </summary>
        AiJuper7 = 298,

        /// <summary>
        /// MODBUS_AI_JUPER8
        /// </summary>
        AiJuper8 = 299,

        /// <summary>
        /// MODBUS_AI_JUPER9
        /// </summary>
        AiJuper9 = 300,

        /// <summary>
        /// MODBUS_AI_JUPER10
        /// </summary>
        AiJuper10 = 301,

        /// <summary>
        /// MODBUS_AI_JUPER11
        /// </summary>
        AiJuper11 = 302,

        /// <summary>
        /// MODBUS_AI_JUPER12
        /// </summary>
        AiJuper12 = 303,

        /// <summary>
        /// MODBUS_AI_JUPER13
        /// </summary>
        AiJuper13 = 304,

        /// <summary>
        /// MODBUS_AI_JUPER14
        /// </summary>
        AiJuper14 = 305,

        /// <summary>
        /// MODBUS_AI_JUPER15
        /// </summary>
        AiJuper15 = 306,

        /// <summary>
        /// MODBUS_AI_JUPER16
        /// </summary>
        AiJuper16 = 307,

        /// <summary>
        /// MODBUS_AI_JUPER17
        /// </summary>
        AiJuper17 = 308,

        /// <summary>
        /// MODBUS_AI_JUPER18
        /// </summary>
        AiJuper18 = 309,

        /// <summary>
        /// MODBUS_AI_JUPER19
        /// </summary>
        AiJuper19 = 310,

        /// <summary>
        /// MODBUS_AI_JUPER20
        /// </summary>
        AiJuper20 = 311,

        /// <summary>
        /// MODBUS_AI_JUPER21
        /// </summary>
        AiJuper21 = 312,

        /// <summary>
        /// MODBUS_CAL_SIGN0
        /// </summary>
        CalSign0 = 313,

        /// <summary>
        /// MODBUS_CAL_SIGN1
        /// </summary>
        CalSign1 = 314,

        /// <summary>
        /// MODBUS_CAL_SIGN2
        /// </summary>
        CalSign2 = 315,

        /// <summary>
        /// MODBUS_CAL_SIGN3
        /// </summary>
        CalSign3 = 316,

        /// <summary>
        /// MODBUS_CAL_SIGN4
        /// </summary>
        CalSign4 = 317,

        /// <summary>
        /// MODBUS_CAL_SIGN5
        /// </summary>
        CalSign5 = 318,

        /// <summary>
        /// MODBUS_CAL_SIGN6
        /// </summary>
        CalSign6 = 319,

        /// <summary>
        /// MODBUS_CAL_SIGN7
        /// </summary>
        CalSign7 = 320,

        /// <summary>
        /// MODBUS_CAL_SIGN8
        /// </summary>
        CalSign8 = 321,

        /// <summary>
        /// MODBUS_CAL_SIGN9
        /// </summary>
        CalSign9 = 322,

        /// <summary>
        /// MODBUS_CAL_SIGN10
        /// </summary>
        CalSign10 = 323,

        /// <summary>
        /// MODBUS_CAL_SIGN11
        /// </summary>
        CalSign11 = 324,
    }
}