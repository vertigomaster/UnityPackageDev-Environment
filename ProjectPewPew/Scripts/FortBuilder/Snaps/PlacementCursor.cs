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

namespace IDEK.Tools.GameplayEssentials.Snapping
{
    //TODO: move each mode into a module to stay ahead of the spaghetti
    //and make this less frustrating to work with.
    public class SnapPointModule { }
    
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
        public enum SnapMode { 
            SnapPoint = 0, 
            ColliderAlignment = 100, 
            GridAlignment = 200, 
            RawExternal = 10000 
        }
        
        /// <inheritdoc cref="SnapMode"/>
        public SnapMode currentMode = SnapMode.SnapPoint;
        
        //how do we detect snaps? triggers and a dedicated collision layer might be efficient - querying space for snap points

        // idk if we'll directly talk to him, but we need one on here instead of on the snaps - one active rigidbody
        // collider on a dedicated layer qeurying against numerous passive triggers is way more efficient,
        // as RIGIDBODIES are the ones that actually broadcast collisions in the engine.
        
        public Material preferredPreviewMaterial;

#if ODIN_INSPECTOR
        [Sirenix.OdinInspector.Required]
#endif
        [AddComponentButton]
        public SnapPointDetectionVolume detectionVolume;
        
        // protected GameObject prefabOfPreviewObject;
        public GameObject PreviewObject { get; protected set; }
        
        public Vector3 Position => PreviewObject.NullCheck(x=>x.transform.position, transform.position);
        public Quaternion Rotation => PreviewObject.NullCheck(x=>x.transform.rotation, transform.rotation);
        
        /// <summary>
        /// Cache of the snap points present on the preview object. Generated from <see cref="_CalcPreviewSnapPoints"/>.
        /// </summary>
        /// <remarks>
        /// Caches like this can be confusing, but we do so many checks in this process that was worth caching this out.
        /// <br/>
        /// If you NEED up to date snaps, please recalc them with <see cref="_CalcPreviewSnapPoints"/>.
        /// </remarks>
        private IEnumerable<SnapPoint> _cachedSnapsOnPreviewedObject;

        private CleanableList<(SnapPoint from, SnapPoint to)> _snapPairCandidates = new();
        // private CleanableList<(SnapPoint from, SnapPoint to)> test = new();
        private bool _snapPairCandidatesDirty;

        [Obsolete("This seems unhelpful")]
        private Vector3 _externalDesiredPosition;

        [Obsolete("This seems unhelpful")]
        private Quaternion _externalDesiredRotation;

        [Obsolete("This seems unhelpful")]
        private Vector3 _externalDesiredScale;

        public HashSet<SnapPoint> NearbySnapPoints => detectionVolume.nearbySnapPoints;
        public bool NullCheckSnapPoints() => detectionVolume != null && detectionVolume.nearbySnapPoints != null;
        public bool AreThereNearbySnapPoints() => NullCheckSnapPoints() && NearbySnapPoints.Count > 0;

        private void OnValidate() => _EnsureCorrectComponentSettings();
        private void OnEnable() => _EnsureCorrectComponentSettings();

        private void Awake()
        {
            _externalDesiredPosition = transform.position;
            _externalDesiredRotation = transform.rotation;
            _externalDesiredScale = transform.localScale;
        }

        private void Update()
        {
            bool resolved = false;
            switch (currentMode)
            {
                case SnapMode.SnapPoint:
                    resolved = _TryPerformSnapPointSnapping();
                    break;
                case SnapMode.ColliderAlignment:
                    break;
                case SnapMode.GridAlignment:
                    break;
                case SnapMode.RawExternal:
                    resolved = true; //explicit
                    _UseRawExternalInput();
                    break;
            }

            //reset to parent transform if we couldn't resolve a snap
            if (!resolved)
            {
                transform.localPosition = Vector3.zero;
                transform.localRotation = Quaternion.identity;
                transform.localScale = Vector3.one;
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

        private bool _TryPerformSnapPointSnapping()
        {
            if (!AreThereNearbySnapPoints()) return false;
            if (!_cachedSnapsOnPreviewedObject.Any()) return false;
            
            var snapPair = _GetLowestEnergySnapPair();

            //process final candidate
            if (snapPair == default) return false;
            
            _SnapThePreview(snapPair);
            return true;
        }

        [Obsolete("This seems unhelpful")]
        private void _UseRawExternalInput()
        {
            transform.position = _externalDesiredPosition;
            transform.rotation = _externalDesiredRotation;
        }

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
            _cachedSnapsOnPreviewedObject = _CalcPreviewSnapPoints();
        }

        private IEnumerable<SnapPoint> _CalcPreviewSnapPoints()
        {
            //thought it was going to be more complicated
            //it may still end up that way tho
            if (PreviewObject == null) return Enumerable.Empty<SnapPoint>();
            return PreviewObject.GetComponentsInChildren<SnapPoint>();
        }

        [Obsolete("This seems unhelpful")]
        public void InputPosition(Vector3 inputPosition) => _externalDesiredPosition = inputPosition;

        [Obsolete("This seems unhelpful")]
        public void InputRotation(Quaternion inputRotation) => _externalDesiredRotation = inputRotation;

        [Obsolete("This seems unhelpful")]
        public void InputScale(Vector3 inputScale) => _externalDesiredScale = inputScale;

        private (SnapPoint from, SnapPoint to) _GetLowestEnergySnapPair()
        {
            //get an up to date thing to work from.
            _RefreshSnapPairCandidates();
            
            float bestSqrDistance = float.MaxValue; //cache
            float currentDist; //cache
            (SnapPoint from, SnapPoint to) candidateSnapPair = default; //cache
            
            //the "from" is the snap point on the preview
            //the "to" is the snap point on the othe object
            //we "snap" them together by moving our preview such that one of that preview's snaps (the from) is
            //at the same position and offset rotation as another snap (the to).
            
            //get the "from -> to" snap operation pair with the smallest delta (requires the minimal shift for the preview object)  
            foreach (var fromToPair in _snapPairCandidates)
            {
                currentDist = Vector3.SqrMagnitude(fromToPair.to.transform.position - fromToPair.from.transform.position);
                if (currentDist < bestSqrDistance)
                {
                    bestSqrDistance = currentDist;
                    candidateSnapPair = fromToPair;
                }
            }

            return candidateSnapPair;
        }

        private void _SnapThePreview((SnapPoint from, SnapPoint to) candidate)
        {
            //this is how much the "from" needs to move to be at the "to"
            //this does not account for rotation
            //whatever rotation we do, we would try to perform about the from point, since "from" is ours.
            Vector3 delta = candidate.to.transform.position - candidate.from.transform.position;
                
            //apply that offset to preview the snap
                
            //this will shift everything, but it SHOULD be stable, in that it collapses the delta/displacement to 0
            //so it should still be the closest. Probably.
            //Multiple points that end up close to other multiple foreign points might cause shifting, but
            //avoiding that is the responsibility of the designer, not the system programmer.
            transform.position += delta; 
                
            //now we need to rotate to match the snap.
            //For this system, we take on the limitation of having a snap point's rotation be the rotation you want to match in order to validly snap to it.
            //this means a snappable object's visuals may need to be a child object to offset it
            //(which is already fairly standard practice for most object instantiation related workflows anyway). 
            transform.rotation = candidate.to.transform.rotation;

            transform.localScale = _externalDesiredScale;
        }

        /// <summary>
        /// refreshes the set of snap points that we could actually snap to on this frame.
        /// </summary>
        /// <remarks>done using the previews from <see cref="_cachedSnapsOnPreviewedObject"/>.</remarks>
        private void _RefreshSnapPairCandidates()
        {
            _snapPairCandidates.Clear();

            //check our current object (nested) for its own snap points and see which of them snap to the nearby ones

            foreach (SnapPoint mySnap in _cachedSnapsOnPreviewedObject)
            {
                //for each snap point on our preview,
                //see if it is compatible with each of the nearby ones   

                foreach (var nearbySnap in NearbySnapPoints)
                {
                    if (mySnap.type.IsCompatibleWith(nearbySnap.type))
                    {
                        //yay compat
                        _snapPairCandidates.Add((from: mySnap, to: nearbySnap));
                    }
                    
                }
            }
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
    }
}