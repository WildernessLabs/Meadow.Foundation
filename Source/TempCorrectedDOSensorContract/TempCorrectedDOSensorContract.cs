using System;
using System.Threading.Tasks;
using Meadow;
using Meadow.Peripherals.Sensors;
using Meadow.Units;

namespace TempCorrectedDOSensorContract
{

    /// <summary>
    /// Dissolved oxygen concetration with temperature correction
    /// </summary>
    public interface ITempCorrectedDOsensor : ISamplingSensor<ConcentrationInWater>, IObservable<IChangeResult<ConcentrationInWater>>
    {
        /// <summary>
        /// Last value calculated from the dissolved Oxygen concentration sensor
        /// </summary>
        public ConcentrationInWater Concentration { get; set; }

        /// <summary>
        /// Temperature, used for temperature correction of voltage read from Sensor
        /// </summary>
        public Temperature WaterTemperature { get; set; }

        /// <summary>
        /// Calculates concentration with current measure from sensor and given temp, also saves temperature
        /// Useful if you don't have an attached temperature sensor
        /// </summary>
        /// <param name="temperature">Water temperature</param>
        /// <returns></returns>
        public ConcentrationInWater GetConcentrationWithTemp(Temperature temperature);

        /// <summary>
        /// A temperature sensor of some sort, may be null
        /// </summary>
        public ITemperatureSensor TempSensor {get; set;}

        /// <summary>
        /// Reads the sensors and returns concentration
        /// </summary>
        /// <returns></returns>
        public new Task<ConcentrationInWater>Read();
    }
}
