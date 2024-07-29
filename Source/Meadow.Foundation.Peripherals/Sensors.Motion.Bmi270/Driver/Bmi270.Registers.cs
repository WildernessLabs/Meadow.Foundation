namespace Meadow.Foundation.Sensors.Motion
{
    public partial class Bmi270
    {
        //BMI270 feature input start addresses
        const byte CONFIG_ID_STRT = 0x00;
        const byte MAX_BURST_LEN_STRT = 0x02;
        const byte CRT_GYRO_SELF_TEST_STRT = 0x03;
        const byte ABORT_STRT = 0x03;
        const byte AXIS_MAP_STRT = 0x04;
        const byte GYRO_SELF_OFF_STRT = 0x05;
        const byte NVM_PROG_PREP_STRT = 0x05;
        const byte GYRO_GAIN_UPDATE_STRT = 0x06;
        const byte ANY_MOT_STRT = 0x0C;
        const byte NO_MOT_STRT = 0x00;
        const byte SIG_MOT_STRT = 0x04;
        const byte STEP_CNT_1_STRT = 0x00;
        const byte STEP_CNT_4_STRT = 0x02;
        const byte WRIST_GEST_STRT = 0x06;
        const byte WRIST_WEAR_WAKE_UP_STRT = 0x00;

        //BMI270 feature output start addresses
        const byte BMI270_STEP_CNT_OUT_STRT = 0x00;
        const byte BMI270_STEP_ACT_OUT_STRT = 0x04;
        const byte BMI270_WRIST_GEST_OUT_STRT = 0x06;
        const byte BMI270_GYR_USER_GAIN_OUT_STRT = 0x08;
        const byte BMI270_GYRO_CROSS_SENSE_STRT = 0x0C;
        const byte BMI270_NVM_VFRM_OUT_STRT = 0x0E;

        //Register addresses 
        const byte CHIP_ID = 0x00;
        const byte STATUS = 0x03;
        const byte AUX_X_LSB = 0x04;
        const byte ACC_X_LSB = 0x0C;
        const byte GYR_X_LSB = 0x12;
        const byte SENSORTIME = 0x18;
        const byte EVENT = 0x1B;
        const byte INT_STATUS_0 = 0x1C;
        const byte INT_STATUS_1 = 0x1D;
        const byte SC_OUT_0 = 0x1E;
        const byte SYNC_COMMAND = 0x1E;
        const byte GYR_CAS_GPIO0 = 0x1E;
        const byte INTERNAL_STATUS = 0x21;
        const byte TEMPERATURE_0 = 0x22;
        const byte TEMPERATURE_1 = 0x23;
        const byte FIFO_LENGTH_0 = 0x24;
        const byte FIFO_DATA = 0x26;
        const byte FEAT_PAGE = 0x2F;
        const byte FEATURES_REG = 0x30;
        const byte ACC_CONF = 0x40;
        const byte ACC_RANGE = 0x41;
        const byte GYR_CONF = 0x42;
        const byte GYR_RANGE = 0x43;
        const byte AUX_CONF = 0x44;
        const byte FIFO_DOWNS = 0x45;
        const byte FIFO_WTM_0 = 0x46;
        const byte FIFO_WTM_1 = 0x47;
        const byte FIFO_CONFIG_0 = 0x48;
        const byte FIFO_CONFIG_1 = 0x49;
        const byte AUX_DEV_ID = 0x4B;
        const byte AUX_IF_CONF = 0x4C;
        const byte AUX_RD = 0x4D;
        const byte AUX_WR = 0x4E;
        const byte AUX_WR_DATA = 0x4F;
        const byte INT1_IO_CTRL = 0x53;
        const byte INT2_IO_CTRL = 0x54;
        const byte INT_LATCH = 0x55;
        const byte INT1_MAP_FEAT = 0x56;
        const byte INT2_MAP_FEAT = 0x57;
        const byte INT_MAP_DATA = 0x58;
        const byte INIT_CTRL = 0x59;
        const byte INIT_0 = 0x5B;
        const byte INIT_1 = 0x5C;
        const byte INIT_DATA = 0x5E;
        const byte AUX_IF_TRIM = 0x68;
        const byte GYR_CRT_CONF = 0x69;
        const byte NVM_CONF = 0x6A;
        const byte IF_CONF = 0x6B;
        const byte ACC_SELF_TEST = 0x6D;
        const byte GYR_SELF_TEST_AXES = 0x6E;
        const byte SELF_TEST_MEMS = 0x6F;
        const byte NV_CONF = 0x70;
        const byte ACC_OFF_COMP_0 = 0x71;
        const byte GYR_OFF_COMP_3 = 0x74;
        const byte GYR_OFF_COMP_6 = 0x77;
        const byte GYR_USR_GAIN_0 = 0x78;
        const byte PWR_CONF = 0x7C;
        const byte PWR_CTRL = 0x7D;
        const byte CMD_REG = 0x7E;
    }
}