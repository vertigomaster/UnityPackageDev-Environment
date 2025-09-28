using IDEK.Tools.GameplayEssentials.Inventory.Unity;
using UnityEngine;

namespace IDEK.ProjectSim.Unity.Runtime
{
    /// <summary>
    /// For classes that run/mutate runtime data of an item
    /// </summary>
    [RequireComponent(typeof(InventoryItemRepresentation))]
    public class ItemLogic : MonoBehaviour
    {
#if ODIN_INSPECTOR
        [Sirenix.OdinInspector.Required]
#endif
        public InventoryItemRepresentation itemRep;
    }
}