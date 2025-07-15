using System.Collections.Generic;
using System.Linq;
using IDEK.Tools.DataStructures;
using UnityEngine;

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
    [CreateAssetMenu(menuName = SnapHelpers.SNAP_MENU_PATH + "/Create SnappingSettingsAsset",
        fileName = "SnappingSettingsAsset", order = 0)]
    public class SnapType : ScriptableObject //, ISerializationCallbackReceiver
    {
        public enum AlignmentMode
        {
            KeepOriginal = 0,
            Match = 100, 
            AngleBisector = 200
        }

        public bool allowUnlistedSnaps = true; //how to handle types not listed in the two below
        //TODO: find some way to enforce symmetry if that becomes a big pain
        
        public SerializedHashSet<SnapType> explicitlyAllowedSnaps;
        public SerializedHashSet<SnapType> explicitlyDisallowedSnaps;
        
        public AlignmentMode alignmentMode = AlignmentMode.Match;
        public SnapAxis snapAxes = SnapAxis.X;
        public bool useWorldSpaceAxes = false;
        [Tooltip("Can backwards alignments count? (like whether an outer edge wall is allow to snap " +
            "its exterior side to its interior side. The closest orientation will be used, if enabled.")]
        public bool allowFlip = true;
        public bool canSnapWithSelf = true;

        /// <summary>
        /// Indicates whether the two types are compatible.
        /// </summary>
        /// <remarks>Null checks are not made here to avoid pointless overhead. 
        /// Ensure your data is valid before querying.</remarks>
        /// <returns>Whether the two types are compatible.</returns>
        public static bool TestCompatibility(SnapType a, SnapType b)
        {
            if (a == b) return a.canSnapWithSelf;
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