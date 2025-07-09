using UnityEngine;

namespace IDEK.Tools.GameplayEssentials.Samples.HackSlash
{
    [CreateAssetMenu(
        menuName = "IDEK/Weather and Time/Weather Asset",
        fileName = "New Weather Asset", order = 0)]
    public class WeatherAsset : ScriptableObject
    {
        [System.Serializable]
        public struct TimeOfDayOverridePreset
        {
            public TimeOfDayInfluenceGraph timingGraph;
            public SkyboxLightingPreset lighting;
        }
        
        //day/night cycle is impacted by these two
        public SkyboxLightingPreset sunDefault;
        public SkyboxLightingPreset moonDefault;
        
        ///<summary>
        ///Overrides intended to be cast over the sun/moon day/night cycle
        ///Can have less than 100% influence,
        ///in which case the underlying day/night cycle preset weight will shine through.
        ///</summary>
        public TimeOfDayOverridePreset[] timeOfDayOverrides;
    }
}