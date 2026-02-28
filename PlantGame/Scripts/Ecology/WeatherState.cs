namespace IDEK.PlantGame.Ecology
{
    /// <summary>
    /// TODO
    /// </summary>
    public class WeatherState
    {
        /// <summary>
        /// Average kinetic energy of the air.
        /// </summary>
        public Temperature AirTemperature { get; set; }
        //not sure what to do with this yet - prob could be related to storm formation but we may want more design control over that
        /// <summary>
        /// Barometric pressure of the air. Not sure what units and stuff yet.
        /// </summary>
        public float AirPressure { get; set; } 
        
        //likely related to rain chance and transpiration and because some plants will specifically target ideal humidities
        public Humidity AirHumidity { get; set; }
    }
}