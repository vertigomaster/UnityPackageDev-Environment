using System.Collections.Generic;
using IDEK.Tools.Logging;
using IDEK.Tools.ShocktroopUtils.Services;
using PlasticGui.WorkspaceWindow.Topbar;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.Universal;

namespace ProjectPortalMaze.Unity.Runtime
{
    /// <summary>
    /// the render pass describing the actual portal render logic (not the raw rendering and math, but the sequence and process)
    /// </summary>
    class PortalRenderPass : ScriptableRenderPass
    {
        public static class PortalTexDescType
        {
            //scaled based on camera res, I think
            //either that or we use the functor version
            // public static readonly TextureDesc RGBColor_FullRes = new TextureDesc(Vector2.one)
            // {
            //     colorFormat = GraphicsFormat.R8G8B8_SRGB //idk if this is best, but good start
            // };
            // public static readonly TextureDesc RGBColor_3QuarterRes = new TextureDesc(Vector2.one * 0.75f)
            // {
            //     colorFormat = GraphicsFormat.R8G8B8_SRGB //idk if this is best, but good start
            // };
            // public static readonly TextureDesc RGBColor_HalfRes = new TextureDesc(Vector2.one * 0.5f)
            // {
            //     colorFormat = GraphicsFormat.R8G8B8_SRGB //idk if this is best, but good start
            // };
            // public static readonly TextureDesc RGBColor_QuarterRes = new TextureDesc(Vector2.one * 0.25f)
            // {
            //     colorFormat = GraphicsFormat.R8G8B8_SRGB //idk if this is best, but good start
            // };
            
            public enum ResTier { Full, Three_Quarter, Half, Quarter }
            
            //sRGB (gamma) better than UNorm for perceptual color maps and auto convert when sampled or rendered
            public const GraphicsFormat MAIN_COLOR_FORMAT = GraphicsFormat.R8G8B8_SRGB; 
            //depth-stencil combined, as the pipeline wishes
            public const GraphicsFormat DEPTH_STENCIL_FORMAT = GraphicsFormat.D24_UNorm_S8_UInt;
            
            // public const GraphicsFormat PORTAL_INDEX_STENCIL_FORMAT = GraphicsFormat.R8G8B8_SRGB;

            //maybe writing them all out is more performant, but prob negligible. we'll factory it.
            public static TextureDesc BuildDesc(ResTier resTier, GraphicsFormat format)
            {
                return new TextureDesc(
                    Vector2.one * resTier switch {
                        ResTier.Full => 1.0f,
                        ResTier.Three_Quarter => 0.75f,
                        ResTier.Half => 0.5f,
                        ResTier.Quarter => 0.25f,
                        _ => 1.0f
                    }
                ) {
                    colorFormat = format
                };
            }
        }
        
        private class PortalDrawPassData
        {
            //stuff visible in the execution phase
            // internal TextureHandle[] portalTextureHandles;
            internal TextureHandle portalColorBuffer;
            //thinking we mark portals with an abnormal depth stencil value
            //to avoid needing a third handle just for that
            //plus, a visible portal's depth isn't actually knowable until we composite
            //though it could be a much lower bit depth for the stencil; even a conservative upper bound for portals won't use a ton of bits
            internal TextureHandle portalDepthStencilBuffer;
            
            public Matrix4x4 portalViewMatrix; //where the camera is
            public Matrix4x4 portalProjectionMatrix; //how the camera projects its frustum into the unit cube

            //low bit-depth buffer indexing the portal indices (maybe)
            // internal TextureHandle portalRecursiveStencilBuffer;
        }
        
        
        //need to somehow source this; a service? some other thing?
        //can we somehow Get() it from frameData?
        // List<PortalViewNode> portalViewsByRecursionDepth;

        #region Overrides of ScriptableRenderPass

        /// <inheritdoc />
        public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
        {
            var cameraData = frameData.Get<UniversalCameraData>();
            // var cullingData = frameData.Get<UniversalCullingData>(); //not sure how to get this
            
            //TODO: somehow grab portalViewsByDepth list (recursion depth I think, may need to sort by depth-depth too?)
            //per camera, can we attach it to UniversalCameraData? or add extra context items or something?

            //we could eventually inject it into the frameData contextContainer (somehow),
            //assuming that still lets us update it per frame, per camera.
            if (!ServiceLocator.TryResolve<PortalVisibilitySystem>(out var portalSys))
            {
                ConsoleLog.LogError("Cannot record portal render pass without a PortalVisibilitySystem!");
                return;
            }
            
            //TODO: multi-camera support - ask the system for the data for a given camera
            var viewsByRecursionDepth = portalSys.PortalFrameData.viewsByRecursionDepth;
            for (var index = 0; index < viewsByRecursionDepth.Count; index++)
            {
                RecordPortalViewPass(renderGraph, cameraData, viewsByRecursionDepth, in index);
            }

            RecordCompositePass(renderGraph);

            //can use frameData to get UniversalCameraData (getting FOV and matrices)
            //and UniversalResourceData (grab existing depth/color buffers)
            
            //ohhh, we can use one camera and just make different projection matrices instead of multiple cameras
            //we don't actually need multiple real cams for this approach

        }

        #endregion

        private void RecordPortalViewPass(RenderGraph renderGraph, UniversalCameraData cameraData,
            List<PortalViewNode> viewNodes, in int index) //index for stencil stuff
        {
            //should draw opaques, transparents, and write depth and color. 
            
            //composition will handle the stitching, these passes just create the textures we'll be layering together and masking

            //draw what the portal sees to a viewport segment matching the rect we pre-calculated
            using (var builder = renderGraph.AddRasterRenderPass("Render Portal View", out PortalDrawPassData passData))
            {
                //TODO: read rect and camera dim to determine which ResTier
                //TODO: register all textures before entering render pass, give only relevant ones to passdata
                passData.portalColorBuffer = renderGraph.CreateTexture(PortalTexDescType.BuildDesc(
                    PortalTexDescType.ResTier.Full, PortalTexDescType.MAIN_COLOR_FORMAT));

                passData.portalDepthStencilBuffer = renderGraph.CreateTexture(PortalTexDescType.BuildDesc(
                    PortalTexDescType.ResTier.Full, PortalTexDescType.DEPTH_STENCIL_FORMAT));
                
                builder.SetRenderAttachment(passData.portalColorBuffer, 0, AccessFlags.Write);
                builder.SetRenderAttachmentDepth(passData.portalDepthStencilBuffer, AccessFlags.Write);
                builder.SetRenderFunc((PortalDrawPassData data, RasterGraphContext context) => ExecutePortalRenderPass(data, context));
            }
            
            //- store array of textures before entering the pass - one for each portal view we need
            //- when entering the pass, set pass data's buffers being used by the GPU
            //- for any portals visible through this portal, update their respective shaders' texture properties to
            //match their views (which should exist, as the precalc list will draw those portals before their ancestor portals)
            //- update viewport and matrices
            //- draw the portal's view to its buffers, with renderer list matching what it should see (includes those child portals)
            //- repeat for next portal in the list
            
            //main note/gotcha: be sure to use MaterialPropertyBlocks to avoid churning material instances per frame
            
            //this handles re-updating textures for portals that may be viewed multiple times from different parents
            
            //////////////////////////////
            
             
            //we may use a double buffer to handle the draw, do some depth test or something to mask with the portal shape
            //then blit that onto the final buffers
            //and recycle the buffer we're drawing these to, which RG makes pretty easy actually
            
            //easier idea:
            //we may also be able to just set the temp drawn textures as the input textures for our screenspace portal shader
            //and just render that object with a render list (and a forced material perhaps? Contrary to what the AI said, this can definitely be done between passes)
            //or we can have the first render use a diff shader that marks the portal in some way
            //want to avoid a general render capturing an old incorrect portal
            //
            //unsure if that would be faster or slower than the stencil stuff because it leverages existing pipeline behaviour
            //both cases require 2 passes anyway, but unsure if that means copying the buffer.
            //Will just have to check the profiler/analysis stuff.
        }

        static void ExecutePortalRenderPass(PortalDrawPassData data, RasterGraphContext context)
        {
            //TODO: once the new matrices have their side planes tightened to the screenspace rect occupying the portal,
            //  try and update the viewport. Get pixel rect and update viewport to match it.
            //context.cmd.SetViewport(data.portalScreenBounds);
            
            //updates the rendering matrices to match the portals
            //before we try to render the stuff it would see via the renderer list
            //skip occlusion culling for now - the reduced frustum will do a lot for us already
            //can mitigate occlusion with shorter sight lines (and thus shorter far clip panes for some portals)
            RenderingUtils.SetViewAndProjectionMatrices(
                context.cmd, 
                data.portalViewMatrix, 
                data.portalProjectionMatrix, 
                setInverseMatrices: true); //may be useful for depth reconstruction
            
            //TODO: draw renderer list, which will use the new matrices
            
            //that draws them to a small part of the render attachments, hopefully with the respective shaders respecting depth tests and stuff
            //we'll want to then do a blit to mask it within the shader mesh using some sort of stencil trick 
        }
        

        private void RecordCompositePass(RenderGraph renderGraph)
        {
            //somehow combine and stencil(?) together each portal view?
            
            /*
             * need:
             * main camera color buffer
             * all the portal view textures
             * the "portal shapes"?
             *
             * uses:
             * stencil
             * depth test
             * portal depth ordering
             *
             * Each Portal:
             * writes stencil mask of portal shape
             * draws portal texture ONLY WHERE STENCIL PASSES
             * clears stencil region (or increments layer)
             */
            
            /*
             * Better than the RT-per-portal approach;
             * no texture reassignments (supposedly)
             * don't have to clunkily mess with materials
             * no extra camera baggage
             * RenderGraph better handles the texture memory 
             * RenderGraph explicitly establishes ordering in a way the RT approach does not.
             */
        }

        // private void RenderPortal(RenderGraph renderGraph, ContextContainer frameData, PortalDriver portal, int currentDepth)
        // {
        //     //portal should already be tracking what other portals can be seen within its frustum
        //     //a given portal may need to be render multiple times (from different angles) based on where in the cycle
        // }

        //uses old pipeline stuff
        // private void RenderPortal(CommandBuffer cmd, ref ScriptableRenderContext context, ref RenderingData renderingData, 
        //     Camera portalCam, RenderTexture rt, Rect viewportArea)
        // {   
        //     cmd.SetRenderTarget(rt); //Update: apparently we actually don't want to call this? Confused.
        //     
        //     cmd.SetViewport(viewportArea); //target that viewport area
        //     cmd.EnableScissorRect(viewportArea); //scissor out that area
        //     
        //     //do not full clear, that writes to the whole texture, which wastes time
        //     
        //     //note: maybe don't share the RT; scissor goes by rect so overlaps could hurt (maybe)
        //     //overlap means there's another portal in front, but since we can't mask it to a non-rectangle at this stage, that doesn't help us much
        //     
        //     //solution would be to have a pool of multiple RTs and on rect overlap, swap RTs, creating only as needed
        //     //prob easier to manage than the multi-resolution pools
        //     
        //     context.ExecuteCommandBuffer(cmd);
        //     cmd.Clear(); //avoids re-running those commands without releasing the whole buffer
        //
        //
        //     var camRequest = new UniversalRenderPipeline.SingleCameraRequest
        //     {
        //         destination = rt
        //     };
        //     RenderPipeline.SubmitRenderRequest(portalCam, camRequest);
        //     
        //     cmd.DisableScissorRect();
        // }
    }
}