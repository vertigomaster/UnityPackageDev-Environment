using System;
using IDEK.Tools.GameplayEssentials.Conflict.Damage;
using IDEK.Tools.GameplayEssentials.Conflict.Weapons.Unity;
using UnityEngine;

namespace IDEK.Tools.GameplayEssentials.Samples.PewPew
{
    public class PewPewGun : HitscanBaseGun
    {
        // private static readonly int shootTriggerHash = Animator.StringToHash("shoot");
#if ODIN_INSPECTOR
        [Sirenix.OdinInspector.ShowInInspector, Sirenix.OdinInspector.ReadOnly]
#endif

        //TODO: damage scaling table
#if ODIN_INSPECTOR
        [Sirenix.OdinInspector.EnableIf("@baseDamageSpec == null")]
#endif
        public float baseDamage = 10f;

        public DamageApplicationSpec baseDamageSpec;
        
        [Obsolete("moved onto the bullet prefab. Just select a hitscan one.")]
        public LineRenderer raycastLine;
        public GameObject bulletPrefab;

        private Vector3[] positions = new Vector3[3];

        #region Overrides of BaseGun

        protected override void Activate()
        {
            var bullet = Instantiate(bulletPrefab, MuzzleSocket.position, MuzzleSocket.rotation);
            if (!bullet.TryGetComponent(out Projectile projectileData)) return;
            
            if (baseDamageSpec != null)
                projectileData.AddAdditionalDamage(baseDamageSpec.Apply());
            else 
                projectileData.AddAdditionalDamage(baseDamage);

            projectileData.OverrideRangeConfig(rangeConfig);

            base.Activate();
        }

        #endregion
    }
}