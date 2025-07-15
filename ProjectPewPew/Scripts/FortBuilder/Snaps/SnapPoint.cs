using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace IDEK.Tools.GameplayEssentials.Snapping
{
    /// <summary>
    /// reps a snappable point that other snap points can, well, snap to
    /// </summary>
    public class SnapPoint : MonoBehaviour
    {
        public SnapType type;
        
        //we denote what snap type we are

        public bool IsCompatibleWith(SnapPoint other) => SnapType.TestCompatibility(type, other.type);
    }
}