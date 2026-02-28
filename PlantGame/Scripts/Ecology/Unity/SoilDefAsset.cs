using UnityEngine;

namespace IDEK.PlantGame.Ecology
{
    [CreateAssetMenu(
        menuName = "IDEK/PlantGame/PlantData/Soil Definition Asset",
        fileName = "Soil_Definition",
        order = 0)]
    public class SoilDefAsset : ScriptableObject
    {
        [SerializeReference]
        public SoilDef data;
    }
}