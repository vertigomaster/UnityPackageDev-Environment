using IDEK.Tools.ShocktroopExtensions;
using UnityEngine;

namespace IDEK.PlantGame.DevTest
{
    public abstract class PlantBase : MonoBehaviour
    {
        public AnimationCurve growthRateOverLifespan;
        public float lifespan = 100f;

#if ODIN_INSPECTOR
        [Sirenix.OdinInspector.ShowInInspector, Sirenix.OdinInspector.ReadOnly]
#endif
        protected float age = 0f;
        public bool Alive { get; private set; } = true;

        private void OnValidate()
        {
            _NormalizeGrowthCurve();
        }

        private void OnEnable()
        {
            _NormalizeGrowthCurve();
        }

        protected virtual void Update()
        {
            if (age >= lifespan)
            {
                _Internal_Die();
                return;
            }
            
            _Internal_Grow(Time.deltaTime);
            age += Time.deltaTime;
        }

        public abstract void OnDeath();

        protected abstract void OnGrow(float deltaTime);

        private void _Internal_Grow(float deltaTime)
        {
            OnGrow(deltaTime);
        }

        private void _Internal_Die()
        {
            Alive = false;
            OnDeath();
        }

        private void _NormalizeGrowthCurve()
        {
            growthRateOverLifespan.NormalizeThis(timeOnly: true);
        }
    }
}