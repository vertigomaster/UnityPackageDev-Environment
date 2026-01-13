using IDEK.Tools.ShocktroopExtensions;
using IDEK.Tools.ShocktroopUtils;
using UnityEngine;

namespace ProjectPortalMaze.Unity.Runtime
{
    [ExecuteAlways]
    [RequireComponent(typeof(Camera))]
    public class ObliqueNearClipTester : MonoBehaviour
    {
        public Camera targetCamera;
        public Transform planeTransform;

        private void OnValidate()
        {
            gameObject.TryGetComponentIfNull(ref targetCamera);
        }

        private void OnDisable()
        {
            targetCamera.ResetProjectionMatrix();
        }

        private void LateUpdate()
        {
            targetCamera.ResetProjectionMatrix(); //make sure we don't compound the transformations
            Vector3 planeWorldPosition = planeTransform.position;
            Vector3 planeWorldNormal = planeTransform.forward;
            targetCamera.projectionMatrix = targetCamera.CalcObliqueNearPlaneProjectionMatrix(
                planeWorldPosition, planeWorldNormal);
        }
    }
}