using System;
using UnityEngine;

namespace IDEK.Tools.GameplayEssentials.Samples.HackSlash
{
    [Serializable]
    public record TimeOfDay(float PercentageOfDay)
    {
        //inlined for speed
        public static TimeOfDay FromEarthHours(float hours) => 
            new(hours / 24f);
    
        public static TimeOfDay FromGivenDayLength(float lengthOfDay, float timeInThatDay) =>
            new(timeInThatDay / lengthOfDay);
        
        public static TimeOfDay FromDateTime(DateTime dateTime) =>
            new((float)dateTime.TimeOfDay.TotalDays);
        
        [SerializeField]
#if ODIN_INSPECTOR
        [HideInInspector]
#endif
        private float _serializedPercentageOfDay;
        
#if ODIN_INSPECTOR
        [Sirenix.OdinInspector.ShowInInspector]
#endif
        public float PercentageOfDay { 
            get => _serializedPercentageOfDay; 
            private set => _serializedPercentageOfDay = PercentageOfDay % 1f; 
        }

#if ODIN_INSPECTOR
        [Sirenix.OdinInspector.ShowInInspector]
        [Sirenix.OdinInspector.ReadOnly]
#endif
        //inlined for speed
        public float AsEarthHours => PercentageOfDay * 24f;
    
        //inlined for speed
        public float AsEarthMinutes => PercentageOfDay * 24f * 60f;

#if ODIN_INSPECTOR
        [Sirenix.OdinInspector.ShowInInspector]
        [Sirenix.OdinInspector.ReadOnly]
#endif
        public bool IsAM => PercentageOfDay > 0.5f;
        public bool IsPM => PercentageOfDay <= 0.5f;
    
        public float ForGivenDayLength(float lengthOfDay) => PercentageOfDay * lengthOfDay;
    }
}