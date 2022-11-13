using System;
using System.Collections;
using Unity.Mathematics;
using UnityEngine;
using Random = UnityEngine.Random;

namespace _Project.Codebase
{
    public class AsteroidSpawner : MonoBehaviour
    {
        [SerializeField] private GameObject _asteroidPrefab;

        private float seed;

        private const float _ASTEROID_FIELD_WIDTH = 150f;
        private const float _ASTEROID_FIELD_NOISE_SAMPLE_RATE = .85f;
        private const int _ITERATIONS_PER_TICK = 500;

        private void OnValidate()
        {
            seed = Random.Range(-999f, 999f);
        }

        private void Start()
        {
            seed = Random.Range(-999f, 999f);
            StartCoroutine(SpawnRoutine());
        }

        private IEnumerator SpawnRoutine()
        {
            int iterationCount = 0;
            for (float x = -_ASTEROID_FIELD_WIDTH / 2f; x < _ASTEROID_FIELD_WIDTH / 2f; x += _ASTEROID_FIELD_NOISE_SAMPLE_RATE)
            {
                for (float y = -_ASTEROID_FIELD_WIDTH / 2f; y < _ASTEROID_FIELD_WIDTH / 2f; y += _ASTEROID_FIELD_NOISE_SAMPLE_RATE)
                {
                    float noise = GetNoise(x, y);
                    
                    if (noise > .5f)
                        Instantiate(_asteroidPrefab, new Vector2(x, y), Quaternion.identity);

                    iterationCount++;

                    if (iterationCount % _ITERATIONS_PER_TICK == 0)
                        yield return null;
                }
            }
        }

        private void OnDrawGizmos()
        {
            int size = 35;
            for (float x = -size / 2f; x < size / 2f; x += _ASTEROID_FIELD_NOISE_SAMPLE_RATE)
            {
                for (float y = -size / 2f; y < size / 2f; y += _ASTEROID_FIELD_NOISE_SAMPLE_RATE)
                {
                    Vector2 pos = new Vector2(x + transform.position.x, y + transform.position.y);
                    float noise = GetNoise(pos.x, pos.y);
                    Gizmos.DrawWireSphere(new Vector3(x,y, 0f), noise / 2f);
                }
            }
        }

        public float GetNoise(float x, float y)
        {
            x += seed;
            y += seed;
            float xSample = x / _ASTEROID_FIELD_WIDTH * 7f;
            float ySample = y / _ASTEROID_FIELD_WIDTH * 7f;
            xSample = Mathf.Pow(xSample, 2f);
            ySample = Mathf.Pow(ySample, 2f);
            
            float noiseSample = noise.cnoise(new float2(xSample, ySample));
            //noiseSample = (noiseSample + 1) / 2f;
            
            return noiseSample;
        }
    }
}