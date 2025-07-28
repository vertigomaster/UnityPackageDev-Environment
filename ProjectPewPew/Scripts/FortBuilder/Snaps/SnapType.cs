using System.Collections.Generic;
using System.Linq;
using IDEK.Tools.DataStructures;
using IDEK.Tools.Logging;
using IDEK.Tools.ShocktroopExtensions;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;

namespace IDEK.Tools.GameplayEssentials.Snapping
{
    /// <summary>
    /// What kind of snap - corner, side, floor-to-wall, whatever
    /// lets you define what other <see cref="SnapType"/> elements it can (or can't) snap with
    /// If none are listed, it is not inherently limited, and will only be subject to the other's limitations
    /// In the event of conflicts, denied snap combos override allowed snap combos 
    /// </summary>
    /// <remarks>
    /// TODO: avoid the mix up by defining a snap point type matrix in settings
    /// </remarks>
    [CreateAssetMenu(menuName = SnapHelpers.SNAP_MENU_PATH + "/Create New SnapType",
        fileName = "SnappingSettingsAsset", order = 0)]
    public class SnapType : ScriptableObject //, ISerializationCallbackReceiver
    {
        public enum AlignmentMode
        {
            KeepOriginal = 0,
            [Tooltip("Should the snapped thing align to the transform")]
            Match = 100,

            //once I wrote out the tooltip, I realized how stupid this mode was.
            //you can just angle your snap point to the orientation you want the piece to snap to.
            // [Tooltip("Treats the rotation of the snap point as the angle bisector. In practice, this takes the quaternion between the snap's and its owning object's rotations, and applies it again to get the final rotation. Example: if your snap point is on the edge of a wall and set to this and is angles 45 degrees toward that wall part, the newly snapped piece will be 90 degress  ")]
            // AngleBisector = 200
        }

        [Tooltip("Whether to allow other Snap types that were not explicitly allowed to snap to this one. " +
            "If enabled, all types which also have this flag enabled will snap to this one unless explicitly told not to.")]
        public bool allowUnlistedSnaps = true; //how to handle types not listed in the two below
        //TODO: find some way to enforce symmetry if that becomes a big pain

        [SerializeField]
        private List<SnapType> explicitlyAllowedSnaps;

        [SerializeField]
        private List<SnapType> explicitlyDisallowedSnaps;
        
        public HashSet<SnapType> runtimeAllowedSnaps;
        public HashSet<SnapType> runtimeDisallowedSnaps;
        
        public AlignmentMode alignmentMode = AlignmentMode.Match;
        [Tooltip("Not really used right now, actually")]
#if ODIN_INSPECTOR
        [Sirenix.OdinInspector.InfoBox("Not really used right now, actually", InfoMessageType.Warning)]
#endif
        public SnapAxis snapAxes = SnapAxis.X;
        [Tooltip("Should it snap along world axes or the local axes of the Snap Point's transform?")]
        public bool useWorldSpaceAxes = false;
        [Tooltip("Can backwards alignments count? (like whether an outer edge wall is allow to snap " +
            "its exterior side to its interior side. The closest orientation will be used, if enabled.")]
        public bool allowFlip = true;
        [Tooltip("Whether Snap Points of this type can snap to other Snap Points of this same type. Good for when you want to use the same snap type for both ends.")]
        public bool canSnapWithSameType = true;

        private void OnEnable()
        {
            // ConsoleLog.Log($"values is {explicitlyAllowedSnaps.ToEnumeratedString()}");
            // ConsoleLog.Log($"{name}: expl allowed hashset is {explicitlyAllowedSnaps.hashSet.ToEnumeratedString()}");
            // ConsoleLog.Log($"{name}: expl allowed hashset as list is {explicitlyAllowedSnaps.hashSet.ToList().ToEnumeratedString()}");
            runtimeAllowedSnaps = explicitlyAllowedSnaps.ToHashSet();
            runtimeDisallowedSnaps = explicitlyDisallowedSnaps.ToHashSet();
        }

        /// <summary>
        /// Indicates whether the two types are compatible.
        /// </summary>
        /// <remarks>Null checks are not made here to avoid pointless overhead. 
        /// Ensure your data is valid before querying.</remarks>
        /// <returns>Whether the two types are compatible.</returns>
        public static bool TestCompatibility(SnapType a, SnapType b)
        {
            if (a == b) return a.canSnapWithSameType;
            if (a.explicitlyDisallowedSnaps.Contains(b) || b.explicitlyDisallowedSnaps.Contains(a)) return false;
            if (a.allowUnlistedSnaps && b.allowUnlistedSnaps) return true; //they treat any non-explicit denials as allowed.
            
            return a.explicitlyAllowedSnaps.Contains(b) || b.explicitlyAllowedSnaps.Contains(a);
        }
        
        public bool IsCompatibleWith(SnapType other) => TestCompatibility(this, other);

        // #region Implementation of ISerializationCallbackReceiver
        //
        // public List<SnapTypeAsset> _explicitlyAllowedSnaps;
        // public List<SnapTypeAsset> _explicitlyDisallowedSnaps;
        //
        // /// <inheritdoc />
        // public void OnBeforeSerialize()
        // {
        //     _explicitlyAllowedSnaps = explicitlyAllowedSnaps.ToList();
        //     _explicitlyDisallowedSnaps = explicitlyDisallowedSnaps.ToList();
        // }
        //
        // /// <inheritdoc />
        // public void OnAfterDeserialize()
        // {
        //     explicitlyAllowedSnaps = _explicitlyAllowedSnaps.ToHashSet();
        //     explicitlyDisallowedSnaps = _explicitlyDisallowedSnaps.ToHashSet();
        // }
        //
        // #endregion
    }
}