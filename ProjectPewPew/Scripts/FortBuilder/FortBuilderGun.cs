using IDEK.Tools.Coroutines.TaskRoutines;
using IDEK.Tools.GameplayEssentials.Conflict.Weapons.Unity;
using IDEK.Tools.GameplayEssentials.Targeting;
using UnityEngine;
using UnityEngine.InputSystem;

namespace IDEK.Tools.GameplayEssentials.Samples.PewPew.FortBuilder
{
    /// <summary>
    /// A means for directing/controlling the FortBuilder
    /// </summary>
    [DefaultExecutionOrder(FortBuilderHelper.BASE_BUILDER_EXEC_TIME)]
    public class FortBuilderGun : HitscanBaseGun
    {
        public FortBuilder fortBuilder;
        public InputActionReference spinClockwiseInput;
        public InputActionReference spinCounterClockwiseInput;
        public InputActionReference goToNextPieceInput;
        public InputActionReference goToPreviousPieceInput;

        public float spinSpeed;
        
        private TargetingInfoCache _nonNullTargetingData;

        private TaskRoutine _ghostLoopRoutine;
        
        
        #region Overrides of BaseGun

        protected override void OnValidate()
        {
            base.OnValidate();
            if (!fortBuilder)
            {
                fortBuilder = FindFirstObjectByType<FortBuilder>();
            }
        }

        protected override void OnEnable()
        {
            base.OnEnable();

            if (!fortBuilder)
            {
                fortBuilder = FindFirstObjectByType<FortBuilder>();
            }
            
            if (fortBuilder)
            {
                if (fortBuilder.placementTransform)
                    _ghostLoopRoutine = TaskRoutine.StartLoop(_UpdateGhost);
            }
        
            if (goToNextPieceInput != null)
                goToNextPieceInput.action.performed += _OnGoNextPieceInput;

            if (goToPreviousPieceInput != null)
                goToPreviousPieceInput.action.performed += _OnGoPrevPieceInput;
            
            if (spinClockwiseInput != null)
                spinClockwiseInput.action.performed += _OnSpinClockwiseInput;

            if (spinCounterClockwiseInput != null)
                spinCounterClockwiseInput.action.performed += _OnSpinCounterClockwiseInput;

        }

        protected override void OnDisable()
        {
            if (goToNextPieceInput != null)
                goToNextPieceInput.action.performed -= _OnGoNextPieceInput;

            if (goToPreviousPieceInput != null)
                goToPreviousPieceInput.action.performed -= _OnGoPrevPieceInput;
        
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
        private void _UpdateGhost()
        {
            if (fortBuilder.placementTransform == null) return;
            
            //update the ghost every frame
            _nonNullTargetingData = GetTargetingInfoCache();

            if (_nonNullTargetingData.HasTarget)
            {
                fortBuilder.PassGhostTransformIntent(
                    _nonNullTargetingData.HitPosition,
                    _nonNullTargetingData.HitSurfaceNormal, 
                    Vector3.one);
            }
            else
            {
                fortBuilder.PassGhostTransformIntent(
                    ExpectedTarget.position,
                    surfaceNormal: Vector3.up,
                    Vector3.one);
            }
        }

        private void _OnGoNextPieceInput(InputAction.CallbackContext obj)
        {
            if (fortBuilder) fortBuilder.GoToNextPiece();
        }

        private void _OnGoPrevPieceInput(InputAction.CallbackContext obj)
        {
            if(fortBuilder) fortBuilder.GoToPreviousPiece();
        }

        private void _OnSpinClockwiseInput(InputAction.CallbackContext obj)
        {
            // if (fortBuilder) fortBuilder.SpinGhost(10f);
            if (fortBuilder) fortBuilder.SpinGhost(true);
        }

        private void _OnSpinCounterClockwiseInput(InputAction.CallbackContext obj)
        {
            // if (fortBuilder) fortBuilder.SpinGhost(-10f);
            if (fortBuilder) fortBuilder.SpinGhost(false);

        }
        
        #endregion
    }
}