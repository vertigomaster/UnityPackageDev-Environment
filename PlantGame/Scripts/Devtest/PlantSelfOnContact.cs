using IDEK.Tools.ShocktroopExtensions;
using UnityEngine;

namespace IDEK.PlantGame.Ecology
{
    [RequireComponent(typeof(Rigidbody))]
    public class PlantSelfOnContact : MonoBehaviour
    {
        public SeedComponent seed;
        public LayerMask eligibleLayers = ~0;//default to all

        private Rigidbody _rigidbody;
        
        private void Start()
        {
            _rigidbody = GetComponent<Rigidbody>();
        }

        private void OnCollisionEnter(Collision other)
        {
            if(!eligibleLayers.ContainsLayer(other.gameObject.layer)) return;

            if(seed.TryPlantHere(other.gameObject, transform.position))
            {
                _rigidbody.isKinematic = true;
                enabled = false;
            }
        }
    }
}