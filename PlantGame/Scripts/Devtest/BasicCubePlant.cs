using IDEK.Tools.Coroutines.TaskRoutines;
using IDEK.Tools.ShocktroopExtensions;
using Sirenix.OdinInspector;
using Unity.Collections;
using UnityEngine;

namespace IDEK.Tools.Misc.DevEnv.Scripts.Devtest
{
    public class BasicCubePlant : PlantBase
    {
#if ODIN_INSPECTOR
        [Sirenix.OdinInspector.ShowInInspector, Sirenix.OdinInspector.ReadOnly]
#endif
        public float Size { get; protected set; } = 1f;

        public ParticleSystem deathVFX;
        public bool overrideParticleParent = true;
        public float deleteVFXAfterDurationDelay = 10f;
        
        #region Overrides of PlantBase

        /// <inheritdoc />
        protected override void Update()
        {
            base.Update();
            transform.localScale = transform.localScale.WithY(Size);
        }

        /// <inheritdoc />
        public override void OnDeath()
        {
            if (deathVFX)
            {
                if (overrideParticleParent)
                {
                    deathVFX.transform.localPosition = Vector3.zero;
                    deathVFX.transform.SetParent(transform.parent);
                    deathVFX.transform.localScale = Vector3.one;
                    deathVFX.transform.position += Vector3.up * 0.05f;
                }
                
                deathVFX.Play();

                TaskRoutine.WaitUntil(
                    () => deathVFX.time > deathVFX.main.duration && deathVFX.particleCount <= 0,
                    () => Destroy(deathVFX.gameObject));
            }
            Destroy(gameObject);
        }

        /// <inheritdoc />
        protected override void OnGrow(float deltaTime)
        {
            Size += deltaTime * growthRateOverLifespan.Evaluate(age / lifespan);
        }

        #endregion
    }
}