namespace Meadow.Foundation.Sensors.Temperature
{
    public partial class Mcp960x
    {
        const byte HOTJUNCTION = 0x00;   // Hot junction temperature T_H
        const byte JUNCTIONDELTA = 0x01; // Hot/Cold junction delta
        const byte COLDJUNCTION = 0x02;  // Cold junction temperature T_C
        const byte RAWDATAADC = 0x03;    // The raw uV reading
        const byte STATUS = 0x04;        // Current device status
        const byte SENSORCONFIG = 0x05;  // Configuration for thermocouple type
        const byte DEVICECONFIG = 0x06;  // Device config like sleep mode
        const byte DEVICEID = 0x20;      // Device ID/Revision
        const byte ALERTCONFIG_1 = 0x08; // The first alert's config
        const byte ALERTHYST_1 = 0x0C;   // The first alert's hystersis
        const byte ALERTLIMIT_1 = 0x10;  // the first alert's limitval

        const byte STATUS_ALERT1 = 0x01;     // Bit flag for alert 1 status
        const byte STATUS_ALERT2 = 0x02;     // Bit flag for alert 2 status
        const byte STATUS_ALERT3 = 0x04;     // Bit flag for alert 3 status
        const byte STATUS_ALERT4 = 0x08;     // Bit flag for alert 4 status
        const byte STATUS_INPUTRANGE = 0x10; // Bit flag for input range
        const byte STATUS_THUPDATE = 0x40;   // Bit flag for TH update
        const byte STATUS_BURST = 0x80;      // Bit flag for burst complete
    }
}
