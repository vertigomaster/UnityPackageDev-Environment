using IDEK.Tools.GameplayEssentials.Characters.Unity;
using IDEK.Tools.GameplayEssentials.Equipment;
using IDEK.Tools.GameplayEssentials.Equipment.Samples;
using IDEK.Tools.GameplayEssentials.Inventory.Core;
using IDEK.Tools.GameplayEssentials.Inventory.Unity;
using IDEK.Tools.GameplayEssentials.Items;
using IDEK.Tools.ShocktroopExtensions;
using IDEK.Tools.ShocktroopUtils;
using UnityEngine;

namespace IDEK.Tools.GameplayEssentials.Inventory.Samples.Unity
{
    /// <summary>
    /// Sample how to use a player-equipped tool 
    /// </summary>
    public class PlayerToolDriver : MonoBehaviour
    {
        
        [SerializeField]
        private InventoryComponent _ourInventory;
        
        //taking items out and equipping them to the hand
        public InventoryRuntime GetInventory()
        {
            if(gameObject.TryGetRedirectedComponentIfNull(ref _ourInventory)) return _ourInventory.Runtime;
            return null;
        }
        
        public void EquipFromInventory()
        {
            // _ourInventory
        }

        private void OnValidate()
        {
            GetInventory();
        }
    }

    // public record EquippableToolMetadata : BaseEquipmentMetadata
    // {
    //     public GameObject prefab;
    // }
}