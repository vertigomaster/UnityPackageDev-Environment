using IDEK.Tools.Logging;
using IDEK.Tools.ShocktroopExtensions;
using UnityEngine;
using UnityEngine.InputSystem;

namespace IDEK.Tools.GameplayEssentials.Interaction.Samples.Unity
{
    //Sample showing how to use the InputSystem to drive a GameplayEssentials Interaction agent.
    public class BasicPlayerInteractDriver : MonoBehaviour
    {
        public InputActionReference interactAction;
        public InteractionAgent agent;

        private void OnValidate()
        {
            gameObject.TryGetComponentIfNull(ref agent);
        }

        private void Update()
        {
            if (!interactAction.action.WasPerformedThisFrame()) return;
            agent.TryTriggerNearestInteractable();
        }
    }
}