using Meadow.Foundation.ICs.IOExpanders;

namespace Meadow.Foundation.FeatherWings
{
    public abstract class Motor
    {
        protected readonly Pca9685 _pca9685;

        public Motor(Pca9685 pca9685)
        {
            _pca9685 = pca9685;
        }

        public abstract void SetSpeed(short speed);
    }
}
