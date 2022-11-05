using Unity.Mathematics;
using UnityEngine;

namespace _Project.Codebase
{
    public class AsteroidSpawner : MonoBehaviour
    {
        [SerializeField] private GameObject _asteroidPrefab;

        private float seed;
        
        private float _blockedHeight;
        private float _lastSpawnTime;
        private float _lastSpawnHeight;

        private const float _ASTEROID_FIELD_WIDTH = 50;
        private const float _ASTEROID_FIELD_NOISE_SAMPLE_RATE = .5f;
        
        private const float _DEFAULT_SPAWN_RATE = .75f;

        private void Start()
        {
            _lastSpawnTime = 0f;
            _lastSpawnHeight = 0f;
            _blockedHeight = 20f;

            for (float x = -_ASTEROID_FIELD_WIDTH / 2f; x < _ASTEROID_FIELD_WIDTH / 2f; x += _ASTEROID_FIELD_NOISE_SAMPLE_RATE)
            {
                for (float y = -_ASTEROID_FIELD_WIDTH / 2f; y < _ASTEROID_FIELD_WIDTH / 2f; y += _ASTEROID_FIELD_NOISE_SAMPLE_RATE)
                {
                    float noise = GetNoise(x, y);

                    //Debug.Log(noise);
                    
                    if (noise > .75f)
                        Instantiate(_asteroidPrefab, new Vector2(x, y), Quaternion.identity);
                }
            }
            
        }

        private void OnDrawGizmos()
        {
            /*
            for (float x = -_ASTEROID_FIELD_WIDTH / 2f; x < _ASTEROID_FIELD_WIDTH / 2f; x += _ASTEROID_FIELD_NOISE_SAMPLE_RATE)
            {
                for (float y = -_ASTEROID_FIELD_WIDTH / 2f; y < _ASTEROID_FIELD_WIDTH / 2f; y += _ASTEROID_FIELD_NOISE_SAMPLE_RATE)
                {
                    float noise = GetNoise(x, y);
                    Gizmos.DrawWireSphere(new Vector3(x,y, 0f), noise / 2f);
                    Debug.Log(noise);
                }
            }
            */
        }

        public float GetNoise(float x, float y)
        {
            float xSample = x / _ASTEROID_FIELD_WIDTH * 25f;
            float ySample = y / _ASTEROID_FIELD_WIDTH * 25f;
            xSample = Mathf.Pow(xSample, 2f);
            ySample = Mathf.Pow(ySample, 2f);

            float noiseSample = noise.cnoise(new float2(xSample, ySample));
            noiseSample = (noiseSample + 1) / 2f;
            
            return noiseSample;
        }
        
        /*

        private void Update()
        {
            //_blockedHeight = Mathf.Sin(Time.time % 1 / 8f) * 4f;
            //Debug.Log(_blockedHeight);
            if (Time.time > _lastSpawnTime + _DEFAULT_SPAWN_RATE)
            {
                var newOrb = Instantiate(_asteroidPrefab).GetComponent<Asteroid>();

                float spawnHeight;
                do
                {
                    spawnHeight = Random.Range(-4.5f, 4.5f);
                } while (Mathf.Abs(_lastSpawnHeight - spawnHeight) <= newOrb.Radius * 2f || 
                         Mathf.Abs(_blockedHeight - spawnHeight) <= 2.5f);
                
                newOrb.transform.position = new Vector3(12f, spawnHeight, 0f);
                
                _lastSpawnTime = Time.time;
                _lastSpawnHeight = spawnHeight;
            }
        }
        */
    }
}