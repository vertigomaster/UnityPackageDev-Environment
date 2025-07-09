using System.Collections;
using System.Collections.Generic;
using IDEK.Tools.Coroutines.TaskRoutines;
using IDEK.Tools.GameplayEssentials.Conflict.Weapons.Unity;
using IDEK.Tools.GameplayEssentials.Interaction.Unity;
using IDEK.Tools.GameplayEssentials.Targeting;
using IDEK.Tools.Logging;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

namespace IDEK.Tools.GameplayEssentials.Samples.PewPew.FortBuilder
{
    /// <summary>
    /// A means for directing/controlling the FortBuilder
    /// </summary>
    public class FortBuilderGun : HitscanBaseGun
    {
        public FortBuilder fortBuilder;
        public InputActionReference spinClockwiseInput;
        public InputActionReference spinCounterClockwiseInput;

        private TargetingInfoCache _nonNullTargetingData;

        private TaskRoutine _ghostLoopRoutine;
        
        #region Overrides of BaseGun

        /// <inheritdoc />
        protected override void OnEnable()
        {
            base.OnEnable();
            
            if (fortBuilder && fortBuilder.ghostTransform)
                _ghostLoopRoutine = TaskRoutine.StartLoop(UpdateGhost);
        }

        /// <inheritdoc />
        protected override void OnDisable()
        {
            _ghostLoopRoutine?.Destroy();
            _ghostLoopRoutine = null;
            
            base.OnDisable();
        }

        #endregion

        #region Overrides of BaseTool

        /// <inheritdoc />
        protected override void Activate()
        {
            fortBuilder.PlacePieceAtGhost();
            base.Activate();
        }

        //Consider changing to a routine that runs while there is a ghost
        private void UpdateGhost()
        {
            if (fortBuilder.ghostTransform == null) return;
            
            //update the ghost every frame
            _nonNullTargetingData = GetTargetingInfoCache();

            if (_nonNullTargetingData.HasTarget)
            {
                fortBuilder.UpdateGhostTransform(
                    _nonNullTargetingData.HitPosition,
                    _nonNullTargetingData.HitSurfaceNormal, 
                    Vector3.one);
            }
            else
            {
                fortBuilder.UpdateGhostTransform(
                    ExpectedTarget.position,
                    surfaceNormal: Vector3.up,
                    Vector3.one);
            }
        }

        #endregion
    }
    
    public class FortBuilder : MonoBehaviour
    {
        public static readonly Vector3 ONE = new Vector3(1f, 1f, 1f);

        //we have that aim target thing
        public ShareableAimTrajectory currentTarget;
        
        public List<GameObject> placeablePieces = new();
        private int _selectedPiece = 0;

        public float currentRotationOffset;
        protected InstantiateParameters placementContext;

        //meant to be the object being placed but with a ghostly material effect over it (maybe post-processing)
        public Transform ghostTransform;
        
        //we don't need to track this yet (if ever)
        // public HashSet<GameObject> placedPieces = new();

        public void PlacePieceAtGhost()
        {
            GameObject newPart = Instantiate(placeablePieces[_selectedPiece],
                ghostTransform.position,
                ghostTransform.rotation);

            newPart.transform.localScale = ghostTransform.localScale;
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="position"></param>
        public void PlacePiece(Vector3 position, Vector3 surfaceNormal, Vector3 scale)
        {
            GameObject newPart = Instantiate(placeablePieces[_selectedPiece], 
                position, 
                Quaternion.AngleAxis(currentRotationOffset, surfaceNormal));

            newPart.transform.localScale = scale;
        }

        public void PlacePiece(Vector3 position, Vector3 surfaceNormal, float scale)
        {
            PlacePiece(position, surfaceNormal, Vector3.one * scale);
        }

        public void PlacePiece(Vector3 position, Vector3 surfaceNormal)
        {
            PlacePiece(position, surfaceNormal, Vector3.one);
        }

        public void PlacePiece(Vector3 position)
        {
            PlacePiece(position, Vector3.up, Vector3.one);
        }

        public void UpdateGhostTransform(Vector3 position, Vector3 surfaceNormal, Vector3 scale)
        {
            ghostTransform.position = position;
            ghostTransform.rotation = Quaternion.AngleAxis(currentRotationOffset, surfaceNormal);
            ghostTransform.localScale = scale;
        }

        public void UpdateGhostTransform(Vector3 position, Vector3 surfaceNormal)
        {
            UpdateGhostTransform(position, surfaceNormal, Vector3.one);
        }

        public void SpinGhost(float delta)
        {
            currentRotationOffset += delta;
        }
    }
}