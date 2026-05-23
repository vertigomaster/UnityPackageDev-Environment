#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#endif
using IDEK.Tools.Coroutines.TaskRoutines;
using UnityEngine;

namespace IDEK.PlantGame.Ecology
{
    public class SeedComponent : MonoBehaviour
    {
        #if ODIN_INSPECTOR
        [InlineEditor]
        #endif
        public SeedDefAsset defAsset;
        public SeedDef SeedData => defAsset.data;
        
        #if ODIN_INSPECTOR
        [field:ShowInInspector, ReadOnly]
        #endif
        public bool IsPlanted { get; protected set; }
        
        private float _germinationStartTime = 0f;
        private TaskRoutine _germinationRoutine;


        /// <summary>
        /// Attempts to plant the seed at the specified position within the target object.
        /// If the planting conditions are met, the seed gets planted.
        /// </summary>
        /// <param name="targetObj">The GameObject where the seed should be planted.</param>
        /// <param name="position">The specific position within the target object where the seed should be planted.</param>
        /// <returns>True if the seed is successfully planted, otherwise false.</returns>
        public virtual bool TryPlantHere(GameObject targetObj, Vector3 position)
        {
            if (!CanPlantHere(targetObj, position)) return false;
            
            PlantSelfIn(targetObj, transform.position);
            return true;
        } 

        public virtual bool CanPlantHere(GameObject potentialDirt) => 
            CanPlantHere(potentialDirt, transform.position);

        public virtual bool CanPlantHere(GameObject potentialDirt, Vector3 queryPosition)
        {
            if (!potentialDirt.TryGetComponent(out SoilComponent soilComp)) return false;
            
            //TODO: check conditions for thresholds
            return SeedData.Conditions.IsViableSoil(soilComp);

        }

        protected virtual void PlantSelfIn(GameObject potentialDirt, Vector3 plantingPosition)
        {
            //set up/spawn the plant object associated with this seed
            PlantSelfIn(potentialDirt, plantingPosition, Quaternion.identity);
        }

        protected virtual void PlantSelfIn(GameObject potentialDirt, Vector3 plantingPosition, Quaternion plantingRotation)
        {
            //TODO: germinate first? or is that a seed state of the plant?

            if(_germinationRoutine == null) return;
            if(_germinationRoutine.IsRunning) return;
            
            _germinationRoutine = _GerminateRoutine().OnFinish(() => {
                _germinationRoutine = null;
                //set up/spawn the plant object associated with this seed
                Instantiate(SeedData.plantPrefab,
                    plantingPosition,
                    plantingRotation,
                    potentialDirt.transform);
            });

            IsPlanted = true;
        }

        //TODO; consider Unity-friendly Async/Awaitable?
        private TaskRoutine _GerminateRoutine()
        {
            //TODO: vary germination time based on soil conditions
            //TODO: update in ticks instead since soil conditions could change over time
            //TODO: consider async/awaitable for multithreaded germinations
            var temp = TaskRoutine.Delay(SeedData.baseGerminationTime);
            //TODO; cancellation handling (...which async/awaitable already does)
            return temp;
        }
    }
}