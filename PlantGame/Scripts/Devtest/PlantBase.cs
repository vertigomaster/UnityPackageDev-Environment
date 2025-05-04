using IDEK.Tools.ShocktroopExtensions;
using UnityEngine;

namespace IDEK.Tools.Misc.DevEnv.Scripts.Devtest
{
    public abstract class PlantBase : MonoBehaviour
    {
        public AnimationCurve growthRateOverLifespan;
        public float lifespan = 100f;
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
            
            Grow(Time.deltaTime);
            age += Time.deltaTime;
        }

        public abstract void Die();

        protected abstract void Grow(float elapsedTime);

        private void _Internal_Die()
        {
            Alive = false;
            Die();
        }

        private void _NormalizeGrowthCurve()
        {
            growthRateOverLifespan.NormalizeThis(timeOnly: true);
        }
    }
}