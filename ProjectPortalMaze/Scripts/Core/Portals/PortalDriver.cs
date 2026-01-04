using System;
using IDEK.Tools.Logging;
using IDEK.Tools.ShocktroopExtensions;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;

namespace ProjectPortalMaze.Core
{
    public class PortalDriver : MonoBehaviour
    {
        public enum PortalMode
        {
            Forward,
            Back,
            OurSurfaceToLinkedPortal,
            LinkedPortalToOurSurface,
            OurPortalToLinkedSurface,
            LinkedSurfaceToOurPortal
        }
        
        // private static readonly int SHADERPROP_MainTex = Shader.PropertyToID("_MainTex");
        //Unity 6 URP
        private static readonly int SHADERPROP_MainTex = Shader.PropertyToID("_BaseMap");

        [Required]
        public Camera portalCam;

        [Required]
        public MeshRenderer surfaceMesh;
        
        [Required]
        public PortalDriver linkedPortal;

        [Required]
        public Transform entry;
        [Required]
        public Transform exit;

        private Camera _playerCam;
        private RenderTexture _viewTexture;
        
        [SerializeField]
        private PortalMode debugMode = PortalMode.Forward;

        // public Transform SurfaceTransform => surfaceMesh.transform;
        
        /// <summary>
        /// matrix mult is associative
        /// <br/>TODO: add a static flag; in the portal maze, these don't usually change, could save on calcs maybe 
        /// </summary>
        public Matrix4x4 TeleportMatrix => linkedPortal.exit.localToWorldMatrix * entry.worldToLocalMatrix;
        // public Matrix4x4 TeleportMatrix => linkedPortal.transform.localToWorldMatrix * transform.worldToLocalMatrix;
        public Matrix4x4 TeleportBackMatrix => transform.localToWorldMatrix * linkedPortal.transform.worldToLocalMatrix;

        public Matrix4x4 TeleportMatrixTestDebug => debugMode switch
        {
            //im going to scream wtf 
            
            PortalMode.Forward => linkedPortal.transform.localToWorldMatrix * transform.worldToLocalMatrix,
            PortalMode.Back => transform.localToWorldMatrix * linkedPortal.transform.worldToLocalMatrix,
            //it should be this one, right? with surfaces flipped - so we get behind the portal?
            PortalMode.OurSurfaceToLinkedPortal => linkedPortal.transform.localToWorldMatrix * surfaceMesh.transform.worldToLocalMatrix,
            PortalMode.LinkedPortalToOurSurface => surfaceMesh.transform.localToWorldMatrix * linkedPortal.transform.worldToLocalMatrix,
            PortalMode.OurPortalToLinkedSurface => linkedPortal.surfaceMesh.transform.localToWorldMatrix * transform.worldToLocalMatrix,
            PortalMode.LinkedSurfaceToOurPortal => transform.localToWorldMatrix * linkedPortal.surfaceMesh.transform.worldToLocalMatrix,
            _ => throw new ArgumentOutOfRangeException()
        };

        //we don't auto query for you - ask if you need to know to avoid null refs.
        public bool CanTeleport => Camera.main != null;

        private void OnValidate()
        {
            gameObject.TryGetComponentInChildrenIfNull(ref portalCam);
            gameObject.TryGetComponentInChildrenIfNull(ref surfaceMesh);
            if (linkedPortal == this)
            {
                linkedPortal = null;
                ConsoleLog.LogWarning($"[{name}] A portal cannot be paired to itself! Unpaired.");
            } 
        }

        private void OnEnable()
        {
            _playerCam = Camera.main;
            if(_playerCam == null) ConsoleLog.LogError($"[{name}] Can't find player camera! Teleports won't work!");

            //it signals to the engine that this camera will only be rendered to a RenderTexture
            //this prevents the Engine from rendering it directly to the backbuffer
            portalCam.forceIntoRenderTexture = true; 
        }
        
        // public void OnPreRender()
        public void Update()
        {
            surfaceMesh.enabled = false;
            _CreateViewTexture();
            
            //see our portal through the other
            // Matrix4x4 teleportedBackMatrix = linkedPortal.TeleportMatrix * _playerCam.transform.localToWorldMatrix;
            _UpdateCameraTransform();
            surfaceMesh.enabled = true;
        }

        public void Teleport(Transform thingToTeleport)
        {
            
            
            // Matrix4x4 teleportedMatrix = linkedPortal.surfaceMesh.transform.localToWorldMatrix * transform.worldToLocalMatrix * _playerCam.transform.localToWorldMatrix;
            Matrix4x4 teleportedMatrix = TeleportMatrix * _playerCam.transform.localToWorldMatrix;

            
            //matrix coords are 0-indexed
            //column index 3 (4th column) is the translation component of a 3D transform matrix (homogenous coords)
            //matrix.rotation computes a quaternion out of the basis vectors (the top left 3x3 corner of the 4x4)
            thingToTeleport.SetPositionAndRotation(teleportedMatrix.GetColumn(3), teleportedMatrix.rotation);
        }

        #region Main Stages
        private void _UpdateCameraTransform()
        {
            Teleport(portalCam.transform);
        }

        private void _CreateViewTexture()
        {
            //TODO: instead of screen w/h, get onscreen bounding box of portal, use that to get resolution and to adjust the projection
            
            if (_viewTexture == null || _viewTexture.width != Screen.width || _viewTexture.height != Screen.height)
            {
                if (_viewTexture != null)
                {
                    _viewTexture.Release();
                }

                //TODO: we may want a depth buffer later
                _viewTexture = new RenderTexture(Screen.width, Screen.height, 0, RenderTextureFormat.ARGB32);

                // _viewTexture.width
                portalCam.targetTexture = _viewTexture;
                
                linkedPortal.surfaceMesh.material.SetTexture(SHADERPROP_MainTex, _viewTexture);
                    
            }
        }
        
        #endregion

        #region Getting Properties for adjusted portal camera


        //bounds of this portal in the given camera's screenspace
        public Rect GetOnScreenBoundsOfPortal(Camera gameCam)
        {
            throw new NotImplementedException();
            
            //get portal surface screen bounds
                //may need to get mesh corners, or can we just directly pull the screen bounds from the mesh/object?
            //map to screen with matrix
            //ensure normalized?
            return Rect.zero;
        }

        #endregion

    }
}
