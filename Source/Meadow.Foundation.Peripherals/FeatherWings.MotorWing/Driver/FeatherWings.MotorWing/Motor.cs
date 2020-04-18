using Meadow.Foundation.ICs;

namespace Meadow.Foundation.FeatherWings
{
    public abstract class Motor
    {
        protected readonly PCA9685 _pca9685;

        public Motor(PCA9685 pca9685)
        {
            _pca9685 = pca9685;
        }

        public abstract void SetSpeed(short speed);
    }
}
