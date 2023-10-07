namespace Meadow.Foundation.ICs.IOExpanders
{
    /// <summary>
    /// Represents Serial Wombat information
    /// </summary>
    public class WombatInfo
    {
        internal WombatInfo(ushort id, ushort rev)
        {
            Identifier = id;
            Revision = rev;
        }

        /// <summary>
        /// Serial Wombat ID
        /// </summary>
        public ushort Identifier { get; }

        /// <summary>
        /// Serial Wombat Revision
        /// </summary>
        public ushort Revision { get; }
    }
}