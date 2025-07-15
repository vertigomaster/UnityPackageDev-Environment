using System.Collections.Generic;
using System.Linq;
using IDEK.Tools.GameplayEssentials.Placement;
using IDEK.Tools.ShocktroopExtensions;
using UnityEngine;
using UnityEngine.Serialization;

namespace IDEK.Tools.GameplayEssentials.Snapping
{
    /// <summary>
    /// Denotes how to snap the transform to snap points
    /// </summary>
    /// <remarks>
    /// splitting this off because I'm realizing the cursor logic is becoming more involved, and there's no reason to
    /// bog the builder down with that; at the end of the day it just uses whatever its placementTransform is -
    /// the logic which that transform's gameobject performs is outside of that.  
    /// </remarks>
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(Collider))]
    public class SnappablePlacementCursor : MonoBehaviour
    {
        //how do we detect snaps? triggers and a dedicated collision layer might be efficient - querying space for snap points

        // idk if we'll directly talk to him, but we need one on here instead of on the snaps - one active rigidbody
        // collider on a dedicated layer qeurying against numerous passive triggers is way more efficient,
        // as RIGIDBODIES are the ones that actually broadcast collisions in the engine.
        public Rigidbody cursorRigidBody;
        public Collider snapDetectionCollider;
        
        public Material preferredPreviewMaterial;
        
        protected HashSet<SnapPoint> nearbySnapPoints = new();
        
        // protected GameObject prefabOfPreviewObject;
        protected GameObject previewObject;
        private IEnumerable<SnapPoint> _ourPreviewSnaps;

        private void OnValidate()
        {
            _EnsureCorrectComponentSettings();
        }

        private void OnEnable()
        {
            _EnsureCorrectComponentSettings();
        }

        private List<(SnapPoint from, SnapPoint to)> _currentFromToCandidates = new();
        private void Update()
        {
            if (nearbySnapPoints.Count <= 0) return;
            if (!_ourPreviewSnaps.Any()) return;

            _currentFromToCandidates.Clear();
            
            //check our current object (nested) for its own snap points and see which of them snap to the nearby ones

            foreach (SnapPoint mySnap in _ourPreviewSnaps)
            {
                //for each snap point on our preview,
                //see if it is compatible with each of the nearby ones   

                foreach (var nearbySnap in nearbySnapPoints)
                {
                    if (mySnap.type.IsCompatibleWith(nearbySnap.type))
                    {
                        //yay compat
                        _currentFromToCandidates.Add((from: mySnap, to: nearbySnap));
                    }
                    
                }
            }
            
            //out of those candidates, get the...smallest delta I guess?

            float bestSqrDistance = float.MaxValue;
            float curr;
            (SnapPoint from, SnapPoint to) candidate = default;
            
            foreach (var fromToPair in _currentFromToCandidates)
            {
                curr = Vector3.SqrMagnitude(fromToPair.to.transform.position - fromToPair.from.transform.position);
                if (curr < bestSqrDistance)
                {
                    bestSqrDistance = curr;
                    candidate = fromToPair;
                }
            }

            if (candidate != default)
            {
                //get delta
                Vector3 delta = candidate.to.transform.position - candidate.from.transform.position;
                //apply that offset to preview the snap
                
                transform.position += delta; //Warning: this will flit around
            }
            
            //see if we need to offset our position or the preview object's position
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.greenYellow;
            foreach (SnapPoint snap in nearbySnapPoints)
            {
                Gizmos.color = Color.HSVToRGB((snap.type?.GetHashCode() ?? 0f) % 360f / 360f, 1f, 1f);
                Gizmos.DrawSphere(snap.transform.position, 0.1f);
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!other.TryGetComponent(out SnapPoint snapPoint)) return;
            if (other.transform.IsDescendantOf(transform)) return;
            
            nearbySnapPoints.Add(snapPoint);
        }

        private void OnTriggerExit(Collider other)
        {
            if (!other.TryGetComponent(out SnapPoint snapPoint)) return;
            if (other.transform.IsDescendantOf(transform)) return;
            
            nearbySnapPoints.Remove(snapPoint);
        }

        public void SetPreviewObject(GameObject newPrefab, bool allowPhysics, bool allowCollision)
        {
            //the system using the cursor takes care of this part 
            // prefabOfPreviewObject = newPrefab;
                
            //TODO: instead cache X amount of previous preview objects for faster swapping.
            Destroy(previewObject);
                
            //instantiate it under this object
            previewObject = Instantiate(newPrefab, transform);
            
            //activate its PlaceableObject component
            var pobj = previewObject.GetOrAddComponent<PlaceableObject>();
            pobj.ActivatePlacementPreview();

            //optionally disable annoying stuff
            if (!allowPhysics)
            {
                //make it not move
                _DisablePhysics(previewObject);
            }

            if (!allowCollision)
            {
                //make it not smack things
                _DisableCollision(previewObject);
            }

            _ourPreviewSnaps = _GetCurrentPreviewSnapPoints();
        }

        private IEnumerable<SnapPoint> _GetCurrentPreviewSnapPoints()
        {
            //thought it was going to be more complicated
            //it may still end up that way tho
            if (previewObject == null) return Enumerable.Empty<SnapPoint>();
            return previewObject.GetComponentsInChildren<SnapPoint>();
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
            gameObject.TryGetComponentIfNull(ref cursorRigidBody);
            gameObject.TryGetComponentIfNull(ref snapDetectionCollider);

            if (cursorRigidBody != null)
            {
                cursorRigidBody.isKinematic = true;
            }

            if (snapDetectionCollider != null)
            {
                snapDetectionCollider.isTrigger = false;
                snapDetectionCollider.excludeLayers = ~GlobalSnappingSettingsAsset.SnappingPointLayers;
            }
        }
    }
}