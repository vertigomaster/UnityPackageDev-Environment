using System.Collections.Generic;

namespace ProjectPortalMaze.Unity.Runtime
{
    public class PortalFrameData
    {
        public List<PortalViewNode> viewsByRecursionDepth;
        private int maxDepth; //may not need to explicitly pass that...
    }
}