using System;
using UnityEngine;

namespace IDEK.PlantGame.Ecology.Unity
{
    [Serializable]
    public class SerializedWeatherState : WeatherState
    {
        [SerializeField]
        private Temperature _temperature = new(
            70f, Temperature.Unit.Fahrenheit);
        
        public override Temperature AirTemperature => _temperature;
        
        [SerializeField]
        private float _airPressure = DEFAULT_AIR_PRESSURE;
        public override float AirPressure => _airPressure;
        
        [SerializeField]
        private Humidity _airHumidity = 0.5f;
        public override Humidity AirHumidity => _airHumidity;
    }
    
    public class PrototypeClimateDataService : MonoBehaviour, IUnityClimateDataService
    {
        [SerializeField]
        private SerializedWeatherState constantWeatherState = new();

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