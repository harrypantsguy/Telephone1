using System;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.VFX;

namespace _Project.Codebase
{
    public class Player : MonoSingleton<Player>
    {
        [SerializeField] private Rigidbody2D _rb;
        [SerializeField] private TractorBeam _tractorBeam;
        [SerializeField] private Transform _spriteTransform;
        [SerializeField] private StasisBarrier _stasisBarrier;
        [SerializeField] private MiningBeam _miningBeam;
        [SerializeField] private VisualEffect _leftThrusterVfx;
        [SerializeField] private VisualEffect _rightThrusterVfx;

        public Vector2 Velocity
        {
            get => _rb.velocity;
            set => _rb.velocity = value;
        }
        public bool StasisActivated { get; private set; }
        public float MaxStamina { get; private set; } = _DEFAULT_MAX_STAMINA;
        public float Stamina { get; private set; }
        public float TractorBeamWidth { get; private set; } = _DEFAULT_TRACTOR_BEAM_WIDTH;
        public float TractorBeamLength { get; private set; } = _DEFAULT_TRACTOR_BEAM_LENGTH;

        private const float _DEFAULT_MOVE_SPEED = 10f;
        private const float _DEFAULT_FORWARD_ACCELERATION = 10f;
        private const float _DEFAULT_BACKWARD_ACCELERATION = 4f;
        private const float _DEFAULT_MINING_BEAM_LENGTH = 3f;
        private const float _DEFAULT_MINING_BEAM_DAMAGE_RATE = .25f;
        private const float _DEFAULT_TRACTOR_BEAM_LENGTH = 4f;
        private const float _DEFAULT_TRACTOR_BEAM_WIDTH = .5f;
        private const float _SPRINT_SPEED_INCREASE = 2.5f;
        private const float _DEFAULT_MAX_STAMINA = 5f;
        private const float _DEFAULT_STAMINA_DECAY_RATE = 1f;
        private const float _DEFAULT_STAMINA_RECOVERY_RATE = .75f;
        private const float _TARGET_POS_CLAMP_RADIUS = 4f;
        private const float _DEFAULT_ROTATE_SPEED = 360f;

        
        private const string _THRUSTER_VFX_SPAWN_RATE_HASH = "SpawnRate";

        private Vector2 _targetAimPos;
        private Vector2 _targetMovePos;
        private Vector2 _clampedTargetMovePos;
        private Vector2 _inputVelocity;
        private Vector2 _thrusterForce;
        private Vector2 _desiredThrusterVelocity;

        private void Start()
        {
            Stamina = MaxStamina;
            _stasisBarrier.SetActive(StasisActivated);
        }

        [UsedImplicitly]
        private void Update()
        {
            HandleStasis();
            HandleMiningBeam();
            //HandleTractorBeam();
            //HandleSpriteRotation();
        }

        private void FixedUpdate()
        {
            HandleMovement();
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
            moveSpeed = Mathf.Max(moveSpeed - _stasisBarrier.TotalNumChildAsteroids * .25f, 0f);

            //_inputVelocity = GameControls.DirectionalInput * 
            //_rb.velocity = Vector2.Lerp(_rb.velocity, _inputVelocity,5f * Time.deltaTime);
            //+ new Vector2(-_stasisBarrier.TotalNumChildAsteroids, 0f),

            _targetAimPos = Utils.WorldMousePos;
            
            
            _spriteTransform.right = Vector3.RotateTowards((Vector2)_spriteTransform.right, (_targetAimPos - (Vector2) transform.position).normalized,
                Mathf.Deg2Rad * _DEFAULT_ROTATE_SPEED * Time.fixedDeltaTime, 1f);

            _targetMovePos = GameControls.FollowTarget.IsHeld ? _targetAimPos : (Vector2)transform.position;
            
            _clampedTargetMovePos = Utils.ClampVectorInRadius(_targetMovePos, transform.position,
                _TARGET_POS_CLAMP_RADIUS);
            
            float speed = moveSpeed * (_clampedTargetMovePos - (Vector2)transform.position).magnitude / _TARGET_POS_CLAMP_RADIUS;

            Vector2 thrusterDirection = _spriteTransform.right;
            // _thrusterForce = thrusterDirection * (speed * Time.fixedDeltaTime);

            _desiredThrusterVelocity = thrusterDirection * speed;

            float acceleration = Vector2.Dot(_thrusterForce, thrusterDirection) < 0f
                ? _DEFAULT_BACKWARD_ACCELERATION
                : _DEFAULT_FORWARD_ACCELERATION;
            
            _thrusterForce = (_desiredThrusterVelocity - Velocity) * acceleration;
            _thrusterForce = _thrusterForce.SetMagnitude(Mathf.Min(_thrusterForce.magnitude, acceleration));
            
            //Debug.Log($"desired mag: {_desiredThrusterVelocity.magnitude}, actual mag: {Velocity.magnitude}");
            _rb.AddForce(_thrusterForce);
    
           // int thrusterSpawnRate = (int)(30f * (_thrusterForce.magnitude / _DEFAULT_FORWARD_ACCELERATION));
           // Debug.Log(thrusterSpawnRate);
           // _rightThrusterVfx.SetInt(_THRUSTER_VFX_SPAWN_RATE_HASH, thrusterSpawnRate);
            //_leftThrusterVfx.SetInt(_THRUSTER_VFX_SPAWN_RATE_HASH, thrusterSpawnRate);
            
            Stamina = Mathf.Clamp(
                Stamina + (pressingSprint ? -_DEFAULT_STAMINA_DECAY_RATE : _DEFAULT_STAMINA_RECOVERY_RATE) * Time.fixedDeltaTime, 0f,
                MaxStamina);
        }

        private void HandleMiningBeam()
        {
            _miningBeam.length = _DEFAULT_MINING_BEAM_LENGTH;
            _miningBeam.damageRate = _DEFAULT_MINING_BEAM_DAMAGE_RATE;
            _miningBeam.firing = GameControls.FireMiningBeam.IsHeld;
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

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawRay(transform.position, _desiredThrusterVelocity);
            Gizmos.color = Color.red;
            Gizmos.DrawRay(transform.position, _thrusterForce);
            Gizmos.color = Color.white;
            Gizmos.DrawRay(transform.position, Velocity);
            Gizmos.DrawWireSphere(_targetMovePos, .25f);
            Gizmos.DrawWireSphere(_clampedTargetMovePos, .25f);
        }
    }
}
