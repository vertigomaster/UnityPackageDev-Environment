using System.Collections.Generic;
using IDEK.Tools.DataStructures;
using IDEK.Tools.Logging;
using IDEK.Tools.ShocktroopExtensions;
using Sirenix.OdinInspector;
using UnityEngine;

namespace IDEK.Tools.GameplayEssentials.Snapping
{
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(Collider))]
    public class SnapPointDetectionVolume : MonoBehaviour
    {
        public Rigidbody cursorRigidBody;
        public Collider snapDetectionCollider;
        public GameObject owner; //backreference pointing to whatever "owns" this
        
        // internal CleanableHashSet<>
        internal HashSet<SnapPoint> nearbySnapPoints = new();
        
        [ShowInInspector, ReadOnly]
        private int Debug_SnapPointCount => nearbySnapPoints?.Count ?? -1;

        private void OnValidate() => _EnsureCorrectComponentSettings();
        private void OnEnable()
        {
            _EnsureCorrectComponentSettings();
            if (owner == null) owner = gameObject;
        }

        private void OnTriggerEnter(Collider other)
        {
            ConsoleLog.Log($"fooble - {name} hit {other}");
            if (!other.TryGetComponent(out SnapPoint snapPoint)) return;

            ConsoleLog.Log($"fooble - {name} Found snap point component {snapPoint} on other object {other}");
            if (other.transform.IsDescendantOf(transform)) return; //don't match against our own snap points

            ConsoleLog.Log($"fooble - {other} is not part of {name}'s hierarchy! Safe to add new snapPoint {snapPoint}");
            
            nearbySnapPoints.Add(snapPoint);
        }

        private void OnTriggerExit(Collider other)
        {
            ConsoleLog.Log($"fooble - {name} stopped hitting {other}");

            if (!other.TryGetComponent(out SnapPoint snapPoint)) return;

            ConsoleLog.Log($"fooble - {name} Found snap point component {snapPoint} on other object {other}");
            
            if (other.transform.IsDescendantOf(owner.transform)) return;

            ConsoleLog.Log(
                $"fooble - {other} is not part of {name}'s hierarchy! Safe to remove old snapPoint {snapPoint}");

            nearbySnapPoints.Remove(snapPoint);
        }

        private void _EnsureCorrectComponentSettings()
        {
            gameObject.TryGetComponentIfNull(ref cursorRigidBody);
            gameObject.TryGetComponentIfNull(ref snapDetectionCollider);

            if (cursorRigidBody != null)
            {
                cursorRigidBody.isKinematic = true;
                cursorRigidBody.includeLayers = GlobalSnappingSettingsAsset.SnappingPointLayers;
                cursorRigidBody.excludeLayers = ~GlobalSnappingSettingsAsset.SnappingPointLayers;
            }

            if (snapDetectionCollider != null)
            {
                snapDetectionCollider.isTrigger = false;
                snapDetectionCollider.includeLayers = 0;
                snapDetectionCollider.excludeLayers = 0;
            }
        }

    }
}