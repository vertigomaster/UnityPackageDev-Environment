using IDEK.Tools.GameplayEssentials.Conflict;
using IDEK.Tools.GameplayEssentials.Conflict.Damage;
using UnityEngine;

namespace IDEK.Tools.Misc.DevEnv.Scripts.Devtest
{
    public class ContactDamageSource : MonoBehaviour, IDamageDealer
    {
        #region Implementation of IDamageDealer
        
        [field: SerializeField]
        /// <inheritdoc />
        public DamageData Damage => damageApplicator.Apply();

        public DamageApplicatorAsset damageApplicator;

        #endregion

        private void OnCollisionEnter(Collision other)
        {
            if (!other.gameObject.TryGetComponent(out BaseHealthComponent x)) return;
            
            x.DealDamage(damageApplicator.Apply());
        }
    }
}