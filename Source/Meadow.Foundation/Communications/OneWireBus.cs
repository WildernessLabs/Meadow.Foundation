using Meadow;
using Meadow.Hardware;
using System;
using System.Collections;

namespace Meadow.Foundation.Communications
{
    /// <summary>
    ///     Singleton class used to allow access to multiple OneWire devices on a single pin
    ///     in a multi-threaded application.
    /// </summary>
    /// <remarks>
    ///     The OneWire class allows for a single bus to have one or more devices attached to it.
    ///     It also allows for multiple OneWire devices to be connected to different pins on the
    ///     Netduino.
    ///
    ///     The multiple devices on different pins is not an issue in a multi-threaded environment.
    ///
    ///     Multiple devices attached to a single pin on a multi-threaded environment could be an
    ///     issue as the command sequencing could clash.  Using this object with locking allows
    ///     for this scenario to be implemented in a safe way.
    /// </remarks>
    public sealed class OneWireBus
    {
        #region Classes

        /// <summary>
        ///     Hold information about the devices that are attached to a single pin.
        /// </summary>
        public class Devices
        {
            #region Properties

            /// <summary>
            ///     Pin connected to the OneWire devices.
            /// </summary>
            public IPin Pin { get; set; }

            /// <summary>
            ///     OutputPoert object used to talk to the OneWire bus.
            /// </summary>
            private DigitalOutputPort Port { get; set; }

            /// <summary>
            ///     Object used to talk to the OneWire devices using the OneWire protocol.
            /// </summary>
            public OneWire DeviceBus { get; set; }

            /// <summary>
            ///     Device IS connected to this instance of the OneWire bus.
            /// </summary>
            public ArrayList DeviceIDs { get; set; }

            #endregion Properties

            #region Constructor

            /// <summary>
            ///     Default constructor is private to prevent it being invoked.
            /// </summary>
            private Devices()
            {
            }

            /// <summary>
            ///     Create a new instance of a OneWire Device object.
            /// </summary>
            /// <param name="pin">Pin connected to the OneWire devices.</param>
            public Devices(IDigitalPin pin)
            {
                Pin = pin;
                Port = new DigitalOutputPort(pin, false);
                DeviceBus = new OneWire(Port);
                DeviceIDs = new ArrayList();
                ScanForDevices();
            }

            #endregion Constructor

            #region Methods

            /// <summary>
            ///     Scan the specified device bus for OneWire devices.
            /// </summary>
            /// <remarks>
            ///     The OneWire protocol allows for multiple OneWire devices to be
            ///     attached to the same pin.  This method identifies all of the devices
            ///     that are connected to the pin used to create this object.
            /// </remarks>
            private void ScanForDevices()
            {
                ArrayList deviceList;
                lock (DeviceBus)
                {
                    DeviceBus.TouchReset();
                    deviceList = DeviceBus.FindAllDevices();
                }

                for (var device = 0; device < deviceList.Count; device++)
                {
                    UInt64 deviceID = 0;
                    byte[] deviceIDArray = ((byte[])deviceList[device]);
                    for (var index = 0; index < 8; index++)
                    {
                        int places = 8 * index;
                        byte value = deviceIDArray[index];
                        deviceID |= ((UInt64)value) << places;
                    }

                    DeviceIDs.Add(deviceID);
                }
            }

            #endregion Methods
        };

        #endregion Classes

        #region Private member variables

        /// <summary>
        ///     Static instance
        /// </summary>
        private static readonly OneWireBus oneWireBus = new OneWireBus();

        /// <summary>
        ///     List of all of the pins and the associates OneWire devices.
        /// </summary>
        private static readonly ArrayList _devices = new ArrayList();

        #endregion Private member variables

        #region Properties

        /// <summary>
        ///     Return the singleton object.
        /// </summary>
        public static OneWireBus Instance { get { return oneWireBus; } }

        #endregion Properties

        #region Constructor(s)

        static OneWireBus()
        {
        }

        /// <summary>
        ///     Default constructor is private to prevent it being invoked.
        /// </summary>
        private OneWireBus()
        {
        }

        #endregion Constructors

        #region Methods

        /// <summary>
        ///     Add a new pin to the list of those with OneWire devices attached.
        /// </summary>
        /// <param name="pin">Pin number with the OneWire devices attached.</param>
        /// <returns><see cref="Devices"/> object for the specified pin.</returns>
        public static Devices Add(IDigitalPin pin)
        {
            Devices result = null;
            foreach (Devices device in _devices)
            {
                if (device.Pin == pin)
                {
                    result = device;
                }
            }

            if (result == null)
            {
                result = new Devices(pin);
                _devices.Add(result);
            }
            return result;
        }

        #endregion Methods
    }
}