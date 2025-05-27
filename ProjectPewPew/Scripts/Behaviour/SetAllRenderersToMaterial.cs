using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

namespace IDEK.Tools.GameplayEssentials.Samples.PewPew
{
    public class SetAllRenderersToMaterial : MonoBehaviour
    {
#if ODIN_INSPECTOR
        [Sirenix.OdinInspector.Button]
#endif
        public void Activate(Material material)
        {
            ListPool<Renderer>.Get(out List<Renderer> rendererList);
            
            GetComponentsInChildren(rendererList);
            foreach (var renderer in rendererList)
            {
                renderer.material = material;
            }
            
            ListPool<Renderer>.Release(rendererList);
        }
    }
}