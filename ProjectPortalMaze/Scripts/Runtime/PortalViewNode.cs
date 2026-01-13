using UnityEngine;

namespace ProjectPortalMaze.Unity.Runtime
{
    public struct PortalViewNode
    { 
        //precalced before the pass
        public readonly Matrix4x4 viewMatrix;
        public readonly Matrix4x4 projectionMatrix;
        public readonly int recursionDepth;
        
        //portal shape - mesh?
    }
}