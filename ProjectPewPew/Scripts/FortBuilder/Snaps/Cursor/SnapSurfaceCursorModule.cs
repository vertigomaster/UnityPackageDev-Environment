namespace IDEK.Tools.GameplayEssentials.Snapping.Cursor
{
    public class SnapSurfaceCursorModule : CursorSnapMode
    {
        //move the stuff from FortBuilder into here so that it doesn't double up with actual mode selection stuff


        #region Overrides of CursorSnapMode

        /// <inheritdoc />
        public override bool TryExecuteSnap()
        {
            //look at cursor position
            //cursor origin is the desired point specified by the external system
            
            return false;
        }

        #endregion
    }
}