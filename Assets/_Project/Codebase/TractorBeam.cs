using UnityEngine;

namespace _Project.Codebase
{
    public class TractorBeam : MonoBehaviour
    {
        [SerializeField] private LineRenderer _lineRenderer;
        private Material _material;
        
        public float length;
        public float width;
        public bool firing;
        private static readonly int BeamSizeMultiplier = Shader.PropertyToID(_BEAM_WIDTH);

        private const string _BEAM_WIDTH = "BeamSizeMultiplier";

        private readonly Asteroid[] _asteroidsInRegion = new Asteroid[20];
        private readonly Collider2D[] _collisions = new Collider2D[20];
        private void Start()
        {
            _material = _lineRenderer.material;
        }

        private void Update()
        {
            ManageLineRenderer();

            int numAsteroids = GetAsteroidsInBeamRegionNonAlloc(_asteroidsInRegion);

            if (firing)
            {
                for (var i = 0; i < numAsteroids; i++)
                {
                    Asteroid asteroid = _asteroidsInRegion[i];
                    
                    var displacement = asteroid.transform.position - transform.position;
                    var force = displacement.normalized * 
                                (Time.deltaTime * Mathf.Lerp(0f, 20f, Mathf.Max(1f - displacement.magnitude / length, .3f)));
                    asteroid.Velocity -= (Vector2)force;
                    
                    asteroid.hasReceivedTractorBeamUpdateThisFrame = true;
                }
            }

            _material.SetFloat(BeamSizeMultiplier, width);
        }

        private int GetAsteroidsInBeamRegionNonAlloc(Asteroid[] asteroids)
        {
            var beamDir = (Vector2)transform.right;
            var dirPerp = Vector2.Perpendicular(beamDir);
            float widthExtents = width / 2f;
            
            Vector2 areaStart = (Vector2) transform.position + dirPerp * widthExtents;
            Vector2 areaEnd = (Vector2) transform.position + length * beamDir - dirPerp * widthExtents;
            
            var size = Physics2D.OverlapAreaNonAlloc(areaStart, areaEnd, _collisions, Layers.AsteroidMask);

            for (var i = 0; i < size; i++)
            {
                Collider2D collider = _collisions[i];
                
                asteroids[i] = collider.GetComponent<Asteroid>();
            }

            return size;
        }
        
        private void ManageLineRenderer()
        {
            _lineRenderer.enabled = firing;
            _lineRenderer.positionCount = 2;
            _lineRenderer.SetPositions(new[]
            {
                transform.position,
                transform.position + transform.right * length
            });
        }
    }
}