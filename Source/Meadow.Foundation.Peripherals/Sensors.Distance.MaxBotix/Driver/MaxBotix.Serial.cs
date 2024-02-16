﻿using Meadow.Hardware;
using Meadow.Units;
using System;

namespace Meadow.Foundation.Sensors.Distance
{

    public partial class MaxBotix
    {
        //The baud rate is 9600, 8 bits, no parity, with one stop bit
        readonly ISerialMessagePort? serialMessagePort;

        static readonly byte[] suffixDelimiter = { 13 }; //ASCII return
        static readonly int portSpeed = 9600;

        DateTime lastUpdate = DateTime.MinValue;

        /// <summary>
        /// Creates a new MaxBotix object communicating over serial
        /// </summary>
        /// <param name="device">The device connected to the sensor</param>
        /// <param name="serialPortName">The serial port name</param>
        /// <param name="sensor">The MaxBotix distance sensor type</param>
        public MaxBotix(IMeadowDevice device, SerialPortName serialPortName, SensorType sensor) :
            this(device.CreateSerialMessagePort(serialPortName, suffixDelimiter, false, baudRate: portSpeed), sensor)
        {
            createdPorts = true;
        }

        /// <summary>
        /// Creates a new MaxBotix object communicating over serial
        /// </summary>
        /// <param name="serialMessage">The serial message port</param>
        /// <param name="sensor">The distance sensor type</param>
        public MaxBotix(ISerialMessagePort serialMessage, SensorType sensor)
        {
            serialMessagePort = serialMessage;
            serialMessagePort.MessageReceived += SerialMessagePort_MessageReceived;

            communication = CommunicationType.Serial;
            sensorType = sensor;
        }

        Length ReadSensorSerial()
        {
            return Distance != null ? Distance.Value : new Length(0);
        }

        private void SerialMessagePort_MessageReceived(object sender, SerialMessageData e)
        {
            //R###\n //cm
            //R####\n //mm
            //R####\n //cm
            //need to check inches
            var message = e.GetMessageString(System.Text.Encoding.ASCII);

            if (message[0] != 'R')
            { return; }

            //strip the leading R
            string cleaned = message[1..];

            // get index of space
            var spaceIndex = message.FirstIndexOf(new char[] { ' ' });
            if (spaceIndex > 0)
            {
                cleaned = cleaned[..spaceIndex];
            }

            var value = double.Parse(cleaned);

            Length.UnitType units = GetUnitsForSensor(sensorType);

            ChangeResult<Length> changeResult = new()
            {
                New = new Length(value, units),
                Old = Distance,
            };

            Distance = changeResult.New;

            if (updateInterval == null || DateTime.UtcNow - lastUpdate >= updateInterval)
            {
                lastUpdate = DateTime.UtcNow;
                RaiseEventsAndNotify(changeResult);
            }
        }
    }
}