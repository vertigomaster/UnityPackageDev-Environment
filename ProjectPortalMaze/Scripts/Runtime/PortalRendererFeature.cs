using UnityEngine.Rendering.Universal;

namespace ProjectPortalMaze.Unity.Runtime
{
    /// <summary>
    /// A custom feature presented/given to the pipeline that pushes and manages the pass(es) for portal rendering
    /// </summary>
    public class PortalRendererFeature : ScriptableRendererFeature
    {
        private PortalRenderPass _portalPass;
        
        #region Overrides of ScriptableRendererFeature

        /// <inheritdoc />
        public override void Create()
        {
            //URP schedules passes, we tell it where (when) we want ours to fit into the main existing pipeline
            //either after opaques or before transparents, not certain in our case what makes the most sense for us
            //can put in custom numbers too; goes into integers in the end
            _portalPass = new PortalRenderPass
            {
                renderPassEvent = RenderPassEvent.AfterRenderingOpaques
            };
        }

        /// <inheritdoc />
        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            renderer.EnqueuePass(_portalPass);
        }

        #endregion
    }
}