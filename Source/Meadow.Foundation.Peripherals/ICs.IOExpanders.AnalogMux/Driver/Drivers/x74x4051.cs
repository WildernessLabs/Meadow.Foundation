using Meadow.Hardware;
using System;

namespace Meadow.Foundation.ICs.IOExpanders
{
    /// <summary>
    /// Represents an NXP 74HC4051 (and variants) 8-channel analog multiplexer
    /// </summary>
    public class x74x4051 : AnalogMuxBase
    {
        /// <summary>
        /// The port connected to the mux's S0 selection pin
        /// </summary>
        public IDigitalOutputPort S0 { get; }
        /// <summary>
        /// The port connected to the mux's S1 selection pin
        /// </summary>
        public IDigitalOutputPort? S1 { get; }
        /// <summary>
        /// The port connected to the mux's S2 selection pin
        /// </summary>
        public IDigitalOutputPort? S2 { get; }

        /// <summary>
        /// Creates a new Nxp74HC4051 object using the default parameters
        /// </summary>
        public x74x4051(IAnalogInputPort z, IDigitalOutputPort s0, IDigitalOutputPort? s1 = null, IDigitalOutputPort? s2 = null, IDigitalOutputPort? enable = null)
            : base (z, enable)
        {
            S0 = s0;
            S1 = s1;
            S2 = s2;
        }

        /// <summary>
        /// Sets the channel input (Y pin) that will be routed to the mux output (Z pin)
        /// </summary>
        /// <param name="channel"></param>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public override void SetInputChannel(int channel)
        {
            if (channel < 0 || channel > 7) throw new ArgumentOutOfRangeException();

            lock (SyncRoot)
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
                        if (S1 != null) { S1.State = false; }
                        if (S2 != null) { S2.State = false; }
                        break;
                    case 1:
                        S0.State = true;
                        if (S1 != null) { S1.State = false; }
                        if (S2 != null) { S2.State = false; }
                        break;
                    case 2:
                        if (S1 == null) throw new ArgumentException("You must have an S1 connected to access channels > 1");
                        S0.State = false;
                        S1.State = true;
                        if (S2 != null) { S2.State = false; }
                        break;
                    case 3:
                        if (S1 == null) throw new ArgumentException("You must have an S1 connected to access channels > 1");
                        S0.State = true;
                        S1.State = true;
                        if (S2 != null) { S2.State = false; }
                        break;
                    case 4:
                        if (S1 == null) throw new ArgumentException("You must have an S1 connected to access channels > 1");
                        if (S2 == null) throw new ArgumentException("You must have an S2 connected to access channels > 3");
                        S0.State = false;
                        S1.State = false;
                        S2.State = true;
                        break;
                    case 5:
                        if (S1 == null) throw new ArgumentException("You must have an S1 connected to access channels > 1");
                        if (S2 == null) throw new ArgumentException("You must have an S2 connected to access channels > 3");
                        S0.State = true;
                        S1.State = false;
                        S2.State = true;
                        break;
                    case 6:
                        if (S1 == null) throw new ArgumentException("You must have an S1 connected to access channels > 1");
                        if (S2 == null) throw new ArgumentException("You must have an S2 connected to access channels > 3");
                        S0.State = false;
                        S1.State = true;
                        S2.State = true;
                        break;
                    case 7:
                        if (S1 == null) throw new ArgumentException("You must have an S1 connected to access channels > 1");
                        if (S2 == null) throw new ArgumentException("You must have an S2 connected to access channels > 3");
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