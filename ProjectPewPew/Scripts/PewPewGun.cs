using System;
using IDEK.Tools.Coroutines.TaskRoutines;
using IDEK.Tools.GameplayEssentials.Abilities;
using IDEK.Tools.GameplayEssentials.Conflict.Damage;
using IDEK.Tools.GameplayEssentials.Conflict.Damage.Unity;
using IDEK.Tools.GameplayEssentials.Conflict.Weapons.Unity;
using IDEK.Tools.GameplayEssentials.Inventory.Unity;
using IDEK.Tools.GameplayEssentials.Items;
using IDEK.Tools.Logging;
using IDEK.Tools.ShocktroopExtensions;
using IDEK.Tools.ShocktroopUtils;
using IDEK.Tools.ShocktroopUtils.Attributes;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;

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