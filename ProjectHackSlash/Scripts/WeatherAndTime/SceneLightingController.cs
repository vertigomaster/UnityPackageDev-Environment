using System;
using System.Buffers;
using System.Collections.Generic;
using IDEK.Tools.GameplayEssentials.Updating;
using IDEK.Tools.Logging;
using IDEK.Tools.ShocktroopExtensions;
using UnityEngine;

namespace IDEK.Tools.GameplayEssentials.Samples.HackSlash
{
    [System.Serializable]
    public class DirectionalLightData
    {
        public Vector3 measuredLightAxis = Vector3.forward;
        public Light lightComponent;

        [Tooltip("Generally Soft or Hard. None makes it never enable and wastes cycles.")]
        public LightShadows preferredShadowMode = LightShadows.Soft;

#if ODIN_INSPECTOR
        [Sirenix.OdinInspector.InlineEditor]
#endif          
        public SkyboxLightingPreset lighting;
        
    }
    
    /// <summary>
    /// Controls scene lighting, 
    /// </summary>
    /// <remarks>
    /// Implemented:
    /// - Determining which of the registered directional lights have permission to cast shadows.
    /// - Weighted Lerping of multiple SkyboxLightingPreset assets to smoothly change between them
    ///   - Currently implicitly does so for every viable registered SkyboxLightingPreset
    /// 
    /// TODO:
    /// - Verify fog lerping is working     
    /// - Auto detect/refresh of <see cref="allLights"/> to pick up on coming and going data
    /// - Region sourced lighting events?
    /// - Event based lighting changes (maybe triggered by regions?)
    ///   - if necessary, do weighting based on proximity to such volumes?
    /// </remarks>
    [ExecuteAlways]
    public class SceneLightingController : TickBehaviour
    {
        [Range(0f,1f)]
        public float cycleProgress = 0f;
        public Material skyboxMaterial;

        public AnimationCurve SkyHeightToWeightCurve = AnimationCurve.EaseInOut(-1f, -0f, 1f, 1f);
        
        [Tooltip("perpendicular to the horizon")]
        public Vector3 noonVector = Vector3.up;

        public List<DirectionalLightData> allLights;

        private SkyboxLightingPreset _currentCachedLightingTotal;

        private ArrayPool<float> rescaledWeights;
        
        //if dot product less than this, the light is considered over the horizon and should have shadow casting disabled. 

        //only one Directional Light can cast shadows at a time, so we need to change shadow settings at runtime

        #region Overrides of TickBehaviour

        private void OnValidate()
        {
            noonVector = noonVector.normalized;

            foreach (DirectionalLightData lightData in allLights)
            {
                lightData.measuredLightAxis = lightData.measuredLightAxis.normalized;
            }

            if (skyboxMaterial == null)
            {
                skyboxMaterial = RenderSettings.skybox;
            }

            if (skyboxMaterial == null)
            {
                ConsoleLog.LogError("Null skybox!");
            }
        }

        /// <inheritdoc />
        protected override void OnEnable()
        {
            _currentCachedLightingTotal = ScriptableObject.CreateInstance<SkyboxLightingPreset>();

            if (!Application.isPlaying) return;
            base.OnEnable();
        }

        /// <inheritdoc />
        protected override void OnDisable()
        {

            if (!Application.isPlaying)
            {
                DestroyImmediate(_currentCachedLightingTotal);
                return;
            }
            else
            {
                Destroy(_currentCachedLightingTotal);
            }
            base.OnDisable();
        }

        // protected override void Awake()
        // {
        //     base.Awake();
        // }
        //
        // private void OnDestroy()
        // {
        // }

#if UNITY_EDITOR
        private void Update()
        {
            //editor only tick
            if (Application.isPlaying || !Application.isEditor) return;
            Tick(Time.deltaTime);
        }
#endif

        /// <inheritdoc />
        protected override void Tick(float deltaTickTime)
        {
            if (allLights.Count <= 0) return;
            
            DirectionalLightData shadowCandidate = null;
            float maxSkyHeight = float.NegativeInfinity;
            float curHeight = 0f;
            
            foreach (DirectionalLightData lightData in allLights)
            {
                lightData.lightComponent.shadows = LightShadows.None;
                curHeight = GetHeightInSky(lightData);
                if (curHeight > maxSkyHeight)
                {
                    shadowCandidate = lightData;
                    maxSkyHeight = curHeight;
                }
            }
            // lightData.lightComponent.shadows = lightData.HeightInSky >= lightData.horizonThreshold ? 
            //         lightData.preferredShadowMode : LightShadows.None;
            shadowCandidate.lightComponent.shadows = shadowCandidate.preferredShadowMode;
            RenderSettings.sun = shadowCandidate.lightComponent;

            TryApplyWeightedSettingsLerpToSkybox();
        }

        ///<summary>
        /// Weighted sum using our curve SkyHeightToWeightCurve to remap skyheights
        /// to influence weights to tune the day/night cycle
        ///</summary>
        private bool TryApplyWeightedSettingsLerpToSkybox()
        {
            _currentCachedLightingTotal.Reset();
            bool atLeastOneSucceeded = false;
            
            
            for (var i = 0; i < allLights.Count; i++)
            {
                var lightData = allLights[i];
                if (!lightData.lighting) continue;

                _currentCachedLightingTotal.AddWithWeight(lightData.lighting, 
                    SkyHeightToWeightCurve.Evaluate(GetHeightInSky(lightData)));
                atLeastOneSucceeded = true;
            }

            //short out - if no lights have any data to process
            if (!atLeastOneSucceeded) return false;
            
            //Apply
            _currentCachedLightingTotal.ApplyToScene(skyboxMaterial);
            
            return true;
        }
        
        /// <summary>
        /// Technically the dot product between the noon vector and the light's measured direction vector.
        /// </summary>
        /// <remarks>
        /// Assuming a sun: Equals 1 at "high noon", sqrt(2) at mid afternoon/morning, 0 at dawn/dusk, -1 at midnight
        /// </remarks>
        public float GetHeightInSky(DirectionalLightData directionalLightData) =>
            Vector3.Dot(noonVector, directionalLightData.lightComponent.transform.TransformDirection(directionalLightData.measuredLightAxis));

        #endregion
    }
}