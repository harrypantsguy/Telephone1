using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.VFX;

namespace _Project.Codebase
{
    public class Player : MonoSingleton<Player>
    {
        [SerializeField] private Rigidbody2D _rb;
        [SerializeField] private LineRenderer _lineRenderer;
        [SerializeField] private Transform _spriteTransform;
        [SerializeField] private StasisBarrier _stasisBarrier;
        [SerializeField] private VisualEffect _beamHitVfx;
        [SerializeField] private Light2D _beamHitLight;
        
        public bool OffScreen { get; private set; }
        public float MaxStamina { get; private set; } = _DEFAULT_MAX_STAMINA;
        public float Stamina { get; private set; }
        

        private const float _DEFAULT_MOVE_SPEED = 5f;
        private const float _DEFAULT_BEAM_LENGTH = 3f;
        private const float _DEFAULT_BEAM_DAMAGE_RATE = .25f;
        private const float _SPRINT_SPEED_INCREASE = 2.5f;
        private const float _DEFAULT_MAX_STAMINA = 5f;
        private const float _DEFAULT_STAMINA_DECAY_RATE = 1f;
        private const float _DEFAULT_STAMINA_RECOVERY_RATE = .75f;

        private const string _BEAM_HIT_VFX_SPAWN_RATE_HASH = "SpawnRate";
        
        private Vector2 _inputVelocity;
        private float _lastDamageTime;
        
        private void Start()
        {
            Stamina = MaxStamina;
        }

        [UsedImplicitly]
        private void Update()
        {
            if (_rb.position.x < -10f)
                OffScreen = true;

            if (OffScreen) return;

            HandleMovement();
            HandleBeam();
            HandleSpriteRotation();
        }

        private void HandleMovement()
        {
            bool pressingSprint = GameControls.Sprint.IsHeld;
            
            float moveSpeed = _DEFAULT_MOVE_SPEED + (pressingSprint && Stamina > 0f ? _SPRINT_SPEED_INCREASE : 0f);

            _inputVelocity = GameControls.DirectionalInput * moveSpeed;
            _rb.velocity = Vector2.Lerp(_rb.velocity, _inputVelocity + new Vector2(-_stasisBarrier.TotalNumChildAsteroids, 0f), 5f * Time.deltaTime);

            Stamina = Mathf.Clamp(Stamina + (pressingSprint ? -_DEFAULT_STAMINA_DECAY_RATE : _DEFAULT_STAMINA_RECOVERY_RATE) * Time.deltaTime, 0f, MaxStamina);
        }

        private void HandleBeam()
        {
            var beamStart = _rb.position;
            Vector2 beamEnd;
            var beamDir = (Utils.WorldMousePos - _rb.position).normalized;

            var hit = Physics2D.Raycast(_rb.position, beamDir, _DEFAULT_BEAM_LENGTH, 
                Layers.AsteroidMask);

            bool firingBeam = GameControls.FireBeam.IsHeld;

            if (firingBeam)
            {
                _lineRenderer.enabled = true;
                
                if (hit.collider != null && hit.collider.TryGetComponent(out IDamageable damageable))
                {
                    beamEnd = hit.point;
                    _beamHitLight.transform.position = beamEnd;
                    _beamHitVfx.transform.position = beamEnd;
                    _beamHitVfx.transform.right = _spriteTransform.right;

                    if (Time.time > _lastDamageTime + _DEFAULT_BEAM_DAMAGE_RATE)
                    {
                        if (firingBeam)
                            damageable.TakeDamage(1);
                        _lastDamageTime = Time.time;
                    }

                    _beamHitLight.enabled = true;
                    _beamHitVfx.SetInt(_BEAM_HIT_VFX_SPAWN_RATE_HASH, 32);
                }
                else
                {
                    beamEnd = (Vector2)transform.position + beamDir * _DEFAULT_BEAM_LENGTH;
                    _beamHitLight.enabled = false;
                    _beamHitVfx.SetInt(_BEAM_HIT_VFX_SPAWN_RATE_HASH, 0);
                }

                _lineRenderer.positionCount = 2;
                _lineRenderer.SetPositions(new Vector3[]
                {
                    beamStart,
                    beamEnd
                });
            }
            else
            {
                _lineRenderer.enabled = false;
                _beamHitLight.enabled = false;
                _beamHitVfx.SetInt(_BEAM_HIT_VFX_SPAWN_RATE_HASH, 0);
            }
        }

        private void HandleSpriteRotation()
        {
            var spriteDir = (Utils.WorldMousePos - _rb.position).normalized;
            _spriteTransform.right = spriteDir;
        }
    }
}
