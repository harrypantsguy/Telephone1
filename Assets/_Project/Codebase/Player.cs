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

        public Transform Transform => transform;
        public List<Orb> DirectChildrenOrbs { get; } = new List<Orb>();

        private const float _DEFAULT_MOVE_SPEED = 5f;
        private const float _DEFAULT_BEAM_LENGTH = 3f;
        private const float _DEFAULT_BEAM_DAMAGE_RATE = .25f;

        private int TotalNumChildOrbs => DirectChildrenOrbs.Sum(childOrb => childOrb.TotalChildOrbCount) + DirectChildrenOrbs.Count;
        
        private Vector2 _inputVelocity;
        private float _lastDamageTime;

        [UsedImplicitly]
        private void Update()
        {
            if (_rb.position.x < -10f)
            {
                return;
            }
            
            _inputVelocity = GameControls.DirectionalInput * _DEFAULT_MOVE_SPEED;
            _rb.velocity = _inputVelocity + new Vector2(-TotalNumChildOrbs, 0f);

            var beamStart = _rb.position;
            Vector2 beamEnd;
            var beamDir = (Utils.WorldMousePos - _rb.position).normalized;

            var hit = Physics2D.Raycast(_rb.position, beamDir, _DEFAULT_BEAM_LENGTH, 
                Layers.OrbMask);
            
            if (hit.collider != null && hit.collider.TryGetComponent(out IDamageable damageable))
            {
                beamEnd = hit.point;
                if (Time.time > _lastDamageTime + _DEFAULT_BEAM_DAMAGE_RATE)
                {
                    damageable.TakeDamage(1);
                    _lastDamageTime = Time.time;
                }
            }
            else
                beamEnd = (Vector2)transform.position + beamDir * _DEFAULT_BEAM_LENGTH;

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
