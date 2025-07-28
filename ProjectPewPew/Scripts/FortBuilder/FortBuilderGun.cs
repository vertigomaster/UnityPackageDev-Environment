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
        
        
        #region Overrides of BaseGun (Lifecycle)

        protected override void OnValidate()
        {
            base.OnValidate();
            _EnsureRequiredRefs();
        }

        protected override void OnEnable()
        {
            base.OnEnable();

            _EnsureRequiredRefs();
            
            //trigger placement ghost update loop if there is a ghost transform to update
            if (fortBuilder)
            {
                if (fortBuilder.FinalPlacementTransform)
                    _ghostLoopRoutine = TaskRoutine.StartLoop(_UpdateGhost);
            }
        
            _EnableInputs();
        }

        protected override void OnDisable()
        {
            _DisableInputs();

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

        #endregion


        //Consider changing to a routine that runs while there is a ghost

        private void _UpdateGhost()
        {
            //future optim - this check MAY be redundant, but safer to keep for now.
            if (fortBuilder.FinalPlacementTransform == null) return;
            
            //update the ghost
            _nonNullTargetingData = GetTargetingInfoCache();

            if (_nonNullTargetingData.HasTarget)
            {
                //has valid target, use that data to position the ghost 
                //on the surface of the target
                fortBuilder.RelayGhostTransformIntent(
                    _nonNullTargetingData.HitPosition,
                    _nonNullTargetingData.HitSurfaceNormal, 
                    Vector3.one);
            }
            else
            {
                //no target, fallback on our hitscan "expected target" point.
                fortBuilder.RelayGhostTransformIntent(
                    ExpectedTarget.position,
                    surfaceNormal: Vector3.up,
                    Vector3.one);
            }
        }

        #region Toggle Inputs

        private void _EnableInputs()
        {
            if (goToNextPieceInput != null)
                goToNextPieceInput.action.performed += _OnGoNextPieceInput;

            if (goToPreviousPieceInput != null)
                goToPreviousPieceInput.action.performed += _OnGoPrevPieceInput;
            
            if (spinClockwiseInput != null)
                spinClockwiseInput.action.performed += _OnSpinClockwiseInput;

            if (spinCounterClockwiseInput != null)
                spinCounterClockwiseInput.action.performed += _OnSpinCounterClockwiseInput;
        }

        private void _DisableInputs()
        {
            if (goToNextPieceInput != null)
                goToNextPieceInput.action.performed -= _OnGoNextPieceInput;

            if (goToPreviousPieceInput != null)
                goToPreviousPieceInput.action.performed -= _OnGoPrevPieceInput;

            if (spinClockwiseInput != null)
                spinClockwiseInput.action.performed -= _OnSpinClockwiseInput;

            if (spinCounterClockwiseInput != null)
                spinCounterClockwiseInput.action.performed -= _OnSpinCounterClockwiseInput;
        }

        #endregion

        private void _EnsureRequiredRefs()
        {
            if (!fortBuilder)
            {
                fortBuilder = FindFirstObjectByType<FortBuilder>();
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
    }
}