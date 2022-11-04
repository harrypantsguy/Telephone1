using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using UnityEngine;

namespace _Project.Codebase
{
    public class Player : MonoSingleton<Player>, IOrbParent
    {
        [field: SerializeField] public float Radius { get; private set; }
        [SerializeField] private Rigidbody2D _rb;
        [SerializeField] private LineRenderer _lineRenderer;

        public bool OffScreen { get; private set; }
        public float MaxStamina { get; private set; } = _DEFAULT_MAX_STAMINA;
        public float Stamina { get; private set; }
        public Transform Transform => transform;
        public List<Orb> DirectChildrenOrbs { get; } = new List<Orb>();

        private const float _DEFAULT_MOVE_SPEED = 5f;
        private const float _DEFAULT_BEAM_LENGTH = 3f;
        private const float _DEFAULT_BEAM_DAMAGE_RATE = .25f;
        private const float _SPRINT_SPEED_INCREASE = 2.5f;
        private const float _DEFAULT_MAX_STAMINA = 5f;
        private const float _DEFAULT_STAMINA_DECAY_RATE = 1f;
        private const float _DEFAULT_STAMINA_RECOVERY_RATE = .75f;

        private int TotalNumChildOrbs => DirectChildrenOrbs.Sum(childOrb => childOrb.TotalChildOrbCount) + DirectChildrenOrbs.Count;
        
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

            bool pressingSprint = GameControls.Sprint.IsHeld;
            
            float moveSpeed = _DEFAULT_MOVE_SPEED + (pressingSprint && Stamina > 0f ? _SPRINT_SPEED_INCREASE : 0f);

            _inputVelocity = GameControls.DirectionalInput * moveSpeed;
            _rb.velocity = _inputVelocity + new Vector2(-TotalNumChildOrbs, 0f);

            Stamina = Mathf.Clamp(Stamina + (pressingSprint ?
                                      -_DEFAULT_STAMINA_DECAY_RATE : _DEFAULT_STAMINA_RECOVERY_RATE) * Time.deltaTime,
                0f, MaxStamina);
        }

        [UsedImplicitly]
        private void LateUpdate()
        {
            if (OffScreen) return;
            
            var beamStart = _rb.position;
            Vector2 beamEnd;
            var beamDir = (Utils.WorldMousePos - _rb.position).normalized;

            var hit = Physics2D.Raycast(_rb.position, beamDir, _DEFAULT_BEAM_LENGTH, 
                Layers.OrbMask);

            bool firingBeam = GameControls.FireBeam.IsHeld;
            
            if (hit.collider != null && hit.collider.TryGetComponent(out IDamageable damageable))
            {
                beamEnd = hit.point;
                if (Time.time > _lastDamageTime + _DEFAULT_BEAM_DAMAGE_RATE)
                {
                    if (firingBeam)
                        damageable.TakeDamage(1);
                    _lastDamageTime = Time.time;
                }
            }
            else
                beamEnd = (Vector2)transform.position + beamDir * _DEFAULT_BEAM_LENGTH;

            float beamAlpha = firingBeam ? 1f : .25f;
            
            _lineRenderer.startColor = _lineRenderer.startColor.SetAlpha(beamAlpha);
            _lineRenderer.endColor = _lineRenderer.endColor.SetAlpha(beamAlpha);
            _lineRenderer.positionCount = 2;
            _lineRenderer.SetPositions(new Vector3[]
            {
                beamStart,
                beamEnd
            });
        }

        public void AddOrbAsChild(in Orb newChildOrb)
        {
            DirectChildrenOrbs.Add(newChildOrb);
            newChildOrb.SetParent(this);
        }

        public void RemoveOrbAsChild(in Orb childOrb)
        {
            DirectChildrenOrbs.Remove(childOrb);
        }
    }
}
