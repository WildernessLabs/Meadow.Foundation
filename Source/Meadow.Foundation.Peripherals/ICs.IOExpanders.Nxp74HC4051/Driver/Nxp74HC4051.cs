using Meadow.Hardware;
using System;

namespace Meadow.Foundation.ICs.IOExpanders
{
    /// <summary>
    /// Represents an NXP 74HC4051 analog multiplexer
    /// </summary>
    public partial class Nxp74HC4051
    {
        private object _syncRoot = new object();

        /// <summary>
        /// The port connected to the Enable pin of the mux (otherwise must be tied low)
        /// </summary>
        public IDigitalOutputPort? EnablePort { get; }
        /// <summary>
        /// The port connected to the mux's S0 selection pin
        /// </summary>
        public IDigitalOutputPort S0 { get; }
        /// <summary>
        /// The port connected to the mux's S1 selection pin
        /// </summary>
        public IDigitalOutputPort S1 { get; }
        /// <summary>
        /// The port connected to the mux's S2 selection pin
        /// </summary>
        public IDigitalOutputPort S2 { get; }
        /// <summary>
        /// The analog input connected to the Mux output pin (Z)
        /// </summary>
        public IAnalogInputPort Z { get; }

        /// <summary>
        /// Creates a new Nxp74HC4051 object using the default parameters
        /// </summary>
        public Nxp74HC4051(IDigitalOutputPort s0, IDigitalOutputPort s1, IDigitalOutputPort s2, IAnalogInputPort z, IDigitalOutputPort? enable = null)
        {
            S0 = s0;
            S1 = s1;
            S2 = s2;
            Z = z;
            EnablePort = enable;
        }

        /// <summary>
        /// Enables the multiplexer (if an enable port was provided)
        /// </summary>
        public void Enable()
        {
            if (EnablePort != null)
            {
                lock (_syncRoot)
                {
                    EnablePort.State = false; // active low
                }
            }
        }

        /// <summary>
        /// Disables the multiplexer (if an enable port was provided)
        /// </summary>
        public void Disable()
        {
            if (EnablePort != null)
            {
                lock (_syncRoot)
                {
                    EnablePort.State = true;
                }
            }
        }

        /// <summary>
        /// Sets the channel input (Y pin) that will be routed to the mux output (Z pin)
        /// </summary>
        /// <param name="channel"></param>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public void SetInputChannel(int channel)
        {
            if(channel < 0 || channel > 7) throw new ArgumentOutOfRangeException();

            lock (_syncRoot)
            {
                var reenable = false;

                if (EnablePort != null)
                {
                    if (EnablePort.State)
                    {
                        reenable = true;

                        // disable before switching to prevent possible mis-routing

                        Disable();
                    }
                }

                /*
                Truth Table
                E   S2  S1  S0
                --  --  --  --
                L   L   L   L   Y0 to Z
                L   L   L   H   Y1 to Z
                L   L   H   L   Y2 to Z
                L   L   H   H   Y3 to Z
                L   H   L   L   Y4 to Z
                L   H   L   H   Y5 to Z
                L   H   H   L   Y6 to Z
                L   H   H   H   Y7 to Z
                H   X   X   X   switches of
                */

                switch (channel)
                {
                    case 0:
                        S0.State = false;
                        S1.State = false;
                        S2.State = false;
                        break;
                    case 1:
                        S0.State = true;
                        S1.State = false;
                        S2.State = false;
                        break;
                    case 2:
                        S0.State = false;
                        S1.State = true;
                        S2.State = false;
                        break;
                    case 3:
                        S0.State = true;
                        S1.State = true;
                        S2.State = false;
                        break;
                    case 4:
                        S0.State = false;
                        S1.State = false;
                        S2.State = true;
                        break;
                    case 5:
                        S0.State = true;
                        S1.State = false;
                        S2.State = true;
                        break;
                    case 6:
                        S0.State = false;
                        S1.State = true;
                        S2.State = true;
                        break;
                    case 7:
                        S0.State = true;
                        S1.State = true;
                        S2.State = true;
                        break;

                }

                if (reenable)
                {
                    Enable();
                }
            }

        }
    }
}