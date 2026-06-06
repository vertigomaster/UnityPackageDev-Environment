using UnityEngine;

namespace IDEK.PlantGame.Ecology
{
    [CreateAssetMenu(
        menuName = "IDEK/PlantGame/PlantData/Seed Definition Asset",
        fileName = "Seed_Definition",
        order = 0)]
    public class SeedDefAsset : ScriptableObject
    {
        [SerializeReference]
        public SeedDef data;
    }
}