using UnityEngine;
using Random = UnityEngine.Random;

namespace _Project.Codebase
{
    public class AsteroidSpawner : MonoBehaviour
    {
        [SerializeField] private GameObject _asteroidPrefab;
        
        private const float DEFAULT_SPAWN_RATE = .75f;

        private float _blockedHeight;
        private float _lastSpawnTime;
        private float _lastSpawnHeight;

        private void Start()
        {
            _lastSpawnTime = 0f;
            _lastSpawnHeight = 0f;
            _blockedHeight = 20f;
        } 

        private void Update()
        {
            //_blockedHeight = Mathf.Sin(Time.time % 1 / 8f) * 4f;
            //Debug.Log(_blockedHeight);
            if (Time.time > _lastSpawnTime + DEFAULT_SPAWN_RATE)
            {
                var newOrb = Instantiate(_asteroidPrefab).GetComponent<Asteroid>();

                float spawnHeight;
                do
                {
                    spawnHeight = Random.Range(-4.5f, 4.5f);
                } while (Mathf.Abs(_lastSpawnHeight - spawnHeight) <= newOrb.Radius * 2f || 
                         Mathf.Abs(_blockedHeight - spawnHeight) <= 2.5f);
                
                newOrb.transform.position = new Vector3(10f, spawnHeight, 0f);
                
                _lastSpawnTime = Time.time;
                _lastSpawnHeight = spawnHeight;
            }
        }
    }
}