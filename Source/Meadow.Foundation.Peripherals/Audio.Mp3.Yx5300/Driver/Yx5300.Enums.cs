namespace Meadow.Foundation.Audio.Mp3
{
    public partial class Yx5300
    {
        /// <summary>
        /// Music play status
        /// </summary>
        public enum PlayStatus
        {
            /// <summary>
            /// Stopped
            /// </summary>
            Stopped = 0,
            /// <summary>
            /// Playing music
            /// </summary>
            Playing = 1,
            /// <summary>
            /// Music is paused
            /// </summary>
            Paused = 2,
            /// <summary>
            /// Status unknown
            /// </summary>
            Unknown,
        }

        enum Responses
        {
            SDCardInserted = 0x3A,
            PlayComplete = 0x3D,
            Error = 0x40,
            DataReceived = 0x41,
            PlayBackStatus = 0x42,
            Volume = 0x43,
            FileCount = 0x48,
            CurrentFile = 0x4C,
            FolderFileCount = 0x4E,
            FolderCount = 0x4F,
        }

        enum Commands
        {
            Next = 0x01,
            Previous = 0x02,
            PlayIndex = 0x03,
            VolumeUp = 0x04,
            VolumeDown = 0x05,
            SetVolume = 0x06,

            Loop = 0x08,
            SelectDevice = 0x09,
            Sleep = 0x0A,
            Wake = 0x0B,
            Reset = 0x0C,
            Play = 0x0D,
            Pause = 0x0E,
            Stop = 0x16,
            PlayFolder = 0x17,
            Shuffle = 0x18, //might not work
            PlayWithVolume = 0x22,

            GetCurrentFile = 0x4C,
            GetStatus = 0x42,
            GetVolume = 0x43,
            GetNumberOfTracksInFolder = 0x4E,
            GetTotalTracks = 0x48,
            GetNumberOfFolders = 0x4F
        }
    }
}