using System;
using UnityEngine;

namespace _Project.Codebase
{
    public class Projectile : MonoBehaviour
    {
        public Vector2 Velocity;
        public float speed;
        public float radius;

        private bool _queuedForDestruction;
        private void Update()
        {
            if (_queuedForDestruction)
            {
                Destroy(gameObject);
            }
        }

        private void FixedUpdate()
        {
            Velocity = transform.right * (speed * Time.fixedDeltaTime);

            RaycastHit2D hit = Physics2D.CircleCast(transform.position, radius, Velocity.normalized, 
                Velocity.magnitude, Layers.AsteroidMask);
            
            Vector2 newPosition;

            if (hit)
            {
                newPosition = hit.point - Velocity.normalized * radius;
                _queuedForDestruction = true;
            }
            else
                newPosition = (Vector2)transform.position + Velocity;

            transform.position = newPosition;
        }
    }
}