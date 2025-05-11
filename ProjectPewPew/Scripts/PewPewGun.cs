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
        
        public LineRenderer raycastLine;
        // public Animator lineAnimator;

        // [ValueDropdown("@DHAnimation.GetTriggers(lineAnimator)")]
        // [SerializeField] private string shootTrigger;
        // private int hashedShootTrigger;
        
        private Vector3[] positions = new Vector3[3];

        // [OverridesMustCallBase]
        // protected virtual void Start()
        // {
        //     hashedShootTrigger = Animator.StringToHash(shootTrigger);
        // }
        // private void OnEnable()
        // {
        //     //tries to fill out the gun metadata with that from an associated item
        //     //we want to centralize that kind of configuration as best as possible,
        //     //so if an item is associated, we should use it
        //     // TryLoadGunMetadataFromAttachedItem();
        // }

        // private void TryLoadGunMetadataFromAttachedItem()
        // {
        //     if (!gameObject.TryGetRedirectedComponent(out InventoryItemRepresentation itemRep)) return;
        //     //we check runtime metadata first to see if it overrides the global metadata for the item.
        //         
        //     GunMetadata givenGunMetadata;
        //     
        //     //This can technically be one if, but it was too long and unreadable.
        //     var stackMeta = itemRep.representedItemStack?.metadata;
        //     if (stackMeta?.TryGetData(out givenGunMetadata) == true)
        //     {
        //         gunMetadata = givenGunMetadata;
        //         return;
        //     }
        //         
        //     var stackDef = itemRep.representedItemStack?.itemDef; 
        //     if(stackDef != null && stackDef.data?.globalMetadata?.TryGetData(out givenGunMetadata) == true)
        //     {
        //         gunMetadata = givenGunMetadata;
        //         return;
        //     }
        // }

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
            if (!Physics.Raycast(
                    MuzzleSocket.position,
                    MuzzleSocket.forward,
                    out RaycastHit hit,
                    rangeConfig.MaxRange))
            {
                ConsoleLog.Log($"fooble - raycast hit nothing.");

                return;
            }

            if (hit.collider.TryGetComponent(out BaseHealthComponent healthComponent))
            {
                var dmg = baseDamage * rangeConfig.DamageFalloffByRangeGraph.Evaluate(hit.distance);
                ConsoleLog.Log($"Bang! Dealing {dmg} damage to {hit.collider.gameObject}");
                healthComponent.TakeDamage(dmg);
            }
            else
            {
                ConsoleLog.Log($"fooble - Can't damage {hit.collider.gameObject}; it doesn't have a health component.");
            }
            
            DrawLineRenderer(hit);
        }
        
#if ODIN_INSPECTOR
        [Sirenix.OdinInspector.Button]
#endif
        private void DrawLineRenderer(RaycastHit hit)
        {
            raycastLine.enabled = true;
            
            positions[0] = MuzzleSocket.position;
            positions[1] = hit.point;
            Vector3 reflectedRichochet = Vector3.Reflect(MuzzleSocket.forward, hit.normal);
            positions[2] = positions[1] + reflectedRichochet * 100;
            raycastLine.SetPositions(positions);

            TaskRoutine.Delay(0.5f, () => raycastLine.enabled = false);
        }
    }
}