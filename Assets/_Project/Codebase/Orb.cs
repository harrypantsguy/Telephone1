using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using UnityEngine;

namespace _Project.Codebase
{
    public class Orb : MonoBehaviour, IOrbParent, IDamageable
    {
        [field: SerializeField] public float Radius { get; private set; }
        [field: SerializeField] public int Health { get; private set; }
        [field: SerializeField] public bool Attached { get; private set; }
        [SerializeField] private Rigidbody2D _rb;
        [SerializeField] private SpriteRenderer _spriteRenderer;
        [SerializeField] private GameObject _scoreIncreasePrefab;

        public Transform Transform => transform;
        public List<Orb> DirectChildrenOrbs { get; } = new List<Orb>();

        public int TotalChildOrbCount => DirectChildrenOrbs.Sum(childOrb => childOrb.TotalChildOrbCount) + DirectChildrenOrbs.Count;

        private IOrbParent _parent;

        [UsedImplicitly]
        private void Start()
        {
            _spriteRenderer.color = new Color(Random.Range(0f, 1f), Random.Range(0f, 1f), Random.Range(0f, 1f));
        }

        [UsedImplicitly]
        private void FixedUpdate()
        {
            _rb.velocity = Vector2.left * 2f;

            if (_rb.position.x < -10f)
            {
                for (var i = DirectChildrenOrbs.Count - 1; i >= 0; i--)
                    RemoveOrbAsChild(DirectChildrenOrbs[i]);
                Destroy(gameObject);
            }
        }

        [UsedImplicitly]
        private void OnCollisionEnter2D(Collision2D col)
        {
            if (Attached) return;

            if (col.gameObject.TryGetComponent(out IOrbParent orbParent))
            {
                orbParent.AddOrbAsChild(this);
                Attached = true;
            }
        }

        public void AddOrbAsChild(in Orb newChildOrb)
        {
            DirectChildrenOrbs.Add(newChildOrb);
            newChildOrb.SetParent(this);
        }

        public void RemoveOrbAsChild(in Orb childOrb)
        {
            childOrb.SetParent(null);
            DirectChildrenOrbs.Remove(childOrb);
        }

        public void SetParent(in IOrbParent parent)
        {
            _parent = parent;

            if (_parent == null) return;
            
            var offset = (transform.position - parent.Transform.position).normalized * (parent.Radius + Radius);
            transform.SetParent(parent.Transform);
            transform.localPosition = offset;
        }

        public void TakeDamage(in int damage)
        {
            Health = Mathf.Max(Health - damage, 0);

            if (Health == 0)
                Kill();
        }

        public void Kill()
        {
            if (_parent != null)
                _parent.RemoveOrbAsChild(this);

            for (var i = DirectChildrenOrbs.Count - 1; i >= 0; i--)
                DirectChildrenOrbs[i].Kill();

            ScoreHolder.Singleton.IncreaseScoreBy(1);

            var scoreIncrease = Instantiate(_scoreIncreasePrefab, _rb.position, Quaternion.identity).GetComponent<ScoreIncreaseUI>();
            scoreIncrease.SetIncrease(1);
            
            Destroy(gameObject);
        }
    }
}