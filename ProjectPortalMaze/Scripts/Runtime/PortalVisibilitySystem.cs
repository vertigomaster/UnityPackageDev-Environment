using System.Collections.Generic;
using IDEK.Tools.Logging;
using IDEK.Tools.ShocktroopUtils.Services;
using UnityEngine;

namespace ProjectPortalMaze.Unity.Runtime
{
    /// <summary>
    /// runs before rendering to precalc <c>portalViewsByRecursionDepth</c>
    /// and other data helpful for this frame's render passes. 
    /// </summary>
    public class PortalVisibilitySystem : MonoBehaviour
    {
        [Tooltip("If a recursion path reaches this depth, it is terminated.")]
        public int maxDepth;
        [Tooltip("If a portal's bounds takes up a smaller percentage than this, its recursion path is terminated.")]
        public float minimumScreenAreaPercent = 0.04f;
        
        public PortalFrameData PortalFrameData { get; private set; }

        private List<PortalDriver> _portalsByIndex;
        private HashSet<PortalDriver> _portalSet;
        
        private void Awake()
        {
            if (!ServiceLocator.TryRegister<PortalVisibilitySystem>(this)) Destroy(gameObject);
        }

        private void Update()
        {
            //update the frame data and build a list of the portal render order, starting with first order portals
            //we want to render them, blit/stencil them, and keep going down to draw the nested parts
            //starting out and going in lets us track the evolving portal shapes in the smae stencil buffer
            //each portal render will reveal its own portals to the stencil buffer
            
            //though lighting calcs make be easier if we go the other way, starting deep and going up,
            //using the rendered portal to inform lighting
            //or we just leave that to a separate pass entirely?
            
            //TODO: for each portal, find out what other portals are in its view frustum (CanSee())
            
            //statics can be grouped manually in the editor, and dynamics have known boundary ranges
            //honestly could prob do k-means clustering or something, or just design friendly groups
            //that works well with open-world cases where we need "chunks"
            //for each group depth, the total bounds are calculated and used in these checks
            
        }
        
        //gravitated away from this in favor of manually determined portal spatial groups.
        //Since the levels are designed and not generated, we can more easily define manual
        //groups in the editor and manipulate them as makes sense for the level.
        //These can still be freely calculated in the editor (for static portals) OR updated
        //at runtime for dynamic portals.
        #region Binary Spatial Partitioning (with portal AABBs)

        void BuildBSPTree()
        {
            
        }
        
        #endregion

        #region Portal Register Management
        private void ClearAllPortals()
        {
            _portalsByIndex.Clear();
            _portalSet.Clear();
        }

        public void RegisterPortal(PortalDriver portal)
        {
            if(_portalSet.Add(portal))
            {
                _portalsByIndex.Add(portal);
            }
            else
            {
                ConsoleLog.LogError($"Cannot register portal \"{portal.name}\" - it's already registered.");
            }
        }

        public void UnregisterPortal(PortalDriver portal)
        {
            if(_portalSet.Remove(portal))
            {
                _portalsByIndex.Remove(portal);
            }
            else
            {
                ConsoleLog.LogError($"Cannot unregister portal \"{portal.name}\" - it wasn't registered.");
            }
        }
        #endregion
    }
}