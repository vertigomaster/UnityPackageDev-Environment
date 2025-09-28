using IDEK.Tools.ShocktroopExtensions;
using IDEK.Tools.ShocktroopUtils;
using UnityEngine;
using UnityEngine.Serialization;

namespace IDEK.ProjectSim.Unity.Runtime
{
    /// <summary>
    /// creatures live, eat, and die
    /// </summary>
    public class Creature : MonoBehaviour
    {
        public bool startAlive = true;
        [FormerlySerializedAs("edibleData")]
        public Edible ourEdibleData;
        
        private bool _alive;

        public bool IsAlive => _alive;

        private void OnValidate()
        {
            gameObject.TryGetComponentIfNull(ref ourEdibleData);
        }

        private void OnEnable()
        {
            gameObject.TryGetComponentIfNull(ref ourEdibleData);
        }

        private void Start()
        {
            _alive = startAlive;
            _RefreshDeathState();
        }

        public bool TryEat(GameObject gameObject)
        {
            if (gameObject == null) return false;

            if (gameObject.TryGetComponent(out Edible ourMeal))
            {
                //this thing has nutrition
                //add its nutrition to yours!
                float newNut = ourMeal.MutableNutrition.GetTotalEnergyOfIngredients();
                // ourMeal.BaseNutrition
                // ourEdibleData.MutableNutrition.baseEnergy += ;

            }
            
            return true;
        }

        public void Kill()
        {
            _alive = false;
            _RefreshDeathState();
        }

        private void _RefreshDeathState()
        {
            ourEdibleData.DecayBehavior.NullCheck(d => d.enabled = !_alive);
        }
    }
}