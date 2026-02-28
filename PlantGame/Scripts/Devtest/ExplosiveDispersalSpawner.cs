using Sirenix.OdinInspector;
using Unity.Cinemachine;
using UnityEngine;

namespace IDEK.PlantGame.Ecology
{
    public class ExplosiveDispersalSpawner : MonoBehaviour
    {
        public GameObject seedPrefab;
        [Vector2AsRange]
        public Vector2 seedCount;

        [Header("Spawn Parameters")]
        [Min(0f)]
        public float spawnOffsetRadius = 0.01f;
        
        [Header("Explosion Parameters")]
        [Min(0f)]
        public float explosionPower = 20f;
        [Range(0f, 180f)]
        public float maxAngleSpreadDeg = 20f;
        public Vector3 explosionDirection = Vector3.up;
        public bool ignoreSeedMass = false;
        
        [Button]
        public void Explode()
        {
            int chosenSeedCount = Mathf.FloorToInt(Random.Range(seedCount.x, seedCount.y));
            
            for(int i = 0; i < chosenSeedCount; i++)
            {
                var launchDirection = explosionDirection == Vector3.zero ? Random.onUnitSphere : CalcLaunchDirection();
                var initPos = transform.position + (Random.onUnitSphere * spawnOffsetRadius);
                var initRot = Quaternion.LookRotation(launchDirection);
                var newSeed = Instantiate(seedPrefab, initPos, initRot);
                
                if(!newSeed.TryGetComponent(out Rigidbody rb)) rb = newSeed.AddComponent<Rigidbody>();
                
                //launch it!
                rb.AddForce(explosionDirection * explosionPower, ignoreSeedMass ? ForceMode.Impulse : ForceMode.VelocityChange);
            }
        }

        private Vector3 CalcLaunchDirection()
        {
            //how many degrees off from the direction to aim
            float angleSpreadDeg = Random.value * maxAngleSpreadDeg;
            
            //pick a random orthogonal vector to rotate the direction around
            Vector3 randomAxis = Vector3.ProjectOnPlane(Random.onUnitSphere, explosionDirection);
            
            //actually rotate the direction by the chosen number of degrees about the chosen orthogonal axis
            return Quaternion.AngleAxis(angleSpreadDeg, randomAxis) * explosionDirection;
        }
    }
}