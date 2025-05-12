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

namespace IDEK.Tools.GameplayEssentials.Samples.PewPew
{
    public class PewPewGun : BaseGun
    {
        // private static readonly int shootTriggerHash = Animator.StringToHash("shoot");
#if ODIN_INSPECTOR
        [Sirenix.OdinInspector.ShowInInspector, Sirenix.OdinInspector.ReadOnly]
#endif
        public float LastShotTime { get; protected set; } = 0.0f;

        //TODO: damage scaling table
        public float baseDamage = 10f;
        
        [Obsolete("moved onto the bullet prefab. Just select a hitscan one.")]
        public LineRenderer raycastLine;
        public GameObject bulletPrefab;

        private Vector3[] positions = new Vector3[3];

        #region Overrides of BaseTool

        /// <inheritdoc />
#if ODIN_INSPECTOR
        [Sirenix.OdinInspector.Button]
#endif
        public override bool TryActivate()
        {
            if (!IsReadyToFire()) return false;

            //do it
            Shoot();

            LastShotTime = Time.time;
            return true;
        }

        #endregion

        #region Overrides of BaseGun

        /// <inheritdoc />
        public override bool IsReadyToFire()
        {
            return (Time.time - LastShotTime) >= fireModeConfig.EstimatedSecondsBetweenShots;
        }

        #endregion

        private void Shoot()
        {
            var bullet = Instantiate(bulletPrefab, MuzzleSocket.position, MuzzleSocket.rotation);
            if (bullet.TryGetComponent(out Projectile projectileData))
            {
                projectileData.AddAdditionalDamage(baseDamage);
                projectileData.OverrideRangeConfig(rangeConfig);
            }
            
            // if (!Physics.Raycast(
            //         MuzzleSocket.position,
            //         MuzzleSocket.forward,
            //         out RaycastHit hit,
            //         rangeConfig.MaxRange))
            // {
            //     ConsoleLog.Log($"fooble - raycast hit nothing.");
            //
            //     return;
            // }

            // DrawLineRenderer(hit);
        }
        
// #if ODIN_INSPECTOR
//         [Sirenix.OdinInspector.Button]
// #endif
//         private void DrawLineRenderer(RaycastHit hit)
//         {
//             raycastLine.enabled = true;
//             
//             positions[0] = MuzzleSocket.position;
//             positions[1] = hit.point;
//             Vector3 reflectedRichochet = Vector3.Reflect(MuzzleSocket.forward, hit.normal);
//             positions[2] = positions[1] + reflectedRichochet * 100;
//             raycastLine.SetPositions(positions);
//
//             TaskRoutine.Delay(0.5f, () => raycastLine.enabled = false);
//         }
    }
}