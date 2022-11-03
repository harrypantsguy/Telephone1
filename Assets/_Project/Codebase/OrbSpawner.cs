using UnityEngine;
using Random = UnityEngine.Random;

namespace _Project.Codebase
{
    public class OrbSpawner : MonoBehaviour
    {
        [SerializeField] private GameObject _orbPrefab;
        
        private float _lastSpawnTime;
        private float _lastSpawnHeight;

        private const float DEFAULT_SPAWN_RATE = .3f;
        private void Start()
        {
            _lastSpawnTime = 0f;
            _lastSpawnHeight = 0f;
        } 

        private void Update()
        {
            if (Time.time > _lastSpawnTime + DEFAULT_SPAWN_RATE)
            {
                float spawnHeight;
                do
                {
                    spawnHeight = Random.Range(-4.5f, 4.5f);
                } while (Mathf.Abs(_lastSpawnHeight - spawnHeight) < .5f);

                Instantiate(_orbPrefab, new Vector3(10f, spawnHeight, 0f), Quaternion.identity);
                _lastSpawnTime = Time.time;
                _lastSpawnHeight = spawnHeight;
            }
        }
    }
}