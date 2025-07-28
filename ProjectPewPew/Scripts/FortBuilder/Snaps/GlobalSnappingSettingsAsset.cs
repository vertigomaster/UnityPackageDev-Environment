using IDEK.Tools.ShocktroopUtils;
using UnityEngine;

namespace IDEK.Tools.GameplayEssentials.Snapping
{
    /// <summary>
    /// Denotes project-wide settings for how to snap things together
    /// (or even whether to do so at all).
    /// </summary>
    /// <remarks>
    /// Scales and iterates better than hardcoding it into <see cref="PlacementCursor"/>. 
    /// <br/>
    /// More specific per-system needs should define their own separate settings assets.
    /// </remarks>
    // [CreateAssetMenu(menuName = SnapHelpers.SNAP_MENU_PATH + "/Create SnappingSettingsAsset",
    //     fileName = "SnappingSettingsAsset", order = 0)]
    public class GlobalSnappingSettingsAsset : ExtantSingletonScrob<GlobalSnappingSettingsAsset>
    {
        public LayerMask snappableCursorLayers;
        public LayerMask snappingPointLayers;
        
        [SerializeField, HideInInspector]
        private bool firstRun = true;

        public static LayerMask SnapCursorLayers => Instance.snappableCursorLayers;
        public static LayerMask SnappingPointLayers => Instance.snappingPointLayers;
        public static LayerMask SnapSystemMask => Instance.snappingPointLayers | Instance.snappingPointLayers;

        private void OnEnable()
        {
            if (firstRun && snappableCursorLayers == 0)
                snappableCursorLayers = LayerMask.GetMask("SnapCursor");
            if (firstRun && snappingPointLayers == 0)
                snappingPointLayers = LayerMask.GetMask("SnapPoint");

            firstRun = false;
        }
    }
}