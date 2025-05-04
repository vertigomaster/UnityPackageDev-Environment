using IDEK.Tools.ShocktroopExtensions;
using Sirenix.OdinInspector;
using Unity.Collections;
using UnityEngine;

namespace IDEK.Tools.Misc.DevEnv.Scripts.Devtest
{
    public class BasicCubePlant : PlantBase
    {
#if ODIN_INSPECTOR
        [ShowInInspector, Sirenix.OdinInspector.ReadOnly]
#endif
        public float Size { get; protected set; } = 1f;

        public ParticleSystem deathVFX;
        
        #region Overrides of PlantBase

        /// <inheritdoc />
        protected override void Update()
        {
            base.Update();
            transform.localScale = transform.localScale.WithY(Size);
        }

        /// <inheritdoc />
        public override void Die()
        {
            deathVFX.Play();
            Destroy(gameObject);
        }

        /// <inheritdoc />
        protected override void Grow(float deltaTime)
        {
            Size += deltaTime * growthRateOverLifespan.Evaluate(age / lifespan);
        }

        #endregion
    }
}