using JetBrains.Annotations;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.VFX;

namespace _Project.Codebase
{
    public class Player : MonoSingleton<Player>
    {
        [SerializeField] private Rigidbody2D _rb;
        [SerializeField] private LineRenderer _miningBeamLineRenderer;
        [SerializeField] private TractorBeam _tractorBeam;
        [SerializeField] private Transform _spriteTransform;
        [SerializeField] private StasisBarrier _stasisBarrier;
        [SerializeField] private VisualEffect _beamHitVfx;
        [SerializeField] private Light2D _beamHitLight;

        public bool StasisActivated { get; private set; }
        public float MaxStamina { get; private set; } = _DEFAULT_MAX_STAMINA;
        public float Stamina { get; private set; }
        public float TractorBeamWidth { get; private set; } = _DEFAULT_TRACTOR_BEAM_WIDTH;
        public float TractorBeamLength { get; private set; } = _DEFAULT_TRACTOR_BEAM_LENGTH;


        private const float _DEFAULT_MOVE_SPEED = 5f;
        private const float _DEFAULT_MINING_BEAM_LENGTH = 3f;
        private const float _DEFAULT_MINING_BEAM_DAMAGE_RATE = .25f;
        private const float _DEFAULT_TRACTOR_BEAM_LENGTH = 4f;
        private const float _DEFAULT_TRACTOR_BEAM_WIDTH = .5f;
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
            _stasisBarrier.SetActive(StasisActivated);
        }

        [UsedImplicitly]
        private void Update()
        {
            HandleStasis();
            HandleMovement();
            HandleMiningBeam();
            HandleTractorBeam();
            HandleSpriteRotation();
        }

        private void HandleStasis()
        {
            if (GameControls.ToggleStasis.IsPressed)
            {
                StasisActivated = !StasisActivated;
                _stasisBarrier.SetActive(StasisActivated);
            }
        }
        
        private void HandleMovement()
        {
            bool pressingSprint = GameControls.Sprint.IsHeld;
            
            float moveSpeed = _DEFAULT_MOVE_SPEED + (pressingSprint && Stamina > 0f ? _SPRINT_SPEED_INCREASE : 0f);

            _inputVelocity = GameControls.DirectionalInput * Mathf.Max(moveSpeed - _stasisBarrier.TotalNumChildAsteroids * .25f, 0f);
            _rb.velocity = Vector2.Lerp(_rb.velocity, _inputVelocity,5f * Time.deltaTime);
                                                      //+ new Vector2(-_stasisBarrier.TotalNumChildAsteroids, 0f),
                                                      
            Stamina = Mathf.Clamp(Stamina + (pressingSprint ? -_DEFAULT_STAMINA_DECAY_RATE : _DEFAULT_STAMINA_RECOVERY_RATE) * Time.deltaTime, 0f, MaxStamina);
        }

        private void HandleMiningBeam()
        {
            var beamStart = _rb.position;
            Vector2 beamEnd;
            var beamDir = (Utils.WorldMousePos - _rb.position).normalized;

            var hit = Physics2D.Raycast(_rb.position, beamDir, _DEFAULT_MINING_BEAM_LENGTH, 
                Layers.AsteroidMask);

            bool firingBeam = GameControls.FireMiningBeam.IsHeld;

            if (firingBeam)
            {
                _miningBeamLineRenderer.enabled = true;
                
                if (hit.collider != null && hit.collider.TryGetComponent(out IDamageable damageable))
                {
                    beamEnd = hit.point;
                    _beamHitLight.transform.position = beamEnd;
                    _beamHitVfx.transform.position = beamEnd;
                    _beamHitVfx.transform.right = _spriteTransform.right;

                    if (Time.time > _lastDamageTime + _DEFAULT_MINING_BEAM_DAMAGE_RATE)
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
                    beamEnd = (Vector2)transform.position + beamDir * _DEFAULT_MINING_BEAM_LENGTH;
                    _beamHitLight.enabled = false;
                    _beamHitVfx.SetInt(_BEAM_HIT_VFX_SPAWN_RATE_HASH, 0);
                }

                _miningBeamLineRenderer.positionCount = 2;
                _miningBeamLineRenderer.SetPositions(new Vector3[]
                {
                    beamStart,
                    beamEnd
                });
            }
            else
            {
                _miningBeamLineRenderer.enabled = false;
                _beamHitLight.enabled = false;
                _beamHitVfx.SetInt(_BEAM_HIT_VFX_SPAWN_RATE_HASH, 0);
            }
        }

        private void HandleTractorBeam()
        {
            _tractorBeam.length = TractorBeamLength;
            _tractorBeam.width = TractorBeamWidth;
            _tractorBeam.firing = GameControls.FireTractorBeam.IsHeld;
        }

        private void HandleSpriteRotation()
        {
            var spriteDir = (Utils.WorldMousePos - _rb.position).normalized;
            _spriteTransform.right = spriteDir;
        }
    }
}
