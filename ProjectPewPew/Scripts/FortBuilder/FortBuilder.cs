using System.Collections.Generic;
using IDEK.Tools.GameplayEssentials.Targeting;
using UnityEngine;

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
        public float spinSpeed = 10f;
        
        //may need later?
        // protected InstantiateParameters placementContext;

        //meant to be the object being placed but with a ghostly material effect over it (maybe post-processing)
        public Transform ghostTransform;
        
        //we don't need to track this yet (if ever)
        // public HashSet<GameObject> placedPieces = new();

        protected virtual void OnEnable()
        {
            if (perspectiveToBuildFrom == null) 
                perspectiveToBuildFrom = Camera.main != null ? Camera.main.transform : transform;
        }

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

        public void UpdateGhostTransform(Vector3 position, Vector3 surfaceNormal, Vector3 scale)
        {
            ghostTransform.position = position;
            ghostTransform.rotation = CalcPlacementRotation(position, surfaceNormal);
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

        public void SpinGhost(bool clockwise)
        {
            currentRotationOffset += spinSpeed * Time.deltaTime * (clockwise ? 1 : -1);
        }

        public Quaternion CalcPlacementRotation(Vector3 position, Vector3 surfaceNormal)
        {
            Vector3 look = Vector3.ProjectOnPlane(transform.position - position, surfaceNormal);
            
            return Quaternion.LookRotation(look, surfaceNormal) *
                Quaternion.AngleAxis(currentRotationOffset, surfaceNormal);
        }

        public void GoToNextPiece() => _selectedPiece = (_selectedPiece + 1) % placeablePieces.Count;
        public void GoToPreviousPiece() => _selectedPiece = (_selectedPiece - 1) % placeablePieces.Count;   
    }
}