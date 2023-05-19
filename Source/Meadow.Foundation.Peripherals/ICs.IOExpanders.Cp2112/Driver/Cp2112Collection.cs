using System.Collections;
using System.Collections.Generic;

#nullable enable

namespace Meadow.Foundation.ICs.IOExpanders
{
    public class Cp2112Collection : IEnumerable<Cp2112>
    {
        private static Cp2112Collection? _instance;

        private List<Cp2112> _list = new List<Cp2112>();

        public int Count => _list.Count;
        public Cp2112 this[int index] => _list[index];

        private Cp2112Collection()
        {
        }

        public void Refresh()
        {
            _list.Clear();

            uint deviceCount = 0;

            var vid = Native.UsbParameters.SG_VID;
            var pid = Native.UsbParameters.CP2112_PID;

            Native.CheckStatus(Native.Functions.HidSmbus_GetNumDevices(ref deviceCount, vid, pid));

            for (var i = 0; i < deviceCount; i++)
            {
                _list.Add(new Cp2112(i, vid, pid));
            }

        }

        public IEnumerator<Cp2112> GetEnumerator()
        {
            return _list.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public static Cp2112Collection Devices
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new Cp2112Collection();
                    _instance.Refresh();
                }
                return _instance;
            }
        }
    }
}