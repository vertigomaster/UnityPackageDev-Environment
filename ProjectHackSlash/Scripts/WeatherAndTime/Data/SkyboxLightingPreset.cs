using IDEK.Tools.Logging;
using UnityEditor;
using UnityEngine;

namespace IDEK.Tools.GameplayEssentials.Samples.HackSlash
{
    [CreateAssetMenu(menuName = "IDEK/Lighting/Skybox Lighting Preset", fileName = "SkyboxLightingPreset", order = 0)]
    public class SkyboxLightingPreset : ScriptableObject
    {
        #region constants
        private const string APPLY_TO_SCENE_MSG = "Are you sure you want to apply this preset to the scene? " +
            "Any unsaved changes to the Skybox material ASSET (not material instance!) or the Lighting panel's " +
            "setting will be overwritten.\n" +
            "Undo may not be 100% sufficient to revert this operation. Make sure you have a backup " +
            "in source control before potentially applying any unintentional changes.";
        #endregion
        
        #region static fields
        public static readonly int SHADER_SUNSIZE = Shader.PropertyToID("_SunSize");
        //not used on "simple" skyboxes
        public static readonly int SHADER_SUNSIZECONVERG = Shader.PropertyToID("_SunSizeConvergence");
        public static readonly int SHADER_ATMOSTHICK = Shader.PropertyToID("_AtmosphereThickness");
        public static readonly int SHADER_SKYTINT = Shader.PropertyToID("_SkyTint");
        public static readonly int SHADER_GROUNDCOLOR = Shader.PropertyToID("_GroundColor");
        public static readonly int SHADER_EXPOSURE = Shader.PropertyToID("_Exposure");
        #endregion
        
        #region instance fields
        public float sunSize = 1f;
        public float sunSizeConvergence = 1f;
        public float atmosphereThickness = 1f;
        public Color skyTint = Color.white;
        public Color groundColor = Color.black;
        public float exposure = 1f;

        public Color realtimeShadowColor = Color.black;
        public float environmentIntensityMult = 1f;
        public float reflectionIntensityMult = 1f;
        
        public Color fogColor = Color.black;
        public float fogDensity = 1f;
        #endregion

        public void Reset()
        {
            sunSize = 0;
            sunSizeConvergence = 0;
            atmosphereThickness = 0;
            skyTint = default;
            groundColor = default;
            exposure = 0;

            realtimeShadowColor = default;
            environmentIntensityMult = 0;
            reflectionIntensityMult = 0;

            fogColor = skyTint;
            fogDensity = 1f;
        }

        public void AddWithWeight(SkyboxLightingPreset otherPreset, float weight)
        {
            ConsoleLog.Log($"fooble - weight {weight}");
            
            sunSize += otherPreset.sunSize * weight;
            sunSizeConvergence += otherPreset.sunSizeConvergence * weight;
            atmosphereThickness += otherPreset.atmosphereThickness * weight;
            skyTint += otherPreset.skyTint * weight;
            groundColor += otherPreset.groundColor * weight;
            exposure += otherPreset.exposure * weight;
            
            realtimeShadowColor += otherPreset.realtimeShadowColor * weight;
            environmentIntensityMult += otherPreset.environmentIntensityMult * weight;
            reflectionIntensityMult += otherPreset.reflectionIntensityMult * weight;
            
            fogColor += otherPreset.fogColor * weight;
            fogDensity += otherPreset.fogDensity * weight;
        }

        public void ApplyToScene() => ApplyToScene(RenderSettings.skybox);

        public void ApplyToScene(Material skyboxMaterial)
        {
            skyboxMaterial.SetFloat(SHADER_SUNSIZE, sunSize);
            skyboxMaterial.SetFloat(SHADER_SUNSIZECONVERG, sunSizeConvergence);
            skyboxMaterial.SetFloat(SHADER_ATMOSTHICK, atmosphereThickness);
            skyboxMaterial.SetColor(SHADER_SKYTINT, skyTint);
            skyboxMaterial.SetColor(SHADER_GROUNDCOLOR, groundColor);
            skyboxMaterial.SetFloat(SHADER_EXPOSURE, exposure);
            
            //TODO set other render settings

            RenderSettings.subtractiveShadowColor = realtimeShadowColor; //i think
            RenderSettings.ambientIntensity = environmentIntensityMult; //i think
            RenderSettings.reflectionIntensity = reflectionIntensityMult;

            RenderSettings.fogColor = fogColor;
            RenderSettings.fogDensity = fogDensity;
        }
        
#if UNITY_EDITOR
        /// <summary>
        /// Odin-only button that applies the current lighting preset to the scene,
        /// even if it is not being directed by a <see cref="SceneLightingController"/>.
        /// </summary>
#if ODIN_INSPECTOR
        [Sirenix.OdinInspector.Button]
#endif
        private void ODIN_BUTTON_ApplyToScene()
        {
            //if in editor, display edit-time warning about potentially losing changes
            if (!Application.isPlaying &&
                !EditorUtility.DisplayDialog("Confirm Overwrite", APPLY_TO_SCENE_MSG, "Apply"))
            {
                return;
            }

            ApplyToScene();
        }
#endif


        //TODO: If it becomes necessary:
        //  maybe separate it from the Scrob so that we can pass the data around w/o it when needed
        // public class SkyboxLightingData
        // {
        //     
        // }
    }
    
}