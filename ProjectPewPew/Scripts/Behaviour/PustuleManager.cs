using System.Collections.Generic;
using System.Linq;
using IDEK.Tools.DataStructures.Probability;
using IDEK.Tools.ShocktroopExtensions;
using IDEK.Tools.ShocktroopUtils.Math.Probabilities;
using IDEK.Tools.ShocktroopUtils.Math.Probabilities.Unity;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;

namespace IDEK.Tools.GameplayEssentials.Samples.PewPew
{
    public class PustuleManager : MonoBehaviour
    {
        public Vector2 scaleRange = new Vector2(0.8f, 1.25f);

        /// <summary>
        /// noramlized on time and value. Given the percentage of pustules spawned so far, 
        /// it returns the probability of spawning another pustule.
        /// If a roll fails at any point, pustule spawning stops for this instance.
        /// </summary>
        /// <remarks>
        /// This is used to determine the probability of a pustule spawning.
        /// The idea being that as we spawn more pustules, the probability of spawning another one changes in accordance with the curve.
        /// </remarks>
        public ProbabilityCurve spawnCountProbability;

        public List<Transform> pustules;

        private void OnValidate()
        {
            if (pustules == null) pustules = new List<Transform>();
            if (pustules.Count != 0) return;
            pustules = gameObject.GetComponentsInDirectChildren<Transform>(
                includeSelf: false, includeInactive: true).ToList();
            
        }

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            RollPustuleState();
        }

#if ODIN_INSPECTOR
        [Sirenix.OdinInspector.Button("Roll All")]
#endif
        private void RollPustuleState()
        {
            RollScaleFactor();
            RollEnabledToggles();
        }
#if ODIN_INSPECTOR
        [Sirenix.OdinInspector.Button, HorizontalGroup("Sub Rolls")]
#endif
        private void RollScaleFactor()
        {
            foreach (Transform p in pustules)
            {
                p.localScale = p.localScale * scaleRange.GetRandomInRange();
            }
        }

#if ODIN_INSPECTOR
        [Sirenix.OdinInspector.Button, HorizontalGroup("Sub Rolls")]
#endif
        protected void RollEnabledToggles()
        {
            bool allowedToSpawn = true;
            bool shouldEnable;
            for (int i = 0; i < pustules.Count; i++)
            {
                shouldEnable = !allowedToSpawn || !spawnCountProbability.RollToHit(i);
                if (!shouldEnable) allowedToSpawn = false; //once we fail a roll, it's game over.
                
                pustules[i].gameObject.SetActive(shouldEnable);
            }
        }
    }
}
