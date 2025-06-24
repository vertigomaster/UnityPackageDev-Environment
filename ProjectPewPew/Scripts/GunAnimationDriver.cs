using UnityEngine;

namespace IDEK.Tools.GameplayEssentials.Samples.PewPew
{
    public class GunAnimationDriver : MonoBehaviour
    {
        Animator linkedAnimator;
        
        //triggered on any frame where the gun is firing a new projectile
        public string fireTriggerParam = "fire";
        
        //If triggered, a reload animation begins
        public string reloadTriggerParam = "reload";
        
        //what step in the reload phase you are in? Animation can use this value to influence/resume animation state
        //(stage 1 might be like mag release, stage 2 mag pull, stage 3 new mag, stage 4 is bolt pull, or something)
        public string reloadStepStageParam = "reloadStep";

        public void OnGunFired()
        {
            linkedAnimator.SetTrigger(fireTriggerParam);
        }
        
        //todo: more when/if we added reloading
    }
}