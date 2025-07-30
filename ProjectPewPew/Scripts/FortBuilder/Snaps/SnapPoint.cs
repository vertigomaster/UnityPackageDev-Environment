using System.Collections.Generic;
using IDEK.Tools.Logging;
using IDEK.Tools.ShocktroopUtils;
using UnityEngine;
using UnityEngine.Serialization;

namespace IDEK.Tools.GameplayEssentials.Snapping
{
    /// <summary>
    /// reps a snappable point that other snap points can, well, snap to
    /// </summary>
    public class SnapPoint : MonoBehaviour
    {
#if ODIN_INSPECTOR
        [Sirenix.OdinInspector.InlineEditor]
#endif
        public SnapType type;
        
        //we denote what snap type we are

        public SnapPoint CurrentAttachment { get; protected set; }

        public bool IsAttached => CurrentAttachment != null;

        /// <summary>
        /// attaches two snaps together.
        /// </summary>
        /// <remarks>
        /// Attaching A to B (or vice versa) sets B as A's attachment AND sets A as B's attachment, so you don't need to call the reverse.
        /// </remarks>
        /// <param name="attachedTo"></param>
        /// <returns></returns>
        public bool TryAttach(SnapPoint attachedTo)
        {
            if (CurrentAttachment != null) return false; //already attached to someone
            if (attachedTo == null) return false; //nothing to attach
            
            CurrentAttachment = attachedTo;
            CurrentAttachment.CurrentAttachment = this;
            return true;
        }

        public bool TryDetach()
        {
            if (CurrentAttachment == null) return false;
            if (CurrentAttachment.CurrentAttachment != this)
            {
                _ReportMismatch();
                return false;
            }
            
            CurrentAttachment.CurrentAttachment = null; //remove ourselves from them
            CurrentAttachment = null; //remove them
            return true;
        }

        private void _ReportMismatch()
        {
            string ourAttachment = "<NULL>";
            string theirAttachment = "<NULL>";

            if (CurrentAttachment)
            {
                ourAttachment = CurrentAttachment.name;
                if (CurrentAttachment.CurrentAttachment)
                {
                    theirAttachment = CurrentAttachment.CurrentAttachment.name;
                }
            }
            
            ConsoleLog.LogError(
                $"attachment mismatch! {name} is reportedly attached to {ourAttachment}, but {ourAttachment} is reported attached to {theirAttachment}");
        }

        public bool IsCompatibleWith(SnapPoint other) => SnapType.TestCompatibility(type, other.type);
    }
}