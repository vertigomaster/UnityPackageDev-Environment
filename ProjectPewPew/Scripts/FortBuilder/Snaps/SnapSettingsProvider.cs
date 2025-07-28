//Copyright: IDEK Studios
//Created by: Julian Noel

using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace IDEK.Tools.GameplayEssentials.Snapping.Editor
{
    #if UNITY_EDITOR
    using UnityEditor;
    
    public static class SnapSettingsProvider
    {
        public static string matrixAssetSaveLocation = "Assets/Plugins/IDEK/Snapping/SnapCompatibilityMatrix.asset";
        public static string globalSnapSettingsLocation = "Assets/Plugins/IDEK/Snapping/GlobalSnapSettings.asset";
        
        [SettingsProvider]
        public static SettingsProvider CreateSnapCompatibilitySettings()
        {
            return new("Project/" + SnapHelpers.SNAP_MENU_PATH, SettingsScope.Project) {
                label = "Snap Settings",
                guiHandler = _DrawSettings
            };
        }
        
        private static void _DrawSettings(string searchContext)
        {
            var matrix = _GetOrCreateCompatMatrix();
            
            ////////////////////////////////////////////

            _DrawMainSettings();

            EditorGUILayout.Space();

            _DrawCompatibiltySyncButton(matrix);

            EditorGUILayout.Space();

            _DrawCompatibilityMatrix(matrix);
        }

        private static void _DrawMainSettings()
        {
            //drawing where to put in the default GlobalSnappingSettingsAsset
            var ass = _GetGlobalSettings();
        }

        //TODO: Determine if need
        private static void _DrawCompatibiltySyncButton(SnapCompatibilityMatrix matrix)
        {
            if (!GUILayout.Button("Refresh Compatibility Data From Assets")) return;
            matrix.SyncWithAvailableSnapTypes();
            EditorUtility.SetDirty(matrix);
        }

        //TODO: do later. getting stupid complicated for right now.
        private static void _DrawCompatibilityMatrix(SnapCompatibilityMatrix matrix)
        {
            //title
            EditorGUILayout.LabelField("Compatibility Matrix", EditorStyles.boldLabel);
            
            //description
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Type --- Can Snap To");
            // GUIUtility.RotateAroundPivot(90);
            EditorGUILayout.EndHorizontal();
            
            foreach (var entryRow in matrix.entries)
            {
                //would love to show a message, but that would spam the logs so badly
                if (entryRow.snapType == null) continue;
                
                // EditorGUILayout.
            }
        }

        private static GlobalSnappingSettingsAsset _GetGlobalSettings()
        {
            var ass = GlobalSnappingSettingsAsset.Instance;

            if (ass == null)
            {
                ass = ScriptableObject.CreateInstance<GlobalSnappingSettingsAsset>();
                AssetDatabase.CreateAsset(ass, globalSnapSettingsLocation);
                AssetDatabase.SaveAssets();
            }
            
            return ass;
        }

        /// <summary>
        /// retrieves or generates a new scrob for it
        /// </summary>
        /// <returns></returns>
        private static SnapCompatibilityMatrix _GetOrCreateCompatMatrix()
        {
            //check expected location first
            var ass = AssetDatabase.LoadAssetAtPath<SnapCompatibilityMatrix>(matrixAssetSaveLocation); 
            
            if (ass != null) return ass;
            
            //if not there, look for one elsewhere
            var guids = AssetDatabase.FindAssets("t:SnapCompatibilityMatrix");
            if (guids.Length > 0)
            {
                //grab first one you find
                return AssetDatabase.LoadAssetAtPath<SnapCompatibilityMatrix>(
                    AssetDatabase.GUIDToAssetPath(guids[0]));
            }
            
            //make, serialize, and save new scrob to send
            ass = ScriptableObject.CreateInstance<SnapCompatibilityMatrix>();
            // AssetDatabase.CreateFolder(direc) //TODO: ensure tree exists
            AssetDatabase.CreateAsset(ass, matrixAssetSaveLocation);
            AssetDatabase.SaveAssets();
            return ass;
        }
    }
    
    #endif
}
