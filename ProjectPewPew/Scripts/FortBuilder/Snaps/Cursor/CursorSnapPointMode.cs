using System.Collections.Generic;
using System.Linq;
using IDEK.Tools.DataStructures;
using UnityEngine;

namespace IDEK.Tools.GameplayEssentials.Snapping.Cursor
{
    public class CursorSnapPointMode : CursorSnapMode
    {
        //helper types
        public record SnapPointPair(SnapPoint from, SnapPoint to);
        
        //private fields
        private CleanableList<SnapPointPair> _currentSnapPairCandidates = new();
        private bool _snapPairCandidatesDirty;
        
        #region API

        /// <inheritdoc />
        public override bool TryExecuteSnap()
        {
            // cursor.ResetOffset();
            transform.localPosition = Vector3.zero;
            
            if (!cursor.CachedSnapsOnActiveObject.Any()) return false;
            if (!AreThereNearbySnapPoints()) return false;

            var snapPair = _GetLowestEnergySnapPair();

            //process final candidate
            if (snapPair == null) return false;

            _SnapThePreview(snapPair);
            return true;
        }
        
        public bool AreThereNearbySnapPoints() => NullCheckSnapPoints() && cursor.NearbySnapPoints.Count > 0;
        #endregion

        #region operations

        private SnapPointPair _GetLowestEnergySnapPair()
        {
            //get an up to date thing to work from.
            _RefreshSnapPairCandidates();

            float bestSqrDistance = float.MaxValue; //cache
            float currentDist; //cache
            SnapPointPair candidateSnapPair = null; //cache

            //the "from" is the snap point on the preview
            //the "to" is the snap point on the othe object
            //we "snap" them together by moving our preview such that one of that preview's snaps (the from) is
            //at the same position and offset rotation as another snap (the to).

            //get the "from -> to" snap operation pair with the smallest delta (requires the minimal shift for the preview object)  
            foreach (var fromToPair in _currentSnapPairCandidates)
            {
                currentDist = Vector3.SqrMagnitude(
                    fromToPair.to.transform.position - fromToPair.from.transform.position);
                
                if (currentDist < bestSqrDistance)
                {
                    bestSqrDistance = currentDist;
                    candidateSnapPair = fromToPair;
                }
            }

            return candidateSnapPair;
        }

        private void _SnapThePreview(SnapPointPair candidate)
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
        }
        #endregion

        #region helpers

        /// <summary>
        /// refreshes the set of snap points that we could actually snap to on this frame.
        /// </summary>
        /// <remarks>done using the previews from <see cref="CachedSnapsOnPreviewedObject"/>.</remarks>
        private void _RefreshSnapPairCandidates()
        {
            _currentSnapPairCandidates.Clear();

            //check our current object (nested) for its own snap points and see which of them snap to the nearby ones

            foreach (SnapPoint mySnap in cursor.CachedSnapsOnActiveObject)
            {
                //for each snap point on our preview,
                //see if it is compatible with each of the nearby ones   

                foreach (var nearbySnap in cursor.NearbySnapPoints)
                {
                    if (mySnap.type.IsCompatibleWith(nearbySnap.type))
                    {
                        //yay compat
                        _currentSnapPairCandidates.Add(new(mySnap, nearbySnap));
                    }

                }
            }
        }
        #endregion
    }
}