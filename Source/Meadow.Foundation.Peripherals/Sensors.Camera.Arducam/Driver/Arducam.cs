using Meadow.Hardware;
using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Meadow.Foundation.Sensors.Camera
{
    /// <summary>
    /// Class that represents a Arducam family of cameras
    /// </summary>
    public partial class Arducam : ICamera
    {
        public Arducam(ISpiBus spiBus, II2cBus i2cBus, byte i2cAddress)
        {

        }

        public bool CapturePhoto()
        {
            throw new NotImplementedException();
        }

        public Task<byte[]> GetPhotoData()
        {
            throw new NotImplementedException();
        }

        public Task<MemoryStream> GetPhotoStream()
        {
            throw new NotImplementedException();
        }

        public bool IsPhotoAvailable()
        {
            throw new NotImplementedException();
        }
    }
}