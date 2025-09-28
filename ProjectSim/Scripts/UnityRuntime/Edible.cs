using System;
using IDEK.Tools.GameplayEssentials.Items;
using IDEK.Tools.GameplayEssentials.Items.Unity;
using IDEK.Tools.Logging;
using IDEK.Tools.ShocktroopExtensions;
using UnityEngine;

namespace IDEK.ProjectSim.Unity.Runtime
{
    [RequireComponent(typeof(Edible))]
    public class DecayBehavior : MonoBehaviour
    {
        private Edible edible;

        private void OnValidate()
        {
            edible = GetComponent<Edible>();
        }

        private void Update()
        {
            edible.TickDecay(Time.deltaTime);
        }
    }
    
    public class Edible : ItemLogic
    {
        [System.Flags]
        public enum DecayMode { DecayQuantity = 1, DecayQuality = 2 }
        
        public DecayMode mode = DecayMode.DecayQuantity;
        public float decayRate = 1f;
        
        private DecayBehavior decayBehavior;
        
        public DecayBehavior DecayBehavior => gameObject.GetComponentIfNull(ref decayBehavior);
        public NutritionMetadata MutableNutrition => 
            itemRep.representedItemStack.runtimeMetadata.Get<NutritionMetadata>();
        public NutritionMetadata BaseNutrition => itemRep.ItemDef.GetGlobalMetadata<NutritionMetadata>();
        
        public float Amount => itemRep.representedItemStack?.QuantityAsFloat ?? 0f;
        public float Toxicity => (BaseNutrition?.toxicity ?? 0f) + (MutableNutrition?.toxicity ?? 0f);
        
        protected virtual void OnValidate()
        {
            gameObject.GetComponentIfNull(ref itemRep);
        }

        public void SetUp()
        {
            
        }
        
        public void TickDecay(float deltaTime)
        {
            if (mode.HasFlag(DecayMode.DecayQuality))
            {
                //todo: decay quality
                throw new NotSupportedException("still deciding whether this makes sense to add");
            }

            if (mode.HasFlag(DecayMode.DecayQuantity))
            {
                itemRep.representedItemStack.Remove(decayRate * deltaTime);
                
                if(itemRep.representedItemStack.QuantityAsFloat <= 0f)
                    Destroy(gameObject);
            }
        }
    }
}