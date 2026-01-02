using IDEK.Tools.GameplayEssentials.Effects;
using IDEK.Tools.Logging;
using UnityEngine;

namespace IDEK.Tools.GameplayEssentials.Samples.PewPew.Effects
{
    // [CreateAssetMenu(fileName = "OxyChangeEffect", menuName = "Pew Pew/Effects/Oxygen Level Change", order = 100)]
    public class OxygenDrainAttributeEffect : TickingGameplayAttributeEffect
    {
        public float drainStrength = 0.05f;
        //when active
        //every frame, make oxygen decrease by x * deltaTime

        #region Overrides of TickingGameplayEffectAsset

        /// <inheritdoc />
        protected override void Tick(GameplayAttributeEffectSystem targetSystem, float deltaTime)
        {
            //look for an oxygen attribute
            if (!targetSystem.TryGetAttribute<OxygenAttribute>(out var oxy)) return;
            
            //TODO; do something like IDamageable does to manage incoming buffs
            oxy.BaseValue -= drainStrength * deltaTime;
            ConsoleLog.Log("Oxygen at " + oxy.BaseValue + "%");
        }

        /// <inheritdoc />
        public override object Clone()
        {
            return (OxygenDrainAttributeEffect)MemberwiseClone();
        }

        #endregion
    }
}