using UnityEngine;

namespace IDEK.PlantGame.Ecology.Unity
{
    public class PrototypeClimateDataService : MonoBehaviour, IUnityClimateDataService
    {
        [SerializeField]
        private WeatherState constantWeatherState = new WeatherState();

        #region Implementation of IUnityClimateDataService
        
        /// <inheritdoc />
        public WeatherState GetWeatherAt(Vector3 worldPosition)
        {
            return constantWeatherState;
        }

        /// <inheritdoc />
        public WeatherState GetAveragedWeatherAt(Vector3 worldPosition, float range)
        {
            return constantWeatherState;
        }

        #endregion
    }
}