namespace Meadow.Foundation.Sensors.Rotary
{
    /// <summary>
    /// Represents a 2 bit version of GrayCode (https://en.wikipedia.org/wiki/Gray_code) used to 
    /// encode the current state of a rotary encoder. 
    /// </summary>
    public struct TwoBitGrayCode
    {
        /// <summary>
        /// A phase of graycode 
        /// </summary>
        public bool APhase;
        /// <summary>
        /// B phase of graycode
        /// </summary>
        public bool BPhase;
    }
}