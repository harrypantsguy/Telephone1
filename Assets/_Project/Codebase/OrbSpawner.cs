using UnityEngine;
using Random = UnityEngine.Random;

namespace _Project.Codebase
{
    public class OrbSpawner : MonoBehaviour
    {
        [SerializeField] private GameObject _orbPrefab;
        
        private const float DEFAULT_SPAWN_RATE = .3f;

        private float _lastSpawnTime;
        private float _lastSpawnHeight;

        private void Start()
        {
            _lastSpawnTime = 0f;
            _lastSpawnHeight = 0f;
        } 

        private void Update()
        {
            if (Time.time > _lastSpawnTime + DEFAULT_SPAWN_RATE)
            {
                var newOrb = Instantiate(_orbPrefab).GetComponent<Orb>();

                float spawnHeight;
                do
                {
                    spawnHeight = Random.Range(-4.5f, 4.5f);
                } while (Mathf.Abs(_lastSpawnHeight - spawnHeight) <= newOrb.Radius * 2f);
                
                newOrb.transform.position = new Vector3(10f, spawnHeight, 0f);
                
                _lastSpawnTime = Time.time;
                _lastSpawnHeight = spawnHeight;
            }
        }
    }
}