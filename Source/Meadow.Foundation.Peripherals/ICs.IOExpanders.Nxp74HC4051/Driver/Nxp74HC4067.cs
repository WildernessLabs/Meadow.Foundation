using Meadow.Hardware;
using System;

namespace Meadow.Foundation.ICs.IOExpanders
{
    /// <summary>
    /// Represents an NXP 74HC4067 16-channel analog multiplexer
    /// </summary>
    public class Nxp74HC4067 : AnalogInputMultiplexerBase
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
        /// The port connected to the mux's S3 selection pin
        /// </summary>
        public IDigitalOutputPort? S3 { get; }

        /// <summary>
        /// Creates a new Nxp74HC4051 object
        /// </summary>
        public Nxp74HC4067(IAnalogInputPort z, IDigitalOutputPort s0, IDigitalOutputPort? s1 = null, IDigitalOutputPort? s2 = null, IDigitalOutputPort? s3 = null, IDigitalOutputPort? enable = null)
            : base(z, enable)
        {
            S0 = s0;
            S1 = s1;
            S2 = s2;
            S3 = s3;
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
                E   S3  S2  S1  S0
                --  --  --  --
                L   L   L   L   L   C0 to Z
                L   L   L   L   H   C1 to Z
                L   L   L   H   L   C2 to Z
                L   L   L   H   H   C3 to Z
                L   L   H   L   L   C4 to Z
                L   L   H   L   H   C5 to Z
                L   L   H   H   L   C6 to Z
                L   L   H   H   H   C7 to Z
                L   L   L   L   L   C8 to Z
                L   H   L   L   H   C9 to Z
                L   H   L   H   L   C10 to Z
                L   H   L   H   H   C11 to Z
                L   H   H   L   L   C12 to Z
                L   H   H   L   H   C13 to Z
                L   H   H   H   L   C14 to Z
                L   H   H   H   H   C15 to Z
                H   X   X   X   X   switches of
                */

                switch (channel)
                {
                    case 0:
                        S0.State = false;
                        if (S1 != null) { S1.State = false; }
                        if (S2 != null) { S2.State = false; }
                        if (S3 != null) { S3.State = false; }
                        break;
                    case 1:
                        S0.State = true;
                        if (S1 != null) { S1.State = false; }
                        if (S2 != null) { S2.State = false; }
                        if (S3 != null) { S3.State = false; }
                        break;
                    case 2:
                        if (S1 == null) throw new ArgumentException("You must have an S1 connected to access channels > 1");
                        S0.State = false;
                        S1.State = true;
                        if (S2 != null) { S2.State = false; }
                        if (S3 != null) { S3.State = false; }
                        break;
                    case 3:
                        if (S1 == null) throw new ArgumentException("You must have an S1 connected to access channels > 1");
                        S0.State = true;
                        S1.State = true;
                        if (S2 != null) { S2.State = false; }
                        if (S3 != null) { S3.State = false; }
                        break;
                    case 4:
                        if (S1 == null) throw new ArgumentException("You must have an S1 connected to access channels > 1");
                        if (S2 == null) throw new ArgumentException("You must have an S2 connected to access channels > 3");
                        S0.State = false;
                        S1.State = false;
                        S2.State = true;
                        if (S3 != null) { S3.State = false; }
                        break;
                    case 5:
                        if (S1 == null) throw new ArgumentException("You must have an S1 connected to access channels > 1");
                        if (S2 == null) throw new ArgumentException("You must have an S2 connected to access channels > 3");
                        S0.State = true;
                        S1.State = false;
                        S2.State = true;
                        if (S3 != null) { S3.State = false; }
                        break;
                    case 6:
                        if (S1 == null) throw new ArgumentException("You must have an S1 connected to access channels > 1");
                        if (S2 == null) throw new ArgumentException("You must have an S2 connected to access channels > 3");
                        S0.State = false;
                        S1.State = true;
                        S2.State = true;
                        if (S3 != null) { S3.State = false; }
                        break;
                    case 7:
                        if (S1 == null) throw new ArgumentException("You must have an S1 connected to access channels > 1");
                        if (S2 == null) throw new ArgumentException("You must have an S2 connected to access channels > 3");
                        S0.State = true;
                        S1.State = true;
                        S2.State = true;
                        if (S3 != null) { S3.State = false; }
                        break;

                    case 8:
                        if (S1 == null) throw new ArgumentException("You must have an S1 connected to access channels > 1");
                        if (S2 == null) throw new ArgumentException("You must have an S2 connected to access channels > 3");
                        if (S3 == null) throw new ArgumentException("You must have an S3 connected to access channels > 7");
                        S0.State = false;
                        S1.State = false;
                        S2.State = false;
                        S3.State = true;
                        break;
                    case 9:
                        if (S1 == null) throw new ArgumentException("You must have an S1 connected to access channels > 1");
                        if (S2 == null) throw new ArgumentException("You must have an S2 connected to access channels > 3");
                        if (S3 == null) throw new ArgumentException("You must have an S3 connected to access channels > 7");
                        S0.State = true;
                        S1.State = false;
                        S2.State = false;
                        S3.State = true;
                        break;
                    case 10:
                        if (S1 == null) throw new ArgumentException("You must have an S1 connected to access channels > 1");
                        if (S2 == null) throw new ArgumentException("You must have an S2 connected to access channels > 3");
                        if (S3 == null) throw new ArgumentException("You must have an S3 connected to access channels > 7");
                        S0.State = false;
                        S1.State = true;
                        S2.State = false;
                        S3.State = true;
                        break;
                    case 11:
                        if (S1 == null) throw new ArgumentException("You must have an S1 connected to access channels > 1");
                        if (S2 == null) throw new ArgumentException("You must have an S2 connected to access channels > 3");
                        if (S3 == null) throw new ArgumentException("You must have an S3 connected to access channels > 7");
                        S0.State = true;
                        S1.State = true;
                        S2.State = false;
                        S3.State = true;
                        break;
                    case 12:
                        if (S1 == null) throw new ArgumentException("You must have an S1 connected to access channels > 1");
                        if (S2 == null) throw new ArgumentException("You must have an S2 connected to access channels > 3");
                        if (S3 == null) throw new ArgumentException("You must have an S3 connected to access channels > 7");
                        S0.State = false;
                        S1.State = false;
                        S2.State = true;
                        S3.State = true;
                        break;
                    case 13:
                        if (S1 == null) throw new ArgumentException("You must have an S1 connected to access channels > 1");
                        if (S2 == null) throw new ArgumentException("You must have an S2 connected to access channels > 3");
                        if (S3 == null) throw new ArgumentException("You must have an S3 connected to access channels > 7");
                        S0.State = true;
                        S1.State = false;
                        S2.State = true;
                        S3.State = true;
                        break;
                    case 14:
                        if (S1 == null) throw new ArgumentException("You must have an S1 connected to access channels > 1");
                        if (S2 == null) throw new ArgumentException("You must have an S2 connected to access channels > 3");
                        if (S3 == null) throw new ArgumentException("You must have an S3 connected to access channels > 7");
                        S0.State = false;
                        S1.State = true;
                        S2.State = true;
                        S3.State = true;
                        break;
                    case 15:
                        if (S1 == null) throw new ArgumentException("You must have an S1 connected to access channels > 1");
                        if (S2 == null) throw new ArgumentException("You must have an S2 connected to access channels > 3");
                        if (S3 == null) throw new ArgumentException("You must have an S3 connected to access channels > 7");
                        S0.State = true;
                        S1.State = true;
                        S2.State = true;
                        S3.State = true;
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