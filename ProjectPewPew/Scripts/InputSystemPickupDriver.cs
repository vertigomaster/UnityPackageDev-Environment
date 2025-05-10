using IDEK.Tools.Logging;
using IDEK.Tools.ShocktroopExtensions;
using UnityEngine;
using UnityEngine.InputSystem;

namespace IDEK.Tools.GameplayEssentials.Interaction.Samples.Unity
{
    //Sample showing how to use the InputSystem to drive a GameplayEssentials Interaction agent.
    public class InputSystemPickupDriver : PickupDriver
    {
        [SerializeField]
        protected InputActionReference interactAction;

        protected virtual void OnEnable()
        {
            interactAction.action.performed += OnInteractInput;
        }

        protected virtual void OnDisable()
        {
            interactAction.action.performed -= OnInteractInput;
        }
        
        private void OnInteractInput(InputAction.CallbackContext obj)
        {
            Pickup();
        }

        // protected virtual void Update()
        // {
        //     if (interactAction.action.WasPerformedThisFrame()) Pickup();
        // }
    }

    public class MouseClickPickupDriver : PickupDriver
    {
        private void Update()
        {
            // if (Cursor.)
        }
    }

    /// <summary>
    /// Pickup driver
    /// great thing to override for implementing various WAYS of triggering
    /// the pickup, be that  the Input System, the Input Manager, a GOFAI agent,
    /// mouse click, etc
    /// </summary>
    public class PickupDriver : MonoBehaviour
    {
        [SerializeField]
        protected InteractionAgent agent;

        private void OnValidate()
        {
            gameObject.TryGetComponentIfNull(ref agent);
        }

        /// <summary>
        /// Specifies what exact action to take in order to pick up.
        /// Base provides the most commonly useful implementation
        /// </summary>
        /// <remarks>
        /// SRP - Specifies what exact action to take in order to pick up.
        /// <br/>
        /// Agents provide multiple mechanisms, so can override to get
        /// the one that works best for your case. 
        /// <para/>
        /// Decoupling - Separates the DECISION of how to pick up from
        /// the LOGIC of how to pick up.
        /// <br/>
        /// Also separates the HOW to pick up from the WHEN to pick up;
        /// the latter is left up to the caller of this function.
        /// </remarks>
        protected virtual void Pickup() => agent.TryTriggerNearestInteractable();
    }
}