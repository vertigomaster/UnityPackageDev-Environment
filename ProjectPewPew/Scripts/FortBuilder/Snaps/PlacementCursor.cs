using System;
using System.Collections.Generic;
using System.Linq;
using IDEK.Tools.DataStructures;
using IDEK.Tools.GameplayEssentials.Placement;
using IDEK.Tools.ShocktroopExtensions;
using IDEK.Tools.ShocktroopUtils;
using IDEK.Tools.ShocktroopUtils.Attributes;
using UnityEngine;
using UnityEngine.Serialization;

namespace IDEK.Tools.GameplayEssentials.Snapping.Cursor
{
    /// <summary>
    /// Denotes how to snap this obj's transform to <see cref="SnapPoint"/>s.
    /// </summary>
    /// <remarks>
    /// splitting this off because I'm realizing the cursor logic is becoming more involved, and there's no reason to
    /// bog the builder down with that; at the end of the day it just uses whatever its placementTransform is -
    /// the logic which that transform's gameobject performs is outside of that.  
    /// </remarks>
    public class PlacementCursor : MonoBehaviour
    {
        /// <summary>
        /// All modes fall back to External if they cannot meet their own snapping mode responsibilities.
        /// </summary>
        // public enum SnapMode { 
        //     SnapPoint = 0, 
        //     ColliderAlignment = 100, 
        //     GridAlignment = 200, 
        //     RawExternal = 10000 
        // }

        /// <inheritdoc cref="SnapMode"/>
        // [Obsolete("doesnt scale")]
        // public SnapMode currentMode = SnapMode.SnapPoint;
        public Material preferredPreviewMaterial;

#if ODIN_INSPECTOR
        [Sirenix.OdinInspector.Required]
#endif
        [AddComponentButton]
        public SnapPointDetectionVolume detectionVolume;

#if ODIN_INSPECTOR
        [Sirenix.OdinInspector.InfoBox("If you want to start with a mode already enabled, set it here.")]
#endif
        public CursorSnapMode currentMode;

        [Obsolete("snap module")]
        private CleanableList<(SnapPoint from, SnapPoint to)> _snapPairCandidates = new();
        private bool _snapPairCandidatesDirty;
        [Obsolete("This seems unhelpful")]
        private Vector3 _externalDesiredPosition;
        [Obsolete("This seems unhelpful")]
        private Quaternion _externalDesiredRotation;
        [Obsolete("This seems unhelpful")]
        private Vector3 _externalDesiredScale;
        
        public GameObject PreviewObject { get; protected set; }
        
        /// <summary>
        /// Cache of the snap points present on the preview object. Generated from <see cref="_RefreshPreviewSnapPointCache"/>.
        /// </summary>
        /// <remarks>
        /// Caches like this can be confusing, but we do so many checks in this process that was worth caching this out.
        /// <br/>
        /// If you NEED up to date snaps, please recalc them with <see cref="_RefreshPreviewSnapPointCache"/>.
        /// </remarks>
        public IEnumerable<SnapPoint> CachedSnapsOnActiveObject { get; protected set; }
        
        public HashSet<SnapPoint> NearbySnapPoints => detectionVolume.nearbySnapPoints;
        
        #region Unity
        private void OnValidate() => _EnsureCorrectComponentSettings();
        private void OnEnable() => _EnsureCorrectComponentSettings();

        private void Update()
        {
            bool snapped = currentMode && currentMode.TryExecuteSnap();
            
            //reset to parent transform if we couldn't resolve a snap
            if (!snapped)
            {
                ResetOffset();
            }
        }

        private void OnDrawGizmos()
        {
            if (!NullCheckSnapPoints()) return;
            Gizmos.color = Color.greenYellow;
            foreach (SnapPoint snap in NearbySnapPoints)
            {
                Gizmos.color = Color.HSVToRGB((snap.type?.GetHashCode() ?? 0f) % 360f / 360f, 1f, 1f);
                Gizmos.DrawSphere(snap.transform.position, 0.1f);
            }
        }
        #endregion

        public void SetSnapMode<TNewSnapMode>() where TNewSnapMode : CursorSnapMode
        {
            var newmode = GetComponent<TNewSnapMode>();
            if (newmode != null) currentMode = newmode;
            currentMode.cursor = this;
        }

        public bool NullCheckSnapPoints() => detectionVolume != null && detectionVolume.nearbySnapPoints != null;

        public void SetPreviewObjectTo(GameObject newPrefab, bool allowPhysics, bool allowCollision)
        {
            //the system using the cursor takes care of this part 
            // prefabOfPreviewObject = newPrefab;
                
            //TODO: instead cache X amount of previous preview objects for faster swapping.
            Destroy(PreviewObject);
                
            //instantiate it under this object
            PreviewObject = Instantiate(newPrefab, transform);
            
            //activate its PlaceableObject component
            var pobj = PreviewObject.GetOrAddComponent<PlaceableObject>();
            pobj.ActivatePlacementPreview();

            //optionally disable annoying stuff
            if (!allowPhysics) _DisablePhysics(PreviewObject);
            if (!allowCollision) _DisableCollision(PreviewObject);

            //update our cache. this will not account for snaps added/removed while the preview is active
            //but that's not really our concern right now.
            //if you're troubleshooting bug from that, you're in the right place!
            _RefreshPreviewSnapPointCache();
        }

        private void _RefreshPreviewSnapPointCache()
        {
            CachedSnapsOnActiveObject = PreviewObject != null
                ? PreviewObject.GetComponentsInChildren<SnapPoint>()
                : Enumerable.Empty<SnapPoint>();
        }

        private void _DisableCollision(GameObject it)
        {
            var cols = it.GetComponentsInChildren<Collider>();
            if (cols.Length > 0)
            {
                foreach (Collider col in cols)
                {
                    col.enabled = false;
                }
            }
        }

        private void _DisablePhysics(GameObject it)
        {
            var rbs = it.GetComponentsInChildren<Rigidbody>();
            if (rbs.Length > 0)
            {
                foreach (Rigidbody rb in rbs)
                {
                    rb.isKinematic = true;
                }
            }
        }

        private void _EnsureCorrectComponentSettings()
        {
            gameObject.TryGetComponentIfNull(ref detectionVolume);
        }

        public void ResetOffset()
        {
            transform.localPosition = Vector3.zero;
            transform.localRotation = Quaternion.identity;
            transform.localScale = Vector3.one;
        }
    }
}