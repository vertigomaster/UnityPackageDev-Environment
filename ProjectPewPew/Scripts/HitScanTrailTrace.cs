using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace IDEK.Tools.GameplayEssentials.Samples.PewPew
{
    public class HitScanTrailTrace : MonoBehaviour
    {
        //this curve essentially "slides" along the line renderer to update the width curve with time offset
        public LineRenderer lineRenderer;

        public int resolution = 100;
        public AnimationCurve widthDistribution;
        public float traversalSpeed = 10f;
        public Gradient colorGradient;
        [Tooltip("Should the color gradient also line up with the width distribution pulse or color across the whole line?")]
        public bool slideGradientWithWidthDistrib;

        public float lifetime = 5f;
        
        [Tooltip("Negative values will be interpreted as running every frame")]
        public float tickRate = -1f;
        
        protected IEnumerator ShootRoutine()
        {
            float elapsedTime = 0f;
            while (elapsedTime < lifetime)
            {
                //update values
                
                //1. move forward by traversal speed * Time.deltaTime
                //2. raycast to determine bounces
                
                //tick forward
                if (tickRate < 0f)
                {
                    yield return null;
                }
                else
                {
                    yield return new WaitForSeconds(tickRate);
                }
                elapsedTime += Time.deltaTime;
            }
            
            Destroy(this.gameObject);
        }
    }
}