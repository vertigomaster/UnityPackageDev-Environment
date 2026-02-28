using System;

namespace IDEK.PlantGame.Ecology
{
    [Serializable]
    public class SoilState
    {
        public Temperature soilTemp;
        public float soilMass_kg;
        public float soilVolume_m3;
        
        public float waterLevel_ml;
        
        public float nutrition; //TODO: potentially expand into different minerals? 
        
        public float WaterMlPerCubicMeter => waterLevel_ml / soilVolume_m3;
    }
}