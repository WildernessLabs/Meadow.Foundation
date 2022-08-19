namespace Meadow.Foundation.Sensors.Accelerometers
{
    public partial class Bmi270
    {
        /*! @name BMI270 feature input start addresses */
        const byte BMI270_CONFIG_ID_STRT_ADDR           = 0x00;
        const byte BMI270_MAX_BURST_LEN_STRT_ADDR       = 0x02;
        const byte BMI270_CRT_GYRO_SELF_TEST_STRT_ADDR  = 0x03;
        const byte BMI270_ABORT_STRT_ADDR               = 0x03;
        const byte BMI270_AXIS_MAP_STRT_ADDR            = 0x04;
        const byte BMI270_GYRO_SELF_OFF_STRT_ADDR       = 0x05;
        const byte BMI270_NVM_PROG_PREP_STRT_ADDR       = 0x05;
        const byte BMI270_GYRO_GAIN_UPDATE_STRT_ADDR    = 0x06;
        const byte BMI270_ANY_MOT_STRT_ADDR             = 0x0C;
        const byte BMI270_NO_MOT_STRT_ADDR              = 0x00;
        const byte BMI270_SIG_MOT_STRT_ADDR             = 0x04;
        const byte BMI270_STEP_CNT_1_STRT_ADDR          = 0x00;
        const byte BMI270_STEP_CNT_4_STRT_ADDR          = 0x02;
        const byte BMI270_WRIST_GEST_STRT_ADDR          = 0x06;
        const byte BMI270_WRIST_WEAR_WAKE_UP_STRT_ADDR  = 0x00;

        /*! @name BMI270 feature output start addresses */
        const byte BMI270_STEP_CNT_OUT_STRT_ADDR        = 0x00;
        const byte BMI270_STEP_ACT_OUT_STRT_ADDR        = 0x04;
        const byte BMI270_WRIST_GEST_OUT_STRT_ADDR      = 0x06;
        const byte BMI270_GYR_USER_GAIN_OUT_STRT_ADDR   = 0x08;
        const byte BMI270_GYRO_CROSS_SENSE_STRT_ADDR    = 0x0C;
        const byte BMI270_NVM_VFRM_OUT_STRT_ADDR        = 0x0E;

        /*! @name BMI2 register addresses */
        const byte BMI2_CHIP_ID_ADDR                    = 0x00;
        const byte BMI2_STATUS_ADDR                     = 0x03;
        const byte BMI2_AUX_X_LSB_ADDR                  = 0x04;
        const byte BMI2_ACC_X_LSB_ADDR                  = 0x0C;
        const byte BMI2_GYR_X_LSB_ADDR                  = 0x12;
        const byte BMI2_SENSORTIME_ADDR                 = 0x18;
        const byte BMI2_EVENT_ADDR                      = 0x1B;
        const byte BMI2_INT_STATUS_0_ADDR               = 0x1C;
        const byte BMI2_INT_STATUS_1_ADDR               = 0x1D;
        const byte BMI2_SC_OUT_0_ADDR                   = 0x1E;
        const byte BMI2_SYNC_COMMAND_ADDR               = 0x1E;
        const byte BMI2_GYR_CAS_GPIO0_ADDR              = 0x1E;
        const byte BMI2_INTERNAL_STATUS_ADDR            = 0x21;
        const byte BMI2_FIFO_LENGTH_0_ADDR              = 0x24;
        const byte BMI2_FIFO_DATA_ADDR                  = 0x26;
        const byte BMI2_FEAT_PAGE_ADDR                  = 0x2F;
        const byte BMI2_FEATURES_REG_ADDR               = 0x30;
        const byte BMI2_ACC_CONF_ADDR                   = 0x40;
        const byte BMI2_GYR_CONF_ADDR                   = 0x42;
        const byte BMI2_AUX_CONF_ADDR                   = 0x44;
        const byte BMI2_FIFO_DOWNS_ADDR                 = 0x45;
        const byte BMI2_FIFO_WTM_0_ADDR                 = 0x46;
        const byte BMI2_FIFO_WTM_1_ADDR                 = 0x47;
        const byte BMI2_FIFO_CONFIG_0_ADDR              = 0x48;
        const byte BMI2_FIFO_CONFIG_1_ADDR              = 0x49;
        const byte BMI2_AUX_DEV_ID_ADDR                 = 0x4B;
        const byte BMI2_AUX_IF_CONF_ADDR                = 0x4C;
        const byte BMI2_AUX_RD_ADDR                     = 0x4D;
        const byte BMI2_AUX_WR_ADDR                     = 0x4E;
        const byte BMI2_AUX_WR_DATA_ADDR                = 0x4F;
        const byte BMI2_INT1_IO_CTRL_ADDR               = 0x53;
        const byte BMI2_INT2_IO_CTRL_ADDR               = 0x54;
        const byte BMI2_INT_LATCH_ADDR                  = 0x55;
        const byte BMI2_INT1_MAP_FEAT_ADDR              = 0x56;
        const byte BMI2_INT2_MAP_FEAT_ADDR              = 0x57;
        const byte BMI2_INT_MAP_DATA_ADDR               = 0x58;
        const byte BMI2_INIT_CTRL_ADDR                  = 0x59;
        const byte BMI2_INIT_ADDR_0                     = 0x5B;
        const byte BMI2_INIT_ADDR_1                     = 0x5C;
        const byte BMI2_INIT_DATA_ADDR                  = 0x5E;
        const byte BMI2_AUX_IF_TRIM                     = 0x68;
        const byte BMI2_GYR_CRT_CONF_ADDR               = 0x69;
        const byte BMI2_NVM_CONF_ADDR                   = 0x6A;
        const byte BMI2_IF_CONF_ADDR                    = 0x6B;
        const byte BMI2_ACC_SELF_TEST_ADDR              = 0x6D;
        const byte BMI2_GYR_SELF_TEST_AXES_ADDR         = 0x6E;
        const byte BMI2_SELF_TEST_MEMS_ADDR             = 0x6F;
        const byte BMI2_NV_CONF_ADDR                    = 0x70;
        const byte BMI2_ACC_OFF_COMP_0_ADDR             = 0x71;
        const byte BMI2_GYR_OFF_COMP_3_ADDR             = 0x74;
        const byte BMI2_GYR_OFF_COMP_6_ADDR             = 0x77;
        const byte BMI2_GYR_USR_GAIN_0_ADDR             = 0x78;
        const byte BMI2_PWR_CONF_ADDR                   = 0x7C;
        const byte BMI2_PWR_CTRL_ADDR                   = 0x7D;
        const byte BMI2_CMD_REG_ADDR                    = 0x7E;
    }
}