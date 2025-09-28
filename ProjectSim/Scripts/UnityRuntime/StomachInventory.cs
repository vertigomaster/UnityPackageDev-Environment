using IDEK.Tools.GameplayEssentials.Inventory.Core;
using IDEK.Tools.GameplayEssentials.Inventory;
using IDEK.Tools.GameplayEssentials.Inventory.Unity;
using IDEK.Tools.GameplayEssentials.Items;
using IDEK.Tools.GameplayEssentials.Quantities;
using UnityEngine;

namespace IDEK.ProjectSim.Unity.Runtime
{
    /// <summary>
    /// Represents the eaten items within a creature's stomach.
    /// Useful for making certain effects happen as a result of what has been eaten 
    /// </summary>
    public class StomachInventory : InventoryComponent<FiniteVolumeInventoryRuntime>
    {
        public bool TryEat(InventoryItemRepresentation itemRep)
        {
            return itemRep.TryObtain(Runtime);
        }
    }
}