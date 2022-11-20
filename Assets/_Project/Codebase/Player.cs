using JetBrains.Annotations;
using UnityEditor;
using UnityEngine;
using UnityEngine.VFX;

namespace _Project.Codebase
{
    public class Player : MonoSingleton<Player>, ICollector
    {
        [SerializeField] private Rigidbody2D _rb;
        [SerializeField] private TractorBeam _tractorBeam;
        [SerializeField] private Transform _spriteTransform;
        [SerializeField] private StasisBarrier _stasisBarrier;
        [SerializeField] private MiningBeam _miningBeam;
        [SerializeField] private ProjectileLauncher _projectileLauncher;
        [SerializeField] private VisualEffect _leftThrusterVfx;
        [SerializeField] private VisualEffect _rightThrusterVfx;

        public Vector2 Velocity
        {
            get => _rb.velocity;
            set => _rb.velocity = value;
        }

        public bool InTraversalMode { get; private set; }
        public bool StasisActivated { get; private set; }
        public float MaxStamina { get; private set; } = _DEFAULT_MAX_STAMINA;
        public float Stamina { get; private set; }
        public float TractorBeamWidth { get; private set; } = _DEFAULT_TRACTOR_BEAM_WIDTH;
        public float TractorBeamLength { get; private set; } = _DEFAULT_TRACTOR_BEAM_LENGTH;
        
        [field: SerializeField] public float MaxSpeedMultiplier { get; private set; }
        [field: SerializeField] public float FinalMaxSpeed => _DEFAULT_MAX_SPEED * MaxSpeedMultiplier;

        [field: SerializeField] public float AccelerationMultiplier { get; private set; }
        [field: SerializeField] public float FinalAcceleration => _DEFAULT_FORWARD_ACCELERATION * AccelerationMultiplier;
        
        public Vector2 ThrusterForce { get; private set; }
        public Vector2 DesiredThrusterVelocity { get; private set; }

        private float ForwardThrusterForce => Mathf.Max(Vector2.Dot(ThrusterForce, _spriteTransform.right), 0f);
        private float DesiredForwardThrusterForce => Mathf.Max(Vector2.Dot(DesiredThrusterVelocity, _spriteTransform.right), 0f);
        private float ArtificialThrusterForce => 0f;
        
        private float _angularVelocity;
        private Vector2 _targetAimPos;
        private Vector2 _targetMovePos;
        private Vector2 _clampedTargetMovePos;
        private Vector2 _inputVelocity;
        private Vector2 _screenMousePos;
        
        public const float TARGET_POS_CLAMP_RADIUS = 5f;
        
        private const float _DEFAULT_MAX_SPEED = 15f;
        private const float _DEFAULT_FORWARD_ACCELERATION = 25f;
        private const float _DEFAULT_BACKWARD_ACCELERATION = 8f;
        private const float _DEFAULT_MINING_BEAM_LENGTH = 3f;
        private const float _DEFAULT_MINING_BEAM_DAMAGE_RATE = .25f;
        private const float _DEFAULT_TRACTOR_BEAM_LENGTH = 4f;
        private const float _DEFAULT_TRACTOR_BEAM_WIDTH = .5f;
        private const float _SPRINT_SPEED_INCREASE = 2.5f;
        private const float _DEFAULT_MAX_STAMINA = 5f;
        private const float _DEFAULT_STAMINA_DECAY_RATE = 1f;
        private const float _DEFAULT_STAMINA_RECOVERY_RATE = .75f;
        private const float _DEFAULT_MAX_ROTATE_SPEED = 540f;
        private const float _DEFAULT_ROTATE_ACCELERATION = 45f;

        private const string _THRUSTER_VFX_SPAWN_RATE_HASH = "SpawnRate";


        private void Start()
        {
            Stamina = MaxStamina;
            _stasisBarrier.SetActive(StasisActivated);
        }
        
        [UsedImplicitly]
        private void Update()
        {
            if (GameControls.FireProjectiles.IsHeld)
            {
                _projectileLauncher.Fire();
            }
            HandleStasis();
            HandleMiningBeam();
            HandleThrusterFX();
            HandleSprint();
            //HandleTractorBeam();
        }

        private void FixedUpdate()
        {
            HandleMovement();
            AttractAndTryCollectCollectables();
        }

        private void HandleSprint()
        {
            bool pressingSprint = GameControls.Sprint.IsHeld;
            Stamina = Mathf.Clamp(
                Stamina + (pressingSprint ? -_DEFAULT_STAMINA_DECAY_RATE : _DEFAULT_STAMINA_RECOVERY_RATE) * Time.deltaTime, 0f,
                MaxStamina);
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
            //float moveSpeed = _DEFAULT_MAX_SPEED + (pressingSprint && Stamina > 0f ? _SPRINT_SPEED_INCREASE : 0f);
            //moveSpeed = Mathf.Max(moveSpeed - _stasisBarrier.TotalNumChildAsteroids * .25f, 0f);

            Vector2 keyboardInput = GameControls.DirectionalInput;
            Vector2 keyboardInputSigns = new Vector2(Mathf.Abs(keyboardInput.x) > 0f ? Mathf.Sign(keyboardInput.x) : 0f, 
                Mathf.Abs(keyboardInput.y) > 0f ? Mathf.Sign(keyboardInput.y) : 0f);
            
            SetTargets();

            ManageRotation();
            
            HandleTranslation();
        }

        private void HandleTranslation()
        {
            float desiredSpeed = FinalMaxSpeed * (_clampedTargetMovePos - (Vector2) transform.position).magnitude / TARGET_POS_CLAMP_RADIUS;

            Vector2 thrusterDirection = _spriteTransform.right;
            DesiredThrusterVelocity = thrusterDirection * desiredSpeed;
            Vector2 desiredToVelDiff = DesiredThrusterVelocity - Velocity;

            float diffDot = Vector2.Dot(desiredToVelDiff.normalized, thrusterDirection);
            float dot = Vector2.Dot(DesiredThrusterVelocity, thrusterDirection);
            float acceleration =
                GameControls.FollowTarget.IsHeld
                    ? FinalAcceleration
                    : _DEFAULT_BACKWARD_ACCELERATION; //Mathf.Clamp01(diffDot) * FinalAcceleration + Mathf.Clamp(diffDot, -1f, 0f) * _DEFAULT_BACKWARD_ACCELERATION;
            //float driftAcceleration = 
            //    Mathf.Clamp01(diffDot) * FinalAcceleration + Mathf.Clamp(diffDot, -1f, 0f) * _DEFAULT_BACKWARD_ACCELERATION;

            Vector2 forwardDir = _spriteTransform.right;
            Vector2 perpDir = Vector2.Perpendicular(forwardDir);

            float perpVel = Vector2.Dot(Velocity, perpDir);
            float forwardVel = Vector2.Dot(Velocity, forwardDir);

            float perpVelDiff = Vector2.Dot(desiredToVelDiff, perpDir);
            float forwardVelDiff = Vector2.Dot(desiredToVelDiff, forwardDir);

            bool driftMode = !GameControls.FollowTarget.IsHeld && Mathf.Abs(diffDot) < .4f;
            //Debug.Log($"dot: {dot}, diffDot: {diffDot}, forwardVelDiff: {forwardVelDiff}, drift: {driftMode}, artificial a: {ArtificialThrusterForce}, a: {acceleration}");

            if (GameControls.DisableThrusters.IsHeld)
                ThrusterForce = Vector2.zero;
            else
            {
                float perpForce = perpVelDiff * acceleration; //(driftMode ? 0f : acceleration);
                float forwardForce = forwardVelDiff * acceleration; //(driftMode ? acceleration : acceleration);

                ThrusterForce = perpDir * perpForce + forwardDir * forwardForce;

                //Debug.DrawRay(transform.position, perpDir * perpVel, Color.white);
                //Debug.DrawRay(transform.position, forwardDir * forwardVel, Color.white);

                Debug.DrawRay(transform.position, perpDir * perpForce, Color.red);
                Debug.DrawRay(transform.position, forwardDir * forwardForce, Color.red);

                //ThrusterForce = (DesiredThrusterVelocity - Velocity) * acceleration;
                ThrusterForce = ThrusterForce.SetMagnitude(Mathf.Min(ThrusterForce.magnitude, acceleration));
            }

            Vector2 perpDrag = Vector2.zero; //Vector2.ClampMagnitude(perpDir * (dragMag * Mathf.Sign(perpVel)), perpVel);
            float perpDragMag = 20f * ForwardThrusterForce / FinalAcceleration;
            float forwardDragMag = 10f * Vector2.Dot(Velocity, Vector2.Perpendicular(thrusterDirection)) / FinalAcceleration;

            //Debug.DrawRay(transform.position, perpDrag, Color.red);
            _rb.AddForce(ThrusterForce + perpDrag);
            Vector2 velWithNoPerpForce = forwardDir * Vector2.Dot(Velocity, forwardDir);
            Vector2 velWithNoForwardForce = perpDir * Vector2.Dot(Velocity, perpDir);
            if (GameControls.FollowTarget.IsHeld)
                _rb.velocity = Vector2.MoveTowards(_rb.velocity, velWithNoPerpForce, perpDragMag * Time.fixedDeltaTime);
            // else if (driftMode)
            //    _rb.velocity = Vector2.MoveTowards(_rb.velocity, velWithNoForwardForce, forwardDragMag * Time.fixedDeltaTime);
        }

        private void SetTargets()
        {
            _targetAimPos = Utils.WorldMousePos;
            if (GameControls.FollowTarget.IsHeld)
                _targetMovePos = _targetAimPos;
            else
                _targetMovePos = transform.position;
            
            _clampedTargetMovePos = Utils.ClampVectorInRadius(_targetMovePos, transform.position,
                TARGET_POS_CLAMP_RADIUS);
        }

        private void ManageRotation()
        {
            Vector2 desiredDir = (_targetAimPos - (Vector2) transform.position).normalized;
            //float desiredAngleChange = Vector2.SignedAngle(_spriteTransform.right, desiredDir);
            //float desiredAngleSpeed = _DEFAULT_MAX_ROTATE_SPEED;
            //float angleAcceleration = _DEFAULT_ROTATE_ACCELERATION;
            //float angleForce =(desiredAngleSpeed - _angularVelocity) * angleAcceleration;
            
            //Mathf.Max(angleAcceleration - _angularVelocity, Mathf.Abs(angleAcceleration)) * 
            //             Mathf.Sign(angleAcceleration);

            //_angularVelocity += angleForce;
            
            _spriteTransform.eulerAngles = new Vector3(_spriteTransform.eulerAngles.x, _spriteTransform.eulerAngles.y, 
                _spriteTransform.eulerAngles.z + _angularVelocity * Time.fixedDeltaTime);
            //Utils.Remap(Mathf.Clamp(angleVelocity, -25f, 25f), -25f, 25f, -120f, 120f)
            
            float desiredAngle = Utils.DirectionToAngle(desiredDir);
            _spriteTransform.eulerAngles = new Vector3(_spriteTransform.eulerAngles.x, _spriteTransform.eulerAngles.y,
                Mathf.MoveTowardsAngle(_spriteTransform.eulerAngles.z, desiredAngle, 
                    _DEFAULT_MAX_ROTATE_SPEED * Time.fixedDeltaTime));

        }
        
        private void HandleThrusterFX()
        {
             int thrusterSpawnRate = (int)(30f * (DesiredForwardThrusterForce / FinalMaxSpeed));
             _rightThrusterVfx.SetInt(_THRUSTER_VFX_SPAWN_RATE_HASH, thrusterSpawnRate);
            _leftThrusterVfx.SetInt(_THRUSTER_VFX_SPAWN_RATE_HASH, thrusterSpawnRate);
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

        private void OnDrawGizmos()
        {
            Handles.Label(transform.position + new Vector3(-1.125f, 0f, 0f), 
                (DesiredForwardThrusterForce / FinalMaxSpeed).ToString());
            Handles.Label(transform.position + new Vector3(.75f, 0f, 0f), 
                (ForwardThrusterForce / FinalAcceleration).ToString());
             Gizmos.color = Color.yellow;
            Gizmos.DrawRay(transform.position, DesiredThrusterVelocity);
            Gizmos.color = Color.red;
            //Gizmos.DrawRay(transform.position, ThrusterForce);
            Gizmos.color = Color.white;
            Gizmos.DrawRay(transform.position, Velocity);
            Gizmos.DrawWireSphere(_targetMovePos, .25f);
            Gizmos.DrawWireSphere(_clampedTargetMovePos, .25f);
            
        }

        public void CollectCollectable(ICollectable collectable)
        {
            collectable.Collect();
        }

        public void AttractAndTryCollectCollectables()
        {
            Vector2 attractionSource = transform.position;
            foreach (IAttractableCollectable collectable in IAttractableCollectable.attractableCollectables)
            {
                Vector2 collectableToPlayerDir = attractionSource - collectable.Position;
                float dist = collectableToPlayerDir.magnitude;
                if (dist < collectable.PickUpDistance)
                {
                    CollectCollectable(collectable);
                    continue;
                }
                if (dist > collectable.AttractionDistance)
                    continue;

                float maxSpeed = 5f;
                float attractionStrength = maxSpeed * Mathf.Exp(-dist * 2f / collectable.AttractionDistance);
                collectable.Velocity = Vector2.MoveTowards(collectable.Velocity, 
                    collectableToPlayerDir.normalized * maxSpeed, 
                    attractionStrength * Time.fixedDeltaTime);
            }
        }
    }
}
