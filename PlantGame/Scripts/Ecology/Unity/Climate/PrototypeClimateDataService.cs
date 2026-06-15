using System;
using UnityEngine;

namespace IDEK.PlantGame.Ecology.Unity
{
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

        #region Implementation of IService

        /// <inheritdoc />
        public void OnRegister(Type type) { }

        /// <inheritdoc />
        public void OnUnregister(Type type) { }

        #endregion
    }
}