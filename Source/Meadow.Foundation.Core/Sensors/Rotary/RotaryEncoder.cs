using System.Threading;
using System.Threading.Tasks;
using Meadow.Hardware;
using Meadow.Peripherals.Sensors.Rotary;

namespace Meadow.Foundation.Sensors.Rotary
{
    /// <summary>
    /// Digital rotary encoder that uses two-bit Gray Code to encode rotation.
    /// </summary>
    public class RotaryEncoder : IRotaryEncoder
    {
        #region Properties

        /// <summary>
        /// Returns the pin connected to the A-phase output on the rotary encoder.
        /// </summary>
        public IDigitalInputPort APhasePort { get; }

        /// <summary>
        /// Returns the pin connected to the B-phase output on the rotary encoder.
        /// </summary>
        public IDigitalInputPort BPhasePort { get; }

        /// <summary>
        /// Raised when the rotary encoder is rotated and returns a RotaryTurnedEventArgs object which describes the direction of rotation.
        /// </summary>
        public event RotaryTurnedEventHandler Rotated = delegate { };

        #endregion

        #region Member variables / fields

        /// <summary>
        /// Contains the previous offset used to find direction information
        /// </summary>
        private int _dynamicOffset = 0;

        /// <summary>
        /// The rotary encoder has 2 inputs, called A and B. Because of its design
        /// either A or B changes but not both on each notification when the encoder
        /// is rotated. Because of this the direction to be determined. If A goes
        /// High before B then we are rotating one direction if B goes high before A
        /// we are rotating the other direction. For each change we must consider the
        /// previous state of A and B and the current state of A and B. This can be
        /// represented as 4-bit number.
        ///  3 2 1 0
        /// |old|new|
        /// |A|B|A|B|
        ///
        /// Bits 0 and 1 represent the current state of A and B while bits 2 and 3
        /// represent previous states of A and B. This 4-bit number yields 16 possible
        /// combination, however, there are combination that for which not change is
        /// represent. For example, the if bits 0-3 are all 0, this would mean that A
        /// was Low and is Low, and that B was Low and is Low (nothing changed.)
        /// </summary>
        private readonly int[] RotEncLookup = { 0, -1, 1, 0, 1, 0, 0, -1, -1, 0, 0, 1, 0, 1, -1, 0 };

        #endregion

        #region Constructors

        /// <summary>
        /// Instantiate a new RotaryEncoder on the specified pins.
        /// </summary>
        /// <param name="device"></param>
        /// <param name="aPhasePin"></param>
        /// <param name="bPhasePin"></param>
        public RotaryEncoder(IIODevice device, IPin aPhasePin, IPin bPhasePin) :
            this(device.CreateDigitalInputPort(aPhasePin, InterruptMode.EdgeBoth, ResistorMode.InternalPullUp, 0, .5),
                 device.CreateDigitalInputPort(bPhasePin, InterruptMode.EdgeBoth, ResistorMode.InternalPullUp, 0, .5))
        { }

        /// <summary>
        /// Instantiate a new RotaryEncoder on the specified ports
        /// </summary>
        /// <param name="aPhasePort"></param>
        /// <param name="bPhasePort"></param>
        public RotaryEncoder(IDigitalInputPort aPhasePort, IDigitalInputPort bPhasePort)
        {
            APhasePort = aPhasePort;
            BPhasePort = bPhasePort;

            APhasePort.Changed += PhaseAPinChanged;
            BPhasePort.Changed += PhaseBPinChanged;
        }

        #endregion

        #region Methods

        private void PhaseAPinChanged(object s, DigitalInputPortEventArgs e)
        {
            // Clear bit A bit
            int new2LsBits = _dynamicOffset & 0x02;    // Save bit 2 (B)
            if (e.Value)
                new2LsBits |= 0x01;                     // Set bit 1 (A)

            FindDirection(new2LsBits);
        }

        private void PhaseBPinChanged(object s, DigitalInputPortEventArgs e)
        {
            // Clear bit B bit
            int new2LsBits = _dynamicOffset & 0x01;    // Save bit 1 (A)
            if (e.Value)
                new2LsBits |= 0x02;                     // Set bit 2 (B)

            FindDirection(new2LsBits);
        }

        private void FindDirection(int new2LsBits)
        {
            _dynamicOffset <<= 2;          // Move previous A & B to bits 2 & 3
            _dynamicOffset |= new2LsBits;  // Set the current A & B states in bits 0 & 1
            _dynamicOffset &= 0x0000000f;  // Save only lowest 4 bits

            int direction = RotEncLookup[_dynamicOffset];
            if (direction == 0)
                return;                 // nothing changed

            if (direction == 1)
                Rotated?.Invoke(this, new RotaryTurnedEventArgs(RotationDirection.Clockwise));
            else
                Rotated?.Invoke(this, new RotaryTurnedEventArgs(RotationDirection.CounterClockwise));
        }

        #endregion
    }
}