using UnityEngine;

namespace ProjectPortalMaze.Unity.Runtime
{
    /// <summary>
    /// The minimal data representing a portal view to the render pass.
    /// </summary>
    public struct PortalViewNode
    { 
        //precalced before the pass
        /// <summary>
        /// TRS matrix for where to render from
        /// </summary>
        public readonly Matrix4x4 viewMatrix;
        /// <summary>
        /// Projection matrix for how to morph the frustum into clip space
        /// </summary>
        public readonly Matrix4x4 projectionMatrix;
        /// <summary>
        /// How many portals you need to look through to see this view
        /// </summary>
        public readonly int recursionDepth;
        
        //portal shape - mesh?
    }
}