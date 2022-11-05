using System;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.VFX;

namespace _Project.Codebase
{
    public class MiningBeam : MonoBehaviour
    {
        [SerializeField] private LineRenderer _lineRenderer;
        [SerializeField] private VisualEffect _beamHitVfx;
        [SerializeField] private Light2D _beamHitLight;
        
        public float length;
        public bool firing;
        public float damageRate;

        private float _lastDamageTime;
        
        private const string _BEAM_HIT_VFX_SPAWN_RATE_HASH = "SpawnRate";

        private void Update()
        {
            var beamStart = transform.position;
            Vector2 beamEnd;
            var beamDir = (Vector2)transform.right;

            var hit = Physics2D.Raycast(transform.position, beamDir, length, 
                Layers.AsteroidMask);

            if (firing)
            {
                _lineRenderer.enabled = true;
                
                if (hit.collider != null && hit.collider.TryGetComponent(out IDamageable damageable))
                {
                    beamEnd = hit.point;
                    _beamHitLight.transform.position = beamEnd;
                    _beamHitVfx.transform.position = beamEnd;
                    _beamHitVfx.transform.right = transform.right;

                    if (Time.time > _lastDamageTime + damageRate)
                    {
                        if (firing)
                            damageable.TakeDamage(1);
                        _lastDamageTime = Time.time;
                    }

                    _beamHitLight.enabled = true;
                    _beamHitVfx.SetInt(_BEAM_HIT_VFX_SPAWN_RATE_HASH, 32);
                }
                else
                {
                    beamEnd = (Vector2)transform.position + beamDir * length;
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
    }
}