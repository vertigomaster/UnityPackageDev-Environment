using System.Collections.Generic;
using IDEK.Tools.Logging;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ProjectPortalMaze.Unity.Runtime
{
    #if UNITY_EDITOR
    [UnityEditor.InitializeOnLoad]
    public static class PortalSpatialGroupNode_EditorUpdater
    {
        static PortalSpatialGroupNode_EditorUpdater()
        {
            UnityEditor.EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
            UnityEditor.SceneManagement.EditorSceneManager.sceneSaving += OnSceneSaving;
        }

        private static void OnSceneSaving(Scene scene, string path)
        {
            var nodesInScene = GetAllNodesInScene(scene);
        }

        private static List<PortalSpatialGroupNode> GetAllNodesInScene(Scene scene)
        {
            List<PortalSpatialGroupNode> nodesInScene = new();
            //find all nodes objects in the scene and update them
            foreach (var root in scene.GetRootGameObjects())
            {
                nodesInScene.AddRange(root.GetComponentsInChildren<PortalSpatialGroupNode>());
            }

            return nodesInScene;
        }

        private static void OnPlayModeStateChanged(UnityEditor.PlayModeStateChange state)
        {
            if (state == UnityEditor.PlayModeStateChange.ExitingEditMode)
                UpdateAllPortalGroupBounds();
        }


        [ContextMenu("Manually Update Portal Spatial Group Bounds (In Active Scene)")]
        static void UpdateAllPortalGroupBounds()
        {
            //TODO: to enable reliable caching optimizations (if needed): arrange the node list in breadth-first bottom-up order
            UpdateGivenPortalGroupBounds(
                GetAllNodesInScene(
                    SceneManager.GetActiveScene()
            ));
        }

        static void UpdateGivenPortalGroupBounds(ICollection<PortalSpatialGroupNode> nodes)
        {
            ConsoleLog.Log($"Updating All {nodes.Count} Portal Spatial Group Bounds...");

            foreach (var node in nodes)
            {
                //can't guarantee the nodes were given in breadth-first bottom-up order,
                //so can't verify the TotalBounds properties are actually up-to-date at read-time
                node.UpdateCachedTotalBounds(); 
            }

            ConsoleLog.Log($"Done Updating All {nodes.Count} Portal Spatial Group Bounds.");
        }
    }
    #endif


    /// <summary>
    /// represents a hierarchical group of portals - great for chunking and helping speed up portal recursion visibility checks
    /// </summary>
    /// <remarks>
    /// TODO: have the positions in editor auto update to the center of their members' bounds
    /// <para/>
    /// TODO: draw the bounds when obj selected
    /// </remarks>
    class PortalSpatialGroupNode : MonoBehaviour
    {
        //portals can belong to a group directly or to a group nested by another
        //would eventually be nice to have an automatic grouper/clusterer, but not important right now.
        //TODO: some way to test for dupes?
        public PortalDriver[] directContents; 
        public PortalSpatialGroupNode[] nestedGroups;
        
        public bool IsEmpty => directContents.Length == 0 && nestedGroups.Length == 0;
        
        [SerializeField, ReadOnly]
        private bool initialized = false;
        
        /// <summary>
        /// Usually read at runtime by external systems using the node tree like the PortalVisibilitySystem.
        /// </summary>
        [field: SerializeField, ReadOnly]
        public Bounds CachedTotalBounds { get; private set; }

        /// <inheritdoc cref="TryCalcTotalBounds"/>
        /// <returns>
        /// Valid <see cref="Bounds"/> if the node contains elements.
        /// <br/>
        /// An empty node returns null.
        /// <para/>
        /// The default value of the <see cref="Bounds"/> type returns a bounds over the origin, which is not the same thing.
        /// </returns>
        public Bounds? CalcTotalBounds(bool ignoreCachedBounds)
        {
            return TryCalcTotalBounds(ignoreCachedBounds, out Bounds newBounds) ? newBounds : null;
        }

        /// <summary>
        /// Calculates the total bounds of this node based on its direct contents and the bounds of any nested nodes.
        /// </summary>
        /// <remarks>
        /// Calculating a tree of nodes accurately requires calculating the nested ones either immediately or
        /// a breadth-first bottom-up order so that children calc first to update their caches
        /// so that parents read correct values from those caches.
        /// <para/>
        /// First case is for manually retrieving the total bounds of a given node once.
        /// Reliably doing so on demands means IGNORING the cached values, as the children's current bounds
        /// may have been updated since their last cache updates.
        /// <para/>
        /// Second case is for updating all the caches in that order specified above, in which case the caches ARE read.
        /// </remarks>
        /// <param name="ignoreCachedBounds">
        /// Whether to use the cached TotalBounds values of nested groups or to recalculate them on demand.
        /// <para/>
        /// Depending on the context and timing of the calculation, the cached bounds may have been recently invalidated.
        /// </param>
        public bool TryCalcTotalBounds(bool ignoreCachedBounds, out Bounds newBounds)
        {
            Bounds? nullable_newBounds = null;

            if (IsEmpty)
            {
                newBounds = default;
                return false;
            }


            Bounds groupBounds;
            //pull precomputed data from nested elements
            foreach (var group in nestedGroups)
            {
                if (group.IsEmpty) continue;

                if (!ignoreCachedBounds)
                {
                    groupBounds = group.CachedTotalBounds;
                }
                else
                {
                    if (!group.TryCalcTotalBounds(true, out Bounds newGroupBounds)) continue;
                    groupBounds = newGroupBounds;
                }
                
                if (!nullable_newBounds.HasValue)
                {
                    nullable_newBounds = groupBounds;
                }
                else
                {
                    nullable_newBounds.Value.Encapsulate(groupBounds);
                }
            }

            //explicit length check since we may need to directly initialize newBounds to the value at index 0 
            if (directContents.Length > 0) 
            {
                /* The default value of a Bounds struct uses Vector3.zero for max and min.
                 * 
                 * So we start by replacing an "empty" bounds with the first element to
                 * avoid accidentally encapsulating the origin alongside the other bounds.
                 * 
                 * Double-counting element 0 is faster than doing a null check for every iteration.
                 * 
                 * The double-counting on empty bounds avoids needing to slapping a
                 * second condition on the loop iteration logic.
                 */

                nullable_newBounds ??= directContents[0].surfaceMesh.bounds;

                foreach (var portal in directContents)
                {
                    nullable_newBounds.Value.Encapsulate(portal.surfaceMesh.bounds);
                }
            }

            if (nullable_newBounds.HasValue)
            {
                newBounds = nullable_newBounds.Value;
                return true;   
            }

            newBounds = default;
            return false;
        }

        /// <summary>
        /// Updates the total bounds of this node based on its direct contents and the bounds of any nested nodes.
        /// </summary>
        /// <remarks>
        /// This method is executed immediately, and thusly must ignore the cached <see cref="CachedTotalBounds"/> values
        /// of its nested nodes to ensure reliably valid results.
        /// Runs <see cref="TryCalcTotalBounds"/> under the hood.
        /// </remarks>
        [Button("Manually Update Cached Bounds")]
        public void UpdateCachedTotalBounds()
        {
            CachedTotalBounds = TryCalcTotalBounds(ignoreCachedBounds: true, out Bounds newBounds) ? newBounds : default;
            initialized = true;
        }
    }
}