using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace IDEK.PlantGame.Ecology
{
    [Serializable]
    public class SoilState
    {
        public Temperature soilTemp;

        //[Min(0.0001f)]
        public float soilMass_kg;
        
        //[Min(0.0001f)]
        public float soilVolume_m3;
        
        //[Min(0.0f)]
        [Tooltip("Total amount of water in this soil object, in milliliters")]
        public float waterLevel_ml;
        
        //[Min(0.0f)]
        public float nutrition; //TODO: potentially expand into different minerals? 
        
        public float WaterMlPerCubicMeter => waterLevel_ml / soilVolume_m3;

        [Title("Helpers")]
        
        // public void SetWaterLevel_UsingSaturation(float saturationPoint) =>
        //     waterLevel_ml = soilVolume_m3 * saturationPoint;
        
        [Button]
        public void SetMass_UsingRigidbody(Rigidbody queryRigidbody) => 
            soilMass_kg = queryRigidbody.mass;
        
        [Button]
        public void SetMass_UsingDensity(float density_kgPerCubicMeter) => 
            soilMass_kg = density_kgPerCubicMeter * soilVolume_m3;
        
        [Button]
        public void SetVolume_UsingTransform(Transform queryTransform) => 
            soilVolume_m3 = queryTransform.lossyScale.x * queryTransform.lossyScale.y * queryTransform.lossyScale.z;

        [Button]
        public void SetVolume_UsingScaleVector(Vector3 queryCuboid) => 
            soilVolume_m3 = queryCuboid.x * queryCuboid.y * queryCuboid.z;

        public void SetVolume_UsingBounds(Bounds bounds) => 
            soilVolume_m3 = bounds.size.x * bounds.size.y * bounds.size.z;
    }
}