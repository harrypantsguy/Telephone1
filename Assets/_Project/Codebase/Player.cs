using System.ComponentModel;
using UnityEngine;

namespace _Project.Codebase
{
    public class Player : MonoSingleton<Player>
    {
        public bool fellOffScreen;
        public float mass;
        private Rigidbody2D _rb;
        private LineRenderer _lineRenderer;

        private Vector2 _inputVelocity;
        private float _lastDamageTime;
        
        private const float DEFAULT_MOVE_SPEED = 5f;
        private const float DEFAULT_BEAM_LENGTH = 3f;
        private const float DEFAULT_BEAM_DAMAGE_RATE = .25f;

        private void Start()
        {
            _rb = GetComponent<Rigidbody2D>();
            _lineRenderer = GetComponent<LineRenderer>();
            _lastDamageTime = 0f;
        }
    
        private void Update()
        {
            if (_rb.position.x < -10f)
            {
                fellOffScreen = true;
                return;
            }
            
            _inputVelocity = GameControls.DirectionalInput * DEFAULT_MOVE_SPEED;
            _rb.velocity = _inputVelocity + new Vector2(-mass, 0f);

            Vector2 beamStart = _rb.position, beamEnd;
            
            Vector2 beamDir = (Utils.WorldMousePos - _rb.position).normalized;
            RaycastHit2D hit = Physics2D.Raycast(_rb.position, beamDir, DEFAULT_BEAM_LENGTH, 
                Layers.OrbMask);

            
            if (hit.collider != null && hit.collider.TryGetComponent(out Orb orb))
            {
                beamEnd = hit.point;
                if (Time.time > _lastDamageTime + DEFAULT_BEAM_DAMAGE_RATE)
                {
                    orb.TakeDamage(1);
                    _lastDamageTime = Time.time;
                }
            }
            else
                beamEnd = (Vector2)transform.position + beamDir * DEFAULT_BEAM_LENGTH;

            _lineRenderer.positionCount = 2;
            _lineRenderer.SetPositions(new Vector3[]
            {
                beamStart,
                beamEnd
            });
        }
    }
}
