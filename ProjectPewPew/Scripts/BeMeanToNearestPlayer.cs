using System.Collections;
using System.Collections.Generic;
using IDEK.Tools.GameplayEssentials.Characters.Unity;
using IDEK.Tools.GameplayEssentials.Targeting;
using IDEK.Tools.GameplayEssentials.Transformation;
using IDEK.Tools.Logging;
using UnityEditor;
using UnityEngine;

namespace IDEK.Tools.GameplayEssentials.Samples.PewPew
{
    public class BeMeanToNearestPlayer : MonoBehaviour
    {
        public ShareableAimTrajectory targeter;
        
        CharacterAvatar[] _avatars;
        
        // Start is called before the first frame update
        void OnEnable()
        {
            AggroClosestPlayer();
        }

        public void AggroClosestPlayer()
        {
            //TODO: keep a registry they can look at instead of every enemy having this.

            _avatars = FindObjectsByType<CharacterAvatar>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
            
            float closestDistance = float.MaxValue;
            CharacterAvatar closestAvatar = null;
            foreach (var avatar in _avatars)
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
        }
    }
}
