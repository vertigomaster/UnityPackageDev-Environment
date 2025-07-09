using System.Collections.Generic;
using IDEK.Tools.GameplayEssentials.Quantities;
using IDEK.Tools.ShocktroopUtils;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.Serialization;

namespace IDEK.Tools.GameplayEssentials.Samples.HackSlash
{
    [CreateAssetMenu(
        menuName = "IDEK/Weather and Time/Regional Weather Group", 
        fileName = "New Regional Weather Group", order = 0)]
    public class RegionalWeatherGroupAsset : ScriptableObject
    {
        [System.Serializable]
        public struct WeatherState
        {
            public string tag;
            public WeatherAsset weatherData;
        }
        
        public List<WeatherState> weatherStates = new();
    }
}