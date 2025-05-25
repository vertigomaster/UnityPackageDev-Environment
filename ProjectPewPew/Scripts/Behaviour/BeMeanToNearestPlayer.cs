using System.Collections;
using System.Collections.Generic;
using System.Linq;
using IDEK.Tools.GameplayEssentials.Characters.Unity;
using IDEK.Tools.GameplayEssentials.Core;
using IDEK.Tools.GameplayEssentials.Targeting;
using IDEK.Tools.GameplayEssentials.Transformation;
using IDEK.Tools.Logging;
#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#endif
using UnityEditor;
using UnityEngine;

namespace IDEK.Tools.GameplayEssentials.Samples.PewPew
{
    public class BeMeanToNearestPlayer : MonoBehaviour
    {
#if ODIN_INSPECTOR
        [InfoBox("")]
#endif
        public ShareableAimTrajectory targeter;
        
        // Start is called before the first frame update
        void OnEnable()
        {
            AggroClosestPlayer();
        }

        public void AggroClosestPlayer()
        {
            IEnumerable<CharacterAvatar> avatars = RuntimeCharacterRegistry.Singleton.GetAllAvatars();
            
            float closestDistance = float.MaxValue;
            CharacterAvatar closestAvatar = null;
            foreach (var avatar in avatars)
            {
                float sqrDistance = (avatar.transform.position - transform.position).sqrMagnitude;
                if (sqrDistance < closestDistance)
                    closestAvatar = avatar;
            }

            if (closestAvatar == null)
            {
                ConsoleLog.LogError("Could not find closest avatar. No one to be mean to.");
                this.enabled = false;
                return;
            }
            
            ConsoleLog.Log($"I found a CharacterAvatar! His name is {closestAvatar.name}. I'm gonna kick your ass, {closestAvatar.name}!");
            
            targeter.target = closestAvatar.transform;
        }
    }

    
}
