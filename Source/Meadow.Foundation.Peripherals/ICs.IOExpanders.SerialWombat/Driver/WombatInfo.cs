namespace Meadow.Foundation.ICs.IOExpanders
{
    public class WombatInfo
    {
        internal WombatInfo(ushort id, ushort rev)
        {
            Identifier = id;
            Revision = rev;
        }

        public ushort Identifier { get; }
        public ushort Revision { get; }
    }
}