using UnityEngine;

namespace _Project.Codebase
{
    public class ShipMovementUI : MonoBehaviour
    {
        [SerializeField] private Transform _targetPos;
        private LineRenderer _lineRenderer;
        
        private Player _player;

        private void Start()
        {
            _player = Player.Singleton;
            _lineRenderer = GetComponent<LineRenderer>();
        }

        private void Update()
        {
            Vector2 mouseDir = Vector2.ClampMagnitude(Utils.WorldMousePos - (Vector2) _player.transform.position, 
                Player.TARGET_POS_CLAMP_RADIUS / 2f);
            Vector2 indicatorPoint = (Vector2) _player.transform.position + mouseDir;

            _targetPos.transform.position = indicatorPoint;

            /*
            _lineRenderer.SetPositions(new[]
            {
                transform.position,
                (Vector3)indicatorPoint
            });
            */
            
            /*
            int iterationsPerSecond = 100;
            float simulationTime = .25f;
            int numIterations = (int)(iterationsPerSecond * simulationTime);
            Vector3[] positions = new Vector3[numIterations];

            for (int i = 0; i < numIterations; i++)
            {
                float t = Time.fixedDeltaTime * i;
                positions[i] = (Vector2) _player.transform.position + _player.Velocity * t + _player.ThrusterForce * (t * t) / 2f;
            }

            _lineRenderer.positionCount = numIterations;
            _lineRenderer.SetPositions(positions);


            */
        }
    }
}