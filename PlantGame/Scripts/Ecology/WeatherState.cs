using System;

namespace IDEK.PlantGame.Ecology
{
    /// <summary>
    /// TODO
    /// </summary>
    [Serializable]
    public class WeatherState
    {
        public const float DEFAULT_AIR_PRESSURE = 1.0f;
        /// <summary>
        /// Average kinetic energy of the air.
        /// </summary>
        public virtual Temperature AirTemperature { get; set; }
        //not sure what to do with this yet - prob could be related to storm formation but we may want more design control over that
        /// <summary>
        /// Barometric pressure of the air. Not sure what units and stuff yet.
        /// </summary>
        public virtual float AirPressure { get; set; } 
        
        //likely related to rain chance and transpiration and because some plants will specifically target ideal humidities
        public virtual Humidity AirHumidity { get; set; }
    }
}