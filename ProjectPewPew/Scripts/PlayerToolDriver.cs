using IDEK.Tools.Coroutines.TaskRoutines;
using IDEK.Tools.GameplayEssentials.Characters.Unity;
using IDEK.Tools.GameplayEssentials.Equipment;
using IDEK.Tools.GameplayEssentials.Equipment.Samples;
using IDEK.Tools.GameplayEssentials.Interaction.Unity;
using IDEK.Tools.GameplayEssentials.Inventory.Core;
using IDEK.Tools.GameplayEssentials.Inventory.Unity;
using IDEK.Tools.GameplayEssentials.Items;
using IDEK.Tools.GameplayEssentials.Items.Unity;
using IDEK.Tools.Logging;
using IDEK.Tools.ShocktroopExtensions;
using IDEK.Tools.ShocktroopUtils.Attributes;
using UnityEngine;
using UnityEngine.InputSystem;

/*taking items out of inventory, instantiating the correct one, and equipping it to the hand so that it
 * - receives activation events from its hand
 * - respects some sort of aiming mechanism that the weapon can decide
 * When do we take stuff out?
 * - on pickup we automatically add it to inventory
 * - but more importantly, on inventory change, we see if:
 *   - do we already have something equipped? return 
 *   - the change was not an add? return
 *   - an item with an incompatible category tag was added? return
 *   - take the item back out
 */

namespace IDEK.Tools.GameplayEssentials.Samples.PewPew
{
    /// <summary>
    /// Sample how to use a player-equipped tool 
    /// </summary>
    public class PlayerToolDriver : MonoBehaviour
    {
        // private static EquipmentCategory;
#if ODIN_INSPECTOR
        [Sirenix.OdinInspector.Required]
#endif
        public BasicVisibleEquipmentSlotManager slotManager;
        public InputActionReference toolActivationInput;
        [Tooltip("Optional field for a separate input to use for detecting the end of a firing action " +
                 "(mostly used for repeatedly trying to fire a weapon when an input is held down, " +
                 "in which case the release input could be a button event bound to the same inputs " +
                 "but which waits for a release instead of a press.")]
        // )]
        public InputActionReference toolReleaseInput;
        
#if ODIN_INSPECTOR
        [Sirenix.OdinInspector.ShowInInspector, Sirenix.OdinInspector.ReadOnly]
#endif
        private GameObject currentEquippedToolObj;
        // public Transform slotTransform;

#if ODIN_INSPECTOR
        [Sirenix.OdinInspector.ShowInInspector, Sirenix.OdinInspector.ReadOnly]
#endif
        private ItemStack currentlyEquippedItem; 
        
        [SerializeField]
        private InventoryComponent _ourInventory;

        private bool somethingEquipped = false;

        [OverridesMustCallBase]
        protected virtual void OnValidate()
        {
            FindBestMatchingInventory();
        }

        [OverridesMustCallBase]
        protected virtual void OnEnable()
        {
            FindBestMatchingInventory();

            if ((currentlyEquippedItem == null || currentlyEquippedItem.IsStale) && _ourInventory)
            {
                ConsoleLog.Log("fooble - about to add listener to on inventory changed");
                _ourInventory.Runtime?.OnInventoryChanged?.TryAddListener(OnInvChangeTryEquip_Internal);
            }

            toolActivationInput.action.performed += OnActivationInput;
            // toolActivationInput.action.IsPressed()
        }

        [OverridesMustCallBase]
        protected virtual void OnDisable()
        {
            //we always want to shut it down if it is still listening. IdekEvent lets us safely try.
            if (_ourInventory)
            {
                ConsoleLog.Log("fooble - about to remove listener from on inventory changed");
                _ourInventory.Runtime?.OnInventoryChanged?.TryRemoveListener(OnInvChangeTryEquip_Internal);
            }

            toolActivationInput.action.performed -= OnActivationInput;

        }

        private void OnActivationInput(InputAction.CallbackContext obj)
        {
            if (currentEquippedToolObj == null ||
                !currentEquippedToolObj.TryGetComponent(out BaseTool equippedTool)) return;
            
            equippedTool.TryActivate();
        }

        /// <summary>
        /// not exposing; we need to always get back the reference we are after,
        /// so we only run this when we are explicitly looking for a potentially new one.
        /// </summary>
        protected virtual void FindBestMatchingInventory()
        {
            if (gameObject.TryGetRedirectedComponentIfNull(ref _ourInventory)) return;
            if (!gameObject.TryGetRedirectedComponent(out CharacterAvatar a)) return;
            if (a.CharacterState == null) return;
            a.CharacterState.TryGetRedirectedComponent(out _ourInventory);
        }

        private void OnInvChangeTryEquip_Internal(InventoryRuntime inv, InventoryChange change)
        {
            //stop listening during the execution to avoid recursive calls and debounce.
            _ourInventory.Runtime.OnInventoryChanged.TryRemoveListener(OnInvChangeTryEquip_Internal);
            
            //do we not have something equipped?
            if(!TryEquip(inv, change))
            {
                ConsoleLog.Log("fooble - Failed to equip the item from this change. " +
                               "Resume listening to see if the next one will work.");
                //Failed to equip the item from this change.
                //Resume listening to see if the next one will work.
                _ourInventory.Runtime.OnInventoryChanged.TryAddListener(OnInvChangeTryEquip_Internal); 
                return;
            }

            ConsoleLog.Log("fooble - Equip must have succeeded!");
            //by this point, the equip succeeded, so we do not need to listen to the event anymore.
        }

        protected virtual bool TryEquip(InventoryRuntime inv, InventoryChange change)
        {
            //the change was not an add? return

            ConsoleLog.Log("fooble - Attempting equip");
            
            if (!change.WasAdded) return false;

            ConsoleLog.Log("fooble - sees change was an Add");

            //the item does not contain the necessary metadata? return
            if(!change.Key.def.globalMetadata.TryGetData(
                    out BaseEquipmentMetadata equipmentMetadata)) 
                return false;

            ConsoleLog.Log("fooble - global has equipment metadata");

            //take the item back out
            if (!inv.TryTake(change.Key, 1, out currentlyEquippedItem))
            {
                ConsoleLog.Log("fooble - inventory take failed");
                return false;
            }

            ConsoleLog.Log("fooble - inventory take worked");

            //spawn it
            return slotManager.TryInstantiateInAppropriateSlot(
                change.Key.def as UnityItemDef,
                out currentEquippedToolObj, 
                equipmentMetadata);
        }
    }
}