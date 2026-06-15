using System;
using IDEK.PlantGame.Ecology;
using IDEK.Tools.Logging;
using IDEK.Tools.ShocktroopUtils.Services;
using UnityEngine;

namespace IDEK.PlantGame.DevTest
{
    public class DevTestPlantingService : MonoBehaviour, IPlantingService
    {
        // [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        // private static void _Bootstrap()
        // {
        //     ServiceLocator.Bind<IPlantingService>(() =>
        //     {
        //         //TODO: other things
        //         return new DevTestPlantingService();
        //     });
        // }

        private void Awake()
        {
            var result = ServiceLocator.TryRegister<IPlantingService>(this);
            switch (result)
            {
                case ServiceLocator.RegistrationResult.Success:
                    ConsoleLog.Log("Registered DevTestPlantingService as IPlantingService");
                    break;
                case ServiceLocator.RegistrationResult.AlreadyRegistered:
                    ConsoleLog.Log("Failed to register DevTestPlantingService as IPlantingService, one is already registered");
                    break;
                case ServiceLocator.RegistrationResult.InvalidInstance:
                    ConsoleLog.Log("Failed to register DevTestPlantingService as IPlantingService, " +
                        "this would imply it is not an IPlantingService");
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        #region Implementation of IService

        /// <inheritdoc />
        public void OnRegister(Type type)
        {
            ConsoleLog.Log($"Registered {type}");
        }

        /// <inheritdoc />
        public void OnUnregister(Type type)
        {
            ConsoleLog.Log($"Unregistered {type}");
        }

        #endregion

        /// <summary>
        /// Sets up this gameobject so that it knows it has been planted
        /// </summary>
        /// <param name="plant"></param>
        /// <param name="seedComponent"></param>
        public void Plant(GameObject plant, SoilComponent soil, SeedComponent seed)
        {
            ConsoleLog.Log($"Planting plant {plant.name} with soil {soil.name} and seed {seed.name}");
            var pb = plant.GetComponent<PlantBase>();
            pb.currentSoil = soil;
        }
    }
}