using IDEK.Tools.ShocktroopUtils.Services;
using UnityEngine;

namespace IDEK.PlantGame.Ecology
{
    public interface IPlantingService : IService
    {
        void Plant(GameObject plant, SoilComponent soil, SeedComponent seed);
    }
}