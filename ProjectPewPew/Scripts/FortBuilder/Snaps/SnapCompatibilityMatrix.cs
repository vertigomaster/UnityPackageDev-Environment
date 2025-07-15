using System.Collections.Generic;
using UnityEngine;

namespace IDEK.Tools.GameplayEssentials.Snapping
{
    public class SnapTypeCompatibilityEntry
    {
        public SnapType snapType;
        public List<SnapType> compatibleWith = new();
    }

    public class SnapCompatibilityMatrix : ScriptableObject
    {
        public List<SnapTypeCompatibilityEntry> entries = new();

        public void SyncWithAvailableSnapTypes()
        {
            //TODO: how
        }
    }
}