using System;
using System.Collections.Generic;
using IDEK.Tools.GameplayEssentials.Snapping;
using IDEK.Tools.GameplayEssentials.Targeting;
using IDEK.Tools.Logging;
using UnityEngine;
using UnityEngine.Serialization;


namespace IDEK.Tools.GameplayEssentials.Samples.PewPew.FortBuilder
{
    //ensuring the OnEnable() for this always occurs after other build components if both activated in same frame
    [DefaultExecutionOrder(FortBuilderHelper.BASE_BUILDER_EXEC_TIME + 1)]
    public class FortBuilder : MonoBehaviour
    {
        public Transform perspectiveToBuildFrom;
        
        //we have that aim target thing
        // public ShareableAimTrajectory currentTarget;
        
        public List<GameObject> placeablePieces = new();
        private int _selectedPiece = 0; //temp

        public float currentRotationOffset;
        public float spinSpeed = 250f;
        
        //may need later?
        // protected InstantiateParameters placementContext;

        //meant to be the object being placed but with a ghostly material effect over it (maybe post-processing)
        // [FormerlySerializedAs("ghostTransform")]
        public Transform FinalPlacementTransform => placementCursor.transform;

        [FormerlySerializedAs("cursor")]
        [Tooltip("Placement Cursor class to ultimately place parts at. " +
            "This allows other systems (like snap systems) to further direct " +
            "the actual placement location beyond the intent communicated " +
            "to/from the fort builder.")]
#if ODIN_INSPECTOR
        [Sirenix.OdinInspector.Required]
#endif
        public PlacementCursor placementCursor;
        
        [Tooltip("The transform the Fort Builder will manipulate to signal " +
            "build location intent.")]
        public Transform builderPlacementIntentTransform;
        
        //we don't need to track this yet (if ever)
        // public HashSet<GameObject> placedPieces = new();

        private void OnValidate()
        {
            if (!placementCursor)
                placementCursor = GetComponentInChildren<PlacementCursor>();
        }

        protected virtual void OnEnable()
        {
            if (perspectiveToBuildFrom == null) 
                perspectiveToBuildFrom = Camera.main != null ? Camera.main.transform : transform;
        }

        private void Start()
        {
            _RefreshPreviewObject();
        }

        public void PlacePieceAtGhost()
        {
            GameObject newPart = Instantiate(placeablePieces[_selectedPiece],
                FinalPlacementTransform.position,
                FinalPlacementTransform.rotation);

            newPart.transform.localScale = FinalPlacementTransform.localScale;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="position"></param>
        public void PlacePiece(Vector3 position, Vector3 surfaceNormal, Vector3 scale)
        {
            Quaternion rot = CalcPlacementRotation(position, surfaceNormal);
            GameObject newPart = Instantiate(placeablePieces[_selectedPiece], position, rot);
            
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

        //TODO: we may need to move the surface normal stuff to a new snapping mode and not apply rotations
        
        /// <summary>
        /// Lets outside systems pass in where they want the transform to be
        /// Allows the FortBuilder to have the final say and process that intent correctly.
        /// </summary>
        /// <param name="position"></param>
        /// <param name="surfaceNormal"></param>
        /// <param name="scale"></param>
        public void RelayGhostTransformIntent(Vector3 position, Vector3 surfaceNormal, Vector3 scale)
        {
            builderPlacementIntentTransform.position = position;
            builderPlacementIntentTransform.rotation = CalcPlacementRotation(position, surfaceNormal);
            builderPlacementIntentTransform.localScale = scale;
            
            // cursor.InputPosition(position);
            // cursor.InputRotation(CalcPlacementRotation(position, surfaceNormal));
            // cursor.InputScale(scale);
        }

        /// <summary>
        /// <inheritdoc cref="RelayGhostTransformIntent(UnityEngine.Vector3,UnityEngine.Vector3,UnityEngine.Vector3)"/>
        /// </summary>
        /// <param name="position"></param>
        /// <param name="surfaceNormal"></param>
        public void RelayGhostTransformIntent(Vector3 position, Vector3 surfaceNormal)
        {
            RelayGhostTransformIntent(position, surfaceNormal, Vector3.one);
        }

        // /// <summary>
        // /// Lets you manually specify an number of degrees
        // /// </summary>
        // /// <param name="delta"></param>
        // public void SpinGhostClockwiseDegress(float delta)
        // {
        //     currentRotationOffset += delta;
        // }

        public void SpinGhost(bool clockwise)
        {
            currentRotationOffset += spinSpeed * Time.deltaTime * (clockwise ? 1 : -1);
        }

        /// <summary>
        /// Calculates the Quaternion that the FortBuilder would interpret
        /// the given position and surface normal as.
        /// </summary>
        /// <param name="position"></param>
        /// <param name="surfaceNormal"></param>
        /// <returns></returns>
        public Quaternion CalcPlacementRotation(Vector3 position, Vector3 surfaceNormal)
        {
            Vector3 look = Vector3.ProjectOnPlane(transform.position - position, surfaceNormal);
            
            return Quaternion.AngleAxis(currentRotationOffset, surfaceNormal) * Quaternion.LookRotation(look, surfaceNormal);
        }

        public void GoToNextPiece()
        {
            _selectedPiece = (_selectedPiece + 1) % placeablePieces.Count;
            ConsoleLog.Log($"fooble - new piece index = {_selectedPiece}");
            _RefreshPreviewObject();
        }

        public void GoToPreviousPiece()
        {
            // _selectedPiece = (_selectedPiece - 1);
            // if (_selectedPiece < 0) _selectedPiece = placeablePieces.Count - 1;
            
            //mod is handling negative numbers differently from my calculator
            //sure we can find a more elegant solution later
            _selectedPiece = (_selectedPiece - 1) % placeablePieces.Count;
            //this brings it back up into the next range set. -1 + 5 = 4
            //it also handles any negative value.
            if (_selectedPiece < 0) _selectedPiece += placeablePieces.Count; 
            
            ConsoleLog.Log($"fooble - new piece index = {_selectedPiece}");
            _RefreshPreviewObject();
        }

        /// <summary>
        /// Triggers the preview object to refresh to what current settings/config indicate.
        /// </summary>
        private void _RefreshPreviewObject()
        {
            if (placeablePieces == null || _selectedPiece >= placeablePieces.Count) return;
            
            placementCursor.SetPreviewObjectTo(placeablePieces[_selectedPiece], 
                allowPhysics: false, 
                allowCollision: false);
        }
    }
}