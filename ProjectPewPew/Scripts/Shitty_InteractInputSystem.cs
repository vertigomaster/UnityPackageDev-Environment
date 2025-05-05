using IDEK.Tools.GameplayEssentials.Interaction;
using IDEK.Tools.Logging;
using IDEK.Tools.ShocktroopExtensions;
using UnityEngine;
using UnityEngine.InputSystem;

namespace IDEK.Tools.Misc.DevEnv.ProjectPewPew
{
    public class Shitty_InteractInputSystem : MonoBehaviour
    {
        public InputActionReference interactAction;
        
        public InteractionAgent agent;

        private void OnValidate()
        {
            gameObject.TryGetComponentIfNull(ref agent);
        }

        // private void OnEnable()
        // {
        //     interactAction.action.performed += OnDidInteractAction;
        // }
        //
        // private void OnDisable()
        // {
        //     interactAction.action.performed -= OnDidInteractAction;
        // }

        private void Update()
        {
            if (interactAction.action.WasPerformedThisFrame())
            {
                ConsoleLog.Log("fooble - pressed interact action");
                agent.TryTriggerNearestInteractable();
            }
        }

        // private void OnDidInteractAction(InputAction.CallbackContext obj)
        // {
        //     ConsoleLog.Log("fooble OnDidInteractAction thing");
        //     if (obj.ReadValue<bool>())
        //     {
        //         ConsoleLog.Log("fooble OnDidInteractAction and thing TRUE");
        //
        //         //do thing
        //         agent.TryTriggerNearestInteractable();
        //     }
        // }
    }
}