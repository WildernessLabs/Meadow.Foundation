namespace Meadow.Foundation.ICs.IOExpanders
{
    /// <summary>
    /// Class that represents the serial wombat version
    /// </summary>
    public class WombatVersion
    {
        internal WombatVersion(string info)
        {
            if (info[0] == 'V')
            {
                Version = info.Substring(1);
            }
            else
            {
                Version = info;
            }
        }

        /// <summary>
        /// Serial Wombat version
        /// </summary>
        public string Version { get; }

        /// <summary>
        /// Serial Wombat model
        /// </summary>
        public string Model => Version.Substring(0, 3);

        /// <summary>
        /// Serial Wombat firmware version
        /// </summary>
        public string Firmware => Version.Substring(4);
    }
}