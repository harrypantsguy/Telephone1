using System;
using UnityEngine;

namespace _Project.Codebase
{
    public class DistanceCollectable : MonoBehaviour, IAttractableCollectable
    {
        [field: SerializeField] public float PickUpDistance { get; set; }
        [field: SerializeField] public float AttractionDistance { get; set; }

        [SerializeField] private SpriteRenderer _spriteRenderer;
        [SerializeField] private TrailRenderer _trailRenderer;
        public Vector2 Velocity
        {
            get => _velocity;
            set
            {
                _velocity = value;
                _recievedVelocityUpdateThisFrame = true;
            }
        }
        private Vector2 _velocity;
        private bool _recievedVelocityUpdateThisFrame;
        
        public Vector2 Position
        {
            get => transform.position;
            set => transform.position = value;
        }

        private void Start()
        {
            IAttractableCollectable.attractableCollectables.Add(this);
        }

        private void OnDestroy()
        {
            _trailRenderer.transform.SetParent(null);
            _trailRenderer.autodestruct = true;
            IAttractableCollectable.attractableCollectables.Remove(this);
        }

        private void FixedUpdate()
        {
            transform.position += (Vector3)_velocity;
            
            if (!_recievedVelocityUpdateThisFrame)
                _velocity = Vector2.Lerp(_velocity, Vector2.zero, 4f * Time.fixedDeltaTime);
            
            _recievedVelocityUpdateThisFrame = false;
        }

        public void Collect()
        {
            Destroy(gameObject);
        }

        public static DistanceCollectable CreateCollectable(Vector2 position, Vector3 eulerAngles)
        {
            DistanceCollectable collectable = Instantiate(PrefabReferences.Singleton.scriptable.DistanceCollectable)
                .GetComponent<DistanceCollectable>();

            collectable.transform.position = position;
            collectable.transform.eulerAngles = eulerAngles;
            return collectable;
        }
    }
}