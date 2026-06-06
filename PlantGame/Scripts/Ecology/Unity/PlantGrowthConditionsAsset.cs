using UnityEngine;

namespace IDEK.PlantGame.Ecology
{
    [CreateAssetMenu(
        menuName = "IDEK/PlantGame/PlantData/Growth Conditions Asset",
        fileName = "Plant_GrowthConditions", 
        order = 1)]
    public class PlantGrowthConditionsAsset : ScriptableObject
    {
        [SerializeReference]
        public PlantGrowthConditions data;
    }
}