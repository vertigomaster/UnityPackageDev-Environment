using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace IDEK.PlantGame.Ecology
{
    [Serializable]
    public class SeedDef 
    {
        public GameObject plantPrefab;
        public float baseGerminationTime;
        [Tooltip("Should the seed be destroyed after germination, leaving only the plant prefab?")]
        public bool shouldDestroyAfterGermination;
        #if ODIN_INSPECTOR
        [InlineEditor]
        #endif
        public PlantGrowthConditionsAsset conditions;
        public PlantGrowthConditions Conditions => conditions.data;
    }
}