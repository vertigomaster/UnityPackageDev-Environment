using System;
using UnityEngine;

namespace IDEK.PlantGame.Ecology
{
    
    /// <summary>
    /// Because it's easy to pass in the wrong float
    /// </summary>
    [Serializable]
    public struct Humidity
    {
        [InspectorName("Humidity %")]
        [Range(0f, 1f)]
        public float humidity;

        public static Humidity FromRatio(float waterPerCubicMeter, float saturationPoint) => new(waterPerCubicMeter, saturationPoint);
        
        public Humidity(float waterPerCubicMeter, float saturationPoint)
        {
            humidity = waterPerCubicMeter / saturationPoint;
        }
        
        public Humidity(float humidity) => this.humidity = humidity;

        public float CalcWaterContent(float maxWaterContent) => humidity * maxWaterContent;
        public float CalcWaterContent(float volume_cubicMeters, float saturationPointPerCubicMeter)
        {
            return humidity * saturationPointPerCubicMeter * volume_cubicMeters;
        }
        
        public static implicit operator float(Humidity h) => h.humidity;
        public static implicit operator Humidity(float h) => new Humidity(h);
    }
}