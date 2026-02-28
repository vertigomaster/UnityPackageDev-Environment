using UnityEngine;

namespace IDEK.PlantGame.Ecology
{
    public class SeedComponent : MonoBehaviour
    {
        public bool isPlanted;

        public virtual bool TryPlantHere(GameObject targetObj, Vector3 position)
        {
            if (CanPlantHere(targetObj)) {
                PlantSelfIn(targetObj, transform.position);
                return true;
            }
            return false;
        } 

        public virtual bool CanPlantHere(GameObject potentialDirt) => CanPlantHere(potentialDirt, transform.position);

        public virtual bool CanPlantHere(GameObject potentialDirt, Vector3 queryPosition)
        {
            return potentialDirt.TryGetComponent(out SoilComponent _);
            //TODO: check conditions for thresholds
        }

        protected virtual void PlantSelfIn(GameObject potentialDirt, Vector3 plantingPosition)
        {
            //set up/spawn the plant object associated with this seed
        }
    }
}