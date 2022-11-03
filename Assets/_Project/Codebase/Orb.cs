using UnityEngine;
using Random = UnityEngine.Random;

namespace _Project.Codebase
{
    public class Orb : MonoBehaviour
    {
        //private bool _attached;
        private bool Attached => transform.parent != null; // I did this for time, I hate it
        private Rigidbody2D _rb;
        private SpriteRenderer _spriteRenderer;

        public int Health { get; private set; } = 5;
        
        private void Start()
        {
            _rb = GetComponent<Rigidbody2D>();

            _spriteRenderer = GetComponent<SpriteRenderer>();
            _spriteRenderer.color = new Color(Random.Range(0f, 1f), Random.Range(0f, 1f), 
                Random.Range(0f, 1f));
        }

        private void Update()
        {
            _rb.velocity = new Vector2(Attached ? 0f : -2f, 0f);
            
            if (_rb.position.x < -10f)
                Destroy(gameObject);
        }

        private void OnDestroy()
        {
            if (Attached)
                Player.Singleton.mass -= 1f;
            transform.DetachChildren();
        }

        public void TakeDamage(float damage)
        {
            Health = Mathf.Max(Health - (int)damage, 0);
            if (Health == 0)
            {
                if (Attached)
                    Player.Singleton.mass -= 1f;
                Destroy(gameObject);
            }
        }

        private void OnCollisionEnter2D(Collision2D other)
        {
            if (other.collider.TryGetComponent(out Player player))
            {
                transform.SetParent(player.transform);
                player.mass += 1f;
            }
            else if (other.collider.TryGetComponent(out Orb orb))
            {
                if (orb.Attached)
                {
                    transform.SetParent(orb.transform);
                    Player.Singleton.mass += 1f;
                }
            }
        }
    }
}