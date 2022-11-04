using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using Random = UnityEngine.Random;

namespace _Project.Codebase
{
    public class Asteroid : MonoBehaviour, IAsteroidParent, IDamageable
    {
        [field: SerializeField] public float Radius { get; private set; }
        [field: SerializeField] public int Health { get; private set; }
        [field: SerializeField] public bool IsInStasis { get; private set; }
        [SerializeField] private Rigidbody2D _rb;
        [SerializeField] private SpriteRenderer _spriteRenderer;
        [SerializeField] private FillImage _healthBar;
        [SerializeField] private GameObject _scoreIncreasePrefab;
        [SerializeField] private ParticleSystem _stasisParticles;
        [SerializeField] private Light2D _stasisLight;
        [SerializeField] private GameObject _debrisParticlesPrefab;

        private int _maxHealth;
        
        public Transform Transform => transform;
        public List<Asteroid> DirectChildrenAsteroids { get; } = new List<Asteroid>();

        public int TotalChildAsteroidCount => DirectChildrenAsteroids.Sum(childOrb => childOrb.TotalChildAsteroidCount) + DirectChildrenAsteroids.Count;

        private IAsteroidParent _parent;
        private float _rotationSpeed;
        private float _moveSpeed;

        [UsedImplicitly]
        private void Start()
        {
            _maxHealth = Health;
            _spriteRenderer.transform.eulerAngles = new Vector3(0f, 0f, Random.Range(0f, 360f));
            _rotationSpeed = Random.Range(3f, 9f);
            _moveSpeed = Random.Range(1.5f, 3f);
        }

        [UsedImplicitly]
        private void Update()
        {
            _healthBar.FillAmount = Utils.Remap01(Health, 0f, _maxHealth);
            _healthBar.Color = Color.HSVToRGB(_healthBar.FillAmount * 100f/360f, 1f, 1f);
            _healthBar.Alpha = Health == _maxHealth ? 0f : 1f;
            
            if (!IsInStasis)
                _spriteRenderer.transform.Rotate(0f, 0f, _rotationSpeed * Time.deltaTime);
        }

        [UsedImplicitly]
        private void FixedUpdate()
        {
            if (!IsInStasis)
            {
                _rb.isKinematic = false;
                _rb.velocity = Vector2.left * _moveSpeed;
            }

            if (_rb.position.x < -10f)
            {
                for (var i = DirectChildrenAsteroids.Count - 1; i >= 0; i--)
                    RemoveAsteroidAsChild(DirectChildrenAsteroids[i]);
                Destroy(gameObject);
            }
        }

        [UsedImplicitly]
        private void LateUpdate()
        {
            var emissionModule = _stasisParticles.emission;
            emissionModule.rateOverTime = IsInStasis ? 25 : 0;
            _stasisLight.enabled = IsInStasis;
        }

        [UsedImplicitly]
        private void OnCollisionEnter2D(Collision2D col)
        {
            if (IsInStasis) return;
            CheckForStasisCollision(col.collider);
        }

        [UsedImplicitly]
        private void OnCollisionStay2D(Collision2D collision)
        {
            if (IsInStasis) return;
            CheckForStasisCollision(collision.collider);
        }

        public void AddAsteroidAsChild(in Asteroid newChildAsteroid)
        {
            DirectChildrenAsteroids.Add(newChildAsteroid);
            newChildAsteroid.SetParent(this);
        }

        public void RemoveAsteroidAsChild(in Asteroid childAsteroid)
        {
            childAsteroid.SetParent(null);
            DirectChildrenAsteroids.Remove(childAsteroid);
        }

        public void GetAllChildAsteroidsNonAlloc(in List<Asteroid> asteroids)
        {
            asteroids.AddRange(DirectChildrenAsteroids);
            foreach (var childAsteroid in DirectChildrenAsteroids)
                childAsteroid.GetAllChildAsteroidsNonAlloc(asteroids);
        }

        public void SetParent(in IAsteroidParent parent)
        {
            _parent = parent;

            if (_parent == null) return;
            
            var offset = (transform.position - parent.Transform.position).normalized * (parent.Radius + Radius);
            transform.SetParent(parent.Transform);
            transform.position = parent.Transform.position + offset;
        }

        public void TakeDamage(in int damage)
        {
            Health = Mathf.Max(Health - damage, 0);

            if (Health == 0)
                Kill();
        }

        public void Kill()
        {
            if (_parent != null)
                _parent.RemoveAsteroidAsChild(this);

            for (var i = DirectChildrenAsteroids.Count - 1; i >= 0; i--)
                DirectChildrenAsteroids[i].Kill();

            ScoreHolder.Singleton.IncreaseScoreBy(1);

            var scoreIncrease = Instantiate(_scoreIncreasePrefab, _rb.position, Quaternion.identity).GetComponent<ScoreIncreaseUI>();
            scoreIncrease.SetIncrease(1);

            var debris = Instantiate(_debrisParticlesPrefab, transform.position, Quaternion.identity);
            Destroy(debris, _debrisParticlesPrefab.GetComponent<ParticleSystem>().main.duration);
            Destroy(gameObject);
        }

        private void CheckForStasisCollision(in Collider2D col)
        {
            if (col.gameObject.TryGetComponent(out StasisBarrier barrier))
            {
                barrier.AddAsteroidAsChild(this);
                IsInStasis = true;
                _rb.isKinematic = true;
            }
            else if (col.gameObject.TryGetComponent(out Asteroid asteroid) && asteroid.IsInStasis)
            {
                asteroid.AddAsteroidAsChild(this);
                IsInStasis = true;
                _rb.isKinematic = true;
            }
        }
    }
}