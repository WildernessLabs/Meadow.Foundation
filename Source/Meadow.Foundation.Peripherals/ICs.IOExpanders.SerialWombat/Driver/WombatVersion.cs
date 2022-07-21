namespace Meadow.Foundation.ICs.IOExpanders
{
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

        public string Version { get; }
        public string Model => Version.Substring(0, 3);
        public string Firmware => Version.Substring(4);
    }
}