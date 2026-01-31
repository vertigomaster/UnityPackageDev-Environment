using System.Collections.Generic;

namespace ProjectPortalMaze.Unity.Runtime
{
    /// <summary>
    /// Data about the portals ready to be handed to the render pass. 
    /// </summary>
    public class PortalFrameData
    {
        /// <summary>
        /// A pre-sorted list of the portals, in the intended render order
        /// </summary>
        public List<PortalViewNode> viewsByRecursionDepth;
        
        /// <summary>
        /// Max recursion depth allowed this frame
        /// (though that info may be more helpful elsewhere)
        /// </summary>
        /// <remarks>
        /// we will generally be using portal screen area percentages to determine
        /// stopping points, but it's always important to have a hard limit as a
        /// safeguard against infinite loops or overflows.
        /// </remarks>
        private int maxDepth; 
    }
}