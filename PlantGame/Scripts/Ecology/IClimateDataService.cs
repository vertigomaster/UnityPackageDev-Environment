using UnityEngine;

namespace IDEK.PlantGame.Ecology
{
    /// <summary>
    /// TODO
    /// </summary>
    public interface IClimateDataService
    {
        /// <summary>
        /// Gets the weather at the given world position 
        /// </summary>
        /// <param name="worldPosition"></param>
        /// <returns></returns>
        WeatherState GetWeatherAt(Vector3 worldPosition);
        
        /// <summary>
        /// Gets the current averaged weather within a sphere of radius range centered at worldPosition 
        /// </summary>
        /// <param name="worldPosition"></param>
        /// <param name="range"></param>
        /// <returns></returns>
        WeatherState GetAveragedWeatherAt(Vector3 worldPosition, float range);
    }
}