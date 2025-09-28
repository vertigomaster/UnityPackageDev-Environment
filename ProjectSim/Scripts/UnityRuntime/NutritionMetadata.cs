using System.Collections.Generic;
using System.Linq;
using IDEK.Tools.DataStructures;
using IDEK.Tools.GameplayEssentials.Items;
using IDEK.Tools.GameplayEssentials.Items.Unity;
using IDEK.Tools.Logging;
using UnityEngine;
using UnityEngine.Serialization;

namespace IDEK.ProjectSim.Unity.Runtime
{
    //eventually if we removed the item metadata framework to its own assembly, we could remove the Unity constraint
    //just a thought.
    public class NutritionMetadata : IItemMetadata//, IResolvableItemMetadata
    {
        public const int ABNORMAL_QUEUE_SIZE_THRESHOLD = 1000;
            
        //if we exceed this, we are probably stuck in a cyclical loop
        // public const int MAX_SEARCH_DEPTH = 20; 
        
        // public float amount; //may relegate this to a stack or fall back on the correct IItemQuantity impl
        // public List<ItemStack> ingredients;
        public CleanableList<ItemStack> ingredients;
        public bool isMeat; //eventually set up items for ingredients. This is enough for our herbivore and carnivore sim.
        public bool isDrinkable;
        public float toxicity;
        /// <summary>
        /// integral energy that is not provided by the ingredients
        /// </summary>
        public float baseEnergy;
        
        // [field: SerializeField]
        // public float IngredientEnergySum { get; protected set; } //cached/calculated by editor
        
        private float _cachedTotalEnergy;
        
        /// <summary>
        /// Checks the base energy of this item and all its ingredients
        /// (and in turn their own ingredients, and so on)
        /// </summary>
        /// <remarks>
        /// Highly suggest caching this if you use it a lot.
        /// <br/>
        /// Beware of cyclical ingredient trees! And be wary of complex recipes. 
        /// </remarks>
        /// <returns></returns>
        public float GetTotalEnergyOfIngredients()
        {
            //quick short-out
            if(ingredients.IsClean)
            {
                return _cachedTotalEnergy;
            }

            if (ingredients == null || ingredients.Count <= 0)
            {
                return baseEnergy;
            }

            HashSet<ItemStack> itemsEncountered = new();
            Queue<(NutritionMetadata, float)> queue = new();
            (NutritionMetadata data, float amount) currNutrition;
            float totalEnergy = 0f;
            
            queue.Enqueue((this, 1));
            
            while (queue.Count > 0)
            {
                if (queue.Count > ABNORMAL_QUEUE_SIZE_THRESHOLD)
                {
                    ConsoleLog.LogWarning("Abnormally large ingredient queue size. Be careful with overly complex recipes!");
                }
                currNutrition = queue.Dequeue();
                
                if(currNutrition.amount <= 0f || currNutrition.data == null)
                    continue;
                
                totalEnergy += (currNutrition.data.baseEnergy * currNutrition.amount);
                
                //if has no ingredients of its own, it is a "base" ingredient 
                if (currNutrition.data.ingredients == null || currNutrition.data.ingredients.Count <= 0)
                {
                    continue;
                }
                
                //queue up the ingredients of the current nutrition item
                foreach (ItemStack ingredientStack in currNutrition.data.ingredients)
                {
                    if (!itemsEncountered.Add(ingredientStack)) //hashset add failure means the item is already present.
                    {
                        ConsoleLog.LogError($"Potentially cyclical ingredient data Error - " +
                            $"an item's ingredients contain itself (found {ingredientStack} twice), which makes no sense and " +
                            $"would give us an infinite loop. Skipping it. Result may be inaccurate.");
                        continue;
                    }
                    
                    queue.Enqueue((
                        ingredientStack.ResolveMetadata<NutritionMetadata>(), 
                        ingredientStack.FloatQuantity));    
                }
            }
            
            baseEnergy = totalEnergy;
            ingredients.Clean();
            
            return totalEnergy;
        }

        #region Implementation of IItemMetadata

        /// <inheritdoc />
        public IItemMetadata DeepCopy() => MemberwiseClone() as NutritionMetadata;

        #endregion

        // #region Implementation of IResolvableItemMetadata
        //
        // // /// <inheritdoc />
        // // public IItemMetadata ResolveRuntimeData(IItemMetadata globalMetadata)
        // // {
        // //     if(!globalMetadata.TryGet(out NutritionMetadata global)) return this;
        // //     
        // //     if()
        // //     baseEnergy = global.baseEnergy;
        // //     
        // //     return null;
        // // }
        // //
        // // /// <inheritdoc />
        // // public IItemMetadata ResolveRuntimeData(ItemDef globalDefinition)
        // // {
        // //     return null;
        // // }
        //
        // #endregion
    }
}