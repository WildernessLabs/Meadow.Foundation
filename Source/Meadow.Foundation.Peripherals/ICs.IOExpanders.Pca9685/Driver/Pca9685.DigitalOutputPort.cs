using Meadow.Hardware;
using System.Linq;

namespace Meadow.Foundation.ICs.IOExpanders;

public partial class Pca9685
{
    public class DigitalOutputPort : IDigitalOutputPort
    {
        public bool InitialState { get; }

        public bool State
        {
            get => state;
            set
            {
                if (value == state) { return; }

                state = value;

                if (state)
                {
                    controller.SetPwm(portNumber, 4096, 0);
                }
                else
                {
                    controller.SetPwm(portNumber, 0, 4096);
                }
            }
        }
        bool state;

        public IDigitalChannelInfo Channel { get; }

        public IPin Pin { get; }

        private readonly Pca9685 controller;
        private readonly byte portNumber;

        internal DigitalOutputPort(Pca9685 controller, IPin pin, bool initialState)
        {
            InitialState = initialState;
            State = initialState;
            Pin = pin;

            this.controller = controller;
            Channel = (IDigitalChannelInfo)pin.SupportedChannels.First(c => c is IDigitalChannelInfo);

            portNumber = (byte)pin.Key;
        }

        public void Dispose()
        {
        }
    }
}