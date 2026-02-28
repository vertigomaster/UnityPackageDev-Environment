using System;
using UnityEngine;

namespace IDEK.PlantGame.Ecology
{
    [Serializable]
    public class SeedDef 
    {
        public GameObject plantPrefab;
        public float baseGerminationTime;
        public PlantGrowthConditionsAsset conditions;
    }
}