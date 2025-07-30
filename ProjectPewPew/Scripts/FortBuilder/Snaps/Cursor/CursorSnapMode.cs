using System.Collections.Generic;
using IDEK.Tools.ShocktroopExtensions;
using IDEK.Tools.ShocktroopUtils.Attributes;
using UnityEngine;

namespace IDEK.Tools.GameplayEssentials.Snapping.Cursor
{
    //TODO: move each mode into a module to stay ahead of the spaghetti
    //and make this less frustrating to work with.
    public abstract class CursorSnapMode : MonoBehaviour
    {
        protected internal PlacementCursor cursor;

        private void OnValidate() => _EnsureCorrectComponentSettings();
        private void OnEnable() => _EnsureCorrectComponentSettings();

        public abstract bool TryExecuteSnap();
        public virtual void OnActiveObjectPlaced() { }

        protected bool NullCheckSnapPoints() => cursor.NullCheckSnapPoints();

        private void _EnsureCorrectComponentSettings()
        {
            gameObject.TryGetComponentIfNull(ref cursor);
        }
    }
}
